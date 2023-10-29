open System
open System.IO
#load "chordModule.fs"
open ChordModule
// global variables
m <- 10
numNodes <- 8
num <- pown 2 m

let idx = [|for i in 1..numNodes -> i*2|]


let mutable ring : ChordNode = create(idx,numNodes)
fingertable_establish(ring)

for i=1 to numNodes do
    // printfn "%d" ring.ID
    printfn "%A" ring.fingertable 
    ring <- ring.successor 
   

// for i=1 to 8 do
//     printfn "%d" ring.ID
//     ring <- ring.predecessor 

// type ChordNode (id: int) =
//     // let mutable successor = -1
//     // let mutable predecessor = -1
//     let mutable successor : ChordNode option = None 
//     let mutable predecessor : ChordNode option = None 
//     member this.ID:int = id
//     member this.fingertable = [|for _ in 1..m -> 0|]

    // Initialization: fingertable construction for every node
    // member this.ConstructFingertable(nodes: Map<int, ChordNode>) =
    //     let mutable successorValue = this.mySuccessor
    //     for i in 0..m-1 do
    //         let mutable incrementedID = (this.ID + (pown 2 i))%num
    //         while incrementedID > successorValue do
    //             // when it iterates back to the start of the ring
    //             if (nodes.[successorValue].mySuccessor < successorValue) then incrementedID <- -1
    //             successorValue <- nodes.[successorValue].mySuccessor
    //         this.fingertable.[i] <- successorValue

    // // set or get successor value
    // member this.mySuccessor
    //     with get() = successor
    //     and set(node) = successor <- node

    // // set or get predecessor value
    // member this.myPredecessor
    //     with get() = predecessor
    //     and set(node) = predecessor <- node

// create Chord ring
// let create (nodes:int array, numNodes:int) : Map<int, ChordNode> =
//     let mutable cr = Map.empty<int, ChordNode> // empty Chord ring
//     //add every initial node to the ring
//     for i in 0..numNodes-1 do
//         let mutable theNode = new ChordNode(nodes[i])
//         cr <- Map.add nodes.[i] theNode cr

//     // set successor
//     for i in 0..numNodes-2 do
//         cr.[nodes[i]].mySuccessor <- cr.[nodes[i+1]].ID
//     cr.[nodes[numNodes-1]].mySuccessor <- cr.[nodes[0]].ID

//     // set predecessor
//     for i in 1..numNodes-1 do
//         cr.[nodes[i]].myPredecessor <- cr.[nodes[i-1]].ID
//     cr.[nodes[0]].myPredecessor <- cr.[nodes[numNodes-1]].ID

//     // set fingertables
//     for i in 0..numNodes-1 do
//         cr.[nodes[i]].ConstructFingertable(cr)

//     cr

// main
// read arguments (tmp for dotnet fsi)
// let args = fsi.CommandLineArgs
// let numNodes = int(args[1])
// let numRequests = int(args[2])
// printfn "Number of nodes: %d" numNodes
// printfn "Number of requests: %d" numRequests
// printfn "This is a %d-bit ID space." m  // m is fixed for now

// randomly generate n nodes with specific ID
// let rnd = new Random()
// let idx = [|for _ in 1..numNodes -> rnd.Next(0, num-1)|]
// let nodes = Array.sort idx  // array of node IDs
// printfn "All the nodes: %A" nodes

// test Chord ring
// let mutable chordRing = create(nodes, numNodes)

// test
// Console.WriteLine(chordRing |> Map.count)
// printfn "%d" chordRing[nodes[0]].mySuccessor
// printfn "%d" chordRing[nodes[0]].myPredecessor
