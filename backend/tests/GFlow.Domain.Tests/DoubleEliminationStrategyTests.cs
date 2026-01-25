using Xunit;
using GFlow.Domain.Services.Pairings;
using GFlow.Domain.Entities;
using GFlow.Domain.ValueObjects;
using System.Collections.Generic;
using System.Linq;
using System;

namespace GFlow.Domain.Tests
{
    public class DoubleEliminationStrategyTests
    {
        private readonly DoubleEliminationStrategy _strategy;

        public DoubleEliminationStrategyTests()
        {
            _strategy = new DoubleEliminationStrategy();
        }

        [Fact]
        public void GenerateFirstRound_PowerOfTwo_ShouldCreateBracket()
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
            // 4 players -> bracket size 4 -> 2 matches
            Assert.Equal(2, matches.Count);
            
            // Verify seeding: 1 vs 4, 2 vs 3
            var m1 = matches.First(m => m.PlayerHomeId == "u1");
            Assert.Equal("u4", m1.PlayerAwayId);
            
            var m2 = matches.First(m => m.PlayerHomeId == "u2");
            Assert.Equal("u3", m2.PlayerAwayId);
            
            // All should be round 1
            Assert.All(matches, m => Assert.Equal(1, m.RoundNumber));
        }

        [Fact]
        public void GenerateFirstRound_OddParticipants_ShouldAddByes()
        {
            // Arrange
            var tournament = new Tournament { Name = "Test" };
            var participants = Enumerable.Range(1, 5)
                .Select(i => new TournamentParticipant($"u{i}", 1000 - i * 10))
                .ToList();
            
            // Act
            var matches = _strategy.GenerateNextRound(tournament, participants, new List<Match>()).ToList();
            
            // Assert
            // 5 players -> bracket 8 -> 4 matches
            Assert.Equal(4, matches.Count);
            
            // Check byes: top 3 seeds get byes
            var byes = matches.Where(m => m.Result?.FinishType == MatchFinishType.Bye).ToList();
            Assert.Equal(3, byes.Count);
        }

        [Fact]
        public void GenerateNextRound_IncompleteRound_ShouldReturnEmpty()
        {
            // Arrange
            var tournament = new Tournament { Name = "Test" };
            var participants = new List<TournamentParticipant> 
            { 
                new TournamentParticipant("u1", 0), 
                new TournamentParticipant("u2", 0) 
            };
            
            // Create incomplete round
            var m1 = new Match("t1", "u1", "u4", 1, "t1") { PositionInRound = 1 };
            // No result set - incomplete
            
            var existingMatches = new List<Match> { m1 };

            // Act
            var matches = _strategy.GenerateNextRound(tournament, participants, existingMatches).ToList();

            // Assert
            // Should return empty because round not complete
            Assert.Empty(matches);
        }

        [Fact]
        public void GenerateNextRound_AfterWBRound1_ShouldCreateWBRound2AndLBRound1()
        {
            // Arrange
            var tournament = new Tournament { Name = "Test" };
            var participants = new List<TournamentParticipant> 
            { 
                new TournamentParticipant("u1", 0), 
                new TournamentParticipant("u2", 0) 
            };
            
            // WB Round 1: 2 matches completed
            // M1: u1 beats u4 (u1 advances to WB, u4 drops to LB)
            // M2: u2 beats u3 (u2 advances to WB, u3 drops to LB)
            var m1 = new Match("t1", "u1", "u4", 1, "t1") { PositionInRound = 1 };
            m1.SetResult(new MatchResult(1, 0, MatchFinishType.Normal));
            
            var m2 = new Match("t1", "u2", "u3", 1, "t1") { PositionInRound = 2 };
            m2.SetResult(new MatchResult(1, 0, MatchFinishType.Normal));
            
            var existingMatches = new List<Match> { m1, m2 };

            // Act
            var matches = _strategy.GenerateNextRound(tournament, participants, existingMatches).ToList();

            // Assert
            // Should generate:
            // 1. WB Round 2: u1 vs u2 (1 match)
            // 2. LB Round 1: u4 vs u3 (1 match)
            Assert.Equal(2, matches.Count);
            
            // Check WB match (PositionInRound < 1000)
            var wbMatch = matches.First(m => m.PositionInRound < 1000);
            Assert.Equal(2, wbMatch.RoundNumber);
            Assert.Contains(wbMatch.PlayerHomeId, new[] { "u1", "u2" });
            Assert.Contains(wbMatch.PlayerAwayId, new[] { "u1", "u2" });
            
            // Check LB match (PositionInRound >= 1000)
            var lbMatch = matches.First(m => m.PositionInRound >= 1000);
            Assert.Equal(2, lbMatch.RoundNumber);
            Assert.Contains(lbMatch.PlayerHomeId, new[] { "u3", "u4" });
            Assert.Contains(lbMatch.PlayerAwayId, new[] { "u3", "u4" });
        }

        [Fact]
        public void FullTournament_4Players_ShouldReachGrandFinals()
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
            
            var allMatches = new List<Match>();
            
            // Round 1 (WB R1): u1 beats u4, u2 beats u3
            var r1 = _strategy.GenerateNextRound(tournament, participants, allMatches).ToList();
            Assert.Equal(2, r1.Count);
            r1[0].SetResult(new MatchResult(1, 0, MatchFinishType.Normal)); // u1 wins
            r1[1].SetResult(new MatchResult(1, 0, MatchFinishType.Normal)); // u2 wins
            allMatches.AddRange(r1);
            
            // Round 2: Should generate WB R2 (u1 vs u2) and LB R1 (u4 vs u3)
            var r2 = _strategy.GenerateNextRound(tournament, participants, allMatches).ToList();
            Assert.Equal(2, r2.Count);
            
            var wbR2 = r2.First(m => m.PositionInRound < 1000);
            var lbR1 = r2.First(m => m.PositionInRound >= 1000);
            
            wbR2.SetResult(new MatchResult(1, 0, MatchFinishType.Normal)); // u1 wins WB
            // Set u3 as winner of LB R1
            if (lbR1.PlayerHomeId == "u3")
                lbR1.SetResult(new MatchResult(1, 0, MatchFinishType.Normal));
            else
                lbR1.SetResult(new MatchResult(0, 1, MatchFinishType.Normal));

            allMatches.AddRange(r2);
            
            // Round 3: Should generate LB R2 (u3 vs u2, the WB loser)
            var r3 = _strategy.GenerateNextRound(tournament, participants, allMatches).ToList();
            Assert.Single(r3);
            Assert.Equal(3, r3[0].RoundNumber);
            Assert.True(r3[0].PositionInRound >= 1000, "Should be LB match");
            
            // Set winner for LB R2 (u3 wins)
            if (r3[0].PlayerHomeId == "u3")
                r3[0].SetResult(new MatchResult(1, 0, MatchFinishType.Normal));
            else
                r3[0].SetResult(new MatchResult(0, 1, MatchFinishType.Normal));

            allMatches.AddRange(r3);
            
            // Round 4: Should generate Grand Finals (u1 vs u3)
            var gf = _strategy.GenerateNextRound(tournament, participants, allMatches).ToList();
            Assert.Single(gf);
            Assert.Equal(4, gf[0].RoundNumber);
            
            var gfPlayers = new HashSet<string> { gf[0].PlayerHomeId, gf[0].PlayerAwayId };
            Assert.Contains("u1", gfPlayers); // WB winner
            Assert.Contains("u3", gfPlayers); // LB winner (u3 won LB R1 and LB R2)
        }

        [Fact]
        public void FullTournament_8Players_ShouldCompleteSuccessfully()
        {
            // Arrange
            var tournament = new Tournament { Name = "Test" };
            var participants = Enumerable.Range(1, 8)
                .Select(i => new TournamentParticipant($"u{i}", 1000 - i))
                .ToList();
            
            var allMatches = new List<Match>();
            
            // Round 1: WB R1 (4 matches) 1-8, 2-7, 3-6, 4-5
            var r1 = _strategy.GenerateNextRound(tournament, participants, allMatches).ToList();
            Assert.Equal(4, r1.Count);
            foreach (var m in r1) m.SetResult(new MatchResult(1, 0, MatchFinishType.Normal)); // Top seeds win (u1, u2, u3, u4)
            allMatches.AddRange(r1);
            
            // Round 2: WB R2 (2 matches: u1-u4, u2-u3) + LB R1 (2 matches: u8-u5, u7-u6)
            var r2 = _strategy.GenerateNextRound(tournament, participants, allMatches).ToList();
            Assert.Equal(4, r2.Count);
            foreach (var m in r2) m.SetResult(new MatchResult(1, 0, MatchFinishType.Normal)); // Top seeds win (u1, u2 win WB; u8, u7 winners LB R1)
            allMatches.AddRange(r2);
            
            // Round 3: WB R3 (Finals: u1-u2) + LB R2 (LB winners u8, u7 vs WB R2 losers u4, u3)
            var r3 = _strategy.GenerateNextRound(tournament, participants, allMatches).ToList();
            Assert.Equal(3, r3.Count); // 1 WB match + 2 LB matches
            foreach (var m in r3) m.SetResult(new MatchResult(1, 0, MatchFinishType.Normal)); // u1 wins WB Finals; u8, u7 win LB R2
            allMatches.AddRange(r3);
            
            // Round 4: LB R3 (Minor round: u8 vs u7)
            var r4 = _strategy.GenerateNextRound(tournament, participants, allMatches).ToList();
            Assert.Single(r4); 
            r4[0].SetResult(new MatchResult(1, 0, MatchFinishType.Normal)); // u8 wins LB R3
            allMatches.AddRange(r4);
            
            // Round 5: LB R4 (Major round: LB R3 winner u8 vs WB Finals loser u2)
            var r5 = _strategy.GenerateNextRound(tournament, participants, allMatches).ToList();
            Assert.Single(r5);
            r5[0].SetResult(new MatchResult(1, 0, MatchFinishType.Normal)); // u8 wins LB R4
            allMatches.AddRange(r5);
            
            // Round 6: Grand Finals (u1 vs u8)
            var gf = _strategy.GenerateNextRound(tournament, participants, allMatches).ToList();
            Assert.Single(gf);
            Assert.Equal(6, gf[0].RoundNumber);
            
            var gfPlayers = new HashSet<string> { gf[0].PlayerHomeId, gf[0].PlayerAwayId };
            Assert.Contains("u1", gfPlayers); 
            Assert.Contains("u8", gfPlayers); 
        }
    }
}
