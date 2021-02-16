#r "nuget: Akka.FSharp" 
#r "nuget: Akka.TestKit" 
open System
open System.Diagnostics
open System.Threading
open System.Collections.Generic
open System.Linq
open Akka.Actor
open Akka.Configuration
open Akka.FSharp
open Akka.TestKit
open Akka.Routing
let rand = System.Random()

let system = System.create "Pastry" (Configuration.load())
//Constructor : Initailizes the Routing table dimensions, Larger Leaf, Smaller Leaf dimensions
//StartActorRequest : Present actor initiates request to some random actors 
//InitalJoin : Joins the actor to predefined actor system[ actor are defined before ] using add buffer function [used for constructing routing table]
//Route : Does Pastry Routing algorithm and also joins an actor using pastry routing algorithm
// StopActor : stops the actor
type PastryMessage = 
    | Constructor of (int*int*int*int)
    | StartActorRequest
    | InitialJoin of List<int> * int  
    | Route of (string * int * int *int)
    | AddRow of (int * List<int>)
    | AddLeaf of (List<int>)
    | Update of (int)
    | Acknowledgement 
    | StopActor

//Initializer : Intializes number of actors, their routing table dimensions and required parameters
//StartJoining : Joins all actor to system
//JoinActor : Joins a Actor into the system
//JoiningFinished : After joining all actors, checks how many actors are can't be accessed and start routing
//AllStartRequest :  Master Actor, Each actor present in the system initiates request to some random actor
//AllRequestFinished : All actors have requested to some random actor. Gives output total number of Routes and average number of hops taken
//NotInBoth : 
type MasterMessage =
    | Initializer of (int*int)
    | StartJoining 
    | JoiningFinished 
    | JoinActor  
    | AllStartRequest 
    | NotInBoth
    | AllRequestFinished of (int*int*int)
    | RouteNotInBoth 

