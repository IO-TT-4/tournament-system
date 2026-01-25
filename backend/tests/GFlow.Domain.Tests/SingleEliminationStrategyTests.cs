using Xunit;
using GFlow.Domain.Services.Pairings;
using GFlow.Domain.Entities;
using GFlow.Domain.ValueObjects;
using System.Collections.Generic;
using System.Linq;
using System;

namespace GFlow.Domain.Tests
{
    public class SingleEliminationStrategyTests
    {
        private readonly SingleEliminationStrategy _strategy;

        public SingleEliminationStrategyTests()
        {
            _strategy = new SingleEliminationStrategy();
        }

        [Fact]
        public void GenerateFirstRound_PowerOfTwo_ShouldCreateBracket()
        {
            // Arrange
            var tournament = new Tournament { Name = "Test" };
            // 4 participants (Power of 2)
            var participants = new List<TournamentParticipant>
            {
                new TournamentParticipant("u1", 1000), // Rank 1
                new TournamentParticipant("u2", 900),  // Rank 2
                new TournamentParticipant("u3", 800),  // Rank 3
                new TournamentParticipant("u4", 700)   // Rank 4
            };
            
            // Act
            var matches = _strategy.GenerateNextRound(tournament, participants, new List<Match>()).ToList();
            
            // Assert
            // Bracket size 4. Matches = 2.
            Assert.Equal(2, matches.Count);
            
            // Seeding: 1 vs 4, 2 vs 3.
            // u1(1000) vs u4(700)
            // u2(900) vs u3(800)
            
            var m1 = matches.First(m => m.PlayerHomeId == "u1");
            Assert.Equal("u4", m1.PlayerAwayId);
            
            var m2 = matches.First(m => m.PlayerHomeId == "u2");
            Assert.Equal("u3", m2.PlayerAwayId);
        }

        [Fact]
        public void GenerateFirstRound_OddParticipants_ShouldAddByes()
        {
            // Arrange
            var tournament = new Tournament { Name = "Test" };
            // 5 participants. Next power of 2 is 8.
            // Bracket 8. Matches 4.
            // 3 Byes.
            var participants = Enumerable.Range(1, 5)
                .Select(i => new TournamentParticipant($"u{i}", 1000 - i * 10)) // u1 highest, u5 lowest
                .ToList();
            
            // Act
            var matches = _strategy.GenerateNextRound(tournament, participants, new List<Match>()).ToList();
            
            // Assert
            Assert.Equal(4, matches.Count); // 4 matches in round of 8
            
            // Check Byes (Result != null && Bye)
            var byes = matches.Where(m => m.Result?.FinishType == MatchFinishType.Bye).ToList();
            Assert.Equal(3, byes.Count);
            
            // Top 3 seeds (u1, u2, u3) should get Byes because they match against 8, 7, 6 (empty).
            var byeRecipients = byes.Select(m => m.PlayerHomeId).OrderBy(id => id).ToList();
            Assert.Equal(new[] { "u1", "u2", "u3" }, byeRecipients);
            
            // Only real match: Seed 4 (u4) vs Seed 5 (u5).
            var realMatch = matches.Single(m => m.Result == null || m.Result.FinishType != MatchFinishType.Bye);
            Assert.Equal("u4", realMatch.PlayerHomeId);
            Assert.Equal("u5", realMatch.PlayerAwayId);
        }

        [Fact]
        public void GenerateNextRound_ShouldAdvanceWinners()
        {
            // Arrange
            var tournament = new Tournament { Name = "Test" };
            var participants = new List<TournamentParticipant> { new TournamentParticipant("u1", 0), new TournamentParticipant("u2", 0) }; // dummies
            
            // Round 1 (Semi-finals). 2 Matches.
            // M1: u1 vs u4. Winner u1. (Pos 1)
            // M2: u2 vs u3. Winner u2. (Pos 2)
            
            var m1 = new Match("t1", "u1", "u4", 1, "t1") { PositionInRound = 1 };
            m1.SetResult(new MatchResult(1, 0, MatchFinishType.Normal)); // u1 wins
            
            var m2 = new Match("t1", "u2", "u3", 1, "t1") { PositionInRound = 2 };
            m2.SetResult(new MatchResult(1, 0, MatchFinishType.Normal)); // u2 wins
            
            var existingMatches = new List<Match> { m1, m2 };

            // Act
            var matches = _strategy.GenerateNextRound(tournament, participants, existingMatches).ToList();

            // Assert
            // Next Round (Final). 1 Match.
            // u1 vs u2.
            Assert.Single(matches);
            var final = matches.First();
            Assert.Equal(2, final.RoundNumber);
            Assert.Equal("u1", final.PlayerHomeId);
            Assert.Equal("u2", final.PlayerAwayId);
        }
    }
}
