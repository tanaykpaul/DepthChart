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
        public async Task GetFullDepthChart_ReturnsExpectedResult()
        {
            // Arrange
            int teamId = 1;
            SeedDatabaseTwoPositionsWithTwoOdersWithEachOrderHasTwoPlayers();

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

        private async void SeedDatabaseTwoPositionsWithTwoOdersWithEachOrderHasTwoPlayers()
        {
            var sport = new Sport { Name = "NFL" };
            await _dbContext.Sports.AddAsync(sport);

            var team = new Team { Name = "Tampa Bay Buccaneers", SportId = sport.SportId };
            await _dbContext.Teams.AddAsync(team);

            var positionLT = new Position { Name = "LT", TeamId = team.TeamId };
            var positionRT = new Position { Name = "RT", TeamId = team.TeamId };
            await _dbContext.Positions.AddRangeAsync([positionLT, positionRT]);

            var player76 = new Player { Number = 76, Name = "Donovan Smith" };
            var player72 = new Player { Number = 72, Name = "JOSH WELLS" };
            var player78 = new Player { Number = 78, Name = "Tristan Wirfs" };
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