# DOS
 
Suhas Kamath Ammembal, &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; UFID : 62846791

Tanvi Reddy Rachamallu, &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;  UFID : 43139981

&nbsp;
&nbsp;



About
This project is the implementation of the twitter API using websharper. In the first part of the project, we were able to simulate the client server model. In this part we are implementing the full API functionality.

Environment and package versions

- FSharp.Core 4.5.1
- Akka.FSharp 1.2.0
- Akka.Remote 1.2.0
- FsPickler 3.4.0

- websharper.fsharp: 4.6.0.361
- WebSharper.Suave: 4.6.0.240
- Suave: 2.5.3

- Fsharp.Data 3.0.1


<b>How to execute?</b>

Step 1 - First run the server on one terminal

dotnet run TwitterServer.fs

Step 2 - Run the TwitterClient.fsx

dotnet fsi --langversion:preview TwitterClient.fsx


Use the TwitterClient.fsx file to execute any of registration, sending, subscribing, querying, ‘#’, ‘@’ etc functionalities with the input format as follows: 9 string format separated by commas.

functionality+","+POST+","+username+","+password+","+target+","+tweet+","+#+","+@+","+register


Example:
reg, ,user1,pass, , , , , ,

reg, ,user2,pass, , , , , ,

send, ,user1,pass, ,tweet1 @Suhas , , ,

send, ,user2,pass, ,tweet2 #Tanvi , , ,

send, ,user2,pass, ,tweet3 @Suhas #Tanvi , , ,

subscribe, ,user1,pass,user2, , , ,

querying, ,user1,pass, , , , ,

querying, ,user2,pass, , , , ,

#, , , , , ,#Tanvi, ,

@, , , , , , ,@Suhas,

retweet, ,user2,pass, ,tweet3 @Suhas #Tanvi , , , 

querying, ,user2,pass, , , , ,

connect, ,user1,pass, , , , ,

send, ,user2,pass, ,tweet4, , ,

send, ,user2,pass, ,tweet5, , ,

send, ,user2,pass, ,tweet6, , ,

disconnect, ,user1,pass, , , , ,

disconnect, ,user1,pass, , , , ,

TwitterTester.fsx is run to do the performance test, the results of which are tabulated in the report.

Implementation - We have 3 files namely TwitterServer.fs, TwitterClient.fsx and TwitterTester.fsx.

TwitterServer.fs - Server

TwitterClient.fsx - Client

Maximum number of users for which the program was run for: 3000
