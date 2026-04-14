using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using ProblemReportingSystem.DAL.Entities;

namespace ProblemReportingSystem.DAL.Infrastructure;

public partial class ProblemReportingSystemDbContext : DbContext
{
    public ProblemReportingSystemDbContext(DbContextOptions<ProblemReportingSystemDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Address> Addresses { get; set; }

    public virtual DbSet<Admin> Admins { get; set; }

    public virtual DbSet<Appeal> Appeals { get; set; }

    public virtual DbSet<CityCouncil> CityCouncils { get; set; }

    public virtual DbSet<CouncilEmployee> CouncilEmployees { get; set; }

    public virtual DbSet<Poll> Polls { get; set; }

    public virtual DbSet<PollOption> PollOptions { get; set; }

    public virtual DbSet<PollVote> PollVotes { get; set; }

    public virtual DbSet<Problem> Problems { get; set; }

    public virtual DbSet<ProblemCategory> ProblemCategories { get; set; }

    public virtual DbSet<ProblemPhoto> ProblemPhotos { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<VCouncilEfficiencyReport> VCouncilEfficiencyReports { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Address>(entity =>
        {
            entity.HasKey(e => e.AddressId).HasName("addresses_pkey");

            entity.ToTable("addresses");

            entity.HasIndex(e => new { e.Latitude, e.Longitude }, "idx_addresses_coordinates");

            entity.Property(e => e.AddressId)
                .ValueGeneratedNever()
                .HasColumnName("address_id");
            entity.Property(e => e.BuildingNumber)
                .HasMaxLength(20)
                .HasColumnName("building_number");
            entity.Property(e => e.City)
                .HasMaxLength(100)
                .HasColumnName("city");
            entity.Property(e => e.Latitude)
                .HasPrecision(9, 6)
                .HasColumnName("latitude");
            entity.Property(e => e.Longitude)
                .HasPrecision(9, 6)
                .HasColumnName("longitude");
            entity.Property(e => e.Street)
                .HasMaxLength(100)
                .HasColumnName("street");
        });

        modelBuilder.Entity<Admin>(entity =>
        {
            entity.HasKey(e => e.AdminId).HasName("admins_pkey");

            entity.ToTable("admins");

            entity.HasIndex(e => e.UserId, "admins_user_id_key").IsUnique();

            entity.Property(e => e.AdminId)
                .ValueGeneratedNever()
                .HasColumnName("admin_id");
            entity.Property(e => e.AccessLevel)
                .HasMaxLength(50)
                .HasColumnName("access_level");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithOne(p => p.Admin)
                .HasForeignKey<Admin>(d => d.UserId)
                .HasConstraintName("admins_user_id_fkey");
        });

        modelBuilder.Entity<Appeal>(entity =>
        {
            entity.HasKey(e => e.AppealId).HasName("appeals_pkey");

            entity.ToTable("appeals");

            entity.Property(e => e.AppealId)
                .ValueGeneratedNever()
                .HasColumnName("appeal_id");
            entity.Property(e => e.AssignedEmployeeId).HasColumnName("assigned_employee_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.ProblemId).HasColumnName("problem_id");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("updated_at");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.AssignedEmployee).WithMany(p => p.Appeals)
                .HasForeignKey(d => d.AssignedEmployeeId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("appeals_assigned_employee_id_fkey");

            entity.HasOne(d => d.Problem).WithMany(p => p.Appeals)
                .HasForeignKey(d => d.ProblemId)
                .HasConstraintName("appeals_problem_id_fkey");

            entity.HasOne(d => d.User).WithMany(p => p.Appeals)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("appeals_user_id_fkey");
        });

        modelBuilder.Entity<CityCouncil>(entity =>
        {
            entity.HasKey(e => e.CouncilId).HasName("city_councils_pkey");

            entity.ToTable("city_councils");

            entity.Property(e => e.CouncilId)
                .ValueGeneratedNever()
                .HasColumnName("council_id");
            entity.Property(e => e.AddressId).HasColumnName("address_id");
            entity.Property(e => e.ContactEmail)
                .HasMaxLength(100)
                .HasColumnName("contact_email");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");

            entity.HasOne(d => d.Address).WithMany(p => p.CityCouncils)
                .HasForeignKey(d => d.AddressId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("city_councils_address_id_fkey");
        });

        modelBuilder.Entity<CouncilEmployee>(entity =>
        {
            entity.HasKey(e => e.EmployeeId).HasName("council_employees_pkey");

            entity.ToTable("council_employees");

            entity.HasIndex(e => e.UserId, "council_employees_user_id_key").IsUnique();

            entity.Property(e => e.EmployeeId)
                .ValueGeneratedNever()
                .HasColumnName("employee_id");
            entity.Property(e => e.CouncilId).HasColumnName("council_id");
            entity.Property(e => e.Position)
                .HasMaxLength(100)
                .HasColumnName("position");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Council).WithMany(p => p.CouncilEmployees)
                .HasForeignKey(d => d.CouncilId)
                .HasConstraintName("council_employees_council_id_fkey");

            entity.HasOne(d => d.User).WithOne(p => p.CouncilEmployee)
                .HasForeignKey<CouncilEmployee>(d => d.UserId)
                .HasConstraintName("council_employees_user_id_fkey");
        });

        modelBuilder.Entity<Poll>(entity =>
        {
            entity.HasKey(e => e.PollId).HasName("polls_pkey");

            entity.ToTable("polls");

            entity.HasIndex(e => new { e.IsActive, e.PublishedAt }, "idx_polls_active");

            entity.Property(e => e.PollId)
                .ValueGeneratedNever()
                .HasColumnName("poll_id");
            entity.Property(e => e.CouncilId).HasColumnName("council_id");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.PublishedAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("published_at");
            entity.Property(e => e.Title)
                .HasMaxLength(255)
                .HasColumnName("title");

            entity.HasOne(d => d.Council).WithMany(p => p.Polls)
                .HasForeignKey(d => d.CouncilId)
                .HasConstraintName("polls_council_id_fkey");
        });

        modelBuilder.Entity<PollOption>(entity =>
        {
            entity.HasKey(e => e.OptionId).HasName("poll_options_pkey");

            entity.ToTable("poll_options");

            entity.Property(e => e.OptionId)
                .ValueGeneratedNever()
                .HasColumnName("option_id");
            entity.Property(e => e.OptionText)
                .HasMaxLength(255)
                .HasColumnName("option_text");
            entity.Property(e => e.PollId).HasColumnName("poll_id");

            entity.HasOne(d => d.Poll).WithMany(p => p.PollOptions)
                .HasForeignKey(d => d.PollId)
                .HasConstraintName("poll_options_poll_id_fkey");
        });

        modelBuilder.Entity<PollVote>(entity =>
        {
            entity.HasKey(e => e.VoteId).HasName("poll_votes_pkey");

            entity.ToTable("poll_votes");

            entity.HasIndex(e => new { e.UserId, e.OptionId }, "unique_user_vote").IsUnique();

            entity.Property(e => e.VoteId)
                .ValueGeneratedNever()
                .HasColumnName("vote_id");
            entity.Property(e => e.OptionId).HasColumnName("option_id");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.VotedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("voted_at");

            entity.HasOne(d => d.Option).WithMany(p => p.PollVotes)
                .HasForeignKey(d => d.OptionId)
                .HasConstraintName("poll_votes_option_id_fkey");

            entity.HasOne(d => d.User).WithMany(p => p.PollVotes)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("poll_votes_user_id_fkey");
        });

        modelBuilder.Entity<Problem>(entity =>
        {
            entity.HasKey(e => e.ProblemId).HasName("problems_pkey");

            entity.ToTable("problems");

            entity.HasIndex(e => e.Status, "idx_problems_status");

            entity.Property(e => e.ProblemId)
                .ValueGeneratedNever()
                .HasColumnName("problem_id");
            entity.Property(e => e.AddressId).HasColumnName("address_id");
            entity.Property(e => e.CategoryId).HasColumnName("category_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasDefaultValueSql("'Pending'::character varying")
                .HasColumnName("status");
            entity.Property(e => e.Title)
                .HasMaxLength(255)
                .HasColumnName("title");

            entity.HasOne(d => d.Address).WithMany(p => p.Problems)
                .HasForeignKey(d => d.AddressId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("problems_address_id_fkey");

            entity.HasOne(d => d.Category).WithMany(p => p.Problems)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("problems_category_id_fkey");
        });

        modelBuilder.Entity<ProblemCategory>(entity =>
        {
            entity.HasKey(e => e.CategoryId).HasName("problem_categories_pkey");

            entity.ToTable("problem_categories");

            entity.Property(e => e.CategoryId)
                .ValueGeneratedNever()
                .HasColumnName("category_id");
            entity.Property(e => e.IconUrl)
                .HasMaxLength(255)
                .HasColumnName("icon_url");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
        });

        modelBuilder.Entity<ProblemPhoto>(entity =>
        {
            entity.HasKey(e => e.PhotoId).HasName("problem_photos_pkey");

            entity.ToTable("problem_photos");

            entity.Property(e => e.PhotoId)
                .ValueGeneratedNever()
                .HasColumnName("photo_id");
            entity.Property(e => e.PhotoUrl)
                .HasMaxLength(255)
                .HasColumnName("photo_url");
            entity.Property(e => e.ProblemId).HasColumnName("problem_id");

            entity.HasOne(d => d.Problem).WithMany(p => p.ProblemPhotos)
                .HasForeignKey(d => d.ProblemId)
                .HasConstraintName("problem_photos_problem_id_fkey");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("users_pkey");

            entity.ToTable("users");

            entity.HasIndex(e => e.Email, "idx_users_email");

            entity.HasIndex(e => e.Email, "users_email_key").IsUnique();

            entity.HasIndex(e => e.GoogleAuthId, "users_google_auth_id_key").IsUnique();

            entity.Property(e => e.UserId)
                .ValueGeneratedNever()
                .HasColumnName("user_id");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .HasColumnName("email");
            entity.Property(e => e.FullName)
                .HasMaxLength(255)
                .HasColumnName("full_name");
            entity.Property(e => e.GoogleAuthId)
                .HasMaxLength(255)
                .HasColumnName("google_auth_id");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
        });

        modelBuilder.Entity<VCouncilEfficiencyReport>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("v_council_efficiency_report");

            entity.Property(e => e.CouncilId).HasColumnName("council_id");
            entity.Property(e => e.CouncilName)
                .HasMaxLength(255)
                .HasColumnName("council_name");
            entity.Property(e => e.EfficiencyPercentage).HasColumnName("efficiency_percentage");
            entity.Property(e => e.ResolvedAppeals).HasColumnName("resolved_appeals");
            entity.Property(e => e.TotalAppeals).HasColumnName("total_appeals");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
