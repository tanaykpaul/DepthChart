# Coding Challenge – NFL Depth Charts
As part of the coding challenge, I have designed and implemented a solution that meets all the requirements. In the solution design, I have followed the Clean architecture, Unit of Work design pattern and Entity framework core as ORM. I have applied the SOLID principles and Clean coding guidelines in the implementation. I have used C# language with ASP.NET Core 8.0 framework. The following sections will explain the underlying technologies and services that can be used in your future project.

## Getting Started
The following instructions will get you a copy of the project. You should be able to compile, build and run the project on your local machine. See the Tests that will help you to understand the layers and data flow of the project. Also, it has a simple WebApi to experiment the use cases. 

### Prerequisites
1. .NET 8 SDK
2. Any IDE that supports ASP.NET Core 8.0 for further development
   
## Environment setup
1. Clone the repository from https://github.com/tanaykpaul/DepthChart.git
2. For development, open the file DepthChart.sln from your IDE and Build the solution
3. If you just want to build the solution, execute "dotnet build" command from the directory of DepthChart.sln file
4. To run the Tests, go to -> "DC.Tests" folder and then execute "dotnet test" command
5. To render the WebAPI project to experiment the solution, go to -> "DC.Presentation" folder and then execute "dotnet watch run" command

## Problem Statements
This coding challenge asks to build and process the Depth Chart for a Team under a Sport. For example, visit https://www.ourlads.com/nfldepthcharts/depthchart/TB
The solution needs to be scalable to add more Sports - MLB, NHL, NBA etc. as well as to add all the NFL teams.

### Use Cases to Implement
1. addPlayerToDepthChart (position, player, position_depth)
    - Adds a player to the depth chart at a given position
    - Adding a player without a position_depth would add them to the end of the depth chart at that position
    - The added player would get priority. Anyone below that player in the depth chart would get moved down a position_depth
2. removePlayerFromDepthChart(position, player)
    - Removes a player from the depth chart for a given position and returns that player
    - An empty list should be returned if that player is not listed in the depth chart at that position
3. getBackups (position, player)
    - For a given player and position, we want to see all players that are “Backups”, those with a lower position_depth
    - An empty list should be returned if the given player has no Backups
    - An empty list should be returned if the given player is not listed in the depth chart at that position
4. getFullDepthChart()
    - Print out the full depth chart with every position on the team and every player within the Depth Chart

## Data Model
Based on the Business requirements and data flow, I have identified 4 entities (i.e., Sport, Team, Player and Position). These entities can manage the Depth Chart efficiently. See the following Entity-Relationship:
1. Sport (NFL) has one-to-many relationship with Team (Tampa Bay Buccaneers, Dallas Cowboys etc.)
2. Team has one-to-many relationship with Player (Tom Brady, Scott Miller etc.)
3. Team has has one-to-many relationship with Position (QB, LT etc.)
4. Player has many-to-many relationship with Position (as such, a JOIN table namely, Order keeps the Depth Chart data)

## Solution Design
As per the Clean architecture guidelines, I have created 5 projects for the solution (DepthChart.sln). They are:
1. DC.Domain - contains the core business logic interfaces and domain models
2. DC.Application - includes the use cases and application logic
3. DC.Infrastructure - contains implementations of the interfaces defined in the domain and application layers, including database access
4. DC.Presentation - contains a asp.net core web api having 6 controllers (1 controller for the 4 use cases and the remainings are for the repositories related to the entities)
5. DC.Tests - contains unit tests for the solutions

## Implementation
1. I have created 5 repository interfaces and their implementation.
2. To meeting the 4 use cases, I have created a Unit of Work interface and its implementation that uses the core repositories
3. I have implemented logging process. It is open to use any Libraries such as SeriLog, Log4net etc.
4. I have used Entity Framework Core as the ORM technology
5. I have used In-memory database for experiment in the Presentation and Test projects. It is open to use any database such as SQL server 2022 etc.
6. I have use Moq in the Test project
 
## Assumptions
1. To experiment the use cases, (a) first, create a Sport (only use Name -> "NFL") entry from SportController (output -> SportId) or use AddAsync method from the SportRepository.cs (b) then, create a Team entry using Name -> "Tampa Bay Buccaneers" and SportId (from the previous step); output is TeamId, (c) then, create a Postion entry using Name -> "QB" and TeamId, and finally (d) create a Player entry using Number -> 12, Name -> Tom Brady and TeamId.
2. Following the previous step, you can create many players and positions (as per your requirements) for a TeamId under a SportId.
3. This solution is scalable for many Teams and Sports.
4. Player Number is unique in the Team, so need to supply the Player Number to access the methods associated to the Player entity
5. Depth_Position is zero based (so, SeqNumber in the Order Entity presents the position, say, SeqNumber=0 means No. One player say "Player-A", SeqNumber=1 means, No. Two player or the Backup of "Player-B" for the Position say "QB")

## Future work
1. This coding challenge does not require the best practices for the presentation layer (e.g., oAuth2, JWT, CORS, Rate Limiting, Caching etc.). As such, there are a lots of scope to work on the DC.Presentation project.
2. Write more logs and tests
