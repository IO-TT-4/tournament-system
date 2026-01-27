using Xunit;
using Moq;
using GFlow.Application.Services;
using GFlow.Application.Ports;
using GFlow.Application.DTOs;
using GFlow.Domain.Entities;
using GFlow.Domain.ValueObjects;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;

using DomainMatch = GFlow.Domain.Entities.Match;

namespace GFlow.Application.Tests
{
    public class MatchEventServiceTests
    {
        private readonly Mock<IMatchEventRepository> _eventRepoMock;
        private readonly Mock<ITournamentRepository> _tournamentRepoMock;
        private readonly Mock<IUserRepository> _userRepoMock;
        private readonly MatchEventService _service;

        public MatchEventServiceTests()
        {
            _eventRepoMock = new Mock<IMatchEventRepository>();
            _tournamentRepoMock = new Mock<ITournamentRepository>();
            _userRepoMock = new Mock<IUserRepository>();
            _service = new MatchEventService(
                _eventRepoMock.Object,
                _tournamentRepoMock.Object,
                _userRepoMock.Object
            );
        }

        [Fact]
        public async Task AddEventAsync_ShouldAddEvent_WhenAuthorized()
        {
            // Arrange
            var matchId = "match1";
            var userId = "user1";
            var tournamentId = "tour1";
            
            var match = new DomainMatch(matchId, "p1", "p2", 1, tournamentId);
            var tournament = new Tournament { Id = tournamentId, OrganizerId = userId, Name = "Test Tournament", Status = TournamentStatus.CREATED, SystemType = TournamentSystemType.SINGLE_ROUND_ROBIN };
            
            var request = new CreateMatchEventRequest 
            { 
                EventType = "YELLOW_CARD",
                Description = "Foul",
                MinuteOfPlay = 10
            };

            _tournamentRepoMock.Setup(x => x.GetMatchById(matchId)).ReturnsAsync(match);
            _tournamentRepoMock.Setup(x => x.GetTournament(tournamentId)).ReturnsAsync(tournament);
            _eventRepoMock.Setup(x => x.Add(It.IsAny<MatchEvent>()))
                .ReturnsAsync((MatchEvent e) => e);

            // Act
            var result = await _service.AddEventAsync(matchId, request, userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("YELLOW_CARD", result.EventType);
            Assert.Equal(userId, result.RecordedBy);
            _eventRepoMock.Verify(x => x.Add(It.IsAny<MatchEvent>()), Times.Once);
        }

        [Fact]
        public async Task AddEventAsync_ShouldUpdateScore_WhenGoalAdded()
        {
            // Arrange
            var matchId = "match1";
            var userId = "user1";
            var tournamentId = "tour1";
            var playerHomeId = "p1";
            var playerAwayId = "p2";
            
            var match = new DomainMatch(matchId, playerHomeId, playerAwayId, 1, tournamentId) 
            { 
                ScoreA = 0, 
                ScoreB = 0 
            };
            var tournament = new Tournament { Id = tournamentId, OrganizerId = userId, Name = "Test Tournament", Status = TournamentStatus.CREATED, SystemType = TournamentSystemType.SINGLE_ROUND_ROBIN };
            
            var request = new CreateMatchEventRequest 
            { 
                EventType = "GOAL",
                PlayerId = playerHomeId,
                MinuteOfPlay = 20
            };

            _tournamentRepoMock.Setup(x => x.GetMatchById(matchId)).ReturnsAsync(match);
            _tournamentRepoMock.Setup(x => x.GetTournament(tournamentId)).ReturnsAsync(tournament);
            _eventRepoMock.Setup(x => x.Add(It.IsAny<MatchEvent>()))
                .ReturnsAsync((MatchEvent e) => e);

            // Act
            var result = await _service.AddEventAsync(matchId, request, userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, match.ScoreA);
            Assert.Equal(0, match.ScoreB);
            _tournamentRepoMock.Verify(x => x.UpdateMatch(match), Times.Once);
        }

        [Fact]
        public async Task AddEventAsync_ShouldReturnNull_WhenUnauthorized()
        {
            // Arrange
            var matchId = "match1";
            var userId = "random_user"; // Not organizer
            var tournamentId = "tour1";
            
            var match = new DomainMatch(matchId, "p1", "p2", 1, tournamentId);
            var tournament = new Tournament { Id = tournamentId, OrganizerId = "organizer", Name = "Test Tournament", Status = TournamentStatus.CREATED, SystemType = TournamentSystemType.SINGLE_ROUND_ROBIN };
            
            var request = new CreateMatchEventRequest { EventType = "GOAL" };

            _tournamentRepoMock.Setup(x => x.GetMatchById(matchId)).ReturnsAsync(match);
            _tournamentRepoMock.Setup(x => x.GetTournament(tournamentId)).ReturnsAsync(tournament);

            // Act
            var result = await _service.AddEventAsync(matchId, request, userId);

            // Assert
            Assert.Null(result);
            _eventRepoMock.Verify(x => x.Add(It.IsAny<MatchEvent>()), Times.Never);
        }

        [Fact]
        public async Task GetMatchEventsAsync_ShouldReturnEvents_WithPlayerNames()
        {
            // Arrange
            var matchId = "match1";
            var playerId = "p1";
            var recorderId = "rec1";
            
            var events = new List<MatchEvent>
            {
                new MatchEvent(matchId, "GOAL", recorderId) { PlayerId = playerId, Id = "e1" }
            };
            
            var player = new User { Id = playerId, Username = "PlayerOne", Email = "p1@test.com", PasswordHash = "hash" };
            var recorder = new User { Id = recorderId, Username = "Recorder", Email = "rec@test.com", PasswordHash = "hash" };

            _eventRepoMock.Setup(x => x.GetByMatch(matchId)).ReturnsAsync(events);
            _userRepoMock.Setup(x => x.Get(playerId)).ReturnsAsync(player);
            _userRepoMock.Setup(x => x.Get(recorderId)).ReturnsAsync(recorder);


            // Act
            var result = await _service.GetMatchEventsAsync(matchId);

            // Assert
            Assert.Single(result);
            Assert.Equal("PlayerOne", result[0].PlayerName);
            Assert.Equal("Recorder", result[0].RecordedByName);
        }
    }
}
