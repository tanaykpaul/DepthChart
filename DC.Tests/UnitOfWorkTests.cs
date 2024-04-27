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

        [TestCleanup]
        public void Cleanup()
        {
            // Dispose the in-memory database
            _dbContext.Dispose();
        }
    }
}