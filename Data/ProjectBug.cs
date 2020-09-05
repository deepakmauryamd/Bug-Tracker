using System.ComponentModel.DataAnnotations;
using System;

namespace BugTracker.Data
{
    public class ProjectBug
    {
        public int Id { get; set; }

        public int ProjectId { get; set; }

        [Required]
        public string Title { get; set; }

        public string Description { get; set; }

        [Required]
        public DateTime CreatedOn { get; set; }

        public DateTime ResolvedOn { get; set; }

        [Required]
        public int BugPriorityId { get; set; }

        public BugPriority BugPriority { get; set; }

        public Project Project { get; set; }
    
        public int BugStatusId { get; set; }
        public BugStatus BugStatus { get; set; }

    }
}