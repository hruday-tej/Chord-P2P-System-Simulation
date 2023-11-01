open System
open System.IO
#load "chord.fs"
open ChordModule
// global variables
m <- 20
numNodes <- 50
num <- pown 2 m
let mutable num_of_requests = 1
// printfn "%d" num
let mutable idx = [|for i in 1..numNodes -> Random().Next(0, num)|]
idx <- Array.sort idx
let mutable requests = [|for i in 1..num_of_requests -> Random().Next(0, num)|]
let mutable ring : ChordNode = create(idx,numNodes)
fingertable_establish(ring)
printfn "nodes:%A" idx
printfn "requests:%A" requests

for i=0 to numNodes-1 do
    printfn "ringID:%d" ring.ID
    for j = 0 to m-1 do
        printfn "%d" ring.fingertable.[j].ID 
    
    // let mutable requests = [|for i in 1..num_of_requests -> Random().Next(0, num)|]
    printfn "requests:%A" requests
    for request in requests do
        ring.find_successor request |> ignore

    // printfn "%d, %d"  ring.ID ring.successor.ID
    ring <- ring.successor 
    printfn "-----------------------------------------------------"

printfn "hops: %d" num_of_hops
printfn "requests: %d" (num_of_requests*numNodes)

// let mutable the_key_successor = ring.find_successor 101
// printfn "%d, %d"  ring.ID the_key_successor.ID

// for i=1 to 8 do
//     printfn "%d" ring.ID
//     ring <- ring.predecessor 

