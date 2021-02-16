#r "nuget: Akka" 
#r "nuget: Akka.FSharp" 
#r "nuget: Akka.TestKit" 

open System
open Akka.Actor
open Akka.FSharp

let args : string array = fsi.CommandLineArgs |> Array.tail
let system = System.create "processor" <| Configuration.load ()
let stopWatch = System.Diagnostics.Stopwatch.StartNew()
let  N = args.[0] |> int
let sc = 5
let topo = args.[1]
let algo = args.[2]

let sqrtN= Convert.ToInt32( round (sqrt (float N)))
let sN =(sqrtN)*(sqrtN)
let lN=(sqrtN+1)*(sqrtN+1)
let mutable sq =(sqrtN)*(sqrtN)
let n=Convert.ToInt32( round (sqrt (float sq)))
if N <= sN
    then 
        sq<-sN
    else
        sq<-lN

let  mutable a1= [| for i in 1.. N -> 0 |]
let  mutable a2 = [| for i in 1.. N -> 0 |]
let mutable a3 = a1|> Array.sum

let  mutable sarr = [| for i in 1.. N ->(double i) |]
let  mutable warr = [| for i in 1.. N -> (double 1) |]
let  mutable rarr = [| for i in 1.. N -> (double i) |]

let  mutable ter = [| for i in 1.. sq -> 0 |] // 0-1
let  mutable count = [| for i in 1.. sq -> 0 |] //0-3
let mutable asum = ter|> Array.sum

type ProcessorMessage = ProcessJob of  string * int * string
type ProcessorMessage1 = ProcessJob1 of   int * string * double * double  
let rand = System.Random()

let processor1 (mailbox: Actor<_>) = 
    let rec loop (count) = actor {
        a3 <- a1|> Array.sum
        if a3 >=N-1
            then
                stopWatch.Stop()
                else
                    let! message = mailbox.Receive ()
                    let sender = mailbox.Sender()
                    let senderpath = mailbox.Sender().Path.ToStringWithAddress()
                    match message  with
                    |   ProcessJob(x,num,topo) ->
                        let mutable rnum = Random( ).Next() % N
                        while a1.[rnum]=1  do
                              rnum <- Random( ).Next() % N
                              while rnum = num  do
                                rnum <- Random( ).Next() % N

                        if (count+1) >= sc
                            then  
                                a1.[num]<-1
                                if a3 <N
                                    then
                                        let g = system.ActorSelection("akka://processor/user/" + string rnum)
                                        g <? ProcessJob( "Rumor",rnum,topo)|>ignore
                            else 
                                let g = system.ActorSelection("akka://processor/user/" + string rnum)
                                g <? ProcessJob( "Rumor",rnum,topo)|>ignore
                    return! loop(count+1)     
    }
    loop 0

let processor2 (mailbox: Actor<_>) = 
    let rec loop (count) = actor {
        asum  <- ter |> Array.sum
        if asum  >=sq-1
            then
                stopWatch.Stop()
                else
                    let! message = mailbox.Receive ()
                    let sender = mailbox.Sender()
                    let senderpath = mailbox.Sender().Path.ToStringWithAddress()
                    match message  with
                    |   ProcessJob(x,num,topo) ->
                        let mutable direction=Random( ).Next() % 4
                        let mutable rnum = num+1
                        
                        direction<-Random( ).Next() % 4 
                        if rnum%n=1 then direction<-1
                        elif rnum%n=n then  direction<-3

                        if rnum <=n then direction<-2
                        elif rnum >= sq-n+1 then direction<-0

                        if direction=0 then rnum<- num-n
                        if direction=1 then rnum<- num+1
                        if direction=2 then rnum<- num+n
                        if direction=3 then rnum<- num-1
  
                        if (count+1) >= sc
                            then  
                                ter .[num]<-1
                                if asum <sq
                                    then

                                        let g = system.ActorSelection("akka://processor/user/" + string rnum)
                                        g <? ProcessJob( "Rumor",rnum,topo)|>ignore
                            else 
                                let g = system.ActorSelection("akka://processor/user/" + string rnum)
                                g <? ProcessJob( "Rumor",rnum,topo)|>ignore
                    return! loop(count+1)     
    }
    loop 0

