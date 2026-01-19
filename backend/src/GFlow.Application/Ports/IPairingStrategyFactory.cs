using GFlow.Domain.Services.Pairings;
using GFlow.Domain.ValueObjects;

namespace GFlow.Application.Ports
{
    public interface IPairingStrategyFactory
    {
        IPairingStrategy GetStrategy(TournamentSystemType systemType);
    }
}