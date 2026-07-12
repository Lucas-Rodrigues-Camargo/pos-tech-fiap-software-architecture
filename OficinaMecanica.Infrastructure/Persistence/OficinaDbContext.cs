using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using OficinaMecanica.Application.Abstractions;
using OficinaMecanica.Domain.Entities;
using OficinaMecanica.Infrastructure.Identity;

namespace OficinaMecanica.Infrastructure.Persistence;

public sealed class OficinaDbContext(DbContextOptions<OficinaDbContext> options)
    : IdentityDbContext<AppUser>(options), IUnitOfWork
{
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Vehicle> Vehicles => Set<Vehicle>();
    public DbSet<ServiceCatalogItem> ServicesCatalog => Set<ServiceCatalogItem>();
    public DbSet<Part> Parts => Set<Part>();
    public DbSet<ServiceOrder> ServiceOrders => Set<ServiceOrder>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<AppUser>().ToTable("tb_users");
        modelBuilder.Entity<IdentityRole>().ToTable("tb_roles");
        modelBuilder.Entity<IdentityUserClaim<string>>().ToTable("tb_users_claims");
        modelBuilder.Entity<IdentityUserRole<string>>().ToTable("tb_users_roles");
        modelBuilder.Entity<IdentityUserLogin<string>>().ToTable("tb_users_logins");
        modelBuilder.Entity<IdentityRoleClaim<string>>().ToTable("tb_roles_claims");
        modelBuilder.Entity<IdentityUserToken<string>>().ToTable("tb_users_tokens");

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.ToTable("tb_customers");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).ValueGeneratedNever();
            entity.HasIndex(x => x.Document).IsUnique();
            entity.Property(x => x.Name).HasMaxLength(120).IsRequired();
            entity.Property(x => x.Document).HasMaxLength(14).IsRequired();
            entity.Property(x => x.Phone).HasMaxLength(20).IsRequired();
            entity.Property(x => x.Email).HasMaxLength(120).IsRequired();
        });

        modelBuilder.Entity<Vehicle>(entity =>
        {
            entity.ToTable("tb_vehicles");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).ValueGeneratedNever();
            entity.HasIndex(x => x.Plate).IsUnique();
            entity.Property(x => x.Plate).HasMaxLength(7).IsRequired();
            entity.Property(x => x.Brand).HasMaxLength(60).IsRequired();
            entity.Property(x => x.Model).HasMaxLength(60).IsRequired();
            entity.HasOne(x => x.Customer)
                .WithMany(x => x.Vehicles)
                .HasForeignKey(x => x.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ServiceCatalogItem>(entity =>
        {
            entity.ToTable("tb_services_catalog");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).ValueGeneratedNever();
            entity.Property(x => x.Name).HasMaxLength(120).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(255).IsRequired();
            entity.Property(x => x.Price).HasColumnType("decimal(10,2)");
            entity.Property(x => x.EstimatedHours).HasColumnType("decimal(10,2)");
        });

        modelBuilder.Entity<Part>(entity =>
        {
            entity.ToTable("tb_parts");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).ValueGeneratedNever();
            entity.HasIndex(x => x.Sku).IsUnique();
            entity.Property(x => x.Name).HasMaxLength(120).IsRequired();
            entity.Property(x => x.Sku).HasMaxLength(40).IsRequired();
            entity.Property(x => x.UnitPrice).HasColumnType("decimal(10,2)");
        });

        modelBuilder.Entity<ServiceOrder>(entity =>
        {
            entity.ToTable("tb_service_orders");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).ValueGeneratedNever();
            entity.HasIndex(x => x.Number).IsUnique();
            entity.Property(x => x.Number).HasMaxLength(32).IsRequired();
            entity.Property(x => x.CustomerRequest).HasColumnType("text[]").IsRequired();
            entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(40).IsRequired();
            entity.HasOne(x => x.Customer).WithMany().HasForeignKey(x => x.CustomerId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Vehicle).WithMany().HasForeignKey(x => x.VehicleId).OnDelete(DeleteBehavior.Restrict);
            entity.Navigation(x => x.Services).AutoInclude();
            entity.Navigation(x => x.Parts).AutoInclude();
        });

        modelBuilder.Entity<ServiceOrderServiceItem>(entity =>
        {
            entity.ToTable("tb_service_order_service_items");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).ValueGeneratedNever();
            entity.Property(x => x.Description).HasMaxLength(120).IsRequired();
            entity.Property(x => x.UnitPrice).HasColumnType("decimal(10,2)");
            entity.Property(x => x.EstimatedHours).HasColumnType("decimal(10,2)");
            entity.Property(x => x.IsAdditional).HasDefaultValue(false);
            entity.Property(x => x.AdditionalRequestId).IsRequired(false);
        });

        modelBuilder.Entity<ServiceOrderPartItem>(entity =>
        {
            entity.ToTable("tb_service_order_part_items");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).ValueGeneratedNever();
            entity.Property(x => x.Description).HasMaxLength(120).IsRequired();
            entity.Property(x => x.UnitPrice).HasColumnType("decimal(10,2)");
            entity.Property(x => x.IsAdditional).HasDefaultValue(false);
            entity.Property(x => x.AdditionalRequestId).IsRequired(false);
        });

        UseSnakeCaseColumnNames(modelBuilder);
    }

    private static void UseSnakeCaseColumnNames(ModelBuilder modelBuilder)
    {
        foreach (var entity in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entity.GetProperties())
            {
                property.SetColumnName(ToSnakeCase(property.Name));
            }
        }
    }

    private static string ToSnakeCase(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return value;
        }

        var builder = new StringBuilder(value.Length + 8);

        for (var i = 0; i < value.Length; i++)
        {
            var current = value[i];

            if (char.IsUpper(current))
            {
                if (i > 0 && value[i - 1] != '_' && (char.IsLower(value[i - 1]) || char.IsDigit(value[i - 1]) || (i + 1 < value.Length && char.IsLower(value[i + 1]))))
                {
                    builder.Append('_');
                }

                builder.Append(char.ToLowerInvariant(current));
                continue;
            }

            builder.Append(current);
        }

        return builder.ToString();
    }
}
