using System.ComponentModel.DataAnnotations;

namespace BugTracker.Models
{
    public class RoleModel
    {
        [Required]
        [Display(Name="Role Name")]
        public string RoleName { get; set; }
    }
}