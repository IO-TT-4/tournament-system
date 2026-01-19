using GFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GFlow.Infrastructure.Persistance.Migrations
{
    public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Tournament> Tournaments => Set<Tournament>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Tournament>()
            .HasOne(t => t.Organizer)
            .WithMany(u => u.OrganizedTournaments)
            .HasForeignKey(t => t.OrganizerId);

        modelBuilder.Entity<Tournament>()
            .HasMany(t => t.Participants)
            .WithMany(u => u.ParticipatedTournaments);
            
        base.OnModelCreating(modelBuilder);
    }
}
}