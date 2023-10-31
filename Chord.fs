module ChordModule
open System.Collections.Generic
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
        member val fingertable = [|for _ in 1..m -> selfRef|] with get, set
        member val fingerDictionary = new Dictionary<int, ChordNode>()
        member this.fingerTableConstruction()=
            // printfn "%d -> %d" this.ID this.successor.ID
            let mutable next_successor = this.successor
            for i=0 to m-1 do // for all fingertable index:
                let key = (this.ID+(pown 2 i))%num
                let mutable isLoop = true
                while isLoop do
                    let mutable start_ID = next_successor.predecessor.ID
                    let mutable end_ID = next_successor.ID
                    if start_ID < end_ID then // normal situation
                        if key > start_ID && key <= end_ID
                        then isLoop <- false
                        else 
                            next_successor <- next_successor.successor
                    else // ring gap
                        if key > start_ID || key <= end_ID
                        then 
                            isLoop <- false
                        else
                            next_successor <- next_successor.successor

                this.fingertable.[i] <- next_successor
                // printfn "%d -=-=- %d " (pown 2 i+this.ID) this.ID 
                if this.fingerDictionary.ContainsKey(pown 2 i + this.ID) then
                    this.fingerDictionary[pown 2 i + this.ID] <- next_successor
                else
                    this.fingerDictionary.Add((pown 2 i+this.ID),next_successor);

        member this.closest_preceding_node(id:int) : ChordNode =
            let mutable result = this.fingertable[m-1]
            for i = m-1 downto 1 do
                let mutable end_ID = this.fingertable.[i].ID
                let mutable start_ID = this.fingertable.[i-1].ID
                if start_ID < end_ID 
                then // normal situation
                    if id>start_ID && id<= end_ID 
                    then
                        result <- this.fingertable.[i-1]
                else // ring gap
                    if id > start_ID || id <= end_ID 
                    then
                        result <- this.fingertable.[i-1]
            result

        member this.find_successor(id:int) : ChordNode = 
            // if id = this.ID then selfRef
            let mutable result = selfRef
            let mutable start_ID = this.ID
            let mutable end_ID = this.successor.ID
            let mutable if_need_getClosest = true
            // printfn "%d ...>>. %d .<<.. %d" start_ID id end_ID

            if start_ID < end_ID // normal situation
            then
                if id > start_ID && id <= end_ID then
                    result <- this.successor
                    if_need_getClosest <- false
            else // ring gap
                if id <= end_ID || id > start_ID then
                    result <- this.successor
                    if_need_getClosest <- false

            if if_need_getClosest 
            then
                let mutable closestNode = this.closest_preceding_node(id)
                result <- closestNode.find_successor(id)
            
            result
        member this.join(newNode: ChordNode) =
            this.successor <- newNode.find_successor(this.ID + 1)
            this.successor.predecessor <- this
            this.fingerTableConstruction()


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

    let addNodeToRing(nodeToAdd: ChordNode, existingNode: ChordNode) =
        let successor = existingNode.find_successor(nodeToAdd.ID)
        let prev = successor.predecessor
        prev.successor <- nodeToAdd
        nodeToAdd.predecessor <- prev
        nodeToAdd.successor <- successor
        successor.predecessor <- nodeToAdd
        nodeToAdd.fingerTableConstruction()
        existingNode.fingerTableConstruction()
        


    
