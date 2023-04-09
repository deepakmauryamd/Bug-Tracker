using System.Collections.Generic;
using System.Threading.Tasks;
using BugTracker.Models;

namespace BugTracker.BusinessLogic
{
    public interface IProject
    {
        Task<ProjectModel> GetProjectDetailById(uint projectId);
        Task<ProjectModel> GetProjectNameById(uint projectId);
        Task<IEnumerable<UserModel>> GetTeamMembersByProjectId(uint projectId);
        Task<bool> AddProject(ProjectModel project);
        Task<bool> EditProject(ProjectModel project);
        Task<bool> DeleteProject(uint projectId);
        Task<IEnumerable<ProjectModel>> GetProjects(int limit, int offset);
        Task<IEnumerable<ProjectModel>> GetAllProjects();
    }
}