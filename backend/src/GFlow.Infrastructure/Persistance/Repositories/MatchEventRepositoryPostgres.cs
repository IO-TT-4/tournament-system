using GFlow.Application.Ports;
using GFlow.Domain.Entities;
using GFlow.Infrastructure.Persistance;
using Microsoft.EntityFrameworkCore;

namespace GFlow.Infrastructure.Persistance.Repositories
{
    public class MatchEventRepositoryPostgres : IMatchEventRepository
    {
        private readonly AppDbContext _context;

        public MatchEventRepositoryPostgres(AppDbContext context)
        {
            _context = context;
        }

        public async Task<MatchEvent> Add(MatchEvent matchEvent)
        {
            await _context.MatchEvents.AddAsync(matchEvent);
            await _context.SaveChangesAsync();
            return matchEvent;
        }

        public async Task<List<MatchEvent>> GetByMatch(string matchId)
        {
            return await _context.MatchEvents
                .Where(e => e.MatchId == matchId)
                .OrderBy(e => e.Timestamp)
                .ToListAsync();
        }
    }
}
