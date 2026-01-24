using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GFlow.Application.DTOs;
using GFlow.Application.Ports;
using GFlow.Domain.Entities;

namespace GFlow.Application.Services
{
    public class UserPreferenceService : IUserPreferenceService
    {
        private readonly ITournamentRepository _tournamentRepo;

        public UserPreferenceService(ITournamentRepository tournamentRepo)
        {
            _tournamentRepo = tournamentRepo;
        }

        public async Task<double> CalculateRelevanceScoreAsync(Tournament tournament, TournamentFilterParams filterParams)
        {
            double score = 0;

            // 1. Geography Preference (High Weight)
            if (!string.IsNullOrEmpty(filterParams.UserIp))
            {
                // We'll assume the filters already include the location resolved from IP in the controller/service
                if (tournament.CountryCode == filterParams.City) // Using 'City' field in DTO to pass country for now if simplified
                {
                    score += 100;
                }
            }

            // 2. User Interest (History)
            if (!string.IsNullOrEmpty(filterParams.UserId))
            {
                var preferredGames = await GetPreferredGameCodesAsync(filterParams.UserId);
                if (preferredGames.Contains(tournament.GameCode ?? ""))
                {
                    score += 50;
                }
            }

            // 3. Global Popularity
            score += Math.Log10(tournament.ViewCount + 1) * 10;
            score += tournament.Participants.Count * 2;

            return score;
        }

        public async Task<List<string>> GetPreferredGameCodesAsync(string userId)
        {
            var activities = await _tournamentRepo.GetUserActivities(userId);
            if (!activities.Any()) return new List<string>();

            return activities
                .GroupBy(a => a.Tournament.GameCode)
                .Where(g => g.Key != null)
                .OrderByDescending(g => g.Count())
                .Select(g => g.Key!)
                .Take(3)
                .ToList();
        }
    }
}
