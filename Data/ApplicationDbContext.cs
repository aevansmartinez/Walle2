using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using testing2.Models;

namespace testing2.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<testing2.Models.Movie>? Movie { get; set; }
        public DbSet<testing2.Models.Actor>? Actor { get; set; }
        public DbSet<testing2.Models.ActorMovie>? ActorMovie { get; set; }
    }
}