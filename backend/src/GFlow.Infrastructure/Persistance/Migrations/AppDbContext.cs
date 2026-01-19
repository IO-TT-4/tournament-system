using GFlow.Domain.Entities;
using GFlow.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace GFlow.Infrastructure.Persistance
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users => Set<User>();
        public DbSet<Tournament> Tournaments => Set<Tournament>();
        public DbSet<Match> Matches => Set<Match>();

        public DbSet<TournamentParticipant> TournamentParticipants => Set<TournamentParticipant>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // 1. Relacja Organizator - Turnieje
            modelBuilder.Entity<Tournament>()
                .HasOne(t => t.Organizer)
                .WithMany(u => u.OrganizedTournaments)
                .HasForeignKey(t => t.OrganizerId);

            // 2. Many-to-Many: Uczestnicy Turnieju
            modelBuilder.Entity<Tournament>()
                .HasMany(t => t.Participants)
                .WithMany(u => u.ParticipatedTournaments);

            // W AppDbContext.cs
            

            // W OnModelCreating
            modelBuilder.Entity<TournamentParticipant>()
                .HasKey(tp => new { tp.TournamentId, tp.UserId }); // Klucz kompozytowy

            // 3. Konfiguracja Match
            modelBuilder.Entity<Match>(entity =>
            {
                entity.HasKey(m => m.Id);

                // Relacja z turniejem
                entity.HasOne<Tournament>()
                    .WithMany()
                    .HasForeignKey(m => m.TournamentId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Relacje z User (Home i Away)
                // Musimy wyłączyć DeleteBehavior.Cascade, bo SQL Server/SQLite 
                // nie pozwoli na dwie ścieżki kaskadowe do tej samej tabeli.
                entity.HasOne<User>()
                    .WithMany()
                    .HasForeignKey(m => m.PlayerHomeId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne<User>()
                    .WithMany()
                    .HasForeignKey(m => m.PlayerAwayId)
                    .OnDelete(DeleteBehavior.Restrict);

                // 4. Konfiguracja Value Object: MatchResult (Owned Type)
                entity.OwnsOne(m => m.Result, result =>
                {
                    result.Property(r => r.ScoreA).HasColumnName("ScoreA");
                    result.Property(r => r.ScoreB).HasColumnName("ScoreB");
                    
                    // Zapisujemy Enum jako string dla czytelności w DB
                    result.Property(r => r.FinishType)
                          .HasColumnName("FinishType")
                          .HasConversion<string>();
                });
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}