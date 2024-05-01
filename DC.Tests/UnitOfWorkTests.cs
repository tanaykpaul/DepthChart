using AutoMapper;
using DC.Application.Mapping;
using DC.Application.Services;
using DC.Domain.Entities;
using DC.Domain.Logging;
using DC.Infrastructure.Data;
using DC.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace DC.Tests
{
    [TestClass]
    public class UnitOfWorkTests
    {
        private DepthChartDbContext _dbContext;
        private UnitOfWork _unitOfWork;
        private Mock<IAppLogger> _mockLogger;
        private IMapper _mockMapper;

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

            _mockLogger = new Mock<IAppLogger>();
            _unitOfWork = new UnitOfWork(_dbContext, _mockLogger.Object);

            // Configure AutoMapper with the MappingProfile
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<MappingProfile>();
            });
            _mockMapper = config.CreateMapper();
        }

        [TestMethod]
        public void Repositories_AreAccessibleAndNotNull()
        {
            // Assert that the repositories are accessible and not null
            Assert.IsNotNull(_unitOfWork.SportRepository);
            Assert.IsNotNull(_unitOfWork.TeamRepository);
            Assert.IsNotNull(_unitOfWork.PositionRepository);
            Assert.IsNotNull(_unitOfWork.PlayerRepository);
            Assert.IsNotNull(_unitOfWork.OrderRepository);
        }

        [TestMethod]
        public async Task SaveChangesAsync_CallsDbContextSaveChangesAsync()
        {
            // Act
            // Call the SaveChangesAsync method and capture the result
            int changes = await _unitOfWork.SaveChangesAsync();

            // Assert
            // Since the database is empty and we're just testing SaveChangesAsync, 
            // the expected result is 0 (no changes).
            Assert.AreEqual(0, changes);

            // You can add some data to the in-memory database to test the SaveChangesAsync method more thoroughly.
            // For instance, you could add a new Sport entity to the context and save changes.

            // Add a new Sport entity
            _dbContext.Sports.Add(new Sport { Name = "Soccer" });
            // Save changes using the method under test
            changes = await _unitOfWork.SaveChangesAsync();

            // Assert that changes were saved successfully
            Assert.AreEqual(1, changes);
        }

        [TestMethod]
        public async Task AddPlayerToDepthChart_ReturnsExpectedResult()
        {
            // Arrange
            await SeedDatabaseTwoPositionsAndSixPlayers();

            // Act
            await _unitOfWork.AddPlayerToDepthChart("QB", 12, 0);
            await _unitOfWork.AddPlayerToDepthChart("QB", 11, 1);
            await _unitOfWork.AddPlayerToDepthChart("QB", 2, 2);

            await _unitOfWork.AddPlayerToDepthChart("LWR", 13, 0);
            await _unitOfWork.AddPlayerToDepthChart("LWR", 1, 1);
            await _unitOfWork.AddPlayerToDepthChart("LWR", 10, 2);

            // Assert
            Assert.AreEqual(6, _dbContext.Orders.Count());
            Assert.AreEqual(1, _dbContext.Orders.First().PositionId);
            Assert.AreEqual(1, _dbContext.Orders.First().PlayerId);
            Assert.AreEqual(2, _dbContext.Orders.Last().PositionId);
            Assert.AreEqual(6, _dbContext.Orders.Last().PlayerId);
        }

        [TestMethod]
        public async Task RemovePlayerFromDepthChartAbsent_ReturnsExpectedEmptyResult()
        {
            // Arrange
            await SeedDatabaseTwoPositionsAndSixPlayers();

            await _unitOfWork.AddPlayerToDepthChart("QB", 12, 0);
            await _unitOfWork.AddPlayerToDepthChart("QB", 11, 1);
            await _unitOfWork.AddPlayerToDepthChart("QB", 2, 2);

            await _unitOfWork.AddPlayerToDepthChart("LWR", 13, 0);
            await _unitOfWork.AddPlayerToDepthChart("LWR", 1, 1);
            await _unitOfWork.AddPlayerToDepthChart("LWR", 10, 2);

            // Act
            var result = await _unitOfWork.RemovePlayerFromDepthChart("LWR", 12);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);            
        }

        [TestMethod]
        public async Task AddPlayerToDepthChartWithoutDepthPosition_ReturnsExpectedResult()
        {
            // Arrange
            var teamId = 1;
            await SeedDatabaseTwoPositionsAndSixPlayers();

            await _unitOfWork.AddPlayerToDepthChart("QB", 12, null);
            await _unitOfWork.AddPlayerToDepthChart("QB", 11, null);
            await _unitOfWork.AddPlayerToDepthChart("QB", 2, null);

            await _unitOfWork.AddPlayerToDepthChart("LWR", 13, null);
            await _unitOfWork.AddPlayerToDepthChart("LWR", 1, null);
            await _unitOfWork.AddPlayerToDepthChart("LWR", 10, null);

            // Act
            IDictionary<string, List<(int, string)>> result1 = await _unitOfWork.GetFullDepthChart(teamId);

            // Assert
            Assert.IsNotNull(result1);
            Assert.AreEqual(2, result1.Count);

            Assert.AreEqual("QB", result1.First().Key);
            Assert.AreEqual(3, result1.First().Value.Count);

            Assert.AreEqual(12, result1.First().Value[0].Item1);
            Assert.AreEqual("Tom Brady", result1.First().Value[0].Item2);
            Assert.AreEqual(11, result1.First().Value[1].Item1);
            Assert.AreEqual("Blaine Gabbert", result1.First().Value[1].Item2);
            Assert.AreEqual(2, result1.First().Value[2].Item1);
            Assert.AreEqual("Kyle Trask", result1.First().Value[2].Item2);

            Assert.AreEqual("LWR", result1.Last().Key);
            Assert.AreEqual(3, result1.Last().Value.Count);

            Assert.AreEqual(13, result1.Last().Value[0].Item1);
            Assert.AreEqual("Mike Evans", result1.Last().Value[0].Item2);
            Assert.AreEqual(1, result1.Last().Value[1].Item1);
            Assert.AreEqual("Jaelon Darden", result1.Last().Value[1].Item2);
            Assert.AreEqual(10, result1.Last().Value[2].Item1);
            Assert.AreEqual("Scott Miller", result1.Last().Value[2].Item2);
        }

        [TestMethod]
        public async Task AddPlayerToDepthChartInOccupiedDepthPosition_ReturnsExpectedResult()
        {
            // Arrange
            var teamId = 1;
            await SeedDatabaseTwoPositionsAndSixPlayers();

            await _unitOfWork.AddPlayerToDepthChart("QB", 12, 0);
            await _unitOfWork.AddPlayerToDepthChart("QB", 11, 1);
            await _unitOfWork.AddPlayerToDepthChart("QB", 2, 0);

            await _unitOfWork.AddPlayerToDepthChart("LWR", 13, 1);
            await _unitOfWork.AddPlayerToDepthChart("LWR", 1, 0);
            await _unitOfWork.AddPlayerToDepthChart("LWR", 10, null);

            // Act
            IDictionary<string, List<(int, string)>> result1 = await _unitOfWork.GetFullDepthChart(teamId);

            // Assert
            Assert.IsNotNull(result1);
            Assert.AreEqual(2, result1.Count);

            Assert.AreEqual("QB", result1.First().Key);
            Assert.AreEqual(3, result1.First().Value.Count);

            Assert.AreEqual(12, result1.First().Value[1].Item1);
            Assert.AreEqual("Tom Brady", result1.First().Value[1].Item2);
            Assert.AreEqual(11, result1.First().Value[2].Item1);
            Assert.AreEqual("Blaine Gabbert", result1.First().Value[2].Item2);
            Assert.AreEqual(2, result1.First().Value[0].Item1);
            Assert.AreEqual("Kyle Trask", result1.First().Value[0].Item2);

            Assert.AreEqual("LWR", result1.Last().Key);
            Assert.AreEqual(3, result1.Last().Value.Count);

            Assert.AreEqual(13, result1.Last().Value[1].Item1);
            Assert.AreEqual("Mike Evans", result1.Last().Value[1].Item2);
            Assert.AreEqual(1, result1.Last().Value[0].Item1);
            Assert.AreEqual("Jaelon Darden", result1.Last().Value[0].Item2);
            Assert.AreEqual(10, result1.Last().Value[2].Item1);
            Assert.AreEqual("Scott Miller", result1.Last().Value[2].Item2);
        }

        [TestMethod]
        public async Task AddPlayerToDepthChartExerciseSampleActions_ReturnsExpectedResult()
        {
            // Arrange
            var teamId = 1;
            await SeedDatabaseTwoPositionsAndSixPlayers();

            await _unitOfWork.AddPlayerToDepthChart("QB", 12, 0);
            await _unitOfWork.AddPlayerToDepthChart("QB", 11, 1);
            await _unitOfWork.AddPlayerToDepthChart("QB", 2, 2);

            await _unitOfWork.AddPlayerToDepthChart("LWR", 13, 0);
            await _unitOfWork.AddPlayerToDepthChart("LWR", 1, 1);
            await _unitOfWork.AddPlayerToDepthChart("LWR", 10, 2);

            await TestDataExactlyToTheDepthChartJsonFile(teamId);
        }

        [TestMethod]
        public async Task TestGetDataFromJsonStringContents_ReturnsExpectedResult()
        {
            // Arrange
            var jsonFilePath = @"JsonInput\DepthChart.json";
            string jsonData = File.ReadAllText(jsonFilePath);

            // Act
            var service = new STP2FromJSON(_mockLogger.Object, _mockMapper);
            var sport = service.GetData(jsonData);

            // Assert
            Assert.IsNotNull(sport);
            Assert.IsNotNull(sport.Item1);
            Assert.IsNotNull(sport.Item2);
            Assert.IsNotNull(sport.Item2.Teams);
            Assert.AreEqual(1, sport.Item2.Teams.Count());

            await _unitOfWork.SportRepository.AddAsync(sport.Item1);
            await _unitOfWork.SportRepository.SaveChangesAsync();

            var ordersDTO = sport.Item2.Teams.First().Orders;
            var teamId = sport.Item1.Teams.First().TeamId;

            Assert.IsNotNull(ordersDTO);
            Assert.AreEqual(1, teamId);

            // Act
            foreach (var orderDTO in ordersDTO)
            {
                await _unitOfWork.AddPlayerToDepthChart(orderDTO.PositionName, orderDTO.PlayerNumber, orderDTO.SeqNumber, teamId);
            }

            // All Asserts
            await TestDataExactlyToTheDepthChartJsonFile(teamId);
        }

        [TestMethod]
        public async Task GetBackupsAbsentPlayer_ReturnsExpectedEmptyResult()
        {
            // Arrange
            await SeedDatabaseTwoPositionsAndSixPlayers();

            await _unitOfWork.AddPlayerToDepthChart("QB", 12, 0);
            await _unitOfWork.AddPlayerToDepthChart("QB", 11, 1);
            await _unitOfWork.AddPlayerToDepthChart("QB", 2, 2);

            await _unitOfWork.AddPlayerToDepthChart("LWR", 13, 0);
            await _unitOfWork.AddPlayerToDepthChart("LWR", 1, 1);
            await _unitOfWork.AddPlayerToDepthChart("LWR", 10, 2);

            // Act
            var result = await _unitOfWork.GetBackups("LWR", 12);
            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public async Task GetBackups_ReturnsExpectedResult()
        {
            // Arrange
            await SeedDatabaseTwoPositionsWithTwoOdersWithEachOrderHasTwoPlayers();

            // Act
            List<(int, string)> result1LT = await _unitOfWork.GetBackups("LT", 76);
            List<(int, string)> result2LT = await _unitOfWork.GetBackups("LT", 72);
            List<(int, string)> result1RT = await _unitOfWork.GetBackups("RT", 78);
            List<(int, string)> result2RT = await _unitOfWork.GetBackups("RT", 72);

            // Assert
            Assert.IsNotNull(result1LT);
            Assert.AreEqual(1, result1LT.Count);
            Assert.AreEqual(72, result1LT.First().Item1);
            Assert.AreEqual("JOSH WELLS", result1LT.First().Item2);

            Assert.IsNotNull(result2LT);
            Assert.AreEqual(0, result2LT.Count);

            Assert.IsNotNull(result1RT);
            Assert.AreEqual(1, result1RT.Count);
            Assert.AreEqual(72, result1RT.First().Item1);
            Assert.AreEqual("JOSH WELLS", result1RT.First().Item2);

            Assert.IsNotNull(result2RT);
            Assert.AreEqual(0, result2RT.Count);            
        }

        [TestMethod]
        public async Task GetFullDepthChart_ReturnsExpectedResult()
        {
            // Arrange
            int teamId = 1;
            await SeedDatabaseTwoPositionsWithTwoOdersWithEachOrderHasTwoPlayers();

            // Act
            IDictionary<string, List<(int, string)>> result = await _unitOfWork.GetFullDepthChart(teamId);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count);

            Assert.AreEqual("LT", result.First().Key);
            Assert.AreEqual(2, result.First().Value.Count);

            Assert.AreEqual(76, result.First().Value[0].Item1);
            Assert.AreEqual("Donovan Smith", result.First().Value[0].Item2);
            Assert.AreEqual(72, result.First().Value[1].Item1);
            Assert.AreEqual("JOSH WELLS", result.First().Value[1].Item2);

            Assert.AreEqual("RT", result.Last().Key);
            Assert.AreEqual(2, result.Last().Value.Count);

            Assert.AreEqual(78, result.Last().Value[0].Item1);
            Assert.AreEqual("Tristan Wirfs", result.Last().Value[0].Item2);
            Assert.AreEqual(72, result.Last().Value[1].Item1);
            Assert.AreEqual("JOSH WELLS", result.Last().Value[1].Item2);
        }

        

        [TestCleanup]
        public void Cleanup()
        {
            // Dispose the in-memory database
            _dbContext.Dispose();
        }

        private async Task TestDataExactlyToTheDepthChartJsonFile(int teamId)
        {
            // Act
            var result = await _unitOfWork.GetBackups("QB", 12);
            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual(11, result.First().Item1);
            Assert.AreEqual("Blaine Gabbert", result.First().Item2);
            Assert.AreEqual(2, result.Last().Item1);
            Assert.AreEqual("Kyle Trask", result.Last().Item2);

            // Act
            result = await _unitOfWork.GetBackups("LWR", 1);
            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(10, result.First().Item1);
            Assert.AreEqual("Scott Miller", result.First().Item2);

            // Act
            result = await _unitOfWork.GetBackups("LWR", 10);
            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);

            // Act
            result = await _unitOfWork.GetBackups("QB", 11);
            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(2, result.First().Item1);
            Assert.AreEqual("Kyle Trask", result.First().Item2);

            // Act
            result = await _unitOfWork.GetBackups("QB", 2);
            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);

            // Act
            IDictionary<string, List<(int, string)>> result1 = await _unitOfWork.GetFullDepthChart(teamId);

            // Assert
            Assert.IsNotNull(result1);
            Assert.AreEqual(2, result1.Count);

            Assert.AreEqual("QB", result1.First().Key);
            Assert.AreEqual(3, result1.First().Value.Count);

            Assert.AreEqual(12, result1.First().Value[0].Item1);
            Assert.AreEqual("Tom Brady", result1.First().Value[0].Item2);
            Assert.AreEqual(11, result1.First().Value[1].Item1);
            Assert.AreEqual("Blaine Gabbert", result1.First().Value[1].Item2);
            Assert.AreEqual(2, result1.First().Value[2].Item1);
            Assert.AreEqual("Kyle Trask", result1.First().Value[2].Item2);

            Assert.AreEqual("LWR", result1.Last().Key);
            Assert.AreEqual(3, result1.Last().Value.Count);

            Assert.AreEqual(13, result1.Last().Value[0].Item1);
            Assert.AreEqual("Mike Evans", result1.Last().Value[0].Item2);
            Assert.AreEqual(1, result1.Last().Value[1].Item1);
            Assert.AreEqual("Jaelon Darden", result1.Last().Value[1].Item2);
            Assert.AreEqual(10, result1.Last().Value[2].Item1);
            Assert.AreEqual("Scott Miller", result1.Last().Value[2].Item2);

            // Act
            result = await _unitOfWork.RemovePlayerFromDepthChart("LWR", 13);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(13, result.First().Item1);
            Assert.AreEqual("Mike Evans", result.First().Item2);

            // Act
            result1 = await _unitOfWork.GetFullDepthChart(teamId);

            // Assert
            Assert.IsNotNull(result1);
            Assert.AreEqual(2, result1.Count);

            Assert.AreEqual("QB", result1.First().Key);
            Assert.AreEqual(3, result1.First().Value.Count);

            Assert.AreEqual(12, result1.First().Value[0].Item1);
            Assert.AreEqual("Tom Brady", result1.First().Value[0].Item2);
            Assert.AreEqual(11, result1.First().Value[1].Item1);
            Assert.AreEqual("Blaine Gabbert", result1.First().Value[1].Item2);
            Assert.AreEqual(2, result1.First().Value[2].Item1);
            Assert.AreEqual("Kyle Trask", result1.First().Value[2].Item2);

            Assert.AreEqual("LWR", result1.Last().Key);
            Assert.AreEqual(2, result1.Last().Value.Count);

            Assert.AreEqual(1, result1.Last().Value[0].Item1);
            Assert.AreEqual("Jaelon Darden", result1.Last().Value[0].Item2);
            Assert.AreEqual(10, result1.Last().Value[1].Item1);
            Assert.AreEqual("Scott Miller", result1.Last().Value[1].Item2);
        }

        private async Task SeedDatabaseTwoPositionsAndSixPlayers()
        {
            var sport = new Sport { Name = "NFL" };
            await _dbContext.Sports.AddAsync(sport);

            var team = new Team { Name = "Tampa Bay Buccaneers", SportId = sport.SportId };
            await _dbContext.Teams.AddAsync(team);

            var positionQB = new Position { Name = "QB", TeamId = team.TeamId };
            var positionLWR = new Position { Name = "LWR", TeamId = team.TeamId };
            await _dbContext.Positions.AddRangeAsync([positionQB, positionLWR]);

            var player12 = new Player { Number = 12, Name = "Tom Brady", TeamId = team.TeamId };
            var player11 = new Player { Number = 11, Name = "Blaine Gabbert", TeamId = team.TeamId };
            var player2 = new Player { Number = 2, Name = "Kyle Trask", TeamId = team.TeamId };
            var player13 = new Player { Number = 13, Name = "Mike Evans", TeamId = team.TeamId };
            var player1 = new Player { Number = 1, Name = "Jaelon Darden", TeamId = team.TeamId };
            var player10 = new Player { Number = 10, Name = "Scott Miller", TeamId = team.TeamId };

            await _dbContext.Players.AddRangeAsync([player12, player11, player2, player13, player1, player10]);

            await _dbContext.SaveChangesAsync();
        }

        private async Task SeedDatabaseTwoPositionsWithTwoOdersWithEachOrderHasTwoPlayers()
        {
            var sport = new Sport { Name = "NFL" };
            await _dbContext.Sports.AddAsync(sport);

            var team = new Team { Name = "Tampa Bay Buccaneers", SportId = sport.SportId };
            await _dbContext.Teams.AddAsync(team);

            var positionLT = new Position { Name = "LT", TeamId = team.TeamId };
            var positionRT = new Position { Name = "RT", TeamId = team.TeamId };
            await _dbContext.Positions.AddRangeAsync([positionLT, positionRT]);

            var player76 = new Player { Number = 76, Name = "Donovan Smith", TeamId = team.TeamId };
            var player72 = new Player { Number = 72, Name = "JOSH WELLS", TeamId = team.TeamId };
            var player78 = new Player { Number = 78, Name = "Tristan Wirfs", TeamId = team.TeamId };
            await _dbContext.Players.AddRangeAsync([player76, player72, player78]);

            var order0_LT = new Order { SeqNumber = 0, PlayerId = player76.PlayerId, PositionId = positionLT.PositionId };
            var order1_LT = new Order { SeqNumber = 1, PlayerId = player72.PlayerId, PositionId = positionLT.PositionId };
            var order0_RT = new Order { SeqNumber = 0, PlayerId = player78.PlayerId, PositionId = positionRT.PositionId };
            var order1_RT = new Order { SeqNumber = 1, PlayerId = player72.PlayerId, PositionId = positionRT.PositionId };
            await _dbContext.Orders.AddRangeAsync([order0_LT, order1_LT, order0_RT, order1_RT]);

            await _dbContext.SaveChangesAsync();
        }
    }
}