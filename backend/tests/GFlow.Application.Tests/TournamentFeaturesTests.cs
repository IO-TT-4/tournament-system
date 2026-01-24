using Xunit;
using Moq;
using GFlow.Application.Services;
using GFlow.Application.Ports;
using GFlow.Application.DTOs;
using GFlow.Domain.Entities;
using GFlow.Domain.ValueObjects;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using System.Linq;

namespace GFlow.Application.Tests
{
    public class TournamentFeaturesTests
    {
        private readonly Mock<ITournamentRepository> _tournamentRepoMock;
        private readonly Mock<IUserRepository> _userRepoMock;
        private readonly Mock<IGeoLocationService> _geoServiceMock;
        private readonly Mock<IUserPreferenceService> _preferenceServiceMock;
        private readonly TournamentService _tournamentService;
        
        public TournamentFeaturesTests()
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
        public async Task WithdrawParticipant_ShouldReturnTrue_WhenParticipantExists()
        {
            // Arrange
            var tId = "t1";
            var uId = "u1";
            var participant = new TournamentParticipant(uId, 1000) { TournamentId = tId };

            _tournamentRepoMock.Setup(x => x.GetParticipant(tId, uId)).ReturnsAsync(participant);
            _tournamentRepoMock.Setup(x => x.UpdateParticipant(It.IsAny<TournamentParticipant>())).ReturnsAsync(true);

            // Act
            var result = await _tournamentService.WithdrawParticipantAsync(tId, uId);

            // Assert
            Assert.True(result);
            Assert.True(participant.IsWithdrawn);
            _tournamentRepoMock.Verify(x => x.UpdateParticipant(participant), Times.Once);
        }

        /*
        [Fact]
        public async Task SubmitMatchResult_ShouldUpdateMatch_WhenMatchExists()
        {
            // ...
        }

        [Fact]
        public async Task GetStandings_ShouldCalculateCorrectly()
        {
            // ...
        }
        */
    }
}
