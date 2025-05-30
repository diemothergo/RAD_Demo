using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using RAD_Demo.Models;
using Microsoft.AspNetCore.Identity;

namespace RAD_Demo.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : IdentityDbContext<IdentityUser, IdentityRole, string>(options)
{
    public DbSet<Ride> Rides { get; set; } = null!;
    public DbSet<Driver> Drivers { get; set; } = null!;
    public DbSet<Customer> Customers { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Name).IsRequired();
        });

        modelBuilder.Entity<Driver>(entity =>
        {
            entity.HasKey(d => d.Id);
            entity.Property(d => d.Name).IsRequired();
            entity.Property(d => d.CurrentLocation).IsRequired();
            entity.Property(d => d.IsAvailable).HasDefaultValue(true);
        });

        modelBuilder.Entity<Ride>(entity =>
        {
            entity.HasKey(r => r.Id);
            entity.Property(r => r.PickupLocation).IsRequired();
            entity.Property(r => r.DropoffLocation).IsRequired();
            entity.Property(r => r.Status).HasDefaultValue(RideStatus.Booked);
            entity.Property(r => r.ETA).HasDefaultValue(15);

            entity.HasOne(r => r.Customer)
                  .WithMany()
                  .HasForeignKey(r => r.CustomerId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(r => r.Driver)
                  .WithMany()
                  .HasForeignKey(r => r.DriverId)
                  .OnDelete(DeleteBehavior.Restrict);
        });
    }
}