let processor3 (mailbox: Actor<_>) = 
    let rec loop (count) = actor {
        asum  <- ter |> Array.sum
        if asum  >=sq-1
            then
                stopWatch.Stop()
                else
                    let! message = mailbox.Receive ()
                    let sender = mailbox.Sender()
                    let senderpath = mailbox.Sender().Path.ToStringWithAddress()
                    match message  with
                    |   ProcessJob(x,num,topo) ->
                        let mutable direction=Random( ).Next() % 4
                        let mutable rnum = num+1                        
                        direction<-Random( ).Next() % 4 
                        if rnum%n=1 then direction<-1
                        elif rnum%n=n then  direction<-3

                        if rnum <=n then direction<-2
                        elif rnum >= sq-n+1 then direction<-0
                        if direction=0 then rnum<- num-n
                        if direction=1 then rnum<- num+1
                        if direction=2 then rnum<- num+n
                        if direction=3 then rnum<- num-1
                        direction<-Random( ).Next() % 5 
                        if direction=4 then rnum<-Random( ).Next() % sq   
                        if (count+1) >= sc
                            then  
                                ter.[num]<-1
                                if asum <sq
                                    then
                                        let g = system.ActorSelection("akka://processor/user/" + string rnum)
                                        g <? ProcessJob( "Rumor",rnum,topo)|>ignore
                            else 
                                let g = system.ActorSelection("akka://processor/user/" + string rnum)
                                g <? ProcessJob( "Rumor",rnum,topo)|>ignore

                    return! loop(count+1)     
    }
    loop 0

let processor4 (mailbox: Actor<_>) = 
    let rec loop (count) = actor {
        a3 <- a1|> Array.sum
        if a3 >= N-1
            then
                stopWatch.Stop()
                else
                    let! message = mailbox.Receive ()
                    let sender = mailbox.Sender()
                    let senderpath = mailbox.Sender().Path.ToStringWithAddress()
                    match message  with
                    |   ProcessJob(x,num,topo) ->
                        let mutable direction=0
                        let mutable rnum = num
                        direction<-Random( ).Next() % 2 
                        if direction=1 then rnum<- num+1
                        if direction=0 then rnum<- num-1                                              
                        if a3>=N-sqrtN then
                            rnum<-Random( ).Next() % N   
                            
                        if rnum=1 then rnum<-num+1
                        if rnum=N then rnum<-num-1                           
                        if (count+1) >= sc
                            then  
                                a1.[num]<-1
                                if a3 <N
                                    then
                                        let g = system.ActorSelection("akka://processor/user/" + string rnum)
                                        g <? ProcessJob( "Rumor",rnum,topo)|>ignore
                            else 
                                let g = system.ActorSelection("akka://processor/user/" + string rnum)
                                g <? ProcessJob( "Rumor",rnum,topo)|>ignore
                    return! loop(count+1)     
    }
    loop 0

let processor5 (mailbox: Actor<_>) = 
    let rec loop (count ) = actor {
        let! message = mailbox.Receive ()
        match message  with
        |   ProcessJob1(num,topo,s,w ) ->
            a3 <- a1|> Array.sum
            if a3 >= N-1 then stopWatch.Stop()
            let mutable rnum = Random( ).Next() % N
            sarr.[num]<-sarr.[num]+s
            warr.[num]<-warr.[num]+w
            let mutable nw=double(s/w)
            let mutable e= abs(double(nw-rarr.[num]))
            if e < 1e-10 then a2.[num]<-a2.[num]+1
            if a2.[num] >= 3
                then 
                    a1.[num]<- 1
                    a3 <- a1|> Array.sum
                    if a3 <N 
                        then
                            sarr.[num]<-sarr.[num]/2.0
                            warr.[num]<-warr.[num]/2.0
                            rarr.[num]<-sarr.[num]/warr.[num]
                            let g = system.ActorSelection("akka://processor/user/" + string rnum)
                            g <? ProcessJob1(rnum,topo,sarr.[num],warr.[num])|>ignore
                            
            else
                sarr.[num]<-sarr.[num]/2.0
                warr.[num]<-warr.[num]/2.0
                rarr.[num]<-sarr.[num]/warr.[num]
                let g = system.ActorSelection("akka://processor/user/" + string rnum)
                g <? ProcessJob1(rnum,topo,sarr.[num],warr.[num])|>ignore
        return! loop(count+1)        
    }
    loop 0
        