let PastryNode (mailbox:Actor<_>) = 
    let mutable smallerLeaf = new List<int>()
    let mutable maximumNodes = 0 
    let mutable largerLeaf = new List<int>()
    let mutable numOfBack = 0
    let mutable routingTable = new List<List<int>>()
    let mutable numRequests = 0
    let mutable numNodes = 0
    let mutable nodeId = 0
    let mutable cols = 0
    let mutable parentRef = null
    

    let minimum ( net: List<int>)=                          
            let mutable index = 0
            for i in 1..net.Count-1 do
                if net.[index]>net.[i] then
                    index <- i
            index

    let maximum ( net: List<int>)=                          
            let mutable index = 0
            for i in 1..net.Count-1 do
                if net.[index]<net.[i] then
                    index <- i
            index

    let toBase4String raw length =                       
        let mutable str=""
        let mutable rr=raw
        while ( rr <> 0 ) do
            let xx=rr%4
            str <- xx.ToString() + str
            rr <- (rr-(rr%4))/4
        
        let diff = length - str.Length
        if diff > 0 then
            let mutable j = 0
            while j < diff do 
                str <- "0" + str
                j <- j + 1
        str

    let commonPrefix (string1:string) (string2:string) =                   
        let mutable j = 0
        while (j < string1.Length && string1.[j] = string2.[j]) do 
            j <- j + 1
        j
    
    //updateState: updates the state of pastry nodes using a list of nodeIds
    let updateState all =                                                  
        for i in all do 
                
            if i > nodeId && not(largerLeaf.Contains(i)) then  
                if largerLeaf.Count < 4 then 
                    largerLeaf.Add(i)
                else 
                    if i < largerLeaf.Max() then
                        largerLeaf.[maximum largerLeaf ] <- i

                        
        
            else if i < nodeId && not(smallerLeaf.Contains(i)) then  
                if smallerLeaf.Count < 4 then
                    smallerLeaf.Add(i)               
                else 
                    if i > smallerLeaf.Min() then
                        smallerLeaf.[minimum smallerLeaf ] <- i
                        
          
            let myIdBase4 = toBase4String nodeId cols
            let iBase4 = toBase4String i cols
            let samePrefix = commonPrefix myIdBase4 iBase4
            
            if routingTable.[samePrefix].[int iBase4.[samePrefix] - 48 ] = -1 then
                routingTable.[samePrefix].[int iBase4.[samePrefix] - 48 ] <- i          
       
    
    // updateNode: updates the state of pastry node using a nodeId
    let updateNode node =                                                                  
    
            if node > nodeId && not(largerLeaf.Contains(node))  then
                if largerLeaf.Count < 4  then
                    largerLeaf.Add(node)
                else 
                    if  node < largerLeaf.Max()  then
                        largerLeaf.[ maximum(largerLeaf) ] <- node
                 
            elif (node < nodeId && not(smallerLeaf.Contains(node))) then
                if smallerLeaf.Count < 4 then
                    smallerLeaf.Add(node)
                else 
                    if node > smallerLeaf.Min() then
                      smallerLeaf.[minimum(smallerLeaf)] <- node
            
            let myIdBase4 = toBase4String nodeId cols
            let nodeBase4 = toBase4String node cols
            let samePrefix = commonPrefix myIdBase4 nodeBase4

            if routingTable.[samePrefix].[int nodeBase4.[samePrefix] - 48 ] = -1 then
                routingTable.[samePrefix].[int nodeBase4.[samePrefix] - 48 ] <- node
        
  
    let rec loop () = actor {
        let! message = mailbox.Receive()
        match message with
        
        | Constructor(nN, nR, mID, c ) ->
            
            numRequests <- nR
            numNodes <- nN
            nodeId <- mID
            cols <- c
            maximumNodes <- pown 4 cols
            parentRef <- mailbox.Sender() 
            for i in 0..cols-1 do
                routingTable.Add(List<int>())
                for j in 0..3 do
                    routingTable.[i].Add( -1 ) 

        | StartActorRequest(_) ->
            for i in 1..numRequests do
                Async.Sleep(1000) |> Async.RunSynchronously    //one request/second
                mailbox.Self <! Route( "Route",nodeId,rand.Next()%maximumNodes,-1)
        
        
        | InitialJoin  ( actorList, position ) ->
                 
            actorList.RemoveAt(position)
            
            updateState actorList
            
            let myIDbase4 = toBase4String nodeId cols 
            for i in 0..cols-1 do
                routingTable.[i].[int myIDbase4.[i] - 48] <- nodeId
                
            mailbox.Sender() <! JoiningFinished

        // Pastry Routing Algorithm

        | Route (msg, requestFrom, requestTo, hops) ->
            if msg = "Join" then
            
                let myIDbase4 = toBase4String nodeId cols
                let requestTobase4 = toBase4String requestTo cols
                let mutable samePrefix = commonPrefix myIDbase4 requestTobase4
                
                if hops = -1 && samePrefix > 0 then
                  for i in 0..samePrefix-1 do


                    let startID = requestTo.ToString()

                    let str = "akka://Pastry/user/"+startID
                    let aref = select str system
                    let routingtableclonei= new List<int>()
                    for i in routingTable.[i] do
                        routingtableclonei.Add(i)
                    aref <! AddRow(i, routingtableclonei) 

                let str = "akka://Pastry/user/"+ requestTo.ToString()
                let aref = select str system
                let routingtableclonesameprefix= new List<int>()
                for i in routingTable.[samePrefix] do
                    routingtableclonesameprefix.Add(i)
                aref <! AddRow(samePrefix, routingtableclonesameprefix)

            // /*
            //      * If id of helping node leafset can be used for next routing or not. 
            //      * Checking all entries in the smaller leaf table and routing the message to node with smallest differnce (proximity). 
            //      * Whether helping node id in range or not
            // */
                let inSmall = smallerLeaf.Count > 0 && requestTo >= smallerLeaf.Min() && requestTo <= nodeId
                let inLarge = largerLeaf.Count > 0 && requestTo <= largerLeaf.Max() && requestTo >= nodeId
                if inSmall || inLarge then //In larger leaf set
                    let mutable diff = maximumNodes + 10
                    let mutable nearest = -1
                    if requestTo < nodeId then 
                        for i in smallerLeaf do 
                            if abs (requestTo - i) < diff then 
                                nearest <- i
                                diff <- abs (requestTo - i)

                    else  //In larger leaf set
                        for i in largerLeaf do 
                            if abs (requestTo - i) < diff then
                                nearest <- i
                                diff <- abs (requestTo - i)
                            
                    if abs (requestTo - nodeId) > diff then  
                        let str = "akka://Pastry/user/" + nearest.ToString()
                        let aref = select str system
                        aref <! Route(msg, requestFrom, requestTo, hops + 1)
                        
                    else  
                        let mutable allLeaf = new List<int>()
                        allLeaf.Add(nodeId)
                        for i in smallerLeaf do
                            allLeaf.Add(i)
                        for i in largerLeaf do
                            allLeaf.Add(i)
                        let str = "akka://Pastry/user/" + requestTo.ToString()
                        let aref = select str system
                        aref <! AddLeaf(allLeaf)
                      
                else if smallerLeaf.Count < 4 && smallerLeaf.Count > 0 && requestTo < smallerLeaf.Min() then
                    let str = "akka://Pastry/user/" + smallerLeaf.Min().ToString()
                    let aref = select str system
                    aref <! Route(msg, requestFrom, requestTo, hops + 1)
                   
                else if largerLeaf.Count < 4 && largerLeaf.Count > 0 && requestTo > largerLeaf.Max() then
                    let str = "akka://Pastry/user/"+ largerLeaf.Max().ToString()
                    let aref = select str system
                    aref <! Route(msg, requestFrom, requestTo, hops + 1)
                

                else if smallerLeaf.Count = 0 && requestTo < nodeId || largerLeaf.Count = 0 && requestTo > nodeId then
                
                    let mutable allLeaf = new List<int>()
                    allLeaf.Add(nodeId)
                    for i in smallerLeaf do
                        allLeaf.Add(i)
                    for i in largerLeaf do
                        allLeaf.Add(i)

                    let str = "akka://Pastry/user/"+ requestTo.ToString()
                    let aref = select str system
                    aref <! AddLeaf(allLeaf)
                   
                else if routingTable.[samePrefix].[int requestTobase4.[samePrefix] - 48] <> -1 then 
               
                    let str = "akka://Pastry/user/"+ routingTable.[samePrefix].[int requestTobase4.[samePrefix] - 48 ].ToString()
                    let aref = select str system
                    aref <! Route(msg, requestFrom, requestTo, hops + 1)
                    
                    
                else if requestTo > nodeId then  
                
                    let str = "akka://Pastry/user/"+ largerLeaf.Max().ToString() 
                    let aref = select str system
                    aref <! Route(msg, requestFrom, requestTo, hops + 1)
                    parentRef <! NotInBoth
                 
                else if requestTo < nodeId then
                    let str = "akka://Pastry/user/"+ smallerLeaf.Min().ToString() 
                    let aref = select str system
                    aref <! Route(msg, requestFrom, requestTo, hops + 1)
                    parentRef <! NotInBoth
                                               
                
                else 
                    printfn "Not Possible"
             
                                      
                                      
            else if msg = "Route" then  //Message = Route, begin sending message
            // Pastry Routing Algorithm
            // If nodeId is equals to requested nodeId  then the routing stops
            // Else the pastry algortihm goes closer to the requested nodeId using routing table 
            //      and leafset. Go to NodeId which much closer to requested Id than current nodeId.
          
                if nodeId = requestTo then
                    parentRef <! AllRequestFinished(requestFrom, requestTo, hops + 1)
                    
                else 
                    let myIdBase4 = toBase4String nodeId cols
                    let toBase4 = toBase4String requestTo cols
                    let mutable samePrefix = commonPrefix  myIdBase4 toBase4 

                    if (smallerLeaf.Count > 0 && requestTo >= smallerLeaf.Min() && requestTo < nodeId) ||(largerLeaf.Count > 0 && requestTo <= largerLeaf.Max() && requestTo > nodeId) then //In larger leaf set or smaller leaf set
                        let mutable diff = maximumNodes + 10
                        let mutable nearest = -1
                        if (requestTo < nodeId) then //In smaller leaf set
                            for i in smallerLeaf do 
                                if (abs(requestTo - i) < diff) then 
                                    nearest <- i
                                    diff <- abs(requestTo - i)
                                
                            
                        
                        else  //In larger leaf set
                            for i in largerLeaf do
                                if (abs(requestTo - i) < diff) then
                                    nearest <- i
                                    diff <- abs(requestTo - i)
                                
                            
                        
                         // In leaf but not near my id
                        if (abs(requestTo - nodeId) > diff) then
                            let str = "akka://Pastry/user/"+ nearest.ToString()
                            let aref = select str system
                            
                            aref <! Route(msg, requestFrom, requestTo, hops + 1)
                            
                        else  // Nearest
                            parentRef <! AllRequestFinished(requestFrom, requestTo, hops + 1) 
                            

                     
                    else if (smallerLeaf.Count < 4 && smallerLeaf.Count > 0 && requestTo < smallerLeaf.Min())  then
                        let str = "akka://Pastry/user/"+ smallerLeaf.Min().ToString()
                        let aref = select str system  
                        aref <! Route(msg, requestFrom, requestTo, hops + 1)
                    
                    else if (largerLeaf.Count < 4 && largerLeaf.Count > 0 && requestTo > largerLeaf.Max()) then
                        let str = "akka://Pastry/user/"+ largerLeaf.Min().ToString() 
                        let aref = select str system 
                        aref <! Route(msg, requestFrom, requestTo, hops + 1)
                    
                    else if ((smallerLeaf.Count = 0 && requestTo < nodeId) || (largerLeaf.Count = 0 && requestTo > nodeId)) then
                        parentRef <! AllRequestFinished(requestFrom, requestTo, hops + 1)
                    
                    else if (routingTable.[samePrefix].[int toBase4.[samePrefix] - 48] <> -1) then  
                        let str = "akka://Pastry/user/" + routingTable.[samePrefix].[int toBase4.[samePrefix] - 48].ToString()
                        let aref = select str system
                        aref <! Route(msg, requestFrom, requestTo, hops + 1)
                    
                    else if (requestTo > nodeId) then//Not in both
                        let str = "akka://Pastry/user/"+ largerLeaf.Min().ToString()
                        let aref = select str system
                        aref <! Route(msg, requestFrom, requestTo, hops + 1)
                        parentRef <! RouteNotInBoth
                    
                    else if (requestTo < nodeId) then
                        let str = "akka://Pastry/user/"+ smallerLeaf.Min().ToString()
                        let aref = select str system
                        aref <! Route(msg, requestFrom, requestTo, hops + 1)
                        parentRef <! RouteNotInBoth
                    
                    else 
                        printfn "Not Possible"
                    
                
            
            
        | AddRow (rowNum, newRow) ->
            for i in 0..3 do
                if routingTable.[rowNum].[i] = -1 then
                    routingTable.[rowNum].[i] <- newRow.[i]

        | AddLeaf(allLeaf) ->
                updateState(allLeaf)
                for i in smallerLeaf do
                    numOfBack <- numOfBack + 1
                    let str = "akka://Pastry/user/"+ i.ToString()
                    let aref = select str system
                    aref <! Update(nodeId)
                
                for i in largerLeaf do 
                    numOfBack <- numOfBack + 1
                    let str = "akka://Pastry/user/"+ i.ToString()
                    let aref = select str system
                    aref <! Update(nodeId)
                
                for i in 0..cols-1 do
                    let mutable j = 0
                    for j in 0..3 do
                        if (routingTable.[i].[j] <> -1) then
                            numOfBack <- numOfBack + 1
                            let str = "akka://Pastry/user/"+ routingTable.[i].[j].ToString()
                            let aref = select str system
                            aref <! AddRow(i, routingTable.[i])
                
                let myIDbase4 = toBase4String nodeId cols
                for i in 0..cols-1 do
                    routingTable.[i].[int myIDbase4.[i] - 48] <- nodeId
                

        | Update(newNodeID) ->
                updateNode(newNodeID)
                mailbox.Sender() <! Acknowledgement

        | Acknowledgement ->
            numOfBack <- numOfBack - 1
            if numOfBack = 0 then
                parentRef <! JoiningFinished

        | StopActor ->
           
            mailbox.Context.Stop(mailbox.Self)             

        return! loop()
    }
    loop()


