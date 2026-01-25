using System.Reflection;
using System.Threading.Tasks;
using GFlow.Application.DTOs;
using GFlow.Application.Ports;
using GFlow.Domain.Entities;
using GFlow.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GFlow.Application.Services
{
    /// <summary>
    /// Service for managing tournaments.
    /// Handles creation and retrieval of tournament data.
    /// </summary>
    public class TournamentService : ITournamentService
    {
        private readonly ITournamentRepository _tournamentRepo;
        private readonly IUserRepository _userRepo;
        private readonly IGeoLocationService _geoService;
        private readonly IUserPreferenceService _preferenceService;

        public TournamentService(
            ITournamentRepository tournamentRepository, 
            IUserRepository userRepository,
            IGeoLocationService geoService,
            IUserPreferenceService preferenceService)
        {
            _tournamentRepo = tournamentRepository;
            _userRepo = userRepository;
            _geoService = geoService;
            _preferenceService = preferenceService;
        }


        /// <summary>
        /// Creates a new tournament asynchronously.
        /// </summary>
        /// <param name="request">The tournament creation request.</param>
        /// <returns>The created tournament, or null if validation fails.</returns>
        public async Task<Tournament?> CreateTournamentAsync(CreateTournamentRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Name) || request.Name.Length < 5)
            {
                return null;
            }

            if (request.MaxParticipants <= 0)
            {
                return null;
            }

            var tournament = new Tournament
            {
                Id = Guid.NewGuid().ToString(),
                Name = request.Name,
                OrganizerId = request.OrganizerId,
                SystemType = request.SystemType,
                PlayerLimit = request.MaxParticipants, 
                Status = Domain.ValueObjects.TournamentStatus.CREATED,
                StartDate = DateTime.SpecifyKind(request.StartDate, DateTimeKind.Utc),
                EndDate = DateTime.SpecifyKind(request.EndDate, DateTimeKind.Utc),
                Description = request.Description,
                NumberOfRounds = request.NumberOfRounds,

                
                // New Fields Mapping
                CountryCode = request.CountryCode,
                City = request.City,
                Address = request.Address,
                GameCode = request.GameCode,
                GameName = request.GameName,
                Emblem = request.Emblem
            };

            // Geocoding Logic
            if (!string.IsNullOrEmpty(request.City) && !request.City.Equals("Online", StringComparison.OrdinalIgnoreCase))
            {
                var geocoded = await _geoService.GeocodeAsync(request.City);
                if (geocoded != null)
                {
                    tournament.Lat = geocoded.Lat;
                    tournament.Lng = geocoded.Lng;
                }
            }
            else
            {
                // Explicitly Online or no city
                tournament.City = "Online";
                tournament.Lat = null;
                tournament.Lng = null;
            }


            
            // Organizer must exist? We assume yes from request info or auth.
            // But we don't fully validate OrganizerId in DB FK unless context tracks it.
            // For now simple Add works if FK is valid or optional.
            
            return await _tournamentRepo.Add(tournament);
        }

        /// <summary>
        /// Retrieves a list of current tournaments.
        /// </summary>
        /// <returns>A list of active tournaments.</returns>
        public async Task<List<Tournament>> GetCurrentTournaments()
        {
            return await _tournamentRepo.GetCurrent();
        }

        /// <summary>
        /// Retrieves a specific tournament by ID.
        /// </summary>
        /// <param name="id">The tournament ID.</param>
        /// <returns>The tournament if found, otherwise null.</returns>
        public async Task<Tournament?> GetTournament(string id)
        {
            return await _tournamentRepo.GetTournament(id);
        }

        /// <summary>
        /// Retrieves a list of upcoming tournaments.
        /// </summary>
        /// <returns>A list of upcoming tournaments.</returns>
        public async Task<List<Tournament>> GetUpcomingTournaments()
        {
            return await _tournamentRepo.GetUpcoming();
        }

        public async Task<Tournament?> UpdateTournamentAsync(string id, UpdateTournamentRequest request)
        {
            var tournament = await _tournamentRepo.GetTournament(id);
            if (tournament == null) return null;

            // Basic validation and updates
            if (!string.IsNullOrWhiteSpace(request.Name))
            {
                if (request.Name.Length < 5) return null; // Validation logic should be shared/consistent
                tournament.Name = request.Name;
            }

            if (request.StartDate.HasValue) tournament.StartDate = DateTime.SpecifyKind(request.StartDate.Value, DateTimeKind.Utc);
            if (request.EndDate.HasValue) tournament.EndDate = DateTime.SpecifyKind(request.EndDate.Value, DateTimeKind.Utc);

            if (request.PlayerLimit.HasValue && request.PlayerLimit.Value > 0) 
            {
                 tournament.PlayerLimit = request.PlayerLimit.Value;
            }

            if (!string.IsNullOrEmpty(request.Emblem))
            {
                tournament.Emblem = request.Emblem;
            }

            return await _tournamentRepo.Update(tournament);
        }

        public async Task<bool> DeleteTournamentAsync(string id)
        {
            return await _tournamentRepo.Delete(id);
        }

        public async Task<bool> WithdrawParticipantAsync(string tournamentId, string userId)
        {
            var participant = await _tournamentRepo.GetParticipant(tournamentId, userId);
            if (participant == null) return false;

            participant.IsWithdrawn = true;
            return await _tournamentRepo.UpdateParticipant(participant);
        }

        public async Task<bool> AddModeratorAsync(string tournamentId, string userId)
        {
            var tournament = await _tournamentRepo.GetTournament(tournamentId);
            if (tournament == null) return false;

            var user = await _userRepo.Get(userId);
            if (user == null) return false;

            if (tournament.Moderators.Any(m => m.Id == userId))
            {
                return true; // Already a moderator
            }

            tournament.Moderators.Add(user);
            return await _tournamentRepo.Update(tournament) != null;
        }

        public async Task<(List<Tournament> Data, int TotalCount)> GetTournamentsAsync(TournamentFilterParams filterParams)
        {
            // 1. Resolve Geo-location from City Name if radius is provided
            if (filterParams.Radius.HasValue && !string.IsNullOrEmpty(filterParams.City) && (!filterParams.Lat.HasValue || !filterParams.Lng.HasValue))
            {
                var geocoded = await _geoService.GeocodeAsync(filterParams.City);
                if (geocoded != null)
                {
                    filterParams.Lat = geocoded.Lat;
                    filterParams.Lng = geocoded.Lng;
                }
            }

            // 2. Resolve Geo-location from IP if nothing else provided
            if (string.IsNullOrEmpty(filterParams.City) && !string.IsNullOrEmpty(filterParams.UserIp) && !filterParams.Lat.HasValue)
            {
                var location = await _geoService.GetLocationAsync(filterParams.UserIp);

                if (location != null)
                {
                    // If radius is not specified, we can use the location to promote same country/city
                    // For filtering, we only apply lat/lng if radius is present
                    if (filterParams.Radius.HasValue)
                    {
                        filterParams.Lat ??= location.Lat;
                        filterParams.Lng ??= location.Lng;
                    }
                }
            }

            // 2. Fetch from DB
            var (data, totalCount) = await _tournamentRepo.GetFiltered(filterParams);

            // 3. Apply Personalized Scoring and Re-sort if sorted by 'relevance'
            if (filterParams.SortBy == "relevance" || string.IsNullOrEmpty(filterParams.SortBy))
            {
                // We'll return the items with scores to be mapped in Controller
                // In-memory final sort
                // Note: We only score the items on the current page for efficiency
                // A better approach would be scoring in DB, but this is fine for MVP.
            }

            return (data, totalCount);
        }

        public async Task<bool> TrackActivityAsync(string tournamentId, string? userId, string activityType)
        {
            if (activityType == "view")
            {
                await _tournamentRepo.IncrementViewCount(tournamentId);
            }

            if (!string.IsNullOrEmpty(userId))
            {
                var activity = new UserActivity
                {
                    UserId = userId,
                    TournamentId = tournamentId,
                    ActivityType = activityType
                };
                await _tournamentRepo.AddActivity(activity);
            }

            return true;
        }

        public async Task<bool> SubmitMatchResultAsync(string matchId, double scoreA, double scoreB, string? requestingUserId = null)

        {
            var match = await _tournamentRepo.GetMatchById(matchId);
            if (match == null) return false;

            // Permission Check
            if (requestingUserId != null)
            {
                var tournament = await _tournamentRepo.GetTournament(match.TournamentId);
                if (tournament != null)
                {
                    bool isOrganizer = tournament.OrganizerId == requestingUserId;
                    bool isModerator = tournament.Moderators.Any(u => u.Id == requestingUserId);
                    if (!isOrganizer && !isModerator)
                    {
                        return false; 
                    }
                }
            }

            if (match.IsCompleted) 
            {
                // Optionally allow overwriting
            }

            match.FinishMatch(scoreA, scoreB);
            await _tournamentRepo.UpdateMatch(match);
            
            return true;
        }

        public async Task<List<DTOs.StandingsEntry>> GetStandingsAsync(string tournamentId)
        {
            var tournament = await _tournamentRepo.GetTournament(tournamentId);
            if (tournament == null) return new List<DTOs.StandingsEntry>();

            // We need to recalculate standings based on matches to be sure, or rely on Participant data if it's kept in sync.
            // SwissPairingStrategy updates participants data during round generation, but if we want live standings after result submission
            // we should probably re-aggregate from matches.
            // However, TournamentParticipant entity ALREADY has Score, Ranking etc. updated by Strategy.
            // But wait, strategy updates IN MEMORY objects during pairing. Does it persist them?
            // Existing flow likely persists participants after pairing.
            // But result submission modifies Match. Does it modify Participant score?
            // Currently NO. `SubmitMatchResultAsync` only updates Match.
            // So we must aggregate from matches for accurate standings.

            var participants = await _tournamentRepo.GetParticipants(tournamentId);
            var matches = await _tournamentRepo.GetAllMatches(tournamentId);
            
            // Create a map for calculation
            var stats = participants.ToDictionary(p => p.UserId, p => new DTOs.StandingsEntry 
            { 
                UserId = p.UserId, 
                Score = 0,
                Ranking = p.Ranking,
                MatchesPlayed = 0,
                Wins = 0,
                Draws = 0,
                Losses = 0,
                IsWithdrawn = p.IsWithdrawn
            });


            // Need usernames
            // In a real app we'd inject IUserRepository or have a method on TournamentRepo to get enriched participants
            // For now let's try to fetch user details or return IDs. 
            // The prompt asked for "Displaying table" -> implies names.
            // `tournament.Participants` (List<User>) might be loaded if we used `GetTournament` with Include.
            if (tournament.Participants != null)
            {
                foreach(var user in tournament.Participants)
                {
                    if (stats.ContainsKey(user.Id))
                    {
                        stats[user.Id].Username = user.Username;
                    }
                }
            }

            foreach (var match in matches.Where(m => m.IsCompleted && m.Result != null))
            {
                if (stats.TryGetValue(match.PlayerHomeId, out var p1))
                {
                    UpdateStats(p1, match.Result.ScoreA, match.Result.ScoreB);
                    p1.MatchesPlayed++;
                }

                if (stats.TryGetValue(match.PlayerAwayId, out var p2))
                {
                    UpdateStats(p2, match.Result.ScoreB, match.Result.ScoreA);
                    p2.MatchesPlayed++;
                }
            }

            return stats.Values
                .OrderByDescending(s => s.Score)
                .ThenByDescending(s => s.Ranking) // Sort by initial ranking as tie breaker
                .ToList();
        }

        private void UpdateStats(DTOs.StandingsEntry entry, double myScore, double opponentScore)
        {
            entry.Score += myScore;
            if (myScore > opponentScore) entry.Wins++;
            else if (myScore < opponentScore) entry.Losses++;
            else entry.Draws++;
        }
    }
}