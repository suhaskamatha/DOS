#r "nuget: Akka" 
#r "nuget: Akka.FSharp" 
#r "nuget: Akka.Remote" 
#r "nuget: Akka.TestKit" 
//#r "nuget: Akkling"
//#r "nuget: Wire"
open System
open Akka.Actor
open Akka.Actor
open Akka.Configuration
open Akka.Dispatch.SysMsg
open Akka.FSharp
open System.Threading

// number of user
let mutable N = 1000

let hashtaglist = ["RandomMax"
;"Randomness"
;"UF"
;"GoGators"
;"football"
;"Netflix"
;"StrangerThings"
;"BreakingBad"
;"MoneyHeist"]

let tweetmessagelist = ["Anyone ready for some  @Gators @UF ? Cross those fingers as #GoGators leave today for “Bubbleville” #football"
;"Another win for @Gators last Saturday #football #UF"
;"Random 238"
;"Another great day in #TheSwamp!  #GoGators #UKvsUF #BeatUK"
;"Random 23"
;"It's #CyberMonday Get your holiday shopping done today! #GoGators"
;"Love watching  AlbertGator & AlbertaGator get fired up for  @Gators! #GoGators #football"
;"Random Tweet1 #RandomMax @Randomness"
;"Random Tweet2 #RandomMax"
;"Random Tweet3 #RandomMax @Randomness"
;"Random Tweet4 #RandomMax @Randomness"
;"Random 1286"
;"Random 12"
;"Random 23"
;"I will never NOT be in the mood to watch @BreakingBad #Netflix @Netflix"
;"Bank robbery like #MoneyHeist executed by thieves in Brazil. #Netflix"
;"@Netflix announces the Korean adaptation of its Spanish original series #MoneyHeist"
;"Random tweets 121"
;"Random tweets 234"]

let configuration = 
    ConfigurationFactory.ParseString(
        @"akka {
            log-dead-letters = 0
            log-dead-letters-during-shutdown = off
            log-config-on-start : on
            stdout-loglevel : DEBUG
            loglevel : ERROR
            actor {
                provider = ""Akka.Remote.RemoteActorRefProvider, Akka.Remote""
                debug : {
                    receive : on
                    autoreceive : on
                    lifecycle : on
                    event-stream : on
                    unhandled : on
                }
            }
            remote {
                helios.tcp {
                    port = "+string (System.Random( ).Next(10000,20000))+" 
                    hostname = localhost
                }
            }
        }")
let system = ActorSystem.Create("RemoteFSharp", configuration)

let echoServer = system.ActorSelection(
                            "akka.tcp://RemoteFSharp@localhost:8777/user/EchoServer")


let mutable prev_query = ""
let mutable auto = false
let actor_user_connect (mailbox: Actor<_>) = 
    let rec loop () = actor {
        let! message = mailbox.Receive ()
        match message with
        | username,password ->
            while not auto do
                Thread.Sleep(500)
            let task = echoServer <?  "querying," + username + "," + password + ", , , , , "
            let response = Async.RunSynchronously (task, 1000) |> string
            if not (response = prev_query) then
                prev_query <- response
                printfn "---Update---%s" response
                printfn "%s" ""
            Thread.Sleep(1000)
            mailbox.Self <? (username, password) |> ignore
            return! loop() 
    }
    loop ()
    
let client_user_connect = spawn system "client_user_connect" actor_user_connect

let actor_user (mailbox: Actor<_>) = 
    let rec loop () = actor {        
//        let! message = mailbox.Receive ()
        let cmd = Console.ReadLine()
        let result = cmd.Split ','
        let opt = result.[0]
        if opt="login" then
            let username=result.[1]
            let password=result.[2]
            auto <- true
            client_user_connect <? (username, password) |> ignore
            return! loop() 
        else if opt="logout" then
            auto <- false
            return! loop() 
        let task = echoServer <? cmd
        let response = Async.RunSynchronously (task, 1000)
        printfn "---done---%s" (string(response))
        printfn "%s" ""
//        mailbox.Self <? "go"
        return! loop()     
    }
    loop ()

let client_user = spawn system "client_user" actor_user



printfn "*********************************************************** \n " 
printfn "*********************************************************** \n \n " 
printfn "*********************************************************** \n " 
printfn "*********************************************************** \n \n " 

printfn "Enter test options: (registration, tweet, subscribe, querying, login, logout) line by line"
printfn "Example inputs: "
printfn "registration,Tanvi,password, , , , ," 
printfn "registration,Suhas,password, , , , ," 
printfn "tweet,Tanvi,password, ,tweet1 @Suhas #LifeAtUF, , ,"
printfn "tweet,Suhas,password, ,tweet2 @Tanvi #GoGators , , ,"
printfn "subscribe,Suhas,password,Tanvi, , , ,"
printfn "querying,Suhas,password, , , , ,"
printfn "#, , , , ,#GoGators, ,"
printfn "@, , , , , ,@Suhas,"
printfn "retweet,Tanvi,password, ,tweet2 @Tanvi #GoGators , , ,"
printfn "connect,user1,pass, , , , ,"
printfn "disconnect,user1,123456, , , , ,"

printfn "*********************************************************** \n " 
client_user <? "go" |>ignore
Thread.Sleep(1000000)
system.Terminate() |> ignore

0 // return an integer exit code

(*

A full Example:
registration,user1,123456, , , , ,
registration,user2,123456, , , , ,
tweet,user1,123456, ,tweet1 @Biden , , ,
tweet,user2,123456, ,tweet2 #Trump , , ,
tweet,user2,123456, ,tweet3 @Biden #Trump , , ,
subscribe, ,user1,123456,user2, , , ,
querying, ,user1,123456, , , , ,
querying, ,user2,123456, , , , ,
#, , , , , ,#Trump, ,
@, , , , , , ,@Biden,

Retweet Example:
retweet, ,user2,123456, ,tweet3 @Biden #Trump , , , 
querying, ,user2,123456, , , , ,

Connect Example:
connect, ,user1,123456, , , , ,
send, ,user2,123456, ,tweet4, , ,
send, ,user2,123456, ,tweet5, , ,
disconnect, ,user1,123456, , , , ,

*)