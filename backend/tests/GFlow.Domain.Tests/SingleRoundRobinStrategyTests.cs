using Xunit;
using GFlow.Domain.Services.Pairings;
using GFlow.Domain.Entities;
using GFlow.Domain.ValueObjects;
using System.Collections.Generic;
using System.Linq;
using System;

namespace GFlow.Domain.Tests
{
    public class SingleRoundRobinStrategyTests
    {
        private readonly SingleRoundRobinStrategy _strategy;

        public SingleRoundRobinStrategyTests()
        {
            _strategy = new SingleRoundRobinStrategy();
        }

        [Fact]
        public void GenerateNextRound_ShouldGenerateAllRounds_WhenNoMatchesExist()
        {
            // Arrange
            var tournament = new Tournament { Name = "Test" };
            var participants = new List<TournamentParticipant>
            {
                new TournamentParticipant("u1", 1000),
                new TournamentParticipant("u2", 1000),
                new TournamentParticipant("u3", 1000),
                new TournamentParticipant("u4", 1000)
            };
            
            // Act
            var matches = _strategy.GenerateNextRound(tournament, participants, new List<Match>()).ToList();
            
            // Assert
            // 4 players -> 3 rounds.
            // Matches per round = 2. Total matches = 6.
            Assert.Equal(6, matches.Count);
            Assert.Equal(3, matches.Max(m => m.RoundNumber));
            
            // Verify each player plays every other player ONCE.
            // Total pairings = N*(N-1)/2 = 4*3/2 = 6.
            var pairings = matches.Select(m => 
                string.Compare(m.PlayerHomeId, m.PlayerAwayId) < 0 
                ? $"{m.PlayerHomeId}-{m.PlayerAwayId}" 
                : $"{m.PlayerAwayId}-{m.PlayerHomeId}")
                .Distinct()
                .ToList();
                
            Assert.Equal(6, pairings.Count);
        }

        [Fact]
        public void GenerateNextRound_OddParticipants_ShouldAddByes()
        {
            // Arrange
            var tournament = new Tournament { Name = "Test" };
            var participants = new List<TournamentParticipant>
            {
                new TournamentParticipant("u1", 1000),
                new TournamentParticipant("u2", 1000),
                new TournamentParticipant("u3", 1000)
            };
            
            // Act
            var matches = _strategy.GenerateNextRound(tournament, participants, new List<Match>()).ToList();
            
            // Assert
            // 3 players -> Treated as 4 (ghost). Rounds = 3.
            // Matches per round: 2 (1 Normal, 1 Bye).
            // Total matches = 6.
            Assert.Equal(6, matches.Count);
            
            var byes = matches.Where(m => m.Result != null && m.Result.FinishType == MatchFinishType.Bye).ToList();
            Assert.Equal(3, byes.Count); // 1 bye per round
            
            // Verify each real player gets exactly 1 bye
            var byeRecipients = byes.Select(m => m.PlayerHomeId).OrderBy(id => id).ToList();
            Assert.Equal(new[] { "u1", "u2", "u3" }, byeRecipients);
        }

        [Fact]
        public void GenerateNextRound_ShouldReturnEmpty_WhenMatchesExist()
        {
             // Arrange
            var tournament = new Tournament { Name = "Test" };
            var participants = new List<TournamentParticipant>
            {
                new TournamentParticipant("u1", 1000),
                new TournamentParticipant("u2", 1000)
            };
            var existingMatches = new List<Match> { new Match("t1", "u1", "u2", 1, "t1") };
            
            // Act
            var matches = _strategy.GenerateNextRound(tournament, participants, existingMatches).ToList();
            
            // Assert
            Assert.Empty(matches);
        }
    }
}
