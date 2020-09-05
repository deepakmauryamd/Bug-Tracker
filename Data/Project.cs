using System.Collections.Generic;

namespace BugTracker.Data
{
    public class Project
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public ApplicationUser ApplicationUser {get; set;}
        public List<UserProjectMap> UserProjectMap { get; set; }
        public ProjectBug ProjectBug {get; set;}
    }
}