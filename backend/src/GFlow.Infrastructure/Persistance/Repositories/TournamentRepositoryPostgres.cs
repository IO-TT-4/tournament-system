using System.Runtime.CompilerServices;
using GFlow.Application.Ports;
using GFlow.Domain.Entities;
using GFlow.Domain.ValueObjects;
using GFlow.Application.DTOs;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GFlow.Infrastructure.Persistance.Repositories
{
    public class TournamentRepositoryPostgres : ITournamentRepository
    {

        public async Task<Tournament?> Add(Tournament tournament)
        {
            await _context.Tournaments.AddAsync(tournament);
            await _context.SaveChangesAsync();
            
            return await GetTournament(tournament.Id);
        }

        private readonly AppDbContext _context;

        public TournamentRepositoryPostgres(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Tournament?> GetTournament(string id)
        {
            return await _context.Tournaments
                .Include(t => t.Organizer)
                .Include(t => t.Participants)
                .Include(t => t.Moderators)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<List<Tournament>> GetAll()
        {
            return await _context.Tournaments.Include(t => t.Organizer).ToListAsync();
        }

        public async Task<List<Tournament>> GetCurrent()
        {
            var now = DateTime.UtcNow;
            return await _context.Tournaments
                .Where(t => t.StartDate <= now && t.EndDate >= now)
                .Include(t => t.Organizer)
                .ToListAsync();
        }

        public async Task<List<Tournament>> GetUpcoming()
        {
            var now = DateTime.UtcNow;
            return await _context.Tournaments
                .Where(t => t.StartDate > now)
                .Include(t => t.Organizer)
                .ToListAsync();
        }

        public async Task<bool> Delete(string id)
        {
            var existing = await _context.Tournaments.FindAsync(id);
            if (existing == null) return false;

            _context.Tournaments.Remove(existing);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<Tournament?> Update(Tournament tournament)
        {
            var existing = await _context.Tournaments.FindAsync(tournament.Id);
            if (existing == null) return null;

            _context.Entry(existing).CurrentValues.SetValues(tournament);
            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task<Match?> GetMatchById(string matchId)
        {
            return await _context.Matches.FirstOrDefaultAsync(m => m.Id == matchId);
        }

        public async Task UpdateMatch(Match match)
        {
            _context.Matches.Update(match);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> UpdateParticipant(TournamentParticipant participant)
        {
            var existing = await _context.TournamentParticipants.FindAsync(participant.TournamentId, participant.UserId);
            if (existing == null) return false;

            _context.Entry(existing).CurrentValues.SetValues(participant);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task AddParticipant(TournamentParticipant participant)
        {
            await _context.TournamentParticipants.AddAsync(participant);
            await _context.SaveChangesAsync();
        }

        public async Task<TournamentParticipant?> GetParticipant(string tournamentId, string userId)
        {
            return await _context.TournamentParticipants.FindAsync(tournamentId, userId);
        }

        public async Task<bool> DeleteParticipant(string tournamentId, string userId)
        {
            var entity = await _context.TournamentParticipants.FindAsync(tournamentId, userId);
            if (entity == null) return false;

            _context.TournamentParticipants.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<Match>> GetMatchesByRound(string tournamentId, int roundNumber)
        {
            return await _context.Matches
                .Where(m => m.TournamentId == tournamentId && m.RoundNumber == roundNumber)
                .ToListAsync();
        }

        public async Task<List<TournamentParticipant>> GetParticipants(string tournamentId)
        {
            return await _context.TournamentParticipants
                .Where(tp => tp.TournamentId == tournamentId)
                .ToListAsync();
        }

        public async Task<List<Match>> GetAllMatches(string tournamentId)
        {
            return await _context.Matches
                .Where(m => m.TournamentId == tournamentId)
                .ToListAsync();
        }

        public async Task SaveMatches(IEnumerable<Match> matches)
        {
            await _context.Matches.AddRangeAsync(matches);
            await _context.SaveChangesAsync();
        }

        public async Task<(List<Tournament> Data, int TotalCount)> GetFiltered(TournamentFilterParams filterParams)
        {
            var query = _context.Tournaments
                .Include(t => t.Organizer)
                .Include(t => t.Participants)
                .AsQueryable();

            // 1. Filtering
            if (!string.IsNullOrWhiteSpace(filterParams.SearchTerm))
            {
                var term = filterParams.SearchTerm.ToLower();
                query = query.Where(t => t.Name.ToLower().Contains(term) || 
                                       (t.City != null && t.City.ToLower().Contains(term)));
            }

            if (string.IsNullOrWhiteSpace(filterParams.SearchTerm) && !string.IsNullOrWhiteSpace(filterParams.City) && filterParams.City != "all" && !filterParams.Radius.HasValue)
            {
                var city = filterParams.City.ToLower();
                query = query.Where(t => t.City != null && t.City.ToLower().Contains(city));
            }


            if (!string.IsNullOrWhiteSpace(filterParams.GameCode) && filterParams.GameCode != "all")
            {
                query = query.Where(t => t.GameCode == filterParams.GameCode);
            }

            if (!string.IsNullOrWhiteSpace(filterParams.Status) && filterParams.Status != "all")
            {
                // Simple status map if needed, assuming match with Domain enum for now or string equality
                // But Domain.ValueObjects.TournamentStatus is an Enum. 
                // For MVP, if status matches enum name
                if (Enum.TryParse<TournamentStatus>(filterParams.Status.ToUpper(), out var statusEnum))
                {
                    query = query.Where(t => t.Status == statusEnum);
                }
            }

            // 2. Distance Filtering (Haversine)
            if (filterParams.Lat.HasValue && filterParams.Lng.HasValue && filterParams.Radius.HasValue)
            {
                var lat = filterParams.Lat.Value;
                var lng = filterParams.Lng.Value;
                var radius = filterParams.Radius.Value;

                // Haversine formula simplified for EF Core / Npgsql (KM)
                query = query.Where(t => t.Lat.HasValue && t.Lng.HasValue && 
                    6371 * 2 * Math.Asin(Math.Sqrt(
                        Math.Pow(Math.Sin((t.Lat.Value - lat) * Math.PI / 180 / 2), 2) +
                        Math.Cos(lat * Math.PI / 180) * Math.Cos(t.Lat.Value * Math.PI / 180) *
                        Math.Pow(Math.Sin((t.Lng.Value - lng) * Math.PI / 180 / 2), 2)
                    )) <= radius);
            }

            var totalCount = await query.CountAsync();

            // 3. Sorting (Initial sort, Service will refine for Relevance)
            if (filterParams.SortBy == "date-asc") query = query.OrderBy(t => t.StartDate);
            else if (filterParams.SortBy == "date-desc") query = query.OrderByDescending(t => t.StartDate);
            else if (filterParams.SortBy == "popular") query = query.OrderByDescending(t => t.ViewCount);
            else query = query.OrderByDescending(t => t.StartDate); // Default

            // 4. Pagination
            var data = await query
                .Skip((filterParams.Page - 1) * filterParams.Limit)
                .Take(filterParams.Limit)
                .ToListAsync();

            return (data, totalCount);
        }

        public async Task AddActivity(UserActivity activity)
        {
            await _context.Set<UserActivity>().AddAsync(activity);
            await _context.SaveChangesAsync();
        }

        public async Task IncrementViewCount(string tournamentId)
        {
            var tournament = await _context.Tournaments.FindAsync(tournamentId);
            if (tournament != null)
            {
                tournament.ViewCount++;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<UserActivity>> GetUserActivities(string userId, int limit = 50)
        {
            return await _context.Set<UserActivity>()
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.Timestamp)
                .Take(limit)
                .ToListAsync();
        }

        public async Task AddMatchResultAudit(MatchResultAudit audit)
        {
            await _context.MatchResultAudits.AddAsync(audit);
            await _context.SaveChangesAsync();
        }

        public async Task<List<MatchResultAudit>> GetMatchResultAudits(string tournamentId)
        {
            return await _context.MatchResultAudits
                .Where(a => a.TournamentId == tournamentId)
                .OrderByDescending(a => a.ModifiedAt)
                .ToListAsync();
        }

        public async Task AddTournamentAuditAsync(TournamentAuditLog log)
        {
            await _context.TournamentAuditLogs.AddAsync(log);
            await _context.SaveChangesAsync();
        }

        public async Task<List<TournamentAuditLog>> GetTournamentAuditsAsync(string tournamentId)
        {
            return await _context.TournamentAuditLogs
                .Where(a => a.TournamentId == tournamentId)
                .OrderByDescending(a => a.Timestamp)
                .ToListAsync();
        }

        public async Task<List<Tournament>> GetTournamentsByUserId(string userId)
        {
            return await _context.Tournaments
                .Include(t => t.Organizer)
                // Filter where user is Organizer OR is in the Participants list (check Participant entity or Participants collection if joined)
                // Since we have TournamentParticipants table, it's safer to query via that or Include Participants.
                // However, navigation property `Participants` in `Tournament` entity (List<User>) is configured.
                .Where(t => t.OrganizerId == userId || t.Participants.Any(p => p.Id == userId))
                .OrderByDescending(t => t.StartDate)
                .ToListAsync();
        }
    }
}
