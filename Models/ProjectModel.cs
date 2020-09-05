using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BugTracker.Models
{
    public class ProjectModel
    {
        public int Id { get; set; }
        [Required]
        [Display(Name="Name of the project")]
        public string Name { get; set; }

        [Required]
        [Display(Name="Project Manager")]
        public string ProjectManagerId {get; set;}
        public string ProjectManager { get; set; }

        [Required]
        [Display(Name="Team members")]
        public List<string> TeamMembersId {get; set;} 
        
        public MultiSelectList TeamMembers {get; set;}

        public int TeamMembersCount {get; set;}

        public int TotalBugs { get; set; }
    }
}