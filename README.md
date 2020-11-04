# DOS
 
Suhas Kamath Ammembal, &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; UFID : 62845791

Tanvi Reddy Rachamallu, &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;  UFID : 43139981


How to execute:

dotnet fsi --langversion:preview fileName <number of nodes> <number of requests>
 
Example : dotnet fsi --langversion:preview project3.fsx 1000 10

What is working?

In this project, we tested the functionality of routing and joining aspects of the pastry protocol. The network has been tested for a maximum of 10000 nodes. 

Observations:

1. Number of Requests = 10

|   Number of Nodes   |   Number of Requestws   |   Avg Number of Hops  |
| ------------------- |:-----------------------:| ---------------------:|
|        10           |           10            |         1.05          |
|       100           |           10            |         2.271         |
|      1000           |           10            |         3.536         |
|     10000           |           10            |         4.924         |

2. Number of Requests = 100

|   Number of Nodes   |   Number of Requestws   |   Avg Number of Hops  |
| ------------------- |:-----------------------:| ---------------------:|
|        10           |          100            |         1.120         |
|       100           |          100            |         2.291         |
|      1000           |          100            |         3.509         |
|     10000           |          100            |         4.924         |
