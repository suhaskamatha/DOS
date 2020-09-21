# DOS
Name : Suhas Kamath Ammembal
UFID : 62845791
How to execute:
dotnet fsi --langversion:preview filename N k
Example: dotnet fsi --langversion:preview proj1.fsx 1000000 4
1.
For a given input N=1000000 and k=4
Number of workers = N/ Number of sub-problems
Work Size	Real Time	CPU TIme	Ratio
1000000	0.267	0.296	1.11
100000	0.189	0.437	2.31
10000	0.205	0.500	2.44
1000	0.110	0.515	4.68
100	0.167	0.578	3.46
10	0.497	0.796	1.60
1	0.773	1.105	1.43

After executing for various work unit sizes as mentioned in the table above, I calculated the ratio of CPU time to Real time, the best utilization (number of cores used) comes for work size around 1000. Program is run in a for loop iterated over number of threads in multiples of 10. For question 3 loop in the program was removed and executed for only the best case of 10000 threads.
2. 
  

3. CPU time: 0.515
   Real time: 0.11
   Number of cores = CPU time/ Real time = 4.68


4. 
 

 
