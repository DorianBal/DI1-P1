using System.Text.Json;
using System.Text.Json.Nodes;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

using Server.Models;

namespace Server.Persistence;

public class WssDbContext(DbContextOptions options, IConfiguration configuration) : DbContext(options)
{
    public DbSet<Company> Companies { get; set; } = null!;
    public DbSet<Consultant> Consultants { get; set; } = null!;
    public DbSet<Employee> Employees { get; set; } = null!;
    public DbSet<Project> Projects { get; set; } = null!;
    public DbSet<Game> Games { get; set; } = null!;
    public DbSet<Player> Players { get; set; } = null!;
    public DbSet<Round> Rounds { get; set; } = null!;
    public DbSet<Skill> Skills { get; set; } = null!;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var dbOptions = configuration.GetSection("Database");

        var dbHost = dbOptions.GetValue<string>("Host");
        var dbPort = dbOptions.GetValue<string>("Port");
        var dbName = dbOptions.GetValue<string>("Name");
        var dbUser = dbOptions.GetValue<string>("User");
        var dbPass = dbOptions.GetValue<string>("Pass");

        var dbConnectionString = $"Host={dbHost};Port={dbPort};Db={dbName};Username={dbUser};Password={dbPass}";

        optionsBuilder.UseNpgsql(dbConnectionString);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Company>(e =>
        {
            e.ToTable("companies");
            e.HasKey(e => e.Id);
            e.Property(e => e.Name).HasColumnType("varchar(255)");
            e.Property(e => e.Treasury).HasColumnType("integer").HasDefaultValue(85000);
            e.HasOne(e => e.Player)
                .WithOne(e => e.Company)
                .HasForeignKey<Company>(e => e.PlayerId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);
            e.HasMany(e => e.Employees)
                .WithOne(e => e.Company)
                .HasForeignKey(e => e.CompanyId);
        });



        modelBuilder.Entity<Employee>(e =>
        {
            e.ToTable("employees");
            e.HasKey(e => e.Id);
            e.Property(e => e.Name).HasColumnType("varchar(255)");
            e.HasOne(e => e.Game)
                .WithMany()
                .HasForeignKey(e => e.GameId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(e => e.Company)
                .WithMany(e => e.Employees)
                .HasForeignKey(e => e.CompanyId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);
            e.OwnsMany(e => e.Skills, builder => builder.ToJson());
        });

        modelBuilder.Entity<Consultant>(e =>
        {
            e.ToTable("consultants");
            e.HasKey(e => e.Id);
            e.Property(e => e.Name).HasColumnType("varchar(255)");
            e.HasOne(e => e.Game)
                .WithMany()
                .HasForeignKey(e => e.GameId)
                .OnDelete(DeleteBehavior.Cascade);
            e.OwnsMany(e => e.Skills, builder => builder.ToJson());
        });

        modelBuilder.Entity<Project>(e =>
        {
            e.ToTable("projects");
            e.HasKey(e => e.Id);
            e.Property(e => e.Name).HasColumnType("varchar(255)");
            e.HasOne(e => e.Game)
                .WithMany()
                .HasForeignKey(e => e.GameId)
                .OnDelete(DeleteBehavior.Cascade);
            e.OwnsMany(e => e.Skills, builder => builder.ToJson());
        });

        modelBuilder.Entity<Game>(e =>
        {
            e.ToTable("games");
            e.HasKey(e => e.Id);
            e.Property(e => e.Name).HasColumnType("varchar(255)");
            e.Property(e => e.Rounds).HasColumnType("integer");
            e.Property(e => e.Status)
                .HasColumnType("varchar(255)")
                .HasDefaultValue(GameStatus.Waiting)
                .HasConversion(new EnumToStringConverter<GameStatus>());
            e.HasMany(e => e.Players)
                .WithOne(e => e.Game)
                .HasForeignKey(e => e.GameId);
            e.HasMany(e => e.Consultants)
                .WithOne(e => e.Game)
                .HasForeignKey(e => e.GameId);
            e.HasMany(e => e.Projects)
                .WithOne(e => e.Game)
                .HasForeignKey(e => e.GameId);
            e.HasMany(e => e.RoundsCollection)
                .WithOne(e => e.Game)
                .HasForeignKey(e => e.GameId);
        });

        modelBuilder.Entity<Player>(e =>
        {
            e.ToTable("players");
            e.HasKey(e => e.Id);
            e.Property(e => e.Name).HasColumnType("varchar(255)");
            e.HasOne(e => e.Game)
                .WithMany(e => e.Players)
                .HasForeignKey(e => e.GameId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(e => e.Company)
                .WithOne(e => e.Player)
                .HasForeignKey<Company>(e => e.PlayerId);
        });

        modelBuilder.Entity<Round>(e =>
        {
            e.ToTable("rounds");
            e.HasKey(e => e.Id);
            e.Property(e => e.Order).HasColumnType("integer");
            e.HasOne(e => e.Game)
                .WithMany(e => e.RoundsCollection)
                .HasForeignKey(e => e.GameId);
            e.OwnsMany(e => e.Actions, builder => builder.ToJson());
        });

        modelBuilder.Entity<Skill>(e =>
        {
            e.ToTable("skills");
            e.HasKey(e => e.Id);
            e.Property(e => e.Name).HasColumnType("varchar(255)");
            e.HasData(
                new Skill("Communication") { Id = 1 },
                new Skill("Programmation") { Id = 2 },
                new Skill("Reseau") { Id = 3 },
                new Skill("Cybersecurite") { Id = 4 },
                new Skill("Management") { Id = 5 }
            );
        });
    }
}
