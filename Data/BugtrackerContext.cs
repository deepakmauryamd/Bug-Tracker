
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BugTracker.Data
{
    public class BugtrackerContext : IdentityDbContext
    {
        public BugtrackerContext(DbContextOptions<BugtrackerContext> options)
        : base(options)
        {

        }

        public DbSet<Project> Projects { get; set; }
        public DbSet<UserProjectMap> UserProjectMaps { get; set; }
        public DbSet<ProjectBug> ProjectBugs { get; set; }
        public DbSet<UserProjectBugMap> UserProjectBugMaps { get; set; }

        public DbSet<BugStatus> BugStatus { get; set; }
    }
}