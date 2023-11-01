#r "nuget: Akka.FSharp" 
#r "nuget: Akka.TestKit"
#r "nuget: Akka.Remote" 
#load "chord.fs"
open System
open System.Security.Cryptography
open System.Text
open Akka.Actor
open System.Collections.Generic
open Akka.Configuration
open Akka.FSharp
open Akka.TestKit
open ChordModule
m <- 20
num <- pown 2 m
let fingerTableSize = m
let netwRingSize = 2.**m |> int64
numNodes <- fsi.CommandLineArgs.[1] |> int
let numRequests = fsi.CommandLineArgs.[2] |> int
let akkaSystem: ActorSystem = ActorSystem.Create("ChordP2P", Configuration.defaultConfig())
let getWorkerById id =
    select (@"akka://ChordP2P/user/worker" + string id) akkaSystem

let randomLongGeneration min max =
    let longRand = int64 (Random().Next(Int32.MaxValue))
    (longRand % (max - min) + min)

let mutable idx = [|for i in 1..numNodes -> Random().Next(0, num)|]
idx <- Array.sort idx

type Config = 
    | Input of (int*int)
    | NetworkDoneNotification of (int)
    | Request of (int64*int64*int)
    | Response of (int64)
    | GenerateReport of (int)
    | Create of (int64*int64*list<int * int64>)
    | Join of (int64)
    | Notify of (int64)
    | FindSuccessor of (int64)
    | CheckPredecessor of (int64)

let mutable actor_try : ChordNode = create(idx,numNodes)
fingertable_establish(actor_try)
let mutable goodactor : ChordNode = create(idx,numNodes)

let createWorkerNode id = 
    spawn akkaSystem ("worker" + string id)
        (fun mailboxMessenger ->
            let bossActor: ActorSelection = select @"akka://ChordP2P/user/boss" akkaSystem

            let mutable predecessor = -1L
            let mutable successor = id;
            let mutable fingerTable = []
            let checkTargetValidity targetId startId endId =
                match startId, endId with
                | _ when startId > endId -> 
                    (targetId > startId && targetId <= netwRingSize - 1L) || (targetId >= 0L && targetId <= endId)
                | _ when startId < endId ->
                    targetId > startId && targetId <= endId
                | _ ->
                    true


            let checkRangeValidity targetId startId endId =
                match startId, endId with
                | s, e when s > e -> 
                    (targetId > s && targetId < netwRingSize - 1L) || (targetId >= 0L && targetId < e)
                | s, e when s < e ->
                    targetId > s && targetId < e
                | _ ->
                    false

            let stabilize _ =
                let mutable response = predecessor
                if id <> successor then 
                    response <- (Async.RunSynchronously((getWorkerById successor) <? CheckPredecessor(id)))
                    if response <> id then
                        if response <> -1L then successor <- response
                        (getWorkerById successor) <! Notify(id)

            let nearestPrecedingNode currId =
                let mutable res = id //0
                for i = fingerTableSize - 1 downto 0 do
                    let (_, successor) = fingerTable.[i]
                    if (checkRangeValidity successor id currId) then res <- successor
                res
                
            let findSuccessor currId = 
                // if target within the range of current node and its successor return successer
                // if not, find the closest preceding node of the target. The successor of preceding node should be same with target
                if (checkTargetValidity currId id successor) then 
                    successor
                else
                    if id = (currId |> nearestPrecedingNode) then successor else Async.RunSynchronously ((currId |> nearestPrecedingNode |> getWorkerById)  <? FindSuccessor(currId))

            let fingerTableUpdater _ = 
                let mutable next = 0
                let mutable finger: (int * int64) list = []
                // update each finger
                let fixFinger _ =
                    next <- next + 1
                    if next > fingerTableSize then next <- 1
                    findSuccessor (id + int64 (2. ** (float (next - 1))))
                for _ in 1..fingerTableSize do 
                    finger <- finger@[(next, fixFinger())]
                finger

            // Periodically run stablize and fixFingerTable
            let rec asyncFingerTableUpdateRunner _ =
                    async {
                        stabilize()
                        fingerTable <- fingerTableUpdater()
                        System.Threading.Thread.Sleep(50)
                        asyncFingerTableUpdateRunner ()
                    } |> Async.Start

            let successorCountUtility targetId ogId jumpNos = 
                if (checkTargetValidity targetId id successor) then successor |> getWorkerById <! Request(targetId, ogId, jumpNos + 1)
                else 
                    if id = (targetId |> nearestPrecedingNode) then successor |> getWorkerById <! Request(targetId, ogId, jumpNos + 1)
                    else
                        (targetId |> nearestPrecedingNode |> getWorkerById)  <! Request(targetId, ogId, jumpNos + 1)


            let rec repeateRecursion() =
                actor {
                    let! receivedMessage = mailboxMessenger.Receive()
                    let senderClient = mailboxMessenger.Sender()
                    match receivedMessage with
                    | Create(succ, pred, fTable) ->
                        successor <- succ
                        predecessor <- pred
                        fingerTable <- fTable
                        asyncFingerTableUpdateRunner ()
                    | Join(target) ->
                        fingerTable <- fingerTableUpdater()
                        successor <- (Async.RunSynchronously ((getWorkerById target) <? FindSuccessor(id)))
                        (getWorkerById successor) <! Notify(id)
                        fingerTable <- fingerTableUpdater()
                        asyncFingerTableUpdateRunner()
                    | FindSuccessor(currId) ->
                        senderClient <! findSuccessor currId
                    | Notify(predId) -> 
                        if predecessor = -1L || checkTargetValidity predId predecessor id then 
                            predecessor <- predId
                    | CheckPredecessor(currId) -> 
                        currId |> ignore
                        senderClient <! predecessor
                    | NetworkDoneNotification(reqNum) -> 
                        // Start sending request
                        for _ in 1 .. reqNum do
                            let key = randomLongGeneration 0L (netwRingSize - 1L)  //need to generate a random key within the chord size
                            let self = getWorkerById id
                            self <! Request(key, id, 0)
                    | Request(targetId, originId, jumpNum) ->
                        if checkTargetValidity targetId predecessor id then 
                            // report id to the node that made request
                            originId |> getWorkerById <! Response(id)
                            // report jumpNum to boss 
                            bossActor <! GenerateReport(jumpNum)
                        else 
                            successorCountUtility targetId originId jumpNum
                    | _ -> ()
                    return! repeateRecursion()
                }
            repeateRecursion()
        )

