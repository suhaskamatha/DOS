# DOS
 
Suhas Kamath Ammembal, &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; UFID : 62846791

Tanvi Reddy Rachamallu, &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;  UFID : 43139981

&nbsp;
&nbsp;

<b>How to execute?</b>

Step 1 - First run the server on one terminal

dotnet fsi --langversion:preview TwitterServer.fsx


Step 2 - Run the client on another terminal

dotnet fsi --langversion:preview TwitterClient.fsx <number of users>
 
Ex: dotnet fsi --langversion:preview TwitterClient.fsx 100

Use the TwitterTester.fsx file to manually execute any of registration, sending, subscribing, querying, ‘#’, ‘@’ etc with the input format as follows:
option+","+POSTrequest+","+username+","+password+","+target+","+tweet+","+query#+","+@+","+register

Implementation - We have 3 files namely TwitterServer.fsx, TwitterClient.fsx and TwitterTester.fsx.

Maximum number of users for which the program was run for: 3000
