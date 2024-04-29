using DC.Application.DTOs;
using DC.Domain.Entities;
using DC.Domain.Logging;
using DC.Infrastructure.Data;
using DC.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Moq;
using System.Text.Json;

namespace DC.Tests
{
    [TestClass]
    public class DepthChartFromJsonTests
    {
        private DepthChartDbContext _dbContext;
        private SportRepository _sportRepository;
        private TeamRepository _teamRepository;
        private PlayerRepository _playerRepository;
        private PositionRepository _positionRepository;
        private OrderRepository _orderRepository;
        private Mock<IAppLogger> _mockSportLogger;
        private Mock<IAppLogger> _mockTeamLogger;
        private Mock<IAppLogger> _mockPlayerLogger;
        private Mock<IAppLogger> _mockPositionLogger;
        private Mock<IAppLogger> _mockOrderLogger;

        [TestInitialize]
        public void Setup()
        {
            // Configure the in-memory database
            var options = new DbContextOptionsBuilder<DepthChartDbContext>()
                .UseInMemoryDatabase(databaseName: "DepthChartFromJsonTestsDatabase")
                .Options;

            _dbContext = new DepthChartDbContext(options);
            _dbContext.Database.EnsureDeletedAsync();
            _dbContext.Database.EnsureCreatedAsync();
        }

        // Setup initial data for sport, team, player, position, and orders
        private async Task SetupInitialData()
        {
            // Initialize repositories and mocked loggers
            _mockSportLogger = new Mock<IAppLogger>();
            _mockTeamLogger = new Mock<IAppLogger>();
            _mockPlayerLogger = new Mock<IAppLogger>();
            _mockPositionLogger = new Mock<IAppLogger>();
            _mockOrderLogger = new Mock<IAppLogger>();

            _sportRepository = new SportRepository(_dbContext, _mockSportLogger.Object);
            _teamRepository = new TeamRepository(_dbContext, _mockTeamLogger.Object);
            _playerRepository = new PlayerRepository(_dbContext, _mockPlayerLogger.Object);
            _positionRepository = new PositionRepository(_dbContext, _mockPositionLogger.Object);
            _orderRepository = new OrderRepository(_dbContext, _mockOrderLogger.Object);

            // Seed data from the JSON file
            var jsonFilePath = @"JsonInput\DepthChart.json";
            if (File.Exists(jsonFilePath))
            {
                string jsonData = File.ReadAllText(jsonFilePath);
                try
                {   
                    var depthChartDto = JsonSerializer.Deserialize<DepthChartDTO>(jsonData);
                    if(depthChartDto != null)
                    {
                        // Setup a sport
                        var sport = new Sport { Name = depthChartDto.Sport };
                        await _sportRepository.AddAsync(sport);
                        await _sportRepository.SaveChangesAsync();
                        
                        // Setup a teams
                        foreach (var teamDto in depthChartDto.Teams)
                        {
                            var team = new Team { Name = teamDto.Name, SportId = sport.SportId };
                            await _teamRepository.AddAsync(team);
                            await _teamRepository.SaveChangesAsync();

                            // Setup positions of the team
                            foreach (var positionDto in teamDto.Positions)
                            {
                                int playerId = -1, positionId = -1;

                                // Add the Position into the team if this is a new Position under the team
                                var positionItem = await _positionRepository.GetByPositionNameAndTeamIdAsync(positionDto.Name, team.TeamId);
                                if (!positionItem.Item2)
                                {
                                    throw new Exception($"Input JSON file is incorrect for a Position Name {positionDto.Name}.");
                                }
                                if (positionItem.Item1 == null)
                                {
                                    // A new Position
                                    var position = new Position { Name = positionDto.Name, TeamId = team.TeamId };
                                    await _positionRepository.AddAsync(position);
                                    await _positionRepository.SaveChangesAsync();
                                    positionId = position.PositionId;
                                }
                                else
                                {
                                    positionId = positionItem.Item1.PositionId;
                                }

                                foreach (var orderDto in positionDto.Orders)
                                {
                                    var playerDto = orderDto.PlayerDetails;

                                    // Add the Player into the team if this is a new Player under the team
                                    // Player Number is unique in a team
                                    var playerItem = await _playerRepository.GetByPlayerNumberAndTeamIdAsync(playerDto.Number, team.TeamId);
                                    if (!playerItem.Item2)
                                    {
                                        throw new Exception($"Input JSON file is incorrect for a Player Number {playerDto.Number}.");
                                    }
                                    if (playerItem.Item1 == null)
                                    {
                                        // A new Player
                                        var player = new Player { Number = playerDto.Number, Name = playerDto.Name, Odds = playerDto.Odds, TeamId = team.TeamId };
                                        await _playerRepository.AddAsync(player);
                                        await _playerRepository.SaveChangesAsync();
                                        playerId = player.PlayerId;
                                    }
                                    else
                                    {
                                        playerId = playerItem.Item1.PlayerId;
                                    }

                                    // Add the order
                                    var order = new Order { SeqNumber = orderDto.SeqNumber, PlayerId = playerId, PositionId = positionId };
                                    await _orderRepository.AddAsync(order);
                                    await _orderRepository.SaveChangesAsync();
                                }
                            }
                        }
                    }
                    else
                    {
                        throw new Exception($"Input JSON file is incorrect.");
                    }
                }
                catch(Exception ex)
                {
                    throw new Exception($"Input JSON file is incorrect. The exception messgage is: {ex.Message}");
                }
            }
            else
            {
                throw new Exception($"{jsonFilePath} JSON file is missing");
            }            
        }

        // Test case for Sport repository
        [TestMethod]
        public async Task TestSportRepositoryFromJsonInput()
        {
            await SetupInitialData();

            // Retrieve all sports
            var sports = await _sportRepository.GetAllAsync();
            Assert.AreEqual(1, sports.Count); // Expect one sport

            // Retrieve the first sport and check its name
            var sport = await _sportRepository.GetByIdAsync(sports[0].SportId);
            Assert.AreEqual("NFL", sport.Name);

            await _dbContext.DisposeAsync();
        }

        // Test case for Team repository
        [TestMethod]
        public async Task TestTeamRepositoryFromJsonInput()
        {
            await SetupInitialData();

            // Retrieve all teams
            var teams = await _teamRepository.GetAllAsync();
            Assert.AreEqual(1, teams.Count); // Expect one team

            // Retrieve the first team and check its name
            var team = await _teamRepository.GetByIdAsync(teams[0].TeamId);
            Assert.AreEqual("Tampa Bay Buccaneers", team.Name);

            await _dbContext.DisposeAsync();
        }

        // Test case for Player repository
        [TestMethod]
        public async Task TestPlayerRepositoryFromJsonInput()
        {
            await SetupInitialData();

            // Retrieve all players
            var players = await _playerRepository.GetAllAsync();
            Assert.AreEqual(6, players.Count); // Expect one player

            // Retrieve player and check its name
            var player1 = await _playerRepository.GetByIdAsync(players[0].PlayerId);
            Assert.AreEqual("Mike Evans", player1.Name);
            var player2 = await _playerRepository.GetByIdAsync(players[1].PlayerId);
            Assert.AreEqual("Jaelon Darden", player2.Name);
            var player3 = await _playerRepository.GetByIdAsync(players[2].PlayerId);
            Assert.AreEqual("Scott Miller", player3.Name);
            var player4 = await _playerRepository.GetByIdAsync(players[3].PlayerId);
            Assert.AreEqual("Donovan Smith", player4.Name);
            var player5 = await _playerRepository.GetByIdAsync(players[4].PlayerId);
            Assert.AreEqual("JOSH WELLS", player5.Name);
            var player6 = await _playerRepository.GetByIdAsync(players[5].PlayerId);
            Assert.AreEqual("Tristan Wirfs", player6.Name);

            await _dbContext.DisposeAsync();
        }

        // Test case for Position repository
        [TestMethod]
        public async Task TestPositionRepositoryFromJsonInput()
        {
            await SetupInitialData();

            var positions = await _positionRepository.GetAllAsync();
            Assert.AreEqual(3, positions.Count); // Expect one position

            // Retrieve position and check its name
            var position1 = await _positionRepository.GetByIdAsync(positions[0].PositionId);
            Assert.AreEqual("LWR", position1.Name);
            var position2 = await _positionRepository.GetByIdAsync(positions[1].PositionId);
            Assert.AreEqual("LT", position2.Name);
            var position3 = await _positionRepository.GetByIdAsync(positions[2].PositionId);
            Assert.AreEqual("RT", position3.Name);

            await _dbContext.DisposeAsync();
        }

        // Test case for Order repository
        [TestMethod]
        public async Task TestOrderRepositoryFromJsonInput()
        {
            await SetupInitialData();

            // Retrieve all orders
            var orders = await _orderRepository.GetAllAsync();
            Assert.AreEqual(7, orders.Count); // Expect two orders

            // Retrieve order and check its name
            var order1 = await _orderRepository.GetByIdAsync(orders[0].PositionId, orders[0].PlayerId);
            Assert.AreEqual(0, order1.SeqNumber);
            var order2 = await _orderRepository.GetByIdAsync(orders[1].PositionId, orders[1].PlayerId);
            Assert.AreEqual(1, order2.SeqNumber);
            var order3 = await _orderRepository.GetByIdAsync(orders[2].PositionId, orders[2].PlayerId);
            Assert.AreEqual(2, order3.SeqNumber);
            var order4 = await _orderRepository.GetByIdAsync(orders[3].PositionId, orders[3].PlayerId);
            Assert.AreEqual(0, order4.SeqNumber);
            var order5 = await _orderRepository.GetByIdAsync(orders[4].PositionId, orders[4].PlayerId);
            Assert.AreEqual(1, order5.SeqNumber);
            var order6 = await _orderRepository.GetByIdAsync(orders[5].PositionId, orders[5].PlayerId);
            Assert.AreEqual(0, order6.SeqNumber);
            var order7 = await _orderRepository.GetByIdAsync(orders[6].PositionId, orders[6].PlayerId);
            Assert.AreEqual(1, order7.SeqNumber);

            await _dbContext.DisposeAsync();
        }
    }
}