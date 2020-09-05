using System.ComponentModel.DataAnnotations;

namespace BugTracker.Models
{
    public class UserLoginModel
    {
        public int Id { get; set; }

        [Required]
        public string Username { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}