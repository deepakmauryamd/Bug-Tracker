using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BugTracker.Models
{
    public class BugModel
    {
        public int Id { get; set; }

        public int ProjectId { get; set; }

        public string ProjectName { get; set; }

        [Required]
        public string Title { get; set; }

        public string Description { get; set; }

        [Required]
        [Display(Name = "Created on")]
        public DateTime CreatedOn { get; set; }
        public DateTime? ResolvedOn { get; set; }

        [Required]
        [Display(Name = "Choose prioriy of bug")]
        public int PriorityId { get; set; }

        public PriorityEnum PriorityEnum { get; set; }

        [Display(Name= "Assign to team member")]
        public List<string> AssignedToIds { get; set; }

        public MultiSelectList AssignedMembers { get; set; }

        public int BugStatusId { get; set; }
        public BugStatusEnum BugStatusEnum { get; set; }

        public bool isAssigned { get; set; }

    }

    public enum PriorityEnum
    {
        Low = 3,
        Medium = 2,
        High = 1
    }

    public enum BugStatusEnum
    {
        Pending = 1,
        Assigned = 2,
        Resolved = 3,
        Close = 4
    }
}