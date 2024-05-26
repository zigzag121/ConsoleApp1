using ConsoleApp1.Interfaces;
using ConsoleApp1.Models;
using ConsoleApp1.Services;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace ConsoleApp1.Tests
{

    public class DepthChartServiceTests
    {
        private readonly Mock<IDepthChartRepository> _mockRepository;
        private readonly DepthChartService _service;

        public DepthChartServiceTests()
        {
            _mockRepository = new Mock<IDepthChartRepository>();
            var logger = Mock.Of<Microsoft.Extensions.Logging.ILogger<DepthChartService>>();
            _service = new DepthChartService(_mockRepository.Object, logger);
        }

        [Fact]
        public void AddPlayerToDepthChart_IncrementsPositionsOfLowerDepthPlayers()
        {
            // Arrange
            var player = new Player { PlayerId = 3, Number = 2, Name = "Kyle Trask" };
            var existingEntry1 = new DepthChartEntry { Position = "QB", PlayerId = 1, PositionDepth = 0 };
            var existingEntry2 = new DepthChartEntry { Position = "QB", PlayerId = 2, PositionDepth = 1 };
            _mockRepository.Setup(r => r.GetPlayerByNumber(player.Number)).Returns((IPlayer)null);
            _mockRepository.Setup(r => r.GetDepthChartEntries("QB")).Returns(new List<DepthChartEntry> { existingEntry1, existingEntry2 });
            _mockRepository.Setup(r => r.GetPlayer(player.PlayerId)).Returns((IPlayer)null);

            // Act
            _service.AddPlayerToDepthChart("QB", player, 1);

            // Assert
            _mockRepository.Verify(r => r.AddPlayer(player), Times.Once);
            _mockRepository.Verify(r => r.AddDepthChartEntry(It.Is<DepthChartEntry>(e => e.PositionDepth == 1 && e.PlayerId == player.PlayerId)), Times.Once);
            _mockRepository.Verify(r => r.SaveChanges(), Times.Once);

            // Ensure existing entries have been incremented
            Assert.Equal(2, existingEntry2.PositionDepth);
        }

        [Fact]
        public void RemovePlayerFromDepthChart_DecrementsPositionsOfLowerDepthPlayers()
        {
            // Arrange
            var player = new Player { PlayerId = 2, Number = 11, Name = "Blaine Gabbert" };
            var entryToRemove = new DepthChartEntry { Position = "QB", PlayerId = player.PlayerId, Player = player, PositionDepth = 1 };
            var remainingEntry1 = new DepthChartEntry { Position = "QB", PlayerId = 1, PositionDepth = 0 };
            var remainingEntry2 = new DepthChartEntry { Position = "QB", PlayerId = 3, PositionDepth = 2 };
            _mockRepository.Setup(r => r.GetDepthChartEntry("QB", player.PlayerId)).Returns(entryToRemove);
            _mockRepository.Setup(r => r.GetDepthChartEntries("QB")).Returns(new List<DepthChartEntry> { remainingEntry1, entryToRemove, remainingEntry2 });

            // Act
            var removedPlayer = _service.RemovePlayerFromDepthChart("QB", player);

            // Assert
            Assert.Equal(player, removedPlayer);
            _mockRepository.Verify(r => r.RemoveDepthChartEntry(entryToRemove), Times.Once);
            _mockRepository.Verify(r => r.SaveChanges(), Times.Once);

            // Ensure existing entries have been decremented
            Assert.Equal(1, remainingEntry2.PositionDepth);
        }

        [Fact]
        public void AddPlayerToDepthChart_AddsPlayerAtGivenPositionDepth()
        {
            // Arrange
            var player = new Player { PlayerId = 1, Number = 12, Name = "Tom Brady" };
            _mockRepository.Setup(r => r.GetPlayerByNumber(player.Number)).Returns((IPlayer)null);
            _mockRepository.Setup(r => r.GetDepthChartEntries("QB")).Returns(new List<DepthChartEntry>());
            _mockRepository.Setup(r => r.GetPlayer(player.PlayerId)).Returns((IPlayer)null);

            // Act
            _service.AddPlayerToDepthChart("QB", player, 0);

            // Assert
            _mockRepository.Verify(r => r.AddPlayer(player), Times.Once);
            _mockRepository.Verify(r => r.AddDepthChartEntry(It.IsAny<DepthChartEntry>()), Times.Once);
            _mockRepository.Verify(r => r.SaveChanges(), Times.Once);
        }

        [Fact]
        public void AddPlayerToDepthChart_AddsPlayerAtEndIfNoPositionDepthSpecified()
        {
            // Arrange
            var player = new Player { PlayerId = 1, Number = 12, Name = "Tom Brady" };
            var existingEntry = new DepthChartEntry { Position = "QB", PlayerId = 2, PositionDepth = 0 };
            _mockRepository.Setup(r => r.GetPlayerByNumber(player.Number)).Returns((IPlayer)null);
            _mockRepository.Setup(r => r.GetDepthChartEntries("QB")).Returns(new List<DepthChartEntry> { existingEntry });
            _mockRepository.Setup(r => r.GetPlayer(player.PlayerId)).Returns((IPlayer)null);

            // Act
            _service.AddPlayerToDepthChart("QB", player);

            // Assert
            _mockRepository.Verify(r => r.AddPlayer(player), Times.Once);
            _mockRepository.Verify(r => r.AddDepthChartEntry(It.Is<DepthChartEntry>(e => e.PositionDepth == 1)), Times.Once);
            _mockRepository.Verify(r => r.SaveChanges(), Times.Once);
        }

        [Fact]
        public void RemovePlayerFromDepthChart_RemovesPlayerAndReturnsPlayer()
        {
            // Arrange
            var player = new Player { PlayerId = 1, Number = 12, Name = "Tom Brady" };
            var entry = new DepthChartEntry { Position = "QB", PlayerId = player.PlayerId, Player = player, PositionDepth = 0 };
            _mockRepository.Setup(r => r.GetDepthChartEntry("QB", player.PlayerId)).Returns(entry);

            // Act
            var removedPlayer = _service.RemovePlayerFromDepthChart("QB", player);

            // Assert
            Assert.Equal(player, removedPlayer);
            _mockRepository.Verify(r => r.RemoveDepthChartEntry(entry), Times.Once);
            _mockRepository.Verify(r => r.SaveChanges(), Times.Once);
        }

        [Fact]
        public void RemovePlayerFromDepthChart_ReturnsNullIfPlayerNotInDepthChart()
        {
            // Arrange
            var player = new Player { PlayerId = 1, Number = 12, Name = "Tom Brady" };
            _mockRepository.Setup(r => r.GetDepthChartEntry("QB", player.PlayerId)).Returns((DepthChartEntry)null);

            // Act
            var removedPlayer = _service.RemovePlayerFromDepthChart("QB", player);

            // Assert
            Assert.Null(removedPlayer);
            _mockRepository.Verify(r => r.RemoveDepthChartEntry(It.IsAny<DepthChartEntry>()), Times.Never);
            _mockRepository.Verify(r => r.SaveChanges(), Times.Never);
        }

        [Fact]
        public void GetBackups_ReturnsAllBackupsForGivenPlayer()
        {
            // Arrange
            var starter = new Player { PlayerId = 1, Number = 12, Name = "Tom Brady" };
            var backup1 = new Player { PlayerId = 2, Number = 11, Name = "Blaine Gabbert" };
            var backup2 = new Player { PlayerId = 3, Number = 2, Name = "Kyle Trask" };
            var starterEntry = new DepthChartEntry { Position = "QB", PlayerId = starter.PlayerId, Player = starter, PositionDepth = 0 };
            var backupEntry1 = new DepthChartEntry { Position = "QB", PlayerId = backup1.PlayerId, Player = backup1, PositionDepth = 1 };
            var backupEntry2 = new DepthChartEntry { Position = "QB", PlayerId = backup2.PlayerId, Player = backup2, PositionDepth = 2 };
            _mockRepository.Setup(r => r.GetDepthChartEntry("QB", starter.PlayerId)).Returns(starterEntry);
            _mockRepository.Setup(r => r.GetDepthChartEntries("QB")).Returns(new List<DepthChartEntry> { starterEntry, backupEntry1, backupEntry2 });

            // Act
            var backups = _service.GetBackups("QB", starter);

            // Assert
            Assert.Contains(backup1, backups);
            Assert.Contains(backup2, backups);
        }

        [Fact]
        public void GetBackups_ReturnsEmptyListIfPlayerHasNoBackups()
        {
            // Arrange
            var starter = new Player { PlayerId = 1, Number = 12, Name = "Tom Brady" };
            var starterEntry = new DepthChartEntry { Position = "QB", PlayerId = starter.PlayerId, Player = starter, PositionDepth = 0 };
            _mockRepository.Setup(r => r.GetDepthChartEntry("QB", starter.PlayerId)).Returns(starterEntry);
            _mockRepository.Setup(r => r.GetDepthChartEntries("QB")).Returns(new List<DepthChartEntry> { starterEntry });

            // Act
            var backups = _service.GetBackups("QB", starter);

            // Assert
            Assert.Empty(backups);
        }

        [Fact]
        public void GetBackups_ReturnsEmptyListIfPlayerNotInDepthChart()
        {
            // Arrange
            var starter = new Player { PlayerId = 1, Number = 12, Name = "Tom Brady" };
            _mockRepository.Setup(r => r.GetDepthChartEntry("QB", starter.PlayerId)).Returns((DepthChartEntry)null);

            // Act
            var backups = _service.GetBackups("QB", starter);

            // Assert
            Assert.Empty(backups);
        }

        [Fact]
        public void GetFullDepthChart_ReturnsFullDepthChart()
        {
            // Arrange
            var qb1 = new Player { PlayerId = 1, Number = 12, Name = "Tom Brady" };
            var qb2 = new Player { PlayerId = 2, Number = 11, Name = "Blaine Gabbert" };
            var lwr1 = new Player { PlayerId = 3, Number = 13, Name = "Mike Evans" };
            var qbEntry1 = new DepthChartEntry { Position = "QB", PlayerId = qb1.PlayerId, Player = qb1, PositionDepth = 0 };
            var qbEntry2 = new DepthChartEntry { Position = "QB", PlayerId = qb2.PlayerId, Player = qb2, PositionDepth = 1 };
            var lwrEntry1 = new DepthChartEntry { Position = "LWR", PlayerId = lwr1.PlayerId, Player = lwr1, PositionDepth = 0 };
            _mockRepository.Setup(r => r.GetDepthChartEntries(null)).Returns(new List<DepthChartEntry> { qbEntry1, qbEntry2, lwrEntry1 });

            // Act
            var depthChart = _service.GetFullDepthChart();

            // Assert
            Assert.Contains("QB – #12, Tom Brady, #11, Blaine Gabbert", depthChart);
            Assert.Contains("LWR – #13, Mike Evans", depthChart);
        }
    }
}
