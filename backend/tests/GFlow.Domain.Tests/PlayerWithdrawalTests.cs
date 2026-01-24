using GFlow.Domain.Entities;
using GFlow.Domain.Services.Pairings;
using GFlow.Domain.ValueObjects;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace GFlow.Domain.Tests
{
    public class PlayerWithdrawalTests
    {
        [Fact]
        public void Swiss_WithdrawnPlayer_ShouldNotBePaired()
        {
            // Arrange
            var strategy = new SwissPairingStrategy(3);
            var tournament = new Tournament { Id = "t1", Name = "Test", SystemType = TournamentSystemType.SWISS };
            var participants = new List<TournamentParticipant>
            {
                new TournamentParticipant("u1", 1000),
                new TournamentParticipant("u2", 1000),
                new TournamentParticipant("u3", 1000) { IsWithdrawn = true }, // Withdrawn
                new TournamentParticipant("u4", 1000)
            };

            // Act
            var matches = strategy.GenerateNextRound(tournament, participants, new List<Match>()).ToList();

            // Assert
            // Should pair u1, u2, u4 -> 1 match + 1 bye (active count = 3)
            // Or if active count = 3, 1 bye. 
            // u3 is withdrawn, so active players: u1, u2, u4.
            // Match count should be 2 (1 match + 1 bye)
            
            Assert.DoesNotContain(matches, m => m.PlayerHomeId == "u3" || m.PlayerAwayId == "u3");
            Assert.Equal(2, matches.Count); // 1 match + 1 bye
        }

        [Fact]
        public void Swiss_UnavailableForRound_ShouldNotBePairedInThatRound()
        {
            // Arrange
            var strategy = new SwissPairingStrategy(3);
            var tournament = new Tournament { Id = "t1", Name = "Test", SystemType = TournamentSystemType.SWISS };
            var p1 = new TournamentParticipant("u1", 1000);
            var p2 = new TournamentParticipant("u2", 1000);
            var p3 = new TournamentParticipant("u3", 1000); // Unavailable for Round 1
            p3.UnavailableRounds.Add(1);
            var p4 = new TournamentParticipant("u4", 1000);

            var participants = new List<TournamentParticipant> { p1, p2, p3, p4 };

            // Act - Round 1
            var matches = strategy.GenerateNextRound(tournament, participants, new List<Match>()).ToList();

            // Assert
            // u3 unavailable -> active: u1, u2, u4
            Assert.DoesNotContain(matches, m => m.PlayerHomeId == "u3" || m.PlayerAwayId == "u3");
            Assert.Equal(2, matches.Count); // 1 match + 1 bye
        }

        [Fact]
        public void Swiss_UnavailableForRound_ShouldBePairedInNextRound()
        {
            // Arrange
            var strategy = new SwissPairingStrategy(3);
            var tournament = new Tournament { Id = "t1", Name = "Test", SystemType = TournamentSystemType.SWISS };
            var p1 = new TournamentParticipant("u1", 1000);
            var p2 = new TournamentParticipant("u2", 1000);
            var p3 = new TournamentParticipant("u3", 1000); 
            p3.UnavailableRounds.Add(1); // Unavailable only for R1
            var p4 = new TournamentParticipant("u4", 1000);

            var participants = new List<TournamentParticipant> { p1, p2, p3, p4 };

            // Round 1 (Already played, u3 was skipped)
            var m1 = new Match("t1", "u1", "u2", 1, "t1");
            m1.SetResult(new MatchResult(1, 0, MatchFinishType.Normal));
            
            // u4 got a bye in R1 effectively (or played someone else? simulating R1 state)
            // Let's say u4 got a bye
            var m2 = new Match("t1", "u4", Guid.Empty.ToString(), 1, "t1");
            m2.SetResult(MatchResult.CreateBye());

            var existingMatches = new List<Match> { m1, m2 };

            // Act - Generate Round 2
            var matches = strategy.GenerateNextRound(tournament, participants, existingMatches).ToList();

            // Assert
            // u3 is available for R2. Active: u1, u2, u3, u4 (All 4)
            // Should have 2 matches, everyone paired.
            Assert.Equal(2, matches.Count);
            Assert.Contains(matches, m => m.PlayerHomeId == "u3" || m.PlayerAwayId == "u3");
        }

        [Fact]
        public void Elimination_WithdrawnPlayer_ShouldNotBeSeededInFirstRound()
        {
            // Arrange
            var strategy = new SingleEleminationStrategy();
            var tournament = new Tournament { Id = "t1", Name = "Test", SystemType = TournamentSystemType.SINGLE_ELEMINATION };
            var participants = new List<TournamentParticipant>
            {
                new TournamentParticipant("u1", 1000),
                new TournamentParticipant("u2", 900),
                new TournamentParticipant("u3", 800) { IsWithdrawn = true },
                new TournamentParticipant("u4", 700)
            };

            // Act
            var matches = strategy.GenerateNextRound(tournament, participants, new List<Match>()).ToList();

            // Assert
            // u3 withdrawn. Active: u1, u2, u4 (3 players).
            // Bracket size 4. 1 match (1 vs 4 -> u1 vs Bye), 1 match (2 vs 3 -> u2 vs u4).
            // Wait, logic is:
            // Active: u1, u2, u4. Sorted: u1, u2, u4.
            // Bracket size for 3 is 4.
            // Match 1: Seed 1 (u1) vs Seed 4 (Bye)
            // Match 2: Seed 2 (u2) vs Seed 3 (u4)
            
            Assert.DoesNotContain(matches, m => m.PlayerHomeId == "u3" || m.PlayerAwayId == "u3");
            
            var matchWithU2 = matches.First(m => m.PlayerHomeId == "u2" || m.PlayerAwayId == "u2");
            Assert.Contains("u4", new[] { matchWithU2.PlayerHomeId, matchWithU2.PlayerAwayId });
        }
    }
}
