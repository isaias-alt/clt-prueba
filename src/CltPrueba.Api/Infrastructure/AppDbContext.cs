using CltPrueba.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace CltPrueba.Api.Infrastructure;

public class AppDbContext : DbContext
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Address> Addresses => Set<Address>();
    public DbSet<Currency> Currencies => Set<Currency>();

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(e =>
        {
            e.Property(u => u.Name).HasMaxLength(200);
            e.Property(u => u.Email).HasMaxLength(320);
            e.Property(u => u.PasswordHash).HasMaxLength(200);
            e.HasIndex(u => u.Email).IsUnique();
        });

        modelBuilder.Entity<Address>(e =>
        {
            e.Property(a => a.City).HasMaxLength(100);
            e.Property(a => a.Country).HasMaxLength(100);
            e.Property(a => a.Street).HasMaxLength(200);
            e.Property(a => a.ZipCode).HasMaxLength(20);
        });

        modelBuilder.Entity<Currency>(e =>
        {
            e.Property(c => c.Code).HasMaxLength(10);
            e.Property(c => c.Name).HasMaxLength(100);
            e.Property(c => c.RateToBase).HasPrecision(18, 6);
            e.HasIndex(c => c.Code).IsUnique();
            e.HasData
                (
                    new Currency {Id = 1, Name = "Guaraní paraguayo", Code = "PYG", RateToBase = 1}, 
                    new Currency {Id = 2, Name = "Dólar estadounidense", Code = "USD", RateToBase = 5765}
                );
        });
    }
}