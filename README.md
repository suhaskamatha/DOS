# DOS
 
Suhas Kamath Ammembal       UFID : 62845791

Tanvi Reddy Rachamallu      UFID : 43139981

How to execute:

dotnet fsi --langversion:preview fileName <number of nodes> <number of requests>
 
Example : dotnet fsi --langversion:preview project3.fsx 1000 10

Observations:

|   Number of Nodes   |   Number of Requestws   |   Avg Number of Hops  |
| ------------------- |:-----------------------:| ---------------------:|
|        10           |           10            |         1.05          |
|       100           |           10            |         2.271         |
|      1000           |           10            |         3.536         |
|     10000           |           10            |         4.924         |
