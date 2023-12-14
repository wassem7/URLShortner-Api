using Microsoft.EntityFrameworkCore;
using URLShortner.Models;

namespace URLShortner.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    public DbSet<UrlManagement> Urls { get; set; }
}