for i=0 to numNodes-1 do
    let mutable requests = [|for i in 1..numRequests -> Random().Next(0, num)|]
    for request in requests do
        actor_try.find_successor request |> ignore

let localActor (mailboxService:Actor<_>) = 
    let mutable completedRequests = 0
    let mutable localActorNum = 0
    let mutable totalJumpNos = 0
    let mutable totalRequests = 0
    // A set for nodes that will join to the network
    let mutable set = Set.empty
    
    // finger table creation for "0" node
    let initFTableZero thisNode otherNode =
        let finger =
            [1..fingerTableSize]
            |> List.map (fun next ->
                let range = thisNode + int64 (2. ** float (next - 1))
                if range <= otherNode then (next, otherNode) else (next, thisNode))
        finger
    
    // finger table for (100) node
    let initFingerTable idVal =
        List.init fingerTableSize (fun itr -> (itr + 1, idVal))


    let rec recursiveLoop () = actor {
        let! receivedMessage = mailboxService.Receive()
        match receivedMessage with 
        | Input(n,r) ->
            totalRequests <- n * r
            // Random actor generation
            if n > 2 then 
                localActorNum <- n - 2
                while set.Count < localActorNum do
                    let radNum = randomLongGeneration 1L (netwRingSize - 1L)
                    if radNum <> 100L then  
                        set <- set.Add(radNum)

            // Initialize the Chord network with two nodes
            let zero = createWorkerNode (0 |> int64) 
            let hundred = createWorkerNode (100 |> int64)
            zero <! Create(100L,100L,(initFTableZero 0L 100L))
            hundred <! Create(0L,0L,(initFingerTable 100L))
            let mutable addedNodeTracker = "0 | 100 "
            // Join nodes to the network
            set |> Set.toSeq |> Seq.iteri (fun _ x -> 
                Async.RunSynchronously // Add waiting time for the network to stabilize
                addedNodeTracker <- addedNodeTracker + " | " + string x
                let currentNode = createWorkerNode (x |> int64)
                currentNode <! Join(0L)
             )
            printfn($"node join() finished")
            printfn($"all nodes = {addedNodeTracker}")
            printfn($"stabilize() triggered")
            System.Threading.Thread.Sleep(5000)
            Async.RunSynchronously

            set |> Set.toSeq |> Seq.iteri (fun i x -> 
                getWorkerById (x |> int64) <! NetworkDoneNotification(numRequests)
            )
            zero <! NetworkDoneNotification(numRequests)
            hundred <! NetworkDoneNotification(numRequests)

        | GenerateReport(numOfJumps) ->
            completedRequests <- completedRequests + 1
            totalJumpNos <- num_of_hops
            if completedRequests = totalRequests then
                printfn($"Total Requests {completedRequests}\nAverage Hops: {float totalJumpNos / float completedRequests} \nTotal jumps: {totalJumpNos} \t")
                mailboxService.Context.System.Terminate() |> ignore
        | _ -> ()
        return! recursiveLoop()
    }
    recursiveLoop()

let bossActorReference = spawn akkaSystem "boss" localActor
bossActorReference <! Input(numNodes, numRequests)

akkaSystem.WhenTerminated.Wait()