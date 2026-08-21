using Domain.Entities.Music;
using Domain.Entities.Playlists;
using Domain.Entities.Podcasts;
using Domain.Entities.Social;
using Domain.Entities.Users;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

public class SonaraDbContext : DbContext
{
    public SonaraDbContext(DbContextOptions<SonaraDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // 1. Конфігурація для соціальної мережі (Підписники)
        modelBuilder.Entity<Follower>(entity =>
        {
            entity.HasKey(f => new { f.FollowerId, f.FollowedId });

            entity.HasOne(f => f.FollowerUser)
                  .WithMany(u => u.Following)
                  .HasForeignKey(f => f.FollowerId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(f => f.FollowedUser)
                  .WithMany(u => u.Followers)
                  .HasForeignKey(f => f.FollowedId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // 2. Конфігурація для зв'язку Треків та Жанрів
        modelBuilder.Entity<TrackGenre>(entity =>
        {
            entity.HasKey(tg => new { tg.TrackId, tg.GenreId });

            entity.HasOne(tg => tg.Track)
                  .WithMany(t => t.TrackGenres) 
                  .HasForeignKey(tg => tg.TrackId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(tg => tg.Genre)
                  .WithMany(g => g.TrackGenres) 
                  .HasForeignKey(tg => tg.GenreId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // 3. Конфігурація для зв'язку Плейлістів та Треків
        modelBuilder.Entity<PlaylistTrack>(entity =>
        {
            entity.HasKey(pt => new { pt.PlaylistId, pt.TrackId });

            entity.HasOne(pt => pt.Playlist)
                  .WithMany(p => p.PlaylistTracks) 
                  .HasForeignKey(pt => pt.PlaylistId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(pt => pt.Track)
                  .WithMany(t => t.PlaylistTracks) 
                  .HasForeignKey(pt => pt.TrackId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // 4. Конфігурація для Улюблених треків (LikedTrack)
        modelBuilder.Entity<LikedTrack>(entity =>
        {
            entity.HasKey(lt => new { lt.UserId, lt.TrackId });

            entity.HasOne(lt => lt.User)
                  .WithMany(u => u.LikedTracks)
                  .HasForeignKey(lt => lt.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(lt => lt.Track)
                  .WithMany(t => t.LikedByUsers)
                  .HasForeignKey(lt => lt.TrackId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // 5. Конфігурація для Учасників кімнат (RoomMember)
        modelBuilder.Entity<RoomMember>(entity =>
        {
            entity.HasKey(rm => new { rm.RoomId, rm.UserId });

            entity.HasOne(rm => rm.Room)
                  .WithMany(r => r.Members)
                  .HasForeignKey(rm => rm.RoomId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(rm => rm.User)
                  .WithMany(u => u.JoinedRooms)
                  .HasForeignKey(rm => rm.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // 6. Конфігурація для Підписок (UserSubscription)
        modelBuilder.Entity<UserSubscription>(entity =>
        {
            entity.HasKey(us => us.Id);

            // Зв'язок із власником підписки
            entity.HasOne(us => us.Owner)
                  .WithMany()
                  .HasForeignKey(us => us.OwnerId)
                  .OnDelete(DeleteBehavior.Restrict);

            // Зв'язок із тарифним планом
            entity.HasOne(us => us.Plan)
                  .WithMany()
                  .HasForeignKey(us => us.PlanId)
                  .OnDelete(DeleteBehavior.Restrict);

            // Зв'язок "один до багатьох" для учасників підписки (Duo / Family)
            entity.HasMany(us => us.Members)
                  .WithOne(u => u.ActiveSubscription)
                  .HasForeignKey(u => u.ActiveSubscriptionId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        SeedDataExtension.Seed(modelBuilder);
    }

    // Music entities
    public DbSet<Album> Albums { get; set; }
    public DbSet<Genre> Genres { get; set; }
    public DbSet<ListeningHistory> ListeningHistories { get; set; }
    public DbSet<Track> Tracks { get; set; }
    public DbSet<TrackGenre> TrackGenres { get; set; }

    // Playlist entities
    public DbSet<Playlist> Playlists { get; set; }
    public DbSet<LikedTrack> LikedTracks { get; set; }
    public DbSet<PlaylistTrack> PlaylistTracks { get; set; }

    // Podcast entities
    public DbSet<Podcast> Podcasts { get; set; }
    public DbSet<PodcastEpisode> PodcastEpisodes { get; set; }

    // Social entities
    public DbSet<Follower> Followers { get; set; }
    public DbSet<ListeningRoom> ListeningRooms { get; set; }
    public DbSet<RoomMember> RoomMembers { get; set; }

    // User entities
    public DbSet<Artist> Artists { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<Session> Sessions { get; set; }    
    public DbSet<SubscriptionPlan> SubscriptionPlans { get; set; }
    public DbSet<UserSubscription> UserSubscriptions { get; set; }
    public DbSet<User> Users { get; set; }
}