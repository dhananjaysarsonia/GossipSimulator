﻿// Learn more about F# at http://fsharp.org
open System
open System.Runtime.CompilerServices
open System.Threading.Tasks
open Akka
open Akka.Actor
open Akka.Dispatch.SysMsg
open Akka.FSharp
open Akka.Actor
open System.Diagnostics
open Akka.Util

//let's make a gossip actor first and then create a topology
//input will be num_nodes, topology, algorithm


//let nNodes = 100
let total_nodes = 1000
let topology = "line"
let algorithm = "gossip"
let random = new System.Random()

type sum_weight = {
  value1:int;
  value2:int
}
let mutable pushsum_array  = Array.init<sum_weight> total_nodes (fun x -> {value1 = 0; value2 = 0})

//what can be the message types
//there can be a gossip type with just state called gossp
//there can be a pushsum type which I need to implement
//
//actor can contain a list of references when it is created
//for each actor I can give it it;s neighbours. so next item will be neighbour list as message

//create message types here
type Message =
    |Gossip of IActorRef[]
    |NeighbourGossip of IActorRef[]
    |PushSum 
    |Initialize of int[]
    |Dead
    |AllDead //called when all the neighbours are dead and I have nothing else to do
    |HeardYou
    |TempDead
    |Start
    |KillMe


let gossipActor(mailbox : Actor<_>) =
    let mutable count = 0
    let mutable start = false
    let mutable neighbour : int[] = [||]
    //let mutable neighbour : int[] = [||]
    let rec loop() = actor{
        let! message = mailbox.Receive()
        match message with
        | Gossip(actors) ->
           
            count <- count + 1
            
//            let selected = random.Next(0, neighbour.Length)
//            actors.[neighbour.[selected]] <! Gossip(actors)
            if start = false then 
                mailbox.Self <! NeighbourGossip(actors)
            
            if start = false then
                start = true
            if count = 10 then
                mailbox.Context.Parent <! KillMe //please
           // else
                
        | NeighbourGossip(actors) ->
            if count < 10 then
                let selected = random.Next(0, neighbour.Length)
              //  printf "selected next: %A \n" actors.[neighbour.[selected]].Path.Name
                actors.[neighbour.[selected]] <! Gossip(actors)
                mailbox.Self <! NeighbourGossip(actors)
            //Async.Sleep 100 |> ignore
        | Initialize(l) ->
            neighbour <- l     
        return! loop()
    }
    loop()

let pushsumActor(mailbox : Actor<_>)=
    let mutable node = random.Next(1,total_nodes)
    let mutable sum = node
    let mutable weight = 1
    let mutable ratio_list = []
    let rec loop() = actor{
        let! message = mailbox.Receive()
        match message with
        | PushSum ->
            let mutable s = pushsum_array.[node].value1 + sum
            let mutable w = pushsum_array.[node].value2+ weight
            pushsum_array.[node] <- {value1 = s ; value2 = w}
            ratio_list <- [float(pushsum_array.[node].value1 / pushsum_array.[node].value2)] |> List.append ratio_list
            if(List.length[ratio_list] > 2) then
                if(ratio_list.Item(List.length[ratio_list]) - ratio_list.Item(List.length[ratio_list] - 1) < 0.0000000001 &&
                   ratio_list.Item(List.length[ratio_list] - 1) - ratio_list.Item(List.length[ratio_list] - 2) < 0.0000000001) then
                     mailbox.Context.System.Terminate () |> ignore
            else
                node <- random.Next(1,total_nodes)
                sum <- sum/2
                weight <- weight/2
        return! loop()    
    }
    loop()
//let mutable actors : list<IActorRef> = []
let actors : IActorRef array = Array.zeroCreate (total_nodes + 1)
//parent actor

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

let generate_random (primary_list: int []) (total_nodes: int) (node:int): int [] =
    let mutable temp = random.Next(1, total_nodes)
    while(primary_list |> Array.exists(fun elem -> elem = temp && elem = node )) do
        temp <- random.Next(1, total_nodes)
    [|temp|]

let imp2D node total_nodes N =
    let primary_list =  twoD node total_nodes N
    let temp = generate_random primary_list total_nodes node
    Array.append primary_list temp





let supervisorActor (mailBox : Actor<_>) =
    let mutable count = 0
    let rec loop() = actor{
       
        let! message = mailBox.Receive()
        match message with
        | Start ->
            for i in 1 .. (total_nodes) do
               // printf "%d" i
                let name = sprintf "%d" i
               // printf "%d" i
                actors.[i] <- spawn mailBox name gossipActor
                let connections = full i total_nodes 
                actors.[i] <! Initialize connections 
                
            actors.[1] <! Gossip(actors)
    
        | KillMe ->
            count <- count + 1
            let child = mailBox.Sender ()
            printf "%A" child.Path.Name
            printf "%d \n" count
        return! loop()
  
    }
    loop()
let system = System.create "system" (Configuration.defaultConfig())
let supervisor = spawn system "supervisor" supervisorActor
supervisor <! Start
System.Console.ReadLine() |> ignore
//    let neighbour_list =
//        (Array.append[|
//          let primary_list =  twoD node total_nodes N
//          generate_random primary_list total_nodes
//        |])   
//    neighbour_list


//to check topology implementations
//let temp2d = imp2D 5 16 4
//for i in temp2d do
//    printfn "%d" i
//

//first the 




//start the process






//terminate and calculate
