using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using BugTracker.Models;
using Dapper;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using BugTracker.Repository;

namespace BugTracker.Repository
{
    public class ProjectRepository : DbConnectionRepo, IProjectRepository
    {
        public ProjectRepository(IConfiguration config)
        : base(config)
        {

        }

        public async Task<bool> AddProject(ProjectModel model)
        {
            try
            {
                using (IDbConnection conn = Connection)
                {
                    string query = @"INSERT INTO Projects(Name, ApplicationUserId) Values(@Name, @ApplicationUserId);
                                    SELECT LAST_INSERT_ID()";
                    conn.Open();

                    var ids = await conn.QueryAsync<int>(query, new
                    {
                        Name = model.Name,
                        ApplicationUserId = model.ProjectManagerId
                    });
                    var projectId = ids.FirstOrDefault();
                    if (projectId > 0)
                    {
                        model.Id = projectId;
                        var result = await AddUserProjectMaps(model, conn);
                        return result;
                    }

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return false;
        }

        public async Task<ProjectModel> GetProjectDetails(int Id)
        {
            ProjectModel projectModel = null;
            if (Id > 0)
            {
                try
                {
                    using (IDbConnection conn = Connection)
                    {
                        string qGetProjectDetails = @"Select Projects.Name, Projects.ApplicationUserId as ProjectManagerId
                                                        From AspNetUsers
                                                        Inner Join Projects
                                                        on AspNetUsers.Id = Projects.ApplicationUserId
                                                        where Projects.Id = @ProjectId;";

                        string qGetAllTeamMenbers = @"Select UserProjectMaps.ApplicationUserId
                                                        From Projects
                                                        Inner Join UserProjectMaps 
                                                        on Projects.Id = UserProjectMaps.ProjectId
                                                        Where Projects.Id = @ProjectId;";

                        conn.Open();
                        var ResultGetProjectDetails = await conn.QueryAsync(qGetProjectDetails, new { ProjectId = Id });
                        var ResultGetAllTeamMembers = await conn.QueryAsync(qGetAllTeamMenbers, new { ProjectId = Id });

                        if (ResultGetProjectDetails.Any())
                        {
                            var project = ResultGetProjectDetails.FirstOrDefault();
                            projectModel = new ProjectModel
                            {
                                Name = project.Name,
                                ProjectManagerId = project.ProjectManagerId
                            };
                        }
                        if (projectModel != null && ResultGetAllTeamMembers.Any())
                        {
                            var teamMemberIds = new List<string>();
                            foreach (var member in ResultGetAllTeamMembers)
                            {
                                teamMemberIds.Add(member.ApplicationUserId);
                            }
                            projectModel.TeamMembersId = teamMemberIds;
                        }
                        return projectModel;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            return projectModel;
        }

        public async Task<IEnumerable<ProjectModel>> GetProjects(int Limit, int Offset)
        {
            try
            {
                using (IDbConnection conn = Connection)
                {
                    string query = @"SELECT Projects.Id, Projects.Name, AspNetUsers.UserName, count(UserProjectMaps.ApplicationUserId) as TotalMemberInTeam
                                        From Projects
                                        Inner Join UserProjectMaps
                                        on Projects.Id = UserProjectMaps.ProjectId
                                        Inner Join AspNetUsers
                                        on Projects.ApplicationUserId = AspNetUsers.Id
                                        group by Projects.Id, Projects.Name, AspNetUsers.UserName
                                        Limit @Limit Offset @Offset;";
                    conn.Open();
                    var ResultList = await conn.QueryAsync(query, new
                    {
                        Limit = Limit,
                        Offset = Offset
                    });
                    List<ProjectModel> projects = new List<ProjectModel>();

                    foreach (var project in ResultList)
                    {
                        projects.Add(new ProjectModel
                        {
                            Id = project.Id,
                            Name = project.Name,
                            ProjectManager = project.UserName,
                            TeamMembersCount = Convert.ToInt32(project.TotalMemberInTeam)
                        });
                    }
                    return projects;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return null;
        }

        public async Task<bool> EditProject(ProjectModel model)
        {
            if (model != null && model.Id > 0)
            {
                try
                {
                    using (IDbConnection conn = Connection)
                    {
                        string updateProject = @" Update Projects 
                                                  Set Name = @ProjectName, ApplicationUserId = @ProjectManagerId
                                                  Where Id = @ProjectId";

                        string deleteUserProjectMaps = @"Delete From UserProjectMaps 
                                                   Where ProjectId = @ProjectId";
                        conn.Open();

                        var updatedRows = await conn.ExecuteAsync(updateProject, new
                        {
                            ProjectName = model.Name,
                            ProjectManagerId = model.ProjectManagerId,
                            ProjectId = model.Id
                        });

                        if (updatedRows > 0)
                        {
                            var ResultUserProjectMaps = await conn.QueryAsync(deleteUserProjectMaps, new
                            {
                                ProjectId = model.Id
                            });

                            var result = await AddUserProjectMaps(model, conn);
                            return result;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            return false;
        }

        private async Task<bool> AddUserProjectMaps(ProjectModel model, IDbConnection conn)
        {
            try
            {
                string insertToUserProjectMap = "INSERT INTO UserProjectMaps(ProjectId, ApplicationUserId) Values(@ProjectId, @ApplicationUserId)";

                int sizeOfTeam = model.TeamMembersId.Count;

                var addMaps = new dynamic[sizeOfTeam];

                for (int i = 0; i < sizeOfTeam; i++)
                {
                    addMaps[i] = new { ProjectId = model.Id, ApplicationUserId = model.TeamMembersId[i] };
                }
                var rowsAffected = await conn.ExecuteAsync(insertToUserProjectMap, addMaps);
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return false;
        }

        public async Task<IEnumerable<UserModel>> GetTeamMembers(int Id)
        {
            List<UserModel> teamMembers = null;
            try
            {
                using (IDbConnection conn = Connection)
                {
                    string query = @"   Select AspNetUsers.Id, AspNetUsers.UserName
                                        From Projects
                                        Inner Join UserProjectMaps 
                                        on Projects.Id = UserProjectMaps.ProjectId
                                        Inner Join AspNetUsers
                                        on UserProjectMaps.ApplicationUserId = AspNetUsers.Id
                                        Where Projects.Id = @ProjectId";

                    conn.Open();
                    var result = await conn.QueryAsync(query, new
                    {
                        ProjectId = Id
                    });
                    if (result != null && result.Any())
                    {
                        teamMembers = new List<UserModel>();
                        foreach (var user in result)
                        {
                            teamMembers.Add(
                                new UserModel
                                {
                                    Id = user.Id,
                                    Name = user.UserName
                                }
                            );
                        }
                        return teamMembers;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return teamMembers;
        }

        public async Task<ProjectModel> GetProjectNameById(int Id)
        {
            ProjectModel project = null;
            try
            {
                using (IDbConnection conn = Connection)
                {
                    string query = @"   Select Name, Id
                                        From Projects 
                                        Where Id = @ProjectId";

                    project = await conn.QueryFirstAsync<ProjectModel>(query, new
                    {
                        ProjectId = Id
                    });

                    return project;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return project;
        }

        public async Task<bool> DeleteProject(int ProjectId)
        {
            try
            {
                using (IDbConnection conn = Connection)
                {
                    string query = @"Delete From Projects
                                     Where Id = @ProjectId";
                    var result = await conn.ExecuteAsync(query, new
                    {
                        ProjectId = ProjectId
                    });
                    return result > 0;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return false;
        }

        public async Task<IEnumerable<ProjectModel>> AllProjects()
        {
            IEnumerable<ProjectModel> projects = null;
            try
            {
                using (IDbConnection conn = Connection)
                {
                    string query = @"Select Id, Name
                                    From Projects;";
                    projects = await conn.QueryAsync<ProjectModel>(query);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return projects;
        }
    }
}