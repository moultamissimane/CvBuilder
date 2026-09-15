using CVBuilder.API.Models;
using Microsoft.EntityFrameworkCore;

namespace CVBuilder.API.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<SavedCv> SavedCvs => Set<SavedCv>();
    }
}
