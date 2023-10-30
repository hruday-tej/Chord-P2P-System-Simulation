module ChordModule
    // global variables
    let mutable m = 0
    let mutable numNodes = 0
    let mutable num = 0

    // ChordNode type
    type ChordNode (id: int) =
        let mutable selfRef = Unchecked.defaultof<ChordNode>
        member val successor = selfRef with get, set
        member val predecessor = selfRef with get, set
        member val ID:int = id with get, set
        member val fingertable = [|for _ in 1..m -> 0|] with get, set
        member this.fingerTableConstruction()=
            let mutable next_successor = this.successor
            for i=0 to m-1 do // for all fingertable index:
                let key = (this.ID+(pown 2 i))%num
                let mutable isLoop = true
                while isLoop do
                    let mutable start_ID = next_successor.predecessor.ID
                    let mutable end_ID = next_successor.ID
                    if start_ID < end_ID then
                        if key > start_ID && key <= end_ID
                        then isLoop <- false
                        else 
                            next_successor <- next_successor.successor
                    else
                        if key > start_ID || key <= end_ID
                        then 
                            isLoop <- false
                        else
                            next_successor <- next_successor.successor

                this.fingertable.[i] <- next_successor.ID


    // create ring
    let create (nodes:int array, numNodes:int):ChordNode =
        // printfn "%A" nodes
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
        let mutable node = ring
        for j=0 to numNodes-1 do
            node.fingerTableConstruction()
            node <- node.successor

        


    
