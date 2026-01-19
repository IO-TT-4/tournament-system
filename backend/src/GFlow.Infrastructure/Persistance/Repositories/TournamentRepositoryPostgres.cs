using System.Runtime.CompilerServices;
using GFlow.Application.Ports;
using GFlow.Domain.Entities;
using GFlow.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace GFlow.Infrastructure.Persistance.Repositories
{
    public class TournamentRepositoryPostgres : ITournamentRepository
    {

        public async Task<Tournament> Add(Tournament tournament)
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

        public async Task<Tournament> GetTournament(string id)
        {
            return await _context.Tournaments
                .Include(t => t.Organizer)
                .Include(t => t.Participants)
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

        public async Task<Tournament> Update(Tournament tournament)
        {
            var existing = await _context.Tournaments.FindAsync(tournament.Id);
            if (existing == null) return null;

            _context.Entry(existing).CurrentValues.SetValues(tournament);
            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task<Match> GetMatchById(string matchId)
        {
            return await _context.Matches.FirstOrDefaultAsync(m => m.Id == matchId);
        }

        public async Task<AsyncVoidMethodBuilder> UpdateMatch(Match match)
        {
            _context.Matches.Update(match);
            await _context.SaveChangesAsync();
            return await Task.FromResult(new AsyncVoidMethodBuilder());
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

        public async Task<AsyncVoidMethodBuilder> SaveMatches(IEnumerable<Match> matches)
        {
            await _context.Matches.AddRangeAsync(matches);
            await _context.SaveChangesAsync();
            return await Task.FromResult(new AsyncVoidMethodBuilder());
        }
    }
}