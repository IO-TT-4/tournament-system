using GFlow.Application.Ports;
using GFlow.Domain.Entities;
using GFlow.Infrastructure.Persistance.Migrations;
using Microsoft.EntityFrameworkCore;

namespace GFlow.Infrastructure.Persistance.Repositories
{
    public class TournamentRepositoryPostgres : ITournamentRepository
    {

        public Tournament? Add(Tournament tournament)
        {
            _context.Tournaments.Add(tournament);
            _context.SaveChanges();
            return tournament;
        }

    private readonly AppDbContext _context;

    public TournamentRepositoryPostgres(AppDbContext context)
    {
        _context = context;
    }

    public Tournament? Get(string id)
    {
        return _context.Tournaments
            .Include(t => t.Organizer)
            .Include(t => t.Participants)
            .FirstOrDefault(t => t.Id == id);
    }

    public List<Tournament> GetAll()
    {
        return _context.Tournaments.Include(t => t.Organizer).ToList();
    }

    public List<Tournament> GetCurrent()
    {
        var now = DateTime.UtcNow;
        return _context.Tournaments
            .Where(t => t.StartDate <= now && t.EndDate >= now)
            .Include(t => t.Organizer)
            .ToList();
    }

    public List<Tournament> GetUpcoming()
    {
        var now = DateTime.UtcNow;
        return _context.Tournaments
            .Where(t => t.StartDate > now)
            .Include(t => t.Organizer)
            .ToList();
    }

    public Tournament? Update(Tournament tournament)
    {
        var existing = _context.Tournaments.Find(tournament.Id);
        if (existing == null) return null;

        _context.Entry(existing).CurrentValues.SetValues(tournament);
        _context.SaveChanges();
        return existing;
    }
}
}