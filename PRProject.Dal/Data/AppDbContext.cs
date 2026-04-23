using Microsoft.EntityFrameworkCore;
using PRProject.Domain.Entities;

namespace PRProject.Dal.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<TaskItem> Tasks { get; set; }
    }
}