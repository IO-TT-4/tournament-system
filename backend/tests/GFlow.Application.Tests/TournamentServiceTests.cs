using Xunit;
using Moq;
using GFlow.Application.Services;
using GFlow.Application.Ports;
using GFlow.Application.DTOs;
using GFlow.Domain.Entities;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;

namespace GFlow.Application.Tests
{
    public class TournamentServiceTests
    {
        private readonly Mock<ITournamentRepository> _tournamentRepoMock;
        private readonly Mock<IUserRepository> _userRepoMock;
        private readonly Mock<IGeoLocationService> _geoServiceMock;
        private readonly Mock<IUserPreferenceService> _preferenceServiceMock;
        private readonly TournamentService _tournamentService;

        public TournamentServiceTests()
        {
            _tournamentRepoMock = new Mock<ITournamentRepository>();
            _userRepoMock = new Mock<IUserRepository>();
            _geoServiceMock = new Mock<IGeoLocationService>();
            _preferenceServiceMock = new Mock<IUserPreferenceService>();
            _tournamentService = new TournamentService(
                _tournamentRepoMock.Object, 
                _userRepoMock.Object,
                _geoServiceMock.Object,
                _preferenceServiceMock.Object);
        }


        [Fact]
        public async Task UpdateTournament_ShouldReturnUpdatedTournament_WhenValid()
        {
            // Arrange
            var id = "test_id";
            var tournament = new Tournament { Id = id, Name = "Old Name" };
            var request = new UpdateTournamentRequest { Name = "New Valid Name" };

            _tournamentRepoMock.Setup(x => x.GetTournament(id)).ReturnsAsync(tournament);
            _tournamentRepoMock.Setup(x => x.Update(It.IsAny<Tournament>())).ReturnsAsync((Tournament t) => t);

            // Act
            var result = await _tournamentService.UpdateTournamentAsync(id, request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("New Valid Name", result.Name);
            _tournamentRepoMock.Verify(x => x.Update(tournament), Times.Once);
        }

         [Fact]
        public async Task UpdateTournament_ShouldReturnNull_WhenTournamentNotFound()
        {
            // Arrange
            var id = "unknown";
            _tournamentRepoMock.Setup(x => x.GetTournament(id)).ReturnsAsync((Tournament?)null);

            // Act
            var result = await _tournamentService.UpdateTournamentAsync(id, new UpdateTournamentRequest { Name = "New" });

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task DeleteTournament_ShouldReturnTrue_WhenTournamentExists()
        {
            // Arrange
            var id = "exist";
            _tournamentRepoMock.Setup(x => x.Delete(id)).ReturnsAsync(true);

            // Act
            var result = await _tournamentService.DeleteTournamentAsync(id);

            // Assert
            Assert.True(result);
            _tournamentRepoMock.Verify(x => x.Delete(id), Times.Once);
        }

        [Fact]
        public async Task DeleteTournament_ShouldReturnFalse_WhenTournamentDoesNotExist()
        {
             // Arrange
            var id = "unknown";
            _tournamentRepoMock.Setup(x => x.Delete(id)).ReturnsAsync(false);

            // Act
            var result = await _tournamentService.DeleteTournamentAsync(id);

            // Assert
            Assert.False(result);
        }
    }
}
