using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Parser.Entities;

public partial class GamechatContext : DbContext
{
    public GamechatContext()
    {
    }

    public GamechatContext(DbContextOptions<GamechatContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Comment> Comments { get; set; }

    public virtual DbSet<Game> Games { get; set; }

    public virtual DbSet<GameJenre> GameJenres { get; set; }

    public virtual DbSet<GamePlatform> GamePlatforms { get; set; }

    public virtual DbSet<Gamelink> Gamelinks { get; set; }

    public virtual DbSet<Jenre> Jenres { get; set; }

    public virtual DbSet<Platform> Platforms { get; set; }

    public virtual DbSet<Theme> Themes { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseMySql("server=localhost;user=root;password=1234;database=gamechat;charset=utf8", Microsoft.EntityFrameworkCore.ServerVersion.Parse("8.0.29-mysql"));

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb4_0900_ai_ci")
            .HasCharSet("utf8mb4");

        modelBuilder.Entity<Comment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("comment");

            entity.HasIndex(e => e.ThemeId, "FK_comment_themeId2");

            entity.HasIndex(e => e.UserId, "FK_comment_userId");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CommentId).HasColumnName("commentId");
            entity.Property(e => e.Date)
                .HasMaxLength(255)
                .HasColumnName("date");
            entity.Property(e => e.Text)
                .HasMaxLength(15000)
                .HasColumnName("text");
            entity.Property(e => e.ThemeId).HasColumnName("themeId");
            entity.Property(e => e.UserId).HasColumnName("userId");
            entity.Property(e => e.UserNickname)
                .HasMaxLength(255)
                .HasColumnName("userNickname");

            entity.HasOne(d => d.Theme).WithMany(p => p.Comments)
                .HasForeignKey(d => d.ThemeId)
                .HasConstraintName("FK_comment_themeId2");

            entity.HasOne(d => d.User).WithMany(p => p.Comments)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_comment_userId");
        });

        modelBuilder.Entity<Game>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("game");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AvgRating)
                .HasMaxLength(255)
                .HasColumnName("avgRating");
            entity.Property(e => e.Date)
                .HasMaxLength(255)
                .HasColumnName("date");
            entity.Property(e => e.Picture).HasColumnName("picture");
            entity.Property(e => e.Title)
                .HasMaxLength(255)
                .HasColumnName("title");
        });

        modelBuilder.Entity<GameJenre>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("game_jenre");

            entity.HasIndex(e => e.GameId, "FK_game_jenre_game_id2");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.GameId).HasColumnName("game_id");
            entity.Property(e => e.JenreId).HasColumnName("jenre_id");
        });

        modelBuilder.Entity<GamePlatform>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("game_platform");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.GameId).HasColumnName("game_id");
            entity.Property(e => e.PlatformId).HasColumnName("platform_id");
        });

        modelBuilder.Entity<Gamelink>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("gamelink");

            entity.HasIndex(e => e.Link, "link").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Link).HasColumnName("link");
            entity.Property(e => e.Parsingdate)
                .HasColumnType("datetime")
                .HasColumnName("parsingdate");
            entity.Property(e => e.Requestdate)
                .HasColumnType("datetime")
                .HasColumnName("requestdate");
        });

        modelBuilder.Entity<Jenre>(entity =>
        {
            entity.HasKey(e => e.Title).HasName("PRIMARY");

            entity.ToTable("jenre");

            entity.Property(e => e.Title)
                .HasDefaultValueSql("''")
                .HasColumnName("title");
            entity.Property(e => e.Id).HasColumnName("id");
        });

        modelBuilder.Entity<Platform>(entity =>
        {
            entity.HasKey(e => e.Title).HasName("PRIMARY");

            entity.ToTable("platform");

            entity.Property(e => e.Title)
                .HasDefaultValueSql("''")
                .HasColumnName("title");
            entity.Property(e => e.Id).HasColumnName("id");
        });

        modelBuilder.Entity<Theme>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("theme");

            entity.HasIndex(e => e.GameId, "FK_theme_gameId");

            entity.HasIndex(e => e.UserId, "FK_theme_userId");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.GameId).HasColumnName("gameId");
            entity.Property(e => e.Title)
                .HasMaxLength(255)
                .HasColumnName("title");
            entity.Property(e => e.UserId).HasColumnName("userId");
            entity.Property(e => e.UserNickname)
                .HasMaxLength(255)
                .HasColumnName("userNickname");

            entity.HasOne(d => d.Game).WithMany(p => p.Themes)
                .HasForeignKey(d => d.GameId)
                .HasConstraintName("FK_theme_gameId");

            entity.HasOne(d => d.User).WithMany(p => p.Themes)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_theme_userId");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("user");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.EMail)
                .HasMaxLength(255)
                .HasColumnName("e-mail");
            entity.Property(e => e.Login)
                .HasMaxLength(255)
                .HasColumnName("login");
            entity.Property(e => e.Nickname)
                .HasMaxLength(255)
                .HasColumnName("nickname");
            entity.Property(e => e.Password)
                .HasMaxLength(255)
                .HasColumnName("password");
            entity.Property(e => e.ProfilePicture).HasColumnName("profilePicture");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
