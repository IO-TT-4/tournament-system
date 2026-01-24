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
    public class ModeratorTests
    {
        private readonly Mock<ITournamentRepository> _tournamentRepoMock;
        private readonly Mock<IUserRepository> _userRepoMock;
        private readonly Mock<IGeoLocationService> _geoServiceMock;
        private readonly Mock<IUserPreferenceService> _preferenceServiceMock;
        private readonly TournamentService _tournamentService;

        public ModeratorTests()
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
        public async Task AddModerator_ShouldAddUserToModerators_WhenValid()
        {
            // Arrange
            var tId = "t1";
            var uId = "u1";
            var tournament = new Tournament { Id = tId, Name = "Test" };
            var user = new User { Id = uId, Username = "Mod", PasswordHash = "hash" };

            _tournamentRepoMock.Setup(x => x.GetTournament(tId)).ReturnsAsync(tournament);
            _userRepoMock.Setup(x => x.Get(uId)).ReturnsAsync(user);
            _tournamentRepoMock.Setup(x => x.Update(tournament)).ReturnsAsync(tournament);

            // Act
            var result = await _tournamentService.AddModeratorAsync(tId, uId);

            // Assert
            Assert.True(result);
            Assert.Contains(user, tournament.Moderators);
            _tournamentRepoMock.Verify(x => x.Update(tournament), Times.Once);
        }

        [Fact]
        public async Task SubmitResult_ShouldSucceed_WhenUserIsOrganizer()
        {
            // Arrange
            var mId = "m1";
            var tId = "t1";
            var orgId = "org1";
            
            var match = new GFlow.Domain.Entities.Match(mId, "p1", "p2", 1, tId);
            var tournament = new Tournament { Id = tId, Name = "Test", OrganizerId = orgId };
            
            _tournamentRepoMock.Setup(x => x.GetMatchById(mId)).ReturnsAsync(match);
            _tournamentRepoMock.Setup(x => x.GetTournament(tId)).ReturnsAsync(tournament);
            _tournamentRepoMock.Setup(x => x.UpdateMatch(match)).Returns(Task.CompletedTask);

            // Act
            var result = await _tournamentService.SubmitMatchResultAsync(mId, 1, 0, orgId);

            // Assert
            Assert.True(result);
            _tournamentRepoMock.Verify(x => x.UpdateMatch(match), Times.Once);
        }

         [Fact]
        public async Task SubmitResult_ShouldSucceed_WhenUserIsModerator()
        {
            // Arrange
            var mId = "m1";
            var tId = "t1";
            var modId = "mod1";
            
            var match = new GFlow.Domain.Entities.Match(mId, "p1", "p2", 1, tId);
            var modUser = new User { Id = modId, Username = "Mod", PasswordHash = "hash" };
            var tournament = new Tournament { Id = tId, Name = "Test", OrganizerId = "org1" };
            tournament.Moderators.Add(modUser);
            
            _tournamentRepoMock.Setup(x => x.GetMatchById(mId)).ReturnsAsync(match);
            _tournamentRepoMock.Setup(x => x.GetTournament(tId)).ReturnsAsync(tournament);
            _tournamentRepoMock.Setup(x => x.UpdateMatch(match)).Returns(Task.CompletedTask);

            // Act
            var result = await _tournamentService.SubmitMatchResultAsync(mId, 1, 0, modId);

            // Assert
            Assert.True(result);
            _tournamentRepoMock.Verify(x => x.UpdateMatch(match), Times.Once);
        }

        [Fact]
        public async Task SubmitResult_ShouldFail_WhenUserHasNoPermission()
        {
            // Arrange
            var mId = "m1";
            var tId = "t1";
            var randomId = "random";
            
            var match = new GFlow.Domain.Entities.Match(mId, "p1", "p2", 1, tId);
            var tournament = new Tournament { Id = tId, Name = "Test", OrganizerId = "org1" };
            
            _tournamentRepoMock.Setup(x => x.GetMatchById(mId)).ReturnsAsync(match);
            _tournamentRepoMock.Setup(x => x.GetTournament(tId)).ReturnsAsync(tournament);

            // Act
            var result = await _tournamentService.SubmitMatchResultAsync(mId, 1, 0, randomId);

            // Assert
            Assert.False(result);
            _tournamentRepoMock.Verify(x => x.UpdateMatch(It.IsAny<GFlow.Domain.Entities.Match>()), Times.Never);
        }
    }
}
