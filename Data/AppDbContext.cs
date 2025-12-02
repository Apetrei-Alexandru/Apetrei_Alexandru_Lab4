using Microsoft.EntityFrameworkCore;
using Apetrei_Alexandru_Lab4.Models;

namespace Apetrei_Alexandru_Lab4.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        // Constructor fallback (folosit doar pentru design-time)
        public AppDbContext() : base(new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=TaxiPredictionDb;Trusted_Connection=True;MultipleActiveResultSets=true")
            .Options)
        {
        }

        public DbSet<PredictionHistory> PredictionHistories { get; set; }
    }

}
