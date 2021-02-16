#r "nuget: Akka" 
#r "nuget: Akka.FSharp" 
#r "nuget: Akka.Remote" 
#r "nuget: Akka.TestKit" 
//#r "nuget: Akkling"
//#r "nuget: Wire"
open System
open Akka.Actor
open Akka.FSharp
open Akka.Configuration

// type Tweet =
//     struct
//         val tweetId : string
//         val username : string
//         val message : string
//         val tweettime : string
//         [<DefaultValue>]
//         val mutable retweet : string
//         [<DefaultValue>]
//         val mutable retweetid : string
//         new (x1, y1, x2, y2) =
//             {tweetId = x1; username = y1; message = x2; tweettime = y2;}
//     end

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
                    port = 8777
                    hostname = localhost
                }
            }
        }")

let system = ActorSystem.Create("RemoteFSharp", configuration)

type RegisterActorMessage = 
    | RegisterMessage of (string * string * string)
type TweetActorMessage = 
    | TweetMessage of  (string  * string * string * bool)
type FollowerActorMessage =
    | FollowerMessage of  (string  * string * string)
type RetweetActorMessage =
    | RetweetMessage of  (string  * string * string)
type UsertimelineActorMessage =
    | UsertimelineMessage of  (string  * string)
type HashtagActorMessage =
    | HashtagMessage of  ( string )   
type MentionActorMessage =
    | MentionMessage of  ( string )


type Tweet(tweetid:string, text:string, isretweet:bool) =
    member this.Tweetid = tweetid
    member this.Text = text
    member this.IsReTweet = isretweet

    override this.ToString() =
      let mutable res = ""
      if isretweet then
        res <- sprintf "---retweet---[%s]%s" this.Tweetid this.Text
      else
        res <- sprintf "---%s---%s" this.Tweetid this.Text
      res

type User(username:string, password:string) =
    let mutable followers = List.empty: User list
    let mutable tweets = List.empty: Tweet list
    member this.Username = username
    member this.Password = password
    member this.AddFollower x =
        followers <- List.append followers [x]
    member this.GetFollowerlist() =
        followers
    member this.AddTweet x =
        tweets <- List.append tweets [x]
    member this.GetTweets() =
        tweets
    override this.ToString() = 
       this.Username
       

//let user1 = new User("user1", "111")
//let user2 = new User("user2", "222")
//let user3 = new User("user3", "333")
//printfn "%A" (user1.user_name, user1.password, user1.getSubscribes(), user1.GetTweets())
//user1.addSubscribe(user1)
//user1.addSubscribe(user3)
//user1.addTweet(tweet1)
//user1.addTweet(tweet3)
//printfn "%A" (user1.user_name, user1.password, user1.getSubscribes(), user1.getTweets())

