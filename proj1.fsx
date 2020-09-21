#time "on"
open System
open System.Diagnostics

let sq  = fun (x:int64)->x*x

let is_sq n = (sq (int64(sqrt (double n)))) = n

let sqk (x:int64) (k:int64) =
    (List.sumBy (sq) [x..x+k-1L]) |> is_sq

let actor first second k =
    [first..second] |> List.filter(fun x->(sqk x k)) |> List.collect(fun x->[x])

let asyncActor first second k = async {return (actor first second k)}   

let args : string array = fsi.CommandLineArgs |> Array.tail

let first = args.[0]

let second = args.[1]

let number = [1;10;100;1000;10000;100000;1000000]
//#time
for num in number do
// Calculating time for each of the work units mentioned in number
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
    let stopWatch = System.Diagnostics.Stopwatch.StartNew()
    let one = 1L
    let two = first |> int64

    let _k = second |> int64
    let temp =(int64)num
    let s = (one/temp)

        
    let Range = fun (i:int64) -> [one+(i-1L)*s;one+i*s-1L]

    let temp_ans =  [(one+((int64)num-1L)*s);two]

    let ans = [1L..(int64)num-1L] |> List.map (fun x-> (Range x)) |> List.append [temp_ans]
      
    let bossFunc() = 
        (ans
            |> List.map(fun x-> asyncActor x.[0] x.[1] _k)
            |> Async.Parallel
            |> Async.RunSynchronously
            |> Array.filter (fun x->(x.Length > 0))
            |> Array.toList)
            |> List.collect(id)
    let list = bossFunc()
    list |> List.map(fun x->(int)x) |> Seq.iter (fun x -> printf "\n %d" x)
    printfn "\n-----------\n"
    
    stopWatch.Stop()
    printfn "%f" stopWatch.Elapsed.TotalMilliseconds