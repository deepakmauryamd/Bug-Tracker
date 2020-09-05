namespace BugTracker.Data
{
    public class UserProjectBugMap
    {
        public int Id { get; set; }
        public int ProjectBugId { get; set; }
        public ProjectBug ProjectBug { get; set; }
        public string ApplicationUserId { get; set; }
        public ApplicationUser ApplicationUser { get; set; }
    }
}