let processor6 (mailbox: Actor<_>) = 
    let rec loop (count ) = actor {
        let! message = mailbox.Receive ()
        match message  with
        |   ProcessJob1(num,topo,s,w ) ->
            a3 <- a1|> Array.sum
            if a3 >= N-1 then stopWatch.Stop()
            let mutable direction=Random( ).Next() % 4
            let mutable rnum = num
            direction<-Random( ).Next() % 4 
            if rnum%n=1 then direction<-1
            elif rnum%n=n then  direction<-3

            if rnum <=n then direction<-2
            elif rnum >= sq-n+1 then direction<-0

            if direction=0 then rnum<- num-n
            if direction=1 then rnum<- num+1
            if direction=2 then rnum<- num+n
            if direction=3 then rnum<- num-1                     
            
            if a3>=N-sqrtN then
              //printfn "Close"
                rnum<-Random( ).Next() % N  

            if rnum<0 then rnum<-num+1
            if rnum>=N then rnum<-num-n
            sarr.[num]<-sarr.[num]+s
            warr.[num]<-warr.[num]+w
            let mutable nw=double(s/w)
            let mutable e= abs(double(nw-rarr.[num]))
            if e < 1e-10 then a2.[num]<-a2.[num]+1
            if a2.[num] >= 3
                then 
                    a1.[num]<- 1
                    a3 <- a1|> Array.sum
                    if a3 <N 
                        then
                            sarr.[num]<-sarr.[num]/2.0
                            warr.[num]<-warr.[num]/2.0
                            rarr.[num]<-sarr.[num]/warr.[num]
                            let g = system.ActorSelection("akka://processor/user/" + string rnum)
                            g <? ProcessJob1(rnum,topo,sarr.[num],warr.[num])|>ignore
                            
            else
                sarr.[num]<-sarr.[num]/2.0
                warr.[num]<-warr.[num]/2.0
                rarr.[num]<-sarr.[num]/warr.[num]
                let g = system.ActorSelection("akka://processor/user/" + string rnum)
                g <? ProcessJob1(rnum,topo,sarr.[num],warr.[num])|>ignore
        return! loop(count+1)        
    }
    loop 0

let processor7 (mailbox: Actor<_>) = 
    let rec loop (count ) = actor {
        let! message = mailbox.Receive ()
        match message  with
        |   ProcessJob1(num,topo,s,w ) ->
            a3 <- a1|> Array.sum
            if a3 >= N-1 then stopWatch.Stop()
            let mutable direction=Random( ).Next() % 4
            let mutable rnum = num
            direction<-Random( ).Next() % 4 
            if rnum%n=1 then direction<-1
            elif rnum%n=n then  direction<-3

            if rnum <=n then direction<-2
            elif rnum >= sq-n+1 then direction<-0

            if direction=0 then rnum<- num-n
            if direction=1 then rnum<- num+1
            if direction=2 then rnum<- num+n
            if direction=3 then rnum<- num-1                      
            if a3>=N-sqrtN then
                //printfn "Close."
                rnum<-Random( ).Next() % N 

            let mutable randomdice=Random( ).Next() % 5
            if randomdice=1 then rnum<-Random( ).Next() % N      
            if rnum<0 then rnum<-num+1
            if rnum>=N then rnum<-num-n

            sarr.[num]<-sarr.[num]+s
            warr.[num]<-warr.[num]+w
            let mutable nw=double(s/w)
            let mutable e= abs(double(nw-rarr.[num]))
            if e < 1e-10 then a2.[num]<-a2.[num]+1
            if a2.[num] >= 3
                then 
                    a1.[num]<- 1
                    a3 <- a1|> Array.sum
                    if a3 <N 
                        then
                            sarr.[num]<-sarr.[num]/2.0
                            warr.[num]<-warr.[num]/2.0
                            rarr.[num]<-sarr.[num]/warr.[num]
                            let g = system.ActorSelection("akka://processor/user/" + string rnum)
                            g <? ProcessJob1(rnum,topo,sarr.[num],warr.[num])|>ignore
                            
            else
                sarr.[num]<-sarr.[num]/2.0
                warr.[num]<-warr.[num]/2.0
                rarr.[num]<-sarr.[num]/warr.[num]
                let g = system.ActorSelection("akka://processor/user/" + string rnum)
                g <? ProcessJob1(rnum,topo,sarr.[num],warr.[num])|>ignore
        return! loop(count+1)        
    }
    loop 0
 
let processor8 (mailbox: Actor<_>) = 

    let rec loop (count ) = actor {
        let! message = mailbox.Receive ()
        match message  with
        |   ProcessJob1(num,topo,s,w ) ->
            a3 <- a1|> Array.sum
            if a3 >= N-1 then stopWatch.Stop()
            let mutable direction=0
            let mutable rnum = num
            direction<-Random( ).Next() % 2 
            if direction=1 then rnum<- num+1
            if direction=0 then rnum<- num-1                     
            if a3>=N-sqrtN then
                rnum<-Random( ).Next() % N      
            
            if rnum<0 then rnum<-num+1
            if rnum>=N then rnum<-num-1
            sarr.[num]<-sarr.[num]+s
            warr.[num]<-warr.[num]+w
            let mutable nw=double(s/w)
            let mutable e= abs(double(nw-rarr.[num]))
            if e < 1e-10 then a2.[num]<-a2.[num]+1
            if a2.[num] >= 3
                then 
                    a1.[num]<- 1
                    a3 <- a1|> Array.sum
                    if a3 <N 
                        then
                            sarr.[num]<-sarr.[num]/2.0
                            warr.[num]<-warr.[num]/2.0
                            rarr.[num]<-sarr.[num]/warr.[num]
                            let g = system.ActorSelection("akka://processor/user/" + string rnum)
                            g <? ProcessJob1(rnum,topo,sarr.[num],warr.[num])|>ignore
                            
            else
                sarr.[num]<-sarr.[num]/2.0
                warr.[num]<-warr.[num]/2.0
                rarr.[num]<-sarr.[num]/warr.[num]
                let g = system.ActorSelection("akka://processor/user/" + string rnum)
                g <? ProcessJob1(rnum,topo,sarr.[num],warr.[num])|>ignore
        return! loop(count+1)        
    }
    loop 0

let time f =
  let ptime = System.Diagnostics.Process.GetCurrentProcess()
  let numGC = System.GC.MaxGeneration
  let startTotal = ptime.TotalProcessorTime
  let startGC = [| for i in 0 .. numGC -> System.GC.CollectionCount(i) |]
  let stopwatch = System.Diagnostics.Stopwatch.StartNew()
  let res = f ()
  stopwatch.Stop()
  let total = ptime.TotalProcessorTime - startTotal
  let spanGC = [ for i in 0 .. numGC-> System.GC.CollectionCount(i) - startGC.[i] ]
  let elapsed = stopwatch.Elapsed 
  printfn "Real: %A, CPU: %A, GC %s" elapsed total ( spanGC |> List.mapi (sprintf "gen%i: %i") |> String.concat ", ")
  res

if algo="gossip" then
    if topo="full" 
        then
            let actorArray = Array.create N (spawn system "processor1" processor1)
            {0..N-1} |> Seq.iter (fun a ->
                actorArray.[a] <- spawn system (string a) processor1
            )
            actorArray.[1] <? ProcessJob("Rumor",2,topo)|>ignore
            let Systemwait=0
            while a3 < N-1 do
                Systemwait|>ignore
            printfn "Running time: %f ms" stopWatch.Elapsed.TotalMilliseconds

    if topo= "2D"
        then
            printfn "Your N is:  %i, turned up N is: %i" N  sq
            let actorArray = Array.create sq (spawn system "processor2" processor2)
            {0..sq-1} |> Seq.iter (fun a ->
                actorArray.[a] <- spawn system (string a) processor2)
            actorArray.[1] <? ProcessJob("Rumor",2,topo)|>ignore
            let Systemwait=0
            while asum < sq-2 do
                Systemwait|>ignore
            printfn "Running time: %f ms" stopWatch.Elapsed.TotalMilliseconds

    if topo= "imp2D"
    then
        printfn "Your N is:  %i, turned up N is: %i" N  sq
        let actorArray = Array.create sq (spawn system "processor3" processor3)
        {0..sq-1} |> Seq.iter (fun a ->
            actorArray.[a] <- spawn system (string a) processor3)
        actorArray.[1] <? ProcessJob("Rumor",2,topo)|>ignore
        let Systemwait=0
        while asum < sq-2 do
            Systemwait|>ignore
        printfn "Running time: %f ms" stopWatch.Elapsed.TotalMilliseconds
            
    if topo="line" then
        let actorArray = Array.create N (spawn system "processor4" processor4)
        {0..N-1} |> Seq.iter (fun a ->
            actorArray.[a] <- spawn system (string a) processor4)
        actorArray.[1] <? ProcessJob("Rumor",2,topo)|>ignore
        let Systemwait=0
        while a3 < N-1 do
            Systemwait|>ignore
        printfn "Running time: %f ms" stopWatch.Elapsed.TotalMilliseconds

