COP5615 - Fall 2020
October 15, 2020

####Team Members

#####Dhananjay Sarsonia  UFID: 1927-5958

#####Forum Gala          UFID: 6635-6557


####How to run
Using terminal, go to the directory containing the program.fsx file.
- Please make sure dotnet core is installed
- Run using the command: 

**dotnet fsi --langversion:preview program.fsx numNodes topology algorithm**
 
numNodes  is the total number of nodes with 10,000 as maximum
topology  can be "line" ; "twod" ; "imptwod" ; "full"
algorithm can be "gossip" ; "pushsum"

####Quick copy paste commands for all topologies and algorithms
**For Gossip Algorithm**
- dotnet fsi --langversion:preview program.fsx 100 line gossip
- dotnet fsi --langversion:preview program.fsx 100 twod gossip
- dotnet fsi --langversion:preview program.fsx 100 imptwod gossip
- dotnet fsi --langversion:preview program.fsx 100 full gossip

**For Push-Sum Algorithm**
- dotnet fsi --langversion:preview program.fsx 100 line pushsum
- dotnet fsi --langversion:preview program.fsx 100 twod pushsum
- dotnet fsi --langversion:preview program.fsx 100 imptwod pushsum
- dotnet fsi --langversion:preview program.fsx 100 full pushsum

####What is working?
All topologies and algorithms are working. Both Push-Sum and Gossip are working with Line, 2D, Imperfect 2D, and full network.

####What is the largest network you managed to deal with for each type of topology and algorithm?

**Push-Sum max tested node count-**
- Line: 
- TwoD: 
- Imperfect Two-D:
- Full:

**Gossip max tested node count-**
- Line: 
- TwoD: 
- Imperfect Two-D:
- Full:
