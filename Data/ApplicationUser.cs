using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace BugTracker.Data
{
    public class ApplicationUser : IdentityUser
    {
        public virtual ICollection<UserProjectMap> UserProjectMap { get; set; }
        public virtual ICollection<UserProjectBugMap> UserProjectBugMap { get; set; }

    }
}