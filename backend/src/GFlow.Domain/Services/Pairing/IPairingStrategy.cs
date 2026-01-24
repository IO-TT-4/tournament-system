using GFlow.Domain.Entities;
using GFlow.Domain.ValueObjects;

namespace GFlow.Domain.Services.Pairings
{
    public interface IPairingStrategy
    {
        IEnumerable<Match> GenerateNextRound(
            Tournament tournament, 
            List<TournamentParticipant> participants, 
            List<Match> existingMatches);
    }
}