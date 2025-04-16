using DbLocator.Domain;
using Microsoft.EntityFrameworkCore;

namespace DbLocator.Db;

internal class DbLocatorContext(DbContextOptions<DbLocatorContext> options) : DbContext(options)
{
    public virtual DbSet<ConnectionEntity> Connections { get; set; }

    public virtual DbSet<DatabaseEntity> Databases { get; set; }

    public virtual DbSet<DatabaseUserEntity> DatabaseUsers { get; set; }

    public virtual DbSet<DatabaseUserRoleEntity> DatabaseUserRoles { get; set; }

    public virtual DbSet<DatabaseRoleEntity> DatabaseRoles { get; set; }

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

        modelBuilder.Entity<DatabaseRoleEntity>(entity =>
        {
            entity.ToTable("DatabaseRole");

            entity.HasKey(e => e.DatabaseRoleId).HasName("PK_DatabaseRole");

            entity.Property(e => e.DatabaseRoleId).HasColumnName("DatabaseRoleID");
            entity.Property(e => e.DatabaseRoleName).HasMaxLength(50).IsUnicode(false);

            entity
                .HasMany(e => e.Users)
                .WithOne(p => p.Role)
                .HasForeignKey(d => d.DatabaseRoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DatabaseUserRole_DatabaseRole");
        });

        modelBuilder.Entity<DatabaseUserDatabaseEntity>(entity =>
        {
            entity.ToTable("DatabaseUserDatabase");

            entity.HasKey(e => e.DatabaseUserDatabaseId).HasName("PK_DatabaseUserDatabase");

            entity.Property(e => e.DatabaseUserDatabaseId).HasColumnName("DatabaseUserDatabaseID");
            entity.Property(e => e.DatabaseUserId).HasColumnName("DatabaseUserID");
            entity.Property(e => e.DatabaseId).HasColumnName("DatabaseID");

            entity
                .HasOne(d => d.Database)
                .WithMany(p => p.Users)
                .HasForeignKey(d => d.DatabaseId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DatabaseUserDatabase_Database");
        });

        modelBuilder.Entity<DatabaseUserEntity>(entity =>
        {
            entity.ToTable("DatabaseUser");

            entity.HasKey(e => e.DatabaseUserId).HasName("PK_DatabaseUser");

            entity.Property(e => e.DatabaseUserId).HasColumnName("DatabaseUserID");

            entity.Property(e => e.UserName).HasMaxLength(50).IsUnicode(false);
            entity.Property(e => e.UserPassword).HasMaxLength(50).IsUnicode(false);

            entity
                .HasMany(d => d.UserRoles)
                .WithOne(p => p.User)
                .HasForeignKey(d => d.DatabaseUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DatabaseUserRole_DatabaseUser");
        });

        modelBuilder.Entity<DatabaseUserRoleEntity>(entity =>
        {
            entity.ToTable("DatabaseUserRole");

            entity.HasKey(e => e.DatabaseUserRoleId).HasName("PK_DatabaseUserRole");

            entity.HasIndex(e => e.DatabaseRoleId, "IX_DatabaseUserRole_DatabaseRoleID");

            entity.Property(e => e.DatabaseUserRoleId).ValueGeneratedOnAdd();
            entity.Property(e => e.DatabaseUserId).HasColumnName("DatabaseUserID");
            entity.Property(e => e.DatabaseRoleId).HasColumnName("DatabaseRoleID");

            entity
                .HasOne(d => d.User)
                .WithMany(p => p.UserRoles)
                .HasForeignKey(d => d.DatabaseUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DatabaseUserRole_DatabaseUser");

            entity
                .HasOne(d => d.Role)
                .WithMany(p => p.Users)
                .HasForeignKey(d => d.DatabaseRoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DatabaseUserRole_DatabaseRole");
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
            entity.Property(e => e.DatabaseServerHostName).HasMaxLength(50).IsUnicode(false);
            entity
                .Property(e => e.DatabaseServerFullyQualifiedDomainName)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity
                .Property(e => e.DatabaseServerIpaddress)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("DatabaseServerIPAddress");
            entity
                .Property(e => e.DatabaseServerName)
                .IsRequired()
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.IsLinkedServer).IsRequired();
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

        // DATA SEED
        foreach (var e in Enum.GetValues(typeof(DatabaseRole)).Cast<DatabaseRole>())
        {
            modelBuilder
                .Entity<DatabaseRoleEntity>()
                .HasData(
                    new DatabaseRoleEntity
                    {
                        DatabaseRoleId = (int)e,
                        DatabaseRoleName = e.ToString()
                    }
                );
        }
    }
}