type Twitter() =
    let mutable tweets = new Map<string,Tweet>([])
    let mutable users = new Map<string,User>([])
    let mutable hashtags = new Map<string, Tweet list>([])
    let mutable mentions = new Map<string, Tweet list>([])
    member this.AddTweet (tweet:Tweet) =
        tweets <- tweets.Add(tweet.Tweetid,tweet)
    member this.AddUser (user:User) =
        users <- users.Add(user.Username, user)
    member this.AddToHashTag hashtag tweet =
        let key = hashtag
        let mutable map = hashtags
        if not (map.ContainsKey(key))
        then
            let l = List.empty: Tweet list
            map <- map.Add(key, l)
        let value = map.[key]
        map <- map.Add(key, List.append value [tweet])
        hashtags <- map
    member this.AddToMention mention tweet = 
        let key = mention
        let mutable map = mentions
        if not (map.ContainsKey(key))
        then
            let l = List.empty: Tweet list
            map <- map.Add(key, l)
        let value = map.[key]
        map <- map.Add(key, List.append value [tweet])
        mentions <- map
    member this.Register username password =
        let mutable res = ""
        if users.ContainsKey(username) then
            res <- "Username already exists!"
        else
            let user = User(username, password)
            this.AddUser user
            user.AddFollower user
            res <- "Registration successful for username: " + username + "  password: " + password
        res
    member this.SendTweet username password text isretweet =
        let mutable res = ""
        if not (this.Authentication username password) then
            res <- "Authentication failed!"
        else
            if not (users.ContainsKey(username)) then
                res <-  "Bummer! username does not exist."
            else
                let tweet = Tweet(System.DateTime.Now.ToFileTimeUtc() |> string, text, isretweet)
                let user = users.[username]
                user.AddTweet tweet
                this.AddTweet tweet
                let idx1 = text.IndexOf("#")
                if idx1 <> -1 then
                    let idx2 = text.IndexOf(" ",idx1)
                    let hashtag = text.[idx1..idx2-1]
                    this.AddToHashTag hashtag tweet
                let idx1 = text.IndexOf("@")
                if idx1 <> -1 then
                    let idx2 = text.IndexOf(" ",idx1)
                    let mention = text.[idx1..idx2-1]
                    this.AddToMention mention tweet
                res <-  "---done--- Tweet sent : " + tweet.ToString()
        res
    member this.Authentication username password =
            let mutable res = false
            if not (users.ContainsKey(username)) then
                printfn "%A" "Bummer! username not present"
            else
                let user = users.[username]
                if user.Password = password then
                    res <- true
            res
    member this.GetUser username = 
        let mutable res = User("","")
        if not (users.ContainsKey(username)) then
            printfn "%A" "Bummer! username not present"
        else
            res <- users.[username]
        res
    member this.Follower username1 password username2 =
        let mutable res = ""
        if not (this.Authentication username1 password) then
            res <- "Authentication failed"
        else
            let user1 = this.GetUser username1
            let user2 = this.GetUser username2
            user1.AddFollower user2
            res <- "---done--- " + username1 + " followed " + username2
        res
    member this.ReTweet username password text =
        let res = "---retweet---" + (this.SendTweet username password text true)
        res
    member this.Usertimeline  username password =
        let mutable res = ""
        if not (this.Authentication username password) then
            res <- "Authentication failed"
        else
            let user = this.GetUser username
            let res1 = user.GetFollowerlist() |> List.map(fun x-> x.GetTweets()) |> List.concat |> List.map(fun x->x.ToString()) |> String.concat "\n"
            res <- "---success--- usertimeline fetched" + "\n" + res1
        res
    member this.QueryHashTag hashtag =
        let mutable res = ""
        if not (hashtags.ContainsKey(hashtag)) then
            res <- "no tweets with this hashtag"
        else
            let res1 = hashtags.[hashtag] |>  List.map(fun x->x.ToString()) |> String.concat "\n"
            res <- "---done--- Querying HashTag " + "\n" + res1
        res
    member this.QueryMention mention =
        let mutable res = ""
        if not (mentions.ContainsKey(mention)) then
            res <- "no tweets with this person mentioned"
        else
            let res1 = mentions.[mention] |>  List.map(fun x->x.ToString()) |> String.concat "\n"
            res <-  "---done--- Querying Mentions" + "\n" + res1
        res
    override this.ToString() =
        "Print everything"+ "\n" + tweets.ToString() + "\n" + users.ToString() + "\n" + hashtags.ToString() + "\n" + mentions.ToString()
        
    
let twitter = new Twitter()

// *****************************************
// Actor that takes care of sending tweets 
//******************************************

let ActorForTweeting (mailbox: Actor<_>) = 
    let rec loop () = actor {        
        let! message = mailbox.Receive ()
        // let sender_path = mailbox.Sender().Path.ToStringWithAddress()
        match message  with
        |   TweetMessage(username,password,tweet_content,false) -> 
            let response = twitter.SendTweet username password tweet_content false
            mailbox.Sender() <? response |> ignore
        | _ ->  failwith "Message is wrong"
        return! loop()     
    }
    loop ()

// *********************************************
// Actor that takes care of Hashtags querying 
//**********************************************
let ActorForHashtag (mailbox: Actor<_>) = 
    let rec loop () = actor {        
        let! message = mailbox.Receive ()
        match message  with
        |   HashtagMessage(queryhashtag) -> 
            let res = twitter.QueryHashTag  queryhashtag
            mailbox.Sender() <? res |> ignore
        return! loop()     
    }
    loop ()

// ****************************************************
// Actor that takes care of Registration of new users 
//*****************************************************

let ActorForRegistration (mailbox: Actor<_>) = 
    let rec loop () = actor {        
        let! message = mailbox.Receive ()
        match message  with
        |   RegisterMessage(register,username,password) ->
            let res = twitter.Register username password
            mailbox.Sender() <? res |> ignore
        return! loop()     
    }
    loop ()


// *******************************************
// Actor that takes care of User Timeline 
//********************************************
//queryTweetsSubscribed -  usertimeline
let ActorForUsertimeline (mailbox: Actor<_>) = 
    let rec loop () = actor {        
        let! message = mailbox.Receive ()
        match message  with
        |   UsertimelineMessage( username,password ) -> 
            let res = twitter.Usertimeline  username password
            mailbox.Sender() <? res |> ignore
        // | _ -> 
        //     failwith "unknown message"
        return! loop()     
    }
    loop ()

// ***********************************
// Actor that takes care of Mentions 
//************************************
let ActorForMentions (mailbox: Actor<_>) = 
    let rec loop () = actor {        
        let! message = mailbox.Receive ()
        let sender = mailbox.Sender()
        match message  with
        |   MentionMessage(at) -> 
            let res = twitter.QueryMention  at
            sender <? res |> ignore
        return! loop()     
    }
    loop ()


// ************************************
// Actor that takes care of Following
//*************************************
let actorfollower (mailbox: Actor<_>) = 
    let rec loop () = actor {        
        let! message = mailbox.Receive ()
        let sender = mailbox.Sender()
        match message  with
        |   FollowerMessage(username,password,target_username) -> 
            let res = twitter.Follower username password target_username
            sender <? res |> ignore
        return! loop()     
    }
    loop ()

