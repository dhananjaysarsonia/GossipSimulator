// Learn more about F# at http://fsharp.org

open System
open System.Runtime.CompilerServices
open System.Threading.Tasks
open Akka
open Akka.Actor
open Akka.Dispatch.SysMsg
open Akka.FSharp
open Akka.Actor
open System.Diagnostics

//let's make a gossip actor first and then create a topology
//input will be num_nodes, topology, algorithm


let nNodes = 100
let topology = "line"
let algorithm = "gossip"

//what can be the message types
//there can be a gossip type with just state called gossp
//there can be a pushsum type which I need to implement
//
//actor can contain a list of references when it is created
//for each actor I can give it it;s neighbours. so next item will be neighbour list as message

//create message types here
type Message =
    |Gossip
    |PushSum
    |Initialize of list<ActorRefs>
    |Dead
    |AllDead //called when all the neighbours are dead and I have nothing else to do
    |HeardYou
    |TempDead
    |KillMe


let gossipActor(mailbox : Actor<_>) =
    let mutable count = 0
    let mutable neighbour : list<ActorRefs> = []
    let rec loop() = actor{
        let! message = mailbox.Receive()
        match message with
        | Gossip ->
            count <- count + 1
            if count = 10 then
                mailbox.Context.Parent <! KillMe //please
                
        | Initialize(l) ->
            neighbour <- l
            
        return! loop()
        
      
    }
    loop()


//parent actor
let supervisorActor (mailBox : Actor<_>) =
    let rec loop() = actor{
        let actors = [for i in 0 .. nNodes ->
            let name = sprintf "%i" i
            spawn mailBox name gossipActor ]
 
        return! loop()
    }  
    loop()
let system = System.create "system" (Configuration.defaultConfig())
let supervisor = spawn system "supervisor" supervisorActor
//create actors here
//first is the Gossip worker, I will call it False Media lol




//toDo: push sum:
//s,w
//s = node number
//w = 1
//2,1 start-> s and w half -> send both
//recive-> add in s + s`, then if I have added s/w is not changing 10^10 variation 3 times




//chose actors







//choose topology and set neighbour to the actor

let random = new System.Random()
//choose topology and set neigbour to the actor

//for 2D and imp2D
//N = sqrt(float total_nodes)

let twoD (node : int) (total_nodes : int) (N : int) =
    let neighbour_list =
        [|
            //left column
            if(node % N = 1 ) then
                if(node = 1) then 
                    2; 1 + N
                else if (node = total_nodes - N + 1) then
                    node - N ; node+1
                else
                    node+1; node - N ; node + N
                
            //right column
            else if(node % N = 0) then
                if(node = N) then
                    node-1; node + N
                else if (node = total_nodes) then
                    node-1; node - N
                else
                    node-1; node - N; node + N
                
            //top row
            else if (node < N && node > 1) then
                node - 1; node + 1; node + N
                
            //bottom row
            else if ((node < total_nodes) && (node > total_nodes - N)) then
                node - 1; node + 1; node - N
            
            //all four neighbours
            else
                node + 1; node - 1; node + N ; node - N 
                    
        |]
        
    neighbour_list
        
let full node total_nodes =
    let neighbour_list =
        [| for x in 1 .. total_nodes do
            if x <> node then x|]
    neighbour_list   
    


let line node total_nodes =
    let neighbour_list =
        [| if node = 1 then 2
           else if
               node = total_nodes then total_nodes - 1
           else
               node - 1 ; node + 1|]
    neighbour_list    
    
    
//let imp2D node total_nodes =
//    let neighbour_list =
//        (Array.append[|
//          let primary_list =  2D(node, total_nodes)
//          let random-node = [|random.Next(1,total_nodes)|]
//        |])
    
//let imp2D node total_nodes N =
//    let neighbour_list =
//        (Array.append[|
//          let primary_list =  twoD node total_nodes N
//          
////          let random_node =
////              [|random.Next(1,total_nodes)|]
//          
//          let mutable isExist = true
//          let mutable index = 0
//          while isExist do
//              isExist <- Array.exists () neighbour_list
//              
//              
//              
//  
//    
////          while Array.exists ((=) random_node) primary_list  do
////            random_node = [|random.Next(1,total_nodes)|]        
//        |])
        
        
    //neighbour_list
        


//first the 




//start the process






//terminate and calculate
