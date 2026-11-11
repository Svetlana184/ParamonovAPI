using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using ParamonovAPI.Models;
using Task = ParamonovAPI.Models.Task;

namespace ParamonovAPI;

public partial class PostgresContext : DbContext
{
    public PostgresContext()
    {
    }

    public PostgresContext(DbContextOptions<PostgresContext> options)
        : base(options)
    {
    }

    public virtual DbSet<ChangeRequest> ChangeRequests { get; set; }

    public virtual DbSet<Company> Companies { get; set; }

    public virtual DbSet<Invoice> Invoices { get; set; }

    public virtual DbSet<Project> Projects { get; set; }

    public virtual DbSet<SpecificationItem> SpecificationItems { get; set; }

    public virtual DbSet<Models.Task> Tasks { get; set; }

    public virtual DbSet<TaskList> TaskLists { get; set; }

    public virtual DbSet<Team> Teams { get; set; }

    public virtual DbSet<TechnicalSpecification> TechnicalSpecifications { get; set; }

    public virtual DbSet<User> Users { get; set; }

//    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
//#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
//        => optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=postgres;Username=postgres;Password=1234");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .HasPostgresEnum("dev_team", "spec_status", new[] { "draft", "pending_approval", "approved" })
            .HasPostgresEnum("dev_team", "change_status", new[] { "created", "approved", "revoked" })
            .HasPostgresEnum("dev_team", "project_status", new[] { "draft", "in_progress", "pending_approval", "pending_invoice_approval", 
                "pending_spec_approval", "completed", "closed", "frozen" })
            .HasPostgresEnum("dev_team", "task_status", new[] { "new", "in_progress", "on_review", "done", "cancelled" })
            .HasPostgresEnum("dev_team", "user_role", new[] { "customer", "manager", "developer" })
            .HasPostgresExtension("finpay", "pgcrypto")
            .HasPostgresExtension("pgagent", "pgagent");

        modelBuilder.Entity<ChangeRequest>(entity =>
        {
            entity.HasKey(e => e.IdChange).HasName("change_request_pkey");

            entity.ToTable("change_request", "dev_team");

            entity.HasIndex(e => e.IdApprover, "idx_change_approver");

            entity.HasIndex(e => e.IdCreator, "idx_change_creator");

            entity.HasIndex(e => e.IdInvoice, "idx_change_invoice");

            entity.HasIndex(e => e.IdSpecItem, "idx_change_spec_item");

            entity.HasIndex(e => e.IdTask, "idx_change_task");

            entity.Property(e => e.IdChange).HasColumnName("id_change");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.IdApprover).HasColumnName("id_approver");
            entity.Property(e => e.IdCreator).HasColumnName("id_creator");
            entity.Property(e => e.IdInvoice).HasColumnName("id_invoice");
            entity.Property(e => e.IdSpecItem).HasColumnName("id_spec_item");
            entity.Property(e => e.IdTask).HasColumnName("id_task");
            entity.Property(e => e.Status)
                .HasColumnName("status");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");

            entity.HasOne(d => d.IdApproverNavigation).WithMany(p => p.ChangeRequestIdApproverNavigations)
                .HasForeignKey(d => d.IdApprover)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_change_approver");

            entity.HasOne(d => d.IdCreatorNavigation).WithMany(p => p.ChangeRequestIdCreatorNavigations)
                .HasForeignKey(d => d.IdCreator)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fk_change_creator");

            entity.HasOne(d => d.IdInvoiceNavigation).WithMany(p => p.ChangeRequests)
                .HasForeignKey(d => d.IdInvoice)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_change_invoice");

            entity.HasOne(d => d.IdSpecItemNavigation).WithMany(p => p.ChangeRequests)
                .HasForeignKey(d => d.IdSpecItem)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_change_spec_item");

            entity.HasOne(d => d.IdTaskNavigation).WithMany(p => p.ChangeRequests)
                .HasForeignKey(d => d.IdTask)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_change_task");
        });

        modelBuilder.Entity<Company>(entity =>
        {
            entity.HasKey(e => e.IdCompany).HasName("company_pkey");

            entity.ToTable("company", "dev_team");

            entity.Property(e => e.IdCompany).HasColumnName("id_company");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
        });

        modelBuilder.Entity<Invoice>(entity =>
        {
            entity.HasKey(e => e.IdInvoice).HasName("invoice_pkey");

            entity.ToTable("invoice", "dev_team");

            entity.Property(e => e.IdInvoice).HasColumnName("id_invoice");
            entity.Property(e => e.Cost)
                .HasPrecision(15, 2)
                .HasColumnName("cost");
            entity.Property(e => e.EndDate).HasColumnName("end_date");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
            entity.Property(e => e.StartDate).HasColumnName("start_date");
            entity.Property(i => i.IdProject).HasColumnName("id_project");
            entity.Property(i => i.TotalCost).HasColumnName("total_cost").HasPrecision(15, 2);
        });

        modelBuilder.Entity<Project>(entity =>
        {
            entity.HasKey(e => e.IdProject).HasName("project_pkey");

            entity.ToTable("project", "dev_team");

            entity.HasIndex(e => e.IdCustomer, "idx_project_customer");

            entity.HasIndex(e => e.IdTeam, "idx_project_team");

            entity.Property(e => e.IdProject).HasColumnName("id_project");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.IdCustomer).HasColumnName("id_customer");
            entity.Property(p => p.TotalCost).HasColumnName("total_cost").HasPrecision(15, 2);
            entity.Property(p => p.IdInvoice).HasColumnName("id_invoice");
            
            entity.Property(e => e.IdTeam).HasColumnName("id_team");
            entity.Property(e => e.Status).HasColumnName("status");
                //.HasColumnType("dev_team.project_status");
                
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");

            entity.HasOne(d => d.IdCustomerNavigation).WithMany(p => p.Projects)
                .HasForeignKey(d => d.IdCustomer)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fk_project_customer");

            entity.HasOne(d => d.IdTeamNavigation).WithMany(p => p.Projects)
                .HasForeignKey(d => d.IdTeam)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_project_team");

            entity.HasOne(p => p.IdInvoiceNavigation)
                  .WithOne(i => i.IdProjectNavigation)
                  .HasForeignKey<Project>(p => p.IdInvoice);
        });

        modelBuilder.Entity<SpecificationItem>(entity =>
        {
            entity.HasKey(e => e.IdItem).HasName("specification_item_pkey");

            entity.ToTable("specification_item", "dev_team");

            entity.HasIndex(e => e.IdSpec, "idx_item_spec");

            entity.Property(e => e.IdItem).HasColumnName("id_item");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.IdSpec).HasColumnName("id_spec");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");

            entity.HasOne(d => d.IdSpecNavigation).WithMany(p => p.SpecificationItems)
                .HasForeignKey(d => d.IdSpec)
                .HasConstraintName("fk_item_spec");
        });

        modelBuilder.Entity<Task>(entity =>
        {
            entity.HasKey(e => e.IdTask).HasName("task_pkey");

            entity.ToTable("task", "dev_team");

            entity.HasIndex(e => e.IdDeveloper, "idx_task_developer");

            entity.HasIndex(e => e.IdTaskList, "idx_task_task_list");

            entity.Property(e => e.IdTask).HasColumnName("id_task");
            entity.Property(e => e.ActualHours)
                .HasPrecision(7, 2)
                .HasColumnName("actual_hours");
            entity.Property(e => e.Cost)
                .HasPrecision(15, 2)
                .HasColumnName("cost");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.IdDeveloper).HasColumnName("id_developer");
            entity.Property(e => e.IdTaskList).HasColumnName("id_task_list");
            entity.Property(e => e.PlannedHours)
                .HasPrecision(7, 2)
                .HasColumnName("planned_hours");

            entity.HasOne(d => d.IdDeveloperNavigation).WithMany(p => p.Tasks)
                .HasForeignKey(d => d.IdDeveloper)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_task_developer");

            entity.HasOne(d => d.IdTaskListNavigation).WithMany(p => p.Tasks)
                .HasForeignKey(d => d.IdTaskList)
                .HasConstraintName("fk_task_task_list");
        });

        modelBuilder.Entity<TaskList>(entity =>
        {
            entity.HasKey(e => e.IdTaskList).HasName("task_list_pkey");

            entity.ToTable("task_list", "dev_team");

            entity.HasIndex(e => e.IdInvoice, "idx_task_list_invoice");

            entity.Property(e => e.IdTaskList).HasColumnName("id_task_list");
            entity.Property(e => e.CompletionPercent)
                .HasPrecision(5, 2)
                .HasColumnName("completion_percent");
            entity.Property(e => e.IdInvoice).HasColumnName("id_invoice");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");

            entity.HasOne(d => d.IdInvoiceNavigation).WithMany(p => p.TaskLists)
                .HasForeignKey(d => d.IdInvoice)
                .HasConstraintName("fk_task_list_invoice");
        });

        modelBuilder.Entity<Team>(entity =>
        {
            entity.HasKey(e => e.IdTeam).HasName("team_pkey");

            entity.ToTable("team", "dev_team");

            entity.HasIndex(e => e.IdCompany, "idx_team_company");

            entity.Property(e => e.IdTeam).HasColumnName("id_team");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.IdCompany).HasColumnName("id_company");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");

            entity.HasOne(d => d.IdCompanyNavigation).WithMany(p => p.Teams)
                .HasForeignKey(d => d.IdCompany)
                .HasConstraintName("fk_team_company");
        });

        modelBuilder.Entity<TechnicalSpecification>(entity =>
        {
            entity.HasKey(e => e.IdSpec).HasName("technical_specification_pkey");

            entity.ToTable("technical_specification", "dev_team");

            entity.HasIndex(e => e.IdProject, "idx_spec_project");

            entity.Property(e => e.IdSpec).HasColumnName("id_spec");
            entity.Property(e => e.Deadline).HasColumnName("deadline");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.IdProject).HasColumnName("id_project");
            entity.Property(s => s.Status).HasColumnName("status");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");

            entity.HasOne(d => d.IdProjectNavigation).WithMany(p => p.TechnicalSpecifications)
                .HasForeignKey(d => d.IdProject)
                .HasConstraintName("fk_spec_project");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.IdUser).HasName("users_pkey");

            entity.ToTable("users", "dev_team");

            entity.HasIndex(e => e.IdCompany, "idx_users_company");

            entity.HasIndex(e => e.IdTeam, "idx_users_team");

            entity.HasIndex(e => e.Login, "users_login_key").IsUnique();

            entity.Property(e => e.IdUser).HasColumnName("id_user");
            entity.Property(e => e.FirstName)
                .HasMaxLength(100)
                .HasColumnName("first_name");
            entity.Property(e => e.IdCompany).HasColumnName("id_company");
            entity.Property(e => e.IdTeam).HasColumnName("id_team");
            entity.Property(e => e.LastName)
                .HasMaxLength(100)
                .HasColumnName("last_name");
            entity.Property(u => u.Role)
                .HasColumnName("role");
            entity.Property(e => e.Login)
                .HasMaxLength(100)
                .HasColumnName("login");
            entity.Property(e => e.PasswordHash)
                .HasMaxLength(255)
                .HasColumnName("password_hash");
            entity.Property(e => e.ShortDescription).HasColumnName("short_description");

            entity.HasOne(d => d.IdCompanyNavigation).WithMany(p => p.Users)
                .HasForeignKey(d => d.IdCompany)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_users_company");

            entity.HasOne(d => d.IdTeamNavigation).WithMany(p => p.Users)
                .HasForeignKey(d => d.IdTeam)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_users_team");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