let timer f =
  let ptime = System.Diagnostics.Process.GetCurrentProcess()
  let numGC = System.GC.MaxGeneration
  let startTotal = ptime.TotalProcessorTime
  let startGC = [| for i in 0 .. numGC -> System.GC.CollectionCount(i) |]
  let stopwatch = System.Diagnostics.Stopwatch.StartNew()
  let res = f ()
  stopwatch.Stop()
  let total = ptime.TotalProcessorTime - startTotal
  let spanGC = [ for i in 0 .. numGC-> System.GC.CollectionCount(i) - startGC.[i] ]
  let elapsed = stopwatch.Elapsed 
  printfn "Real: %A, CPU: %A, GC %s" elapsed total ( spanGC |> List.mapi (sprintf "gen%i: %i") |> String.concat ", ")
  res       

if algo="push-sum" then
    if topo="full" 
        then
            let actorArray = Array.create N (spawn system "processor5" processor5)
            {0..N-1} |> Seq.iter (fun a ->
                actorArray.[a] <- spawn system (string a) processor5)
            sarr.[1]<-sarr.[1]/2.0
            warr.[1]<-warr.[1]/2.0
            actorArray.[1] <? ProcessJob1(2,topo,sarr.[1],warr.[1] )|>ignore
            let Systemwait=0
            a3 <- a1|> Array.sum
            while a3 < N do
                Systemwait|>ignore
            printfn "Running time: %f ms" stopWatch.Elapsed.TotalMilliseconds

    if topo="2D" 
    then
        let actorArray = Array.create N (spawn system "processor6" processor6)
        {0..N-1} |> Seq.iter (fun a ->
            actorArray.[a] <- spawn system (string a) processor6
        )
        sarr.[1]<-sarr.[1]/2.0
        warr.[1]<-warr.[1]/2.0
        actorArray.[1] <? ProcessJob1(2,topo,sarr.[1],warr.[1] )|>ignore
        let Systemwait=0
        a3 <- a1|> Array.sum
        while a3 < N do
            Systemwait|>ignore
        printfn "Running time: %f ms" stopWatch.Elapsed.TotalMilliseconds
    
    if topo="imp2D" 
    then
        let actorArray = Array.create N (spawn system "processor7" processor7)
        {0..N-1} |> Seq.iter (fun a ->
            actorArray.[a] <- spawn system (string a) processor7)

        sarr.[1]<-sarr.[1]/2.0
        warr.[1]<-warr.[1]/2.0
        actorArray.[1] <? ProcessJob1(2,topo,sarr.[1],warr.[1] )|>ignore
        let Systemwait=0
       
        a3 <- a1|> Array.sum
        while a3 < N do
            Systemwait|>ignore
        printfn "Running time: %f ms" stopWatch.Elapsed.TotalMilliseconds
    
    if topo="line" 
    then
        let actorArray = Array.create N (spawn system "processor8" processor8)
        {0..N-1} |> Seq.iter (fun a ->
            actorArray.[a] <- spawn system (string a) processor8)
        sarr.[1]<-sarr.[1]/2.0
        warr.[1]<-warr.[1]/2.0
        actorArray.[1] <? ProcessJob1(2,topo,sarr.[1],warr.[1] )|>ignore
        let Systemwait=0
        
        a3 <- a1|> Array.sum
        while a3 < N do
            Systemwait|>ignore
        printfn "Running time: %f ms" stopWatch.Elapsed.TotalMilliseconds