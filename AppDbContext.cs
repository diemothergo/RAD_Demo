using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using RAD_Demo.Models;

namespace RAD_Demo.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : IdentityDbContext(options)
{
    public DbSet<Ride> Rides { get; set; } = null!;
    public DbSet<Driver> Drivers { get; set; } = null!;
    public DbSet<Customer> Customers { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Ride>()
            .HasOne(r => r.Customer)
            .WithMany()
            .HasForeignKey(r => r.CustomerId)
            .IsRequired(false);

        modelBuilder.Entity<Ride>()
            .HasOne(r => r.Driver)
            .WithMany()
            .HasForeignKey(r => r.DriverId)
            .IsRequired(false);
    }
}