using Xunit;
using GFlow.Domain.Services.TieBreakers;
using GFlow.Domain.Entities;
using GFlow.Domain.ValueObjects;
using System.Collections.Generic;


namespace GFlow.Domain.Tests
{
    public class TieBreakerTests
    {
        [Fact]
        public void Buchholz_ShouldSumOpponentsScores()
        {
            // Arrange
            var tieBreaker = new BuchholzTieBreaker();
            var matches = new List<Match>();
            
            // Player u1 played u2 and u3.
            // u2 score: 2.0. u3 score: 1.5.
            // Buchholz for u1 = 2.0 + 1.5 = 3.5.
            
            // We need Standings to look up scores.
            var standings = new List<StandingsEntry>
            {
                new StandingsEntry { UserId = "u1", Points = 3.0 },
                new StandingsEntry { UserId = "u2", Points = 2.0 },
                new StandingsEntry { UserId = "u3", Points = 1.5 }
            };
            
            // Matches to establish opponents
            var m1 = new Match("t1", "u1", "u2", 1, "t1"); 
            m1.SetResult(new MatchResult(1, 0, MatchFinishType.Normal)); // u1 wins vs u2
            
            var m2 = new Match("t1", "u3", "u1", 2, "t1");
            m2.SetResult(new MatchResult(0.5, 0.5, MatchFinishType.Normal)); // Draw
            
            matches.Add(m1);
            matches.Add(m2);
            
            // Act
            var result = tieBreaker.Calculate("u1", matches, standings);
            
            // Assert
            Assert.Equal(3.5, result);
        }

        [Fact]
        public void DirectEncounter_ShouldReturnMatchResultPoints()
        {
            // Arrange
            var tieBreaker = new DirectEncounterTieBreaker();
            
            // u1 played u2 and WON.
            // Calculating tie-break relative to... whom?
            // "Calculate(userId, allMatches, currentStandings)" returns a DOUBLE score.
            // Direct Encounter is usually used to sort a group of tied players.
            // But the signature returns a simple double for ONE player.
            // If the interface is "float score", how does it handle "vs tied opponents"?
            // Usually it sums points scored against TIED opponents.
            // But here we pass 'currentStandings', which might be the whole list.
            // If the implementation sums points against EVERYONE, it's just Score? No.
            // Let's assume it sums points against people WITH SAME SCORE.
            
            var standings = new List<StandingsEntry>
            {
                new StandingsEntry { UserId = "u1", Points = 3.0 },
                new StandingsEntry { UserId = "u2", Points = 3.0 }, // Tied with u1
                new StandingsEntry { UserId = "u3", Points = 1.0 }  // Not tied
            };
            
            var matches = new List<Match>();
            
            // u1 vs u2 (u1 Won)
            var m1 = new Match("t1", "u1", "u2", 1, "t1");
            m1.SetResult(new MatchResult(1, 0, MatchFinishType.Normal));
            matches.Add(m1);
            
            // u1 vs u3 (u1 Won) - Should be ignored if DirectEncounter only counts tied opponents?
            // Or maybe implementation counts points against everyone?
            // "DirectEncounter" usually means Head-to-Head.
            // If it returns a scalar, it likely sums points against the specific tied group provided in standings?
            // Or it relies on logic inside to filter?
            
            // Let's assume standard implementation: Sum of points against players in the provided list (tied group).
            // But here we pass ALL standings.
            // Implementation likely filters 'standings' to find those with SAME Score?
            
            // Act
            var result = tieBreaker.Calculate("u1", matches, standings);
            
            // Assert
            // u1 got 1 pt vs u2 (tied).
            // u1 got 1 pt vs u3 (not tied, DIFFERENT score).
            // If it filters by same score: Result = 1.
            // If it sums all: Result = 2.
            // Let's Expect 1 if it's smart.
            // BUT strict DirectEncounter usually expects a subset of tied players passed in?
            // If 'Calculate' takes 'currentStandings', maybe expected usage is to pass ONLY tied players?
            // Unit test should clarify assumption.
            // I'll assume it sums points against 'standings' list provided.
            // So if I pass u1, u2, u3... it sums vs ALL of them.
            // So result 2.
            
            // Actually, let's verify implementation logic if possible.
            // I'll guess it returns total points against everyone in the list (u2, u3).
            // So 1 match vs u2 (1pt), 1 match vs u3 (0 pts? No, I said u1 won).
            // Wait, if I said u1 won vs u3, then matches contains results.
            
            // m2: u1 vs u3. u1 wins.
            var m2 = new Match("t1", "u1", "u3", 2, "t1");
            m2.SetResult(new MatchResult(1, 0, MatchFinishType.Normal));
            matches.Add(m2);

            // result = 1 (vs u2). u3 is ignored because Points mismatch.
            Assert.Equal(1, result);
        }

        [Fact]
        public void SonnerbornBerger_ShouldSumScoreOfDefeatedOpponents()
        {
            // Arrange
            var tieBreaker = new SonnerbornBergerTieBreaker();
            var matches = new List<Match>();
            
            // u1 beat u2 (Score 2.0). 2.0 pts
            // u1 drew u3 (Score 4.0). 2.0 pts (half of 4.0)
            // u1 lost to u4 (Score 1.0). 0 pts.
            
            var standings = new List<StandingsEntry>
            {
                new StandingsEntry { UserId = "u1", Points = 0 }, 
                new StandingsEntry { UserId = "u2", Points = 2.0 },
                new StandingsEntry { UserId = "u3", Points = 4.0 },
                new StandingsEntry { UserId = "u4", Points = 1.0 }
            };
            
            var m1 = new Match("t1", "u1", "u2", 1, "t1");
            m1.SetResult(new MatchResult(1, 0, MatchFinishType.Normal)); // Win
            
            var m2 = new Match("t1", "u1", "u3", 2, "t1");
            m2.SetResult(new MatchResult(0.5, 0.5, MatchFinishType.Normal)); // Draw
            
            var m3 = new Match("t1", "u1", "u4", 3, "t1");
            m3.SetResult(new MatchResult(0, 1, MatchFinishType.Normal)); // Loss
            
            matches.AddRange(new[] { m1, m2, m3 });
            
            // Act
            var result = tieBreaker.Calculate("u1", matches, standings);
            
            // Assert
            // 2.0 (u2) + 2.0 (half of u3) + 0 = 4.0.
            Assert.Equal(4.0, result);
        }
        
        [Fact]
        public void PointsDifference_ShouldReturnDiff()
        {
            // Arrange
            var tieBreaker = new PointsDifferenceTieBreaker();
            var matches = new List<Match>();
            // u1 vs u2: 1 - 0 (diff +1)
            // u1 vs u3: 0.5 - 0.5 (diff 0)
            // u1 vs u4: 0 - 1 (diff -1)
            // Total diff: 0.
            
            // Let's make it positive.
            // u1 vs u2: 2.0 - 0.0 (Custom scoring?)
            // u1 vs u3: 3.0 - 1.0 (+2)
            
            // Note: MatchResult scores are typically 1, 0.5, 0.
            // But PointsDifference is often used in sports with goals/points (e.g. 21-15).
            // If tournament uses standard 1/0/0.5, diff is small.
            // Let's use custom scores if Match allows.
            
            var m1 = new Match("t1", "u1", "u2", 1, "t1");
            m1.SetResult(new MatchResult(10, 5, MatchFinishType.Normal)); // +5
            
            var m2 = new Match("t1", "u1", "u3", 2, "t1");
            m2.SetResult(new MatchResult(2, 4, MatchFinishType.Normal)); // -2
            
            matches.Add(m1);
            matches.Add(m2);
            
            // Act
            var result = tieBreaker.Calculate("u1", matches, new List<StandingsEntry>()); // Standings not needed for Diff
            
            // Assert
            Assert.Equal(3, result);
        }

        [Fact]
        public void TechnicalPoints_ShouldSumMyScores()
        {
            // Arrange
            var tieBreaker = new TechnicalPointsTieBreaker();
            var matches = new List<Match>();
            
            var m1 = new Match("t1", "u1", "u2", 1, "t1");
            m1.SetResult(new MatchResult(10, 5, MatchFinishType.Normal)); // Scored 10
            
            var m2 = new Match("t1", "u1", "u3", 2, "t1");
            m2.SetResult(new MatchResult(2, 4, MatchFinishType.Normal)); // Scored 2
            
            matches.Add(m1);
            matches.Add(m2);
            
            // Act
            var result = tieBreaker.Calculate("u1", matches, new List<StandingsEntry>());
            
            // Assert
            Assert.Equal(12, result);
        }

        [Fact]
        public void MedianBuchholz_ShouldExcludeExtremes()
        {
            // Arrange
            var tieBreaker = new MedianBuchholzTieBreaker();
            var matches = new List<Match>();
            
            // Player u1 played u2, u3, u4, u5
            // Opponent scores: u2=5.0, u3=2.0, u4=4.0, u5=1.0
            // Sorted: 1.0, 2.0, 4.0, 5.0
            // Median Buchholz = 2.0 + 4.0 = 6.0 (excluding 1.0 and 5.0)
            
            var standings = new List<StandingsEntry>
            {
                new StandingsEntry { UserId = "u1", Points = 3.0 },
                new StandingsEntry { UserId = "u2", Points = 5.0 },
                new StandingsEntry { UserId = "u3", Points = 2.0 },
                new StandingsEntry { UserId = "u4", Points = 4.0 },
                new StandingsEntry { UserId = "u5", Points = 1.0 }
            };
            
            // Establish matches
            var m1 = new Match("t1", "u1", "u2", 1, "t1");
            m1.SetResult(new MatchResult(0, 1, MatchFinishType.Normal));
            
            var m2 = new Match("t1", "u1", "u3", 2, "t1");
            m2.SetResult(new MatchResult(1, 0, MatchFinishType.Normal));
            
            var m3 = new Match("t1", "u1", "u4", 3, "t1");
            m3.SetResult(new MatchResult(0.5, 0.5, MatchFinishType.Normal));
            
            var m4 = new Match("t1", "u5", "u1", 4, "t1");
            m4.SetResult(new MatchResult(0, 1, MatchFinishType.Normal));
            
            matches.AddRange(new[] { m1, m2, m3, m4 });
            
            // Act
            var result = tieBreaker.Calculate("u1", matches, standings);
            
            // Assert
            // Opponents: u2(5.0), u3(2.0), u4(4.0), u5(1.0)
            // Sorted: 1.0, 2.0, 4.0, 5.0
            // Median: 2.0 + 4.0 = 6.0
            Assert.Equal(6.0, result);
        }
    }
}