// ************************************
// Actor that takes care of Retweets
//*************************************

let ActorForRetweet (mailbox: Actor<_>) = 
    let rec loop () = actor {        
        let! message = mailbox.Receive ()
        let sender = mailbox.Sender()
        match message  with
        |   RetweetMessage(username,password,tweet_content) -> 
            let res = twitter.ReTweet  username password tweet_content
            sender <? res |> ignore
        
        return! loop()     
    }
    loop ()



////////////////////////////////////////
// opt is the operation we will use
// opt= "reg", "send", "subscribe", "retweet", "querying", "#" , "@"
// let mutable opt= "reg" 
// let mutable POST="POST"
// let mutable username="user2"
// let mutable password="123456"
// let mutable register="register"
// let mutable target_username="user1"
// let mutable tweet_content="Today is a good day!"
// let mutable queryhashtag="#Trump"
// let mutable at="@Biden"
// MessagePack between processor defined below:
//( opt,POST,username,password,target_username,tweet_content,queryhashtag,at,register)
//type MessagePack_processor = MessagePack8 of  string  * string * string* string* string * string* string* string * string
//MessagePack8( opt,POST,username,password,target_username,tweet_content,queryhashtag,at,register)
// dedined the message received actor


let actorRegistration = spawn system "server1" ActorForRegistration
printfn "Actor for Registration spawned"
let actorTweet = spawn system "server2" ActorForTweeting
printfn "Actor for Tweeting spawned"
let actorFollower = spawn system "server3" actorfollower
printfn "Actor for Followers spawned"
let actorRetweet = spawn system "server4" ActorForRetweet
printfn "Actor for Retweets spawned"
let actorUsertimeline = spawn system "server5" ActorForUsertimeline 
printfn "Actor for User Timeline spawned"
let actorHashtag = spawn system "server6" ActorForHashtag
printfn "Actor for Hashtags spawned"
let actorMentions = spawn system "server7" ActorForMentions
printfn "Actor for Mentions spawned"

// for the server, received string and dispatch
let ServerWrapper (mailbox: Actor<_>) = 
    let rec loop () = actor {        
        let! message = mailbox.Receive ()
        let sender = mailbox.Sender()
        match box message with
        | :? string   ->
            if message="" then
                return! loop() 
            //(opt,POST,username,password,target_username,tweet_content,queryhashtag,at,register)
            // printfn "%s" ""
            // printfn "[message received] %s" message
            let result = message.Split ','
            let mutable opt= result.[0]
            let mutable username=result.[1]
            let mutable password=result.[2]
            let mutable target_username=result.[3]
            let mutable tweet_content=result.[4]
            let mutable queryhashtag=result.[5]
            let mutable at=result.[6]
            let mutable register=result.[7]
            let mutable task = actorRegistration <? RegisterMessage("","","")
            // For function reg
            if opt= "registration" then
                // printfn "---Registration--- of username:%s password: %s" username password
                task <- actorRegistration <? RegisterMessage(register,username,password)
            // For function send
            if opt= "tweet" then
                // printfn "---Tweet--- by username:%s password: %s tweet_content: %s" username password tweet_content
                task <- actorTweet <? TweetMessage(username,password,tweet_content,false)
            // For function subscribe
            if opt= "subscribe" then
                // printfn "---Follow--- username:%s password: %s followed username: %s" username password target_username
                task <- actorFollower <? FollowerMessage(username,password,target_username )
            // For function retweet
            if opt= "retweet" then
                // printfn "[retweet] username:%s password: %s tweet_content: %s" username password tweet_content
                task <- actorRetweet <? RetweetMessage(username,password,tweet_content)
            // For function retweet
            if opt= "querying" then
                // printfn "[querying] username:%s password: %s" username password
                task <- actorUsertimeline <? UsertimelineMessage(username,password )
            // For function retweet queryhashtag
            if opt= "#" then
                // printfn "[#Hashtag] %s: " queryhashtag
                task <- actorHashtag <? HashtagMessage(queryhashtag )
            // For function @
            if opt= "@" then
                // printfn "[@mention] %s" at
                task <- actorMentions <? MentionMessage(at )
            let response = Async.RunSynchronously (task, 1000)
            sender <? response |> ignore
            printfn "%s" response
        | _ ->  failwith "unknown message"
        return! loop()     
    }
    loop ()
let ActorServerWrapper = spawn system "EchoServer" ServerWrapper

// once we received a set of string, dispatch to different functional actor
// dispatch was based on the opt.
ActorServerWrapper <? "" |> ignore
printfn "*********************************************************** \n " 
printfn "*********************************************************** \n " 
printfn "*********************************"   
printfn "TWITTER SERVER RUNNING" 
printfn "*********************************   "

// For function reg
Console.ReadLine() |> ignore

printfn "*********************************************************** \n " 
printfn "*********************************************************** \n " 
printfn "*********************************************************** \n " 

0