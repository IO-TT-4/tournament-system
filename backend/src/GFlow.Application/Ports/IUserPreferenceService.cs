using System.Collections.Generic;
using System.Threading.Tasks;
using GFlow.Application.DTOs;
using GFlow.Domain.Entities;

namespace GFlow.Application.Ports
{
    public interface IUserPreferenceService
    {
        Task<double> CalculateRelevanceScoreAsync(Tournament tournament, TournamentFilterParams filterParams);
        Task<List<string>> GetPreferredGameCodesAsync(string userId);
    }
}
