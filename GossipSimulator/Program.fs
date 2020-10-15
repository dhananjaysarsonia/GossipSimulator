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
open System.Threading;

//let's make a gossip actor first and then create a topology
//input will be num_nodes, topology, algorithm


//let nNodes = 100
let total_nodes = 500
let topology = "line"
//let algorithm = "gossip"
let algorithm = "pushsum"  
let random = new System.Random()
let deadOrStuck = 1
type sum_weight = {
  value1:int;
  value2:int
}
let mutable pushsum_array  = Array.init<sum_weight> total_nodes (fun x -> {value1 = 0; value2 = 0})
let actors : IActorRef array = Array.zeroCreate (total_nodes + 1)
let deadActors : int array = Array.zeroCreate(total_nodes + 1)
let stuckActors : int array = Array.zeroCreate(total_nodes + 1)


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
    |PushSum of decimal*decimal
    |Initialize of int[]
    |InitializePushSum of decimal*decimal
    |Dead
    |AllDead //called when all the neighbours are dead and I have nothing else to do
    |HeardYou
    |TempDead
    |Start
    |PSCantProcess of decimal*decimal
    |KillMe
    |Stuck



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
                start <- true
            if count = 10 then
                mailbox.Context.Parent <! KillMe //please
           // else
                
        | NeighbourGossip(actors) ->
            if count < 10 then
                
                let selected = random.Next(0, neighbour.Length)
                //printf "selected next: %A \n" actors.[neighbour.[selected]].Path.Name
                actors.[neighbour.[selected]] <! Gossip(actors)
                mailbox.Self <! NeighbourGossip(actors)
                Thread.Sleep(10)
           //else
                //printf "finally not selecting \n"
            //Async.Sleep 100 |> ignore
        | Initialize(l) ->
            neighbour <- l     
        return! loop()
    }
    loop()

let pushsumActor(mailbox : Actor<_>)=
    //let mutable node = random.Next(1,total_nodes)
    let mutable sum : decimal = 0m
    let mutable weight : decimal = 0m
    let mutable neighbour : int[] = [||]
    let mutable index = 0
    let mutable running = true
    let mutable prev : decimal = 0m
    let mutable count = 0
    let mutable ratio : decimal = 0m
    let rec loop() = actor{
        let! message = mailbox.Receive()
        //Thread.Sleep(10) |> ignore
        //Thread.Sleep(10)
        match message with
        |InitializePushSum(s,w) ->
            sum <- s
            weight <- w
            
        | PushSum(s,w) ->

            if running then
                sum <- sum + s
                weight <- weight + w
                //printf "still trying"
                sum <- sum / 2m
                weight <- weight / 2m
                
                let mutable iHaveFriends = false
                for i in 0 .. (neighbour.Length - 1) do
                    if deadActors.[neighbour.[i]] = 0 then
                        iHaveFriends <- true
                        
                let mutable selected = random.Next(0, neighbour.Length)
                let mutable notbreak = true
                while iHaveFriends &&  notbreak do
                    selected <- random.Next(0, neighbour.Length)
                    notbreak <- deadActors.[neighbour.[selected]] = deadOrStuck
                    
               // if iHaveFriends then
                   // printf "sending %s -> %s with s %f and w %f \n" mailbox.Context.Self.Path.Name  actors.[neighbour.[selected]].Path.Name sum weight
                actors.[neighbour.[selected]] <! PushSum(sum,weight)
                ratio <- sum / weight
               // printf "the checked ratio %f \n" (Math.Abs(prev - ratio))
                if ( Math.Abs(prev - ratio)) < 0.0000000001m then
                   count <- count + 1 
                else
                    count <- 0
                    
                prev <- ratio
                if count = 3 then
                    running <- false
                   //
                    printf "the final ratio is %A \n" ratio
                    mailbox.Context.Parent <! KillMe
//                else
//                    //printf "here"
//                    mailbox.Context.Parent <! Stuck
                     // please
                    
            else
               // let sender = mailbox.Sender ()
                let mutable selected = random.Next(0, neighbour.Length)
                actors.[neighbour.[selected]] <! PushSum(s,w)
                //Thread.Sleep(100)
               //printf "dead, send it to someone else \n"
              //  sender <! PSCantProcess(sum,weight)
            //Thread.Sleep(10) |> ignore
        | Initialize(l) ->
            neighbour <- l
          //  printf "%A has neighbours %A \n" mailbox.Context.Self.Path.Name neighbour 
            
            
        | PSCantProcess(s,w) ->
            
            let mutable iHaveFriends = false
            printf "sending to someone else \n"
            for i in 0 .. (neighbour.Length - 1) do
                if deadActors.[neighbour.[i]] <> deadOrStuck then
                    iHaveFriends <- true
                    
            let mutable selected = -1
            let mutable notbreak = true
            //printf "neighbour length %d" neighbour.Length
            
//            for i in 0 .. (neighbour.Length - 1) do
//              if deadActors.[neighbour.[i]] <> deadOrStuck then
//                  selected <- i
            while iHaveFriends &&  notbreak do
                selected <- random.Next(0, neighbour.Length)
                notbreak <- deadActors.[neighbour.[selected]] = deadOrStuck

              //  if notbreak then printf "another dead \n"
            
            //printf "selected %d " selected   
            if iHaveFriends then
                 actors.[neighbour.[selected]] <! PushSum(s,w)
            else
                 //Thread.Sleep(10)
                // printf "cant handle"
                 mailbox.Context.Parent <! Stuck
            
        
        return! loop()    
    }
    loop()
//let mutable actors : list<IActorRef> = []


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
    let mutable stuck = 0
    let rec loop() = actor{
       
        let! message = mailBox.Receive()
        match message with
        | Start ->
            
            match algorithm with
            | "gossip" ->
                for i in 1 .. (total_nodes) do
                   // printf "%d" i
                    let name = sprintf "%d" i
                   // printf "%d" i
                    actors.[i] <- spawn mailBox name gossipActor
                    let connections = line i total_nodes 
                    actors.[i] <! Initialize connections 
                    
                actors.[50] <! Gossip(actors)
                
            | "pushsum" ->
                for i in 1 .. total_nodes do
                    let name = sprintf "%d" i
                    actors.[i] <- spawn mailBox name pushsumActor
                    let connections = line i total_nodes 
                    actors.[i] <! Initialize connections
                    actors.[i] <! InitializePushSum(decimal <| i,decimal <| 1)
                
                actors.[50] <! PushSum(0m,0m) 
            
            
            
    
        | KillMe ->
            count <- count + 1
            let child = mailBox.Sender ()
            printf "%A" child.Path.Name
            printf "%d \n" count
            deadActors.[int <| child.Path.Name] <- deadOrStuck
            //child <! PoisonPill.Instance
            //printf "dead child name: %d \n" (int <| child.Path.Name)
            
            
            
        | Stuck ->
            printf "here %d" stuck
            let child = mailBox.Sender ()
            let index = int <| child.Path.Name
            if stuckActors.[index] = 0 then
                stuck <- stuck + 1
                stuckActors.[index] <- deadOrStuck
                
        
            
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
