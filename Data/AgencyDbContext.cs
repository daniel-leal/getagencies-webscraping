using Microsoft.EntityFrameworkCore;

namespace GetAgency.Data
{
    public class AgencyDbContext : DbContext
    {
        public DbSet<AgencyDB> Agencies { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(
                @"Server=localhost;Database=bpagency;User ID=sa;Password=yourStrong(!)Password",
                x => x.UseNetTopologySuite());
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new AgencyMap());

            base.OnModelCreating(modelBuilder);
        }

    }
}