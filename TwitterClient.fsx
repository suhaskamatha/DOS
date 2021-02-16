#r "nuget: Akka" 
#r "nuget: Akka.FSharp" 
#r "nuget: Akka.Remote" 
#r "nuget: Akka.TestKit" 
//#r "nuget: Akkling"
//#r "nuget: Wire"
open System
open System.Threading
open System.Collections.Generic
open Akka.Actor
open Akka.Configuration
open Akka.FSharp
open Akka.TestKit


// let config = 
//     ConfigurationFactory.ParseString(
//         @"akka {
//             actor {
//                 provider = ""Akka.Remote.RemoteActorRefProvider, Akka.Remote""
//                 deployment {
//                     /remoteecho {
//                         remote = ""akka.tcp://RemoteFSharp@127.0.0.1:9001""
//                     }
//                 }
//             }
//             remote {
//                 helios.tcp {
//                     port = 0
//                     hostname = ""127.0.0.1""
//                 }
//             }
//         }")\


// number of user

  
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
                    port = 8123
                    hostname = localhost
                }
            }
        }")
        
let system = ActorSystem.Create("RemoteFSharp", configuration)

let args : string array = fsi.CommandLineArgs |> Array.tail
let N= args.[0] |> int
let M = N
let mutable i = 0
let mutable ii = 0
let obj = Object()
let addIIByOne() =
    Monitor.Enter obj
    ii<- ii+1
    Monitor.Exit obj
    

let echoServer = system.ActorSelection(
                            "akka.tcp://RemoteFSharp@localhost:8777/user/EchoServer")

let rand = System.Random(1)
let actorForRegister (mailbox: Actor<_>) = 
    let rec loop () = actor {
        let! message = mailbox.Receive()
        let idx = message
        let mutable opt = "registration"           
        // let mutable POST = " "
        let mutable username = "user"+(string idx)
        let mutable password = "password" + (string idx)
        let mutable target_username = " "
        let mutable queryhashtag = " "
        let mutable at = " "
        let mutable tweet_content = " "
        let mutable register = " "
        let cmd = opt+","+username+","+password+","+target_username+","+tweet_content+","+queryhashtag+","+at+","+register
        let task = echoServer <? cmd
        let response = Async.RunSynchronously (task, 1000)
        printfn "***command*** : %s" cmd
        printfn "%s" (string(response))
        printfn "%s" ""
        addIIByOne()
        return! loop()
    }
    loop ()
    
let actorForSimulation (mailbox: Actor<_>) = 
    let rec loop () = actor {        
        let! message = mailbox.Receive ()
        let sender = mailbox.Sender()
        let idx = message
        match box message with
        | :? string   ->
            let mutable rand_num = Random( ).Next() % 7
            let mutable opt = "registration"           
            let mutable username = "user"+(string idx)
            let mutable password = "password" + (string idx)
            let mutable target_username = "user"+rand.Next(N) .ToString()
            let mutable queryhashtag = "#hashTag"+rand.Next(N) .ToString()
            let mutable at = "@user"+rand.Next(N) .ToString()
            let mutable tweet_content = "tweet"+rand.Next(N).ToString()+"... " + queryhashtag + "..." + at + " " 
            let mutable register = "register"
            if rand_num=0 then  opt <-"registration"
            if rand_num=1 then  opt <-"tweet"
            if rand_num=2 then  opt <-"subscribe"
            if rand_num=3 then  opt <-"retweet"
            if rand_num=4 then  opt <-"querying"
            if rand_num=5 then  opt <-"#"
            if rand_num=6 then  opt <-"@" 
            // msg can be anything like "start"
            let cmd = opt+","+username+","+password+","+target_username+","+tweet_content+","+queryhashtag+","+at+","+register
            let task = echoServer <? cmd
            let response = Async.RunSynchronously (task, 3000)
            printfn "***command*** : %s" cmd
            printfn "%s" (string(response))
            printfn "%s" ""
            addIIByOne()
        | _ ->  failwith "unknown message"
        return! loop()     
    }
    loop ()

let client_user_register = spawn system "client_user_register" actorForRegister    
let client_simulator = spawn system "client_simulator" actorForSimulation


printfn "*********************************************************** \n " 
printfn "*********************************************************** \n " 
printfn "Registering Accounts   \n\n" 

printfn "*********************************************************** \n " 
printfn "*********************************************************** \n " 

let timer = Diagnostics.Stopwatch.StartNew()
timer.Start()
i<-0
ii<-0
while i<N do
    client_user_register <! string i |>ignore
    i<-i+1
while ii<N-1 do
    Thread.Sleep(50)
let time_register = timer.Elapsed.TotalMilliseconds
timer.Stop()
//    Thread.Sleep(5000)




printfn "*********************************************************** \n " 
printfn "*********************************************************** \n \n " 

printfn "5 TWEETS FOR EACH USER\n\n" 

printfn "*********************************************************** \n " 
printfn "*********************************************************** \n " 
let timersend = Diagnostics.Stopwatch.StartNew()
timersend.Start()
for i in 0..N-1 do
    for j in 0..5 do
        let cmd = "tweet,user"+(string i)+",password"+(string i)+", , Tweet : user"+(string i)+" ... tweet number: "+(string j)+"  @user"+(string (rand.Next(N)))+" #hashTag"+(string (rand.Next(N)))+" , , , "
//            let cmd = "send, ,user"+(string i)+",password"+(string i)+", ,@user"+(string (rand.Next(N)))+" #topic"+(string (rand.Next(N)))+" , , , "
//            let cmd = "send, ,user"+(string i)+",password"+(string i)+", ,t, , , "
        let task = echoServer <? cmd
        let response = Async.RunSynchronously (task, 3000)
        printfn "***command*** : %s" cmd
        printfn "%s" (string(response))
        printfn "%s" ""
timersend.Stop()
let time_send = timersend.Elapsed.TotalMilliseconds





let mutable step = 1
let timerzipf = System.Diagnostics.Stopwatch.StartNew()
timerzipf.Start()
printfn "*********************************************************** \n " 
printfn "*********************************************************** \n " 
printfn "ZIPF SUBSCRIPTIONS"  
for i in 0..N-1 do
    for j in 0..step..N-1 do
        if not (j=i) then
            let cmd = "subscribe,user"+(string j)+",password"+(string j)+",user"+(string i)+", , , , "
            let task = echoServer <? cmd
            let response = Async.RunSynchronously (task, 3000)
            printfn "***command*** : %s" cmd
            printfn "%s" (string(response))
            printfn "%s" ""
        step <- step+1
timerzipf.Stop()
let time_zipf_subscribe = timerzipf.Elapsed.TotalMilliseconds
    


let timerquery = System.Diagnostics.Stopwatch.StartNew()
timerquery.Start()
for i in 0..N-1 do
    let cmd = "querying,user"+(string i)+",password"+(string i)+", , , , , "
    let task = echoServer <? cmd
    let response = Async.RunSynchronously (task, 5000)
    printfn "***command*** : %s" cmd
    printfn "%s" (string(response))
    printfn "%s" ""
timerquery.Stop()
let time_query = timerquery.Elapsed.TotalMilliseconds



let timerhashtag = System.Diagnostics.Stopwatch.StartNew()
timerhashtag.Start()
for i in 0..N-1 do
    let cmd = "#, , , , ,#hashTag"+(string (rand.Next(N)))+", ,"
    let task = echoServer <? cmd
    let response = Async.RunSynchronously (task, 3000)
    printfn "***command*** : %s" cmd
    printfn "%s" (string(response))
    printfn "%s" ""
timerhashtag.Stop()
let time_hashtag = timerhashtag.Elapsed.TotalMilliseconds




let timermention = System.Diagnostics.Stopwatch.StartNew()
timermention.Start()
for i in 0..N-1 do
    let cmd = "@, , , , , ,@user"+(string (rand.Next(N)))+","
    let task = echoServer <? cmd
    let response = Async.RunSynchronously (task, 3000)
    printfn "***command*** : %s" cmd
    printfn "%s" (string(response))
    printfn "%s" ""
timermention.Stop()
let time_mention = timermention.Elapsed.TotalMilliseconds




// printfn "*********************************************************** \n " 
// printfn "*********************************************************** \n " 
// printfn "*********************************************************** \n " 
// printfn "*********************************************************** \n " 
// printfn " %d RANDOM REQUESTS" M 
// printfn "*********************************************************** \n " 
// let timerrandom = System.Diagnostics.Stopwatch.StartNew()
// timerrandom.Start()
// i<-0
// ii<-0
// while i<M do
//     client_simulator<! string (rand.Next(N)) |>ignore
//     i <- i+1
// while ii<M-1 do
//     Thread.Sleep(50)
// timerrandom.Stop()
// let time_random = timerrandom.Elapsed.TotalMilliseconds
// printfn "*********************************************************** \n " 
// printfn "*********************************************************** \n " 
// printfn "*********************************************************** \n " 
// printfn "*********************************************************** \n " 

let hashtime_mentiontime = time_hashtag + time_mention
printfn "Time taken for registration of %d users is %f" N time_register
printfn "Time taken for %d users to make 10 tweets each %f" N time_send
printfn "The time taken for Zipf subscriptions for %d users is %f" N time_zipf_subscribe
printfn "usertimelines fetching time for %d users is %f" N time_query
printfn "Hashtag query time for %d hashtags and %d mentions is %f" N N hashtime_mentiontime

// printfn "Time taken to perform %d random operations is %f" M time_random

// printfn "Total Result: %f %f %f %f %f %f %f" time_register time_send time_zipf_subscribe time_query time_hashtag time_mention time_random
printfn "*********************************************************** \n " 
printfn "*********************************************************** \n " 
printfn "*********************************************************** \n " 
printfn "*********************************************************** \n " 
system.Terminate() |> ignore
0 // return an integer exit code

