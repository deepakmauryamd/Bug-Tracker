using System.Collections.Generic;
using System.Threading.Tasks;
using BugTracker.Models;
using BugTracker.Repository;

namespace BugTracker.BusinessLogic
{
    public class Project : IProject
    {
        private readonly IProjectRepository _projectRepository;

        public Project(IProjectRepository projectRepository)
        {
            _projectRepository = projectRepository;
        }

        public async Task<bool> AddProject(ProjectModel project)
        {
            return await _projectRepository.AddProject(project);
        }

        public async Task<bool> DeleteProject(uint projectId)
        {
            return await _projectRepository.DeleteProject((int)projectId);
        }

        public async Task<bool> EditProject(ProjectModel project)
        {
            return await _projectRepository.EditProject(project);
        }

        public async Task<IEnumerable<ProjectModel>> GetAllProjects()
        {
            return await _projectRepository.AllProjects();
        }

        public async Task<ProjectModel> GetProjectDetailById(uint projectId)
        {
            return await _projectRepository.GetProjectDetails((int)projectId);
        }

        public Task<ProjectModel> GetProjectNameById(uint projectId)
        {
            throw new System.NotImplementedException();
        }

        public async Task<IEnumerable<ProjectModel>> GetProjects(int pageSize, int excludeRecords)
        {
            return await _projectRepository.GetProjects(pageSize, excludeRecords);
        }

        public async Task<IEnumerable<UserModel>> GetTeamMembersByProjectId(uint projectId)
        {
            return await _projectRepository.GetTeamMembers((int)projectId);
        }
    }
}