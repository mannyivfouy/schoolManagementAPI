using Microsoft.EntityFrameworkCore;

namespace SchoolManagementAPI.Models
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions options) : base(options)
        {
        }

        protected ApplicationDbContext()
        {
        }

        public DbSet<Student> Students { get; set; }
    }
}
