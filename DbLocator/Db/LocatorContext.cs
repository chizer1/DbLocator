using Microsoft.EntityFrameworkCore;

namespace DbLocator.Db;

internal class DbLocatorContext(DbContextOptions<DbLocatorContext> options) : DbContext(options)
{
    public virtual DbSet<ConnectionEntity> Connections { get; set; }

    public virtual DbSet<DatabaseEntity> Databases { get; set; }

    public virtual DbSet<DatabaseServerEntity> DatabaseServers { get; set; }

    public virtual DbSet<DatabaseTypeEntity> DatabaseTypes { get; set; }

    public virtual DbSet<TenantEntity> Tenants { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
            throw new InvalidOperationException("DbContextOptions must be configured externally.");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ConnectionEntity>(entity =>
        {
            entity.HasKey(e => e.ConnectionId).HasName("PK_Connection");

            entity.ToTable("Connection");

            entity.Property(e => e.ConnectionId).HasColumnName("ConnectionID");
            entity.Property(e => e.DatabaseId).HasColumnName("DatabaseID");
            entity.Property(e => e.TenantId).HasColumnName("TenantID");

            entity
                .HasOne(d => d.Database)
                .WithMany(p => p.Connections)
                .HasForeignKey(d => d.DatabaseId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Connection_Database");

            entity
                .HasOne(d => d.Tenant)
                .WithMany(p => p.Connections)
                .HasForeignKey(d => d.TenantId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Connection_Tenant");
        });

        modelBuilder.Entity<DatabaseEntity>(entity =>
        {
            entity.ToTable("Database");

            entity.HasKey(e => e.DatabaseId).HasName("PK_Database");

            entity.HasIndex(e => e.DatabaseServerId, "IX_Database_DatabaseServerID");

            entity.HasIndex(e => e.DatabaseTypeId, "IX_Database_DatabaseTypeID");

            entity.Property(e => e.DatabaseId).HasColumnName("DatabaseID");
            entity.Property(e => e.DatabaseName).IsRequired().HasMaxLength(50).IsUnicode(false);
            entity.Property(e => e.DatabaseServerId).HasColumnName("DatabaseServerID");
            entity.Property(e => e.DatabaseStatusId).HasColumnName("DatabaseStatusID");
            entity.Property(e => e.DatabaseTypeId).HasColumnName("DatabaseTypeID");
            entity.Property(e => e.DatabaseUser).HasMaxLength(50).IsUnicode(false);
            entity.Property(e => e.DatabaseUserPassword).HasMaxLength(50).IsUnicode(false);

            entity
                .HasOne(d => d.DatabaseServer)
                .WithMany(p => p.Databases)
                .HasForeignKey(d => d.DatabaseServerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Database_DatabaseServer");

            entity
                .HasOne(d => d.DatabaseType)
                .WithMany(p => p.Databases)
                .HasForeignKey(d => d.DatabaseTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Database_DatabaseType");

            entity.Property(e => e.UseTrustedConnection).HasColumnName("UseTrustedConnection");
        });

        modelBuilder.Entity<DatabaseServerEntity>(entity =>
        {
            entity.ToTable("DatabaseServer");

            entity.HasKey(e => e.DatabaseServerId).HasName("PK_DatabaseServer");

            entity.Property(e => e.DatabaseServerId).HasColumnName("DatabaseServerID");
            entity
                .Property(e => e.DatabaseServerIpaddress)
                .IsRequired()
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("DatabaseServerIPAddress");
            entity
                .Property(e => e.DatabaseServerName)
                .IsRequired()
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<DatabaseTypeEntity>(entity =>
        {
            entity.ToTable("DatabaseType");

            entity.HasKey(e => e.DatabaseTypeId).HasName("PK_DatabaseType");

            entity
                .Property(e => e.DatabaseTypeId)
                .ValueGeneratedOnAdd()
                .HasColumnName("DatabaseTypeID");
            entity.Property(e => e.DatabaseTypeName).IsRequired().HasMaxLength(20).IsUnicode(false);
        });

        modelBuilder.Entity<TenantEntity>(entity =>
        {
            entity.HasKey(e => e.TenantId).HasName("PK_Client");

            entity.ToTable("Tenant");

            entity.HasKey(e => e.TenantId).HasName("PK_Tenant");

            entity.Property(e => e.TenantId).HasColumnName("TenantID");
            entity.Property(e => e.TenantCode).HasMaxLength(10).IsUnicode(false);
            entity.Property(e => e.TenantName).IsRequired().HasMaxLength(50).IsUnicode(false);
            entity.Property(e => e.TenantStatusId).HasColumnName("TenantStatusID");
        });
    }
}
