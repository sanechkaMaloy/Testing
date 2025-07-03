using Microsoft.EntityFrameworkCore;
using Testing.Models;

namespace Testing.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Asset> Assets { get; set; }
        public DbSet<AssetPrice> AssetPrices { get; set; }
    }
}
