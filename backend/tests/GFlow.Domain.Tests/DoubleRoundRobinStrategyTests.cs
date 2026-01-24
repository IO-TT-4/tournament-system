using Xunit;
using GFlow.Domain.Services.Pairings;
using GFlow.Domain.Entities;
using GFlow.Domain.ValueObjects;
using System.Collections.Generic;
using System.Linq;
using System;

namespace GFlow.Domain.Tests
{
    public class DoubleRoundRobinStrategyTests
    {
        private readonly DoubleRoundRobinStrategy _strategy;

        public DoubleRoundRobinStrategyTests()
        {
            _strategy = new DoubleRoundRobinStrategy();
        }

        [Fact]
        public void GenerateNextRound_ShouldGenerateTwoCycles()
        {
            // Arrange
            var tournament = new Tournament { Name = "Test" };
            var participants = new List<TournamentParticipant>
            {
                new TournamentParticipant("u1", 1000),
                new TournamentParticipant("u2", 900),
                new TournamentParticipant("u3", 800),
                new TournamentParticipant("u4", 700)
            };
            
            // Act
            var matches = _strategy.GenerateNextRound(tournament, participants, new List<Match>()).ToList();
            
            // Assert
            // 4 players -> (n-1) = 3 rounds per cycle
            // Matches per round = n/2 = 2
            // Total: 3 rounds * 2 matches * 2 cycles = 12 matches
            Assert.Equal(12, matches.Count);
            
            // Check round numbers span from 1 to 6 (3 rounds * 2 cycles)
            Assert.Equal(1, matches.Min(m => m.RoundNumber));
            Assert.Equal(6, matches.Max(m => m.RoundNumber));
            
            // Verify each pair plays exactly twice (home and away)
            var pairings = matches
                .Select(m => new { 
                    Pair = string.Compare(m.PlayerHomeId, m.PlayerAwayId) < 0 
                        ? $"{m.PlayerHomeId}-{m.PlayerAwayId}" 
                        : $"{m.PlayerAwayId}-{m.PlayerHomeId}",
                    Round = m.RoundNumber
                })
                .GroupBy(p => p.Pair)
                .ToList();
            
            // Each pair should appear exactly twice (once per cycle)
            foreach (var pair in pairings)
            {
                Assert.Equal(2, pair.Count());
            }
        }

        [Fact]
        public void GenerateNextRound_OddParticipants_ShouldHandleByes()
        {
            // Arrange
            var tournament = new Tournament { Name = "Test" };
            var participants = new List<TournamentParticipant>
            {
                new TournamentParticipant("u1", 1000),
                new TournamentParticipant("u2", 900),
                new TournamentParticipant("u3", 800)
            };
            
            // Act
            var matches = _strategy.GenerateNextRound(tournament, participants, new List<Match>()).ToList();
            
            // Assert
            // 3 players treated as 4 (ghost)
            // Rounds per cycle: 3
            // Total rounds: 6
            // Each round has 2 matches (1 normal, 1 bye)
            // Total: 12 matches
            Assert.Equal(12, matches.Count);
            
            // Check byes
            var byes = matches.Where(m => m.Result?.FinishType == MatchFinishType.Bye).ToList();
            Assert.Equal(6, byes.Count); // 3 per cycle
            
            // Each real player should get 2 byes (1 per cycle)
            var byesByPlayer = byes.GroupBy(m => m.PlayerHomeId);
            foreach (var playerByes in byesByPlayer)
            {
                Assert.Equal(2, playerByes.Count());
            }
        }

        [Fact]
        public void GenerateNextRound_ShouldSwapRoles()
        {
            // Arrange
            var tournament = new Tournament { Name = "Test" };
            var participants = new List<TournamentParticipant>
            {
                new TournamentParticipant("u1", 1000),
                new TournamentParticipant("u2", 900)
            };
            
            // Act
            var matches = _strategy.GenerateNextRound(tournament, participants, new List<Match>()).ToList();
            
            // Assert
            // 2 players: 1 round per cycle, 2 cycles total
            Assert.Equal(2, matches.Count);
            
            var firstCycleMatch = matches.First(m => m.RoundNumber == 1);
            var secondCycleMatch = matches.First(m => m.RoundNumber == 2);
            
            // Roles should be swapped between cycles
            Assert.NotEqual(firstCycleMatch.PlayerHomeId, secondCycleMatch.PlayerHomeId);
            Assert.NotEqual(firstCycleMatch.PlayerAwayId, secondCycleMatch.PlayerAwayId);
            
            // Verify it's the same pair
            var pair1 = new HashSet<string> { firstCycleMatch.PlayerHomeId, firstCycleMatch.PlayerAwayId };
            var pair2 = new HashSet<string> { secondCycleMatch.PlayerHomeId, secondCycleMatch.PlayerAwayId };
            Assert.True(pair1.SetEquals(pair2));
        }

        [Fact]
        public void GenerateNextRound_ShouldReturnEmpty_WhenMatchesExist()
        {
            // Arrange
            var tournament = new Tournament { Name = "Test" };
            var participants = new List<TournamentParticipant>
            {
                new TournamentParticipant("u1", 1000),
                new TournamentParticipant("u2", 900)
            };
            var existingMatches = new List<Match> { new Match("t1", "u1", "u2", 1, "t1") };
            
            // Act
            var matches = _strategy.GenerateNextRound(tournament, participants, existingMatches).ToList();
            
            // Assert
            Assert.Empty(matches);
        }
    }
}
