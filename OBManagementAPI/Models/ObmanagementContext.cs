using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace OBManagementAPI.Models;

public partial class ObmanagementContext : DbContext
{
    public ObmanagementContext()
    {
    }

    public ObmanagementContext(DbContextOptions<ObmanagementContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Account> Accounts { get; set; }

    public virtual DbSet<BuildingFloor> BuildingFloors { get; set; }

    public virtual DbSet<FacultyMemberOffice> FacultyMemberOffices { get; set; }

    public virtual DbSet<FacultyTracking> FacultyTrackings { get; set; }

    public virtual DbSet<Location> Locations { get; set; }

    public virtual DbSet<Office> Offices { get; set; }

    public virtual DbSet<OfficeBoyAssignedFloor> OfficeBoyAssignedFloors { get; set; }

    public virtual DbSet<OfficeBoyLeave> OfficeBoyLeaves { get; set; }

    public virtual DbSet<OfficeBoyLocation> OfficeBoyLocations { get; set; }

    public virtual DbSet<Task> Tasks { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=.\\SQLEXPRESS;Database=OBManagement;Trusted_Connection=True;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Account>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Account__3214EC07B24F32FD");

            entity.ToTable("Account");

            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.Password).HasMaxLength(255);
        });

        modelBuilder.Entity<BuildingFloor>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Building__3214EC07D3EF6487");

            entity.ToTable("BuildingFloor");

            entity.Property(e => e.Number).HasMaxLength(50);
        });

        modelBuilder.Entity<FacultyMemberOffice>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__FacultyM__3214EC07B22D5D90");

            entity.ToTable("FacultyMemberOffice");

            entity.Property(e => e.Status).HasMaxLength(50);

            entity.HasOne(d => d.FacultyAccount).WithMany(p => p.FacultyMemberOffices)
                .HasForeignKey(d => d.FacultyAccountId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__FacultyMe__Facul__6C190EBB");

            entity.HasOne(d => d.Office).WithMany(p => p.FacultyMemberOffices)
                .HasForeignKey(d => d.OfficeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__FacultyMe__Offic__6D0D32F4");
        });

        modelBuilder.Entity<FacultyTracking>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__FacultyT__3214EC077E79343C");

            entity.ToTable("FacultyTracking");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.IsInsideRegion).HasDefaultValue(false);
            entity.Property(e => e.RadiusMeters).HasDefaultValue(300.0);
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("Pending");
            entity.Property(e => e.TaskDescription).HasMaxLength(500);
            entity.Property(e => e.TaskType).HasMaxLength(20);
            entity.Property(e => e.TriggerType).HasMaxLength(10);
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.FacultyAccount).WithMany(p => p.FacultyTrackingFacultyAccounts)
                .HasForeignKey(d => d.FacultyAccountId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__FacultyTr__Facul__0B91BA14");

            entity.HasOne(d => d.OfficeBoyAccount).WithMany(p => p.FacultyTrackingOfficeBoyAccounts)
                .HasForeignKey(d => d.OfficeBoyAccountId)
                .HasConstraintName("FK_FacultyTracking_OfficeBoy");
        });

        modelBuilder.Entity<Location>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Location__3214EC0734852BAE");

            entity.ToTable("Location");

            entity.Property(e => e.Name).HasMaxLength(100);
        });

        modelBuilder.Entity<Office>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Office__3214EC07C7A4FC91");

            entity.ToTable("Office");

            entity.Property(e => e.OfficeName).HasMaxLength(100);

            entity.HasOne(d => d.BuildingFloor).WithMany(p => p.Offices)
                .HasForeignKey(d => d.BuildingFloorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Office__Building__6EF57B66");
        });

        modelBuilder.Entity<OfficeBoyAssignedFloor>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__OfficeBo__3214EC070E7D803C");

            entity.Property(e => e.Status).HasMaxLength(50);

            entity.HasOne(d => d.Floor).WithMany(p => p.OfficeBoyAssignedFloors)
                .HasForeignKey(d => d.FloorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__OfficeBoy__Floor__6FE99F9F");

            entity.HasOne(d => d.OfficeBoyAccount).WithMany(p => p.OfficeBoyAssignedFloors)
                .HasForeignKey(d => d.OfficeBoyAccountId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__OfficeBoy__Offic__71D1E811");

            entity.HasOne(d => d.Office).WithMany(p => p.OfficeBoyAssignedFloors)
                .HasForeignKey(d => d.OfficeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__OfficeBoy__Offic__70DDC3D8");
        });

        modelBuilder.Entity<OfficeBoyLeave>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__OfficeBo__3214EC07AA5D7C1E");

            entity.ToTable("OfficeBoyLeave");

            entity.Property(e => e.ApprovedEndDate).HasColumnType("datetime");
            entity.Property(e => e.ApprovedStartDate).HasColumnType("datetime");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Reason).HasMaxLength(255);
            entity.Property(e => e.RequestedEndDate).HasColumnType("datetime");
            entity.Property(e => e.RequestedStartDate).HasColumnType("datetime");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("Pending");
            entity.Property(e => e.SupervisorRemarks).HasMaxLength(255);

            entity.HasOne(d => d.OfficeBoyAccount).WithMany(p => p.OfficeBoyLeaveOfficeBoyAccounts)
                .HasForeignKey(d => d.OfficeBoyAccountId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Leave_OfficeBoy");

            entity.HasOne(d => d.ReplacementOfficeBoy).WithMany(p => p.OfficeBoyLeaveReplacementOfficeBoys)
                .HasForeignKey(d => d.ReplacementOfficeBoyId)
                .HasConstraintName("FK_Leave_Replacement");
        });

        modelBuilder.Entity<OfficeBoyLocation>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__OfficeBo__3214EC07E60B1ECD");

            entity.ToTable("OfficeBoyLocation");

            entity.Property(e => e.LocationName).HasMaxLength(200);
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.OfficeBoyAccount).WithMany(p => p.OfficeBoyLocations)
                .HasForeignKey(d => d.OfficeBoyAccountId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__OfficeBoy__Offic__0F624AF8");

            entity.HasOne(d => d.Task).WithMany(p => p.OfficeBoyLocations)
                .HasForeignKey(d => d.TaskId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__OfficeBoy__TaskI__10566F31");
        });

        modelBuilder.Entity<Task>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Task__3214EC0709E41319");

            entity.ToTable("Task");

            entity.Property(e => e.Description).HasMaxLength(255);
            entity.Property(e => e.IsScheduled).HasColumnName("isScheduled");
            entity.Property(e => e.Remarks).HasMaxLength(255);
            entity.Property(e => e.ScheduledAt).HasColumnType("datetime");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("Pending", "DF_Task_Status");
            entity.Property(e => e.TaskTime)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.FacultyAccount).WithMany(p => p.TaskFacultyAccounts)
                .HasForeignKey(d => d.FacultyAccountId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Task__FacultyAcc__72C60C4A");

            entity.HasOne(d => d.Location).WithMany(p => p.Tasks)
                .HasForeignKey(d => d.LocationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Task__LocationId__73BA3083");

            entity.HasOne(d => d.OfficeBoyAccount).WithMany(p => p.TaskOfficeBoyAccounts)
                .HasForeignKey(d => d.OfficeBoyAccountId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Task__OfficeBoyA__74AE54BC");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
