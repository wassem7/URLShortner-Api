using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using URLShortner.Models;
using URLShortner.Models.DomainModels;

namespace URLShortner.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    public DbSet<UrlManagement> Urls { get; set; }
    public DbSet<User> Users { get; set; }

    public DbSet<SubscriptionPackage> SubscriptionPackages { get; set; }
}
