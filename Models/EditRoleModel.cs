using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BugTracker.Models
{
    public class EditRoleModel
    {
        [Display(Name="Role ID")]
        public string Id { get; set; }

        [Required(ErrorMessage="Role name is required")]
        [Display(Name="Role Name")]
        public string RoleName { get; set; }

        public List<string> Users { get; set; }
    }
}