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
        
        public DbSet<MatchEvent> MatchEvents => Set<MatchEvent>();
        public DbSet<UserActivity> UserActivities => Set<UserActivity>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // UserActivity configuration
            modelBuilder.Entity<UserActivity>(entity =>
            {
                entity.HasKey(a => a.Id);
                entity.HasOne(a => a.User).WithMany().HasForeignKey(a => a.UserId);
                entity.HasOne(a => a.Tournament).WithMany().HasForeignKey(a => a.TournamentId);
            });

            // 1. Relacja Organizator - Turnieje
            modelBuilder.Entity<Tournament>()
                .HasOne(t => t.Organizer)
                .WithMany(u => u.OrganizedTournaments)
                .HasForeignKey(t => t.OrganizerId);

            // 2. Many-to-Many: Uczestnicy Turnieju
            modelBuilder.Entity<Tournament>()
                .HasMany(t => t.Participants)
                .WithMany(u => u.ParticipatedTournaments);

            // 3. Many-to-Many: Moderatorzy (bez nawigacji zwrotnej w User, lub można dodać)
            modelBuilder.Entity<Tournament>()
                 .HasMany(t => t.Moderators)
                 .WithMany()
                 .UsingEntity(j => j.ToTable("TournamentModerators"));

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

            // 5. Konfiguracja MatchEvent
            modelBuilder.Entity<MatchEvent>(entity =>
            {
                entity.HasKey(e => e.Id);
                
                entity.HasOne<Match>()
                    .WithMany(m => m.Events)
                    .HasForeignKey(e => e.MatchId)
                    .OnDelete(DeleteBehavior.Cascade);
                    
                entity.HasOne<User>()
                    .WithMany()
                    .HasForeignKey(e => e.RecordedBy)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}