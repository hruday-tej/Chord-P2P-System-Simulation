module ChordModule
    // ChordNode type
    let mutable m = 0
    let mutable numNodes = 0
    type ChordNode (id: int) =
        let mutable selfRef = Unchecked.defaultof<ChordNode>
        member val successor = selfRef with get, set
        member val predecessor = selfRef with get, set
        member this.ID:int = id
        member this.fingertable = [|for _ in 1..m -> 0|]



    
    // create ring
    let create (nodes:int array, numNodes:int):ChordNode =
        printfn "%A" nodes
        let root = new ChordNode(nodes[0])
        let mutable current_node = root
        for i=0 to numNodes-1  do
            if i = numNodes-1 
            then 
                current_node.successor <- root
                root.predecessor <- current_node
            else    
                let newNode = new ChordNode(nodes[i+1])
                current_node.successor <- newNode
                newNode.predecessor <- current_node
                current_node <- newNode
                // printfn "%d" newNode.ID
        root
    
    let fingertable_establish(ring:ChordNode) = 
        for i=0 to 
        ring.fingertable.[0] <- 5
        


    
