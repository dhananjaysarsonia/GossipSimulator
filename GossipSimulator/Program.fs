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
    |Initialize 
    |Dead
    |AllDead //called when all the neighbours are dead and I have nothing else to do
    |HeardYou
    |TempDead
    |KillMe

//parent actor
let supervisorActor (mailBox : Actor<_>) =
    let rec loop() = actor{
        
        
        return! loop()
    }
    
    loop()
let system = System.create "system" (Configuration.defaultConfig())
let supervisor = spawn system "supervisor" supervisorActor
//create actors here
//first is the Gossip worker, I will call it False Media
let gossipActor(mailbox : Actor<_>) =
    let mutable count = 0
    let rec loop() = actor{
        let! message = mailbox.Receive()
        match message with
        | Gossip ->
            count <- count + 1
            if count = 10 then
                supervisor <! KillMe //please
              
        //if count < 10 then
            
        
        
        
        return! loop()
    }
    loop()






//chose actors







//choose topology and set neigbour to the actor






//start the process






//terminate and calculate