let masterActor (mailbox: Actor<_>) =
    
    let mutable randomList = new List<int>()
    let mutable actorList = new List<int>()
    let mutable actorListSize = 0
    let mutable numHops = 0
    let mutable numJoined = 0
    let mutable numMissed = 0
    let mutable numRouteMissed = 0
    let mutable numRouted = 0
    let mutable numNodes = 0
    let mutable numRequests = 0
    let mutable cols =  0
    let mutable nodeidSpace = 0
    let mutable set = Set.empty 
    let maximumNodes = 65535
    let mutable flag = true
    let rec masterloop() = actor{
        let! message = mailbox.Receive()

        match message with
        | Initializer (nN, nR)->
            numNodes <- nN 
            numRequests <- nR
            actorListSize <- if (numNodes <= maximumNodes) then numNodes else maximumNodes
            let cc=Math.Ceiling(Math.Log( float actorListSize , 4.)) 
            cols <- int cc 
            nodeidSpace <- Convert.ToInt32(pown 4 cols)

            while (randomList.Count < nodeidSpace) do
                let i = rand.Next()%nodeidSpace
                if ( not (set.Contains(i)) ) then
                    randomList.Add(i)
                    set <- set.Add(i)
                    
            
            for i in 0..actorListSize-1 do
                actorList.Add(randomList.[i])
            
            for i in 0..numNodes-1 do
      
                let str = actorList.[i].ToString()
                let childActor = spawn system str PastryNode
                let str = "akka://Pastry/user/"+randomList.[i].ToString()
                let aref = select str system
                aref <! Constructor(numNodes, numRequests, randomList.[i], cols)
               

        | StartJoining ->
            printfn "Joining"
            for i in 0..actorListSize-1 do
                let str = "akka://Pastry/user/"+randomList.[i].ToString()
                let aref = select str system
                let groupOneClone= new List<int> ()
                for i in actorList do
                    groupOneClone.Add(i)
                aref <! InitialJoin ( groupOneClone , i )
           
        | JoiningFinished ->
            
            numJoined <- numJoined + 1
            
            if numJoined = actorListSize then
                printfn "Initial Join Finished!"

                if numJoined >= numNodes then
                  mailbox.Self <! AllStartRequest
                else 
                  mailbox.Self <! JoinActor

            if numJoined > actorListSize then
                if numJoined = numNodes then
                    //printfn "Routing Not In Both Count: %d "  numMissed
                    // printfn "Ratio: %d %" + (100 * numMissed.toDouble / numNodes.toDouble)
                    mailbox.Self <! AllStartRequest
                else
                    mailbox.Self <! JoinActor
            
        | JoinActor ->
            let startID = randomList.[rand.Next()%numJoined]
           
            let str = "akka://Pastry/user/"+startID.ToString()
            let aref = select str system
            aref <! Route("Join", startID , randomList.[numJoined], -1)
                      
            
        | AllStartRequest ->
            printfn "Joined"
            printfn "Routing"
            let str = "akka://Pastry/user/*"
            let aref = select str system
            aref <! StartActorRequest

        | NotInBoth ->
            numMissed <- numMissed + 1

        | AllRequestFinished (requestFrom, requestTo, hops) ->
            numRouted <- numRouted + 1
            numHops <- numHops + hops
            for i in 1..10 do
                if numRouted = numNodes * numRequests * i / 10 then
                    for j in 1..i do
                        printf "."
                    printf "."  

            if (numRouted >= numNodes * numRequests) then
                printfn "\n"
                printfn "Total Routes -> %d Total Hops -> %d" numRouted numHops
                let x = numNodes
                
                let temp = float (numHops) /  float (numRouted)
                printfn "Average Hops Per Route -> %f" temp
                
                while flag do
                    ignore()
                mailbox.Context.System.Terminate() |> ignore

        | RouteNotInBoth ->
            numRouteMissed <- numRouteMissed + 1
        return! masterloop()
    }
    masterloop()

let pastryProtocol numNodes numRequests =
      
    let master = spawn system "master" masterActor
    master <! Initializer(numNodes, numRequests)
    master <! StartJoining
    

                                             



let mainModule = 
    let args : string array = fsi.CommandLineArgs |> Array.tail
    let mutable numNodes= args.[0] |> int
    let mutable numRequests = args.[1] |> int
    

    
    if args.Length <> 2 then 
        printfn "************************Invalid Input**********************"
      
    else
       
        printfn "Num Nodes : %d Num Requests : %d" numNodes numRequests
        pastryProtocol numNodes numRequests
        
        system.WhenTerminated.Wait()
