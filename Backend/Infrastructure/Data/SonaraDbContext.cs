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

        modelBuilder.Entity<Track>(entity =>
        {
            entity.HasIndex(t => t.Title).HasDatabaseName("IX_Tracks_Title");
            entity.HasIndex(t => t.PlaysCount).HasDatabaseName("IX_Tracks_PlaysCount");
            entity.HasIndex(t => t.CreatedAt).HasDatabaseName("IX_Tracks_CreatedAt");
        });

        modelBuilder.Entity<Album>(entity =>
            entity.HasIndex(a => a.Title).HasDatabaseName("IX_Albums_Title"));

        modelBuilder.Entity<Artist>(entity =>
            entity.HasIndex(a => a.Name).HasDatabaseName("IX_Artists_Name"));

        modelBuilder.Entity<Playlist>(entity =>
        {
            entity.HasIndex(p => p.Name).HasDatabaseName("IX_Playlists_Name");
            entity.HasIndex(p => new { p.UserId, p.CreatedAt }).HasDatabaseName("IX_Playlists_UserId_CreatedAt");
        });

        modelBuilder.Entity<PlaylistTrack>(entity =>
            entity.HasIndex(pt => new { pt.PlaylistId, pt.AddedAt }).HasDatabaseName("IX_PlaylistTracks_PlaylistId_AddedAt"));

        modelBuilder.Entity<LikedTrack>(entity =>
            entity.HasIndex(l => new { l.UserId, l.LikedAt }).HasDatabaseName("IX_LikedTracks_UserId_LikedAt"));

        modelBuilder.Entity<ListeningHistory>(entity =>
            entity.HasIndex(h => new { h.UserId, h.ListenedAt }).HasDatabaseName("IX_ListeningHistories_UserId_ListenedAt"));

        SeedDataExtension.Seed(modelBuilder);
    }

    public DbSet<Album> Albums { get; set; }
    public DbSet<Genre> Genres { get; set; }
    public DbSet<ListeningHistory> ListeningHistories { get; set; }
    public DbSet<Track> Tracks { get; set; }
    public DbSet<TrackGenre> TrackGenres { get; set; }

    public DbSet<Playlist> Playlists { get; set; }
    public DbSet<LikedTrack> LikedTracks { get; set; }
    public DbSet<PlaylistTrack> PlaylistTracks { get; set; }

    public DbSet<Podcast> Podcasts { get; set; }
    public DbSet<PodcastEpisode> PodcastEpisodes { get; set; }

    public DbSet<Follower> Followers { get; set; }
    public DbSet<ListeningRoom> ListeningRooms { get; set; }
    public DbSet<RoomMember> RoomMembers { get; set; }

    public DbSet<Artist> Artists { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<Session> Sessions { get; set; }
    public DbSet<Subscription> Subscriptions { get; set; }
    public DbSet<User> Users { get; set; }
}
