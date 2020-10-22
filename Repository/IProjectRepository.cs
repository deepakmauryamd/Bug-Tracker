using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using BugTracker.Models;

namespace BugTracker.Repository
{
    public interface IProjectRepository
    {
        Task<bool> AddProject(ProjectModel model);
        Task<bool> DeleteProject(int ProjectId);
        Task<int> TotalProjects();
        Task<IEnumerable<ProjectModel>> GetAllProject(int Limit, int Offset);
        Task<ProjectModel> GetProjectNameById(int Id);
        Task<bool> EditProject(ProjectModel model);
        Task<ProjectModel> GetProjectDetails(int Id);
        Task<IEnumerable<UserModel>> GetTeamMembers(int Id);
    }
}