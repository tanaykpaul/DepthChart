using DC.Domain.Entities;
using DC.Domain.Logging;
using DC.Infrastructure.Data;
using DC.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace DC.Tests
{
    [TestClass]
    public class DepthChartTests
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
                .UseInMemoryDatabase(databaseName: "DepthChartTestsDatabase")
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

            // Setup a sport
            var sport = new Sport { Name = "Football" };
            await _sportRepository.AddAsync(sport);
            await _sportRepository.SaveChangesAsync();

            // Setup a team
            var team = new Team { Name = "Team A", SportId = sport.SportId };
            await _teamRepository.AddAsync(team);
            await _teamRepository.SaveChangesAsync();

            // Setup a player
            var player = new Player { Name = "Player 1", Number = 1, TeamId = team.TeamId };
            await _playerRepository.AddAsync(player);
            await _playerRepository.SaveChangesAsync();

            // Setup a position
            var position = new Position { Name = "Forward", TeamId = team.TeamId };
            await _positionRepository.AddAsync(position);
            await _positionRepository.SaveChangesAsync();

            // Setup orders for player and position
            var order1 = new Order { SeqNumber = 1, PlayerId = player.PlayerId, PositionId = position.PositionId };
            
            await _orderRepository.AddAsync(order1);
            await _orderRepository.SaveChangesAsync();
        }

        // Test case for Sport repository
        [TestMethod]
        public async Task TestSportRepository()
        {
            await SetupInitialData();

            // Retrieve all sports
            var sports = await _sportRepository.GetAllAsync();
            Assert.AreEqual(1, sports.Count); // Expect one sport

            // Retrieve the first sport and check its name
            var sport = await _sportRepository.GetByIdAsync(sports[0].SportId);
            Assert.AreEqual("Football", sport.Name);

            await _dbContext.DisposeAsync();
        }

        // Test case for Team repository
        [TestMethod]
        public async Task TestTeamRepository()
        {
            await SetupInitialData();

            // Retrieve all teams
            var teams = await _teamRepository.GetAllAsync();
            Assert.AreEqual(1, teams.Count); // Expect one team

            // Retrieve the first team and check its name
            var team = await _teamRepository.GetByIdAsync(teams[0].TeamId);
            Assert.AreEqual("Team A", team.Name);

            await _dbContext.DisposeAsync();
        }

        // Test case for Player repository
        [TestMethod]
        public async Task TestPlayerRepository()
        {
            await SetupInitialData();

            // Retrieve all players
            var players = await _playerRepository.GetAllAsync();
            Assert.AreEqual(1, players.Count); // Expect one player

            // Retrieve the first player and check its name
            var player = await _playerRepository.GetByIdAsync(players[0].PlayerId);
            Assert.AreEqual("Player 1", player.Name);

            await _dbContext.DisposeAsync();
        }

        // Test case for Position repository
        [TestMethod]
        public async Task TestPositionRepository()
        {
            await SetupInitialData();

            var positions = await _positionRepository.GetAllAsync();
            Assert.AreEqual(1, positions.Count); // Expect one position

            // Retrieve the first position and check its name
            var position = await _positionRepository.GetByIdAsync(positions[0].PositionId);
            Assert.AreEqual("Forward", position.Name);

            await _dbContext.DisposeAsync();
        }

        // Test case for Order repository
        [TestMethod]
        public async Task TestOrderRepository()
        {
            await SetupInitialData();

            // Retrieve all orders
            var orders = await _orderRepository.GetAllAsync();
            Assert.AreEqual(1, orders.Count); // Expect two orders

            // Retrieve the first order and check its name
            var order = await _orderRepository.GetByIdAsync(orders[0].PositionId, orders[0].PlayerId);
            Assert.AreEqual(1, order.SeqNumber);

            await _dbContext.DisposeAsync();
        }
    }
}