using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using BugTracker.Data;
using BugTracker.Models;
using Dapper;
using System.Linq;
using Microsoft.Extensions.Configuration;

namespace BugTracker.Repository
{
    public class BugRepository : DbConnectionRepo, IBugRepository
    {
        public BugRepository(IConfiguration config)
        : base(config)
        {

        }

        public async Task<bool> AddBug(BugModel model)
        {
            try
            {
                using (IDbConnection conn = Connection)
                {
                    string qAddBug = @" Insert Into ProjectBugs(Title, Description, ProjectId, CreatedOn, BugPriorityId)
                                        Values(@Title, @Description, @ProjectId, @CreatedOn, @BugPriorityId);
                                        SELECT LAST_INSERT_ID()";
                    conn.Open();

                    var result = await conn.QueryAsync<int>(qAddBug, new
                    {
                        Title = model.Title,
                        Description = model.Description,
                        ProjectId = model.ProjectId,
                        CreatedOn = model.CreatedOn,
                        BugPriorityId = model.PriorityId
                    });

                    var lastInsertedId = result.FirstOrDefault();

                    if (lastInsertedId > 0 && model.AssignedToIds != null && model.AssignedToIds.Count > 0)
                    {
                        return await AddUserProjectBugMap(model, conn, lastInsertedId);
                    }
                    return lastInsertedId > 0;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return false;
        }

        public async Task<bool> DeleteBug(int Id)
        {
            try
            {
                using (IDbConnection conn = Connection)
                {
                    string qDelete = @"Delete From ProjectBugs Where Id=@BugId";
                    var rowsAffected = await conn.ExecuteAsync(qDelete, new
                    {
                        BugId = Id
                    });

                    return rowsAffected > 0;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return false;
        }

        public async Task<IEnumerable<BugModel>> GetAllBugs(int Id, int Limit, int Offset)
        {
            List<BugModel> BugList = null;
            try
            {
                using (IDbConnection conn = Connection)
                {
                    string qGetAllBugs = @" Select ProjectBugs.Id, ProjectBugs.Title, ProjectBugs.Description, 
                                            ProjectBugs.CreatedOn, ProjectBugs.ResolvedOn, ProjectBugs.BugStatusId ,BugPriority.Id as PriorityId
                                            From ProjectBugs
                                            Inner Join BugPriority
                                            on ProjectBugs.BugPriorityId = BugPriority.Id
                                            Where ProjectBugs.ProjectId = @ProjectId
                                            Order by ProjectBugs.Id DESC
                                            Limit @Limit Offset @Offset;";
                    conn.Open();
                    var result = await conn.QueryAsync(qGetAllBugs, new
                    {
                        ProjectId = Id,
                        Limit = Limit,
                        Offset = Offset
                    });
                    if (result != null && result.AsList().Count > 0)
                    {
                        BugList = new List<BugModel>();

                        foreach (var bug in result)
                        {
                            BugList.Add(
                                new BugModel
                                {
                                    Id = bug.Id,
                                    Title = bug.Title,
                                    Description = bug.Description,
                                    CreatedOn = bug.CreatedOn,
                                    ResolvedOn = bug.ResolvedOn,
                                    BugStatusEnum = (BugStatusEnum)bug.BugStatusId,
                                    PriorityEnum = (PriorityEnum)bug.PriorityId
                                });

                        }
                    }
                    return BugList;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return BugList;
        }

        public async Task<IEnumerable<BugStatus>> GetAllBugStatus()
        {
            try
            {
                using (IDbConnection conn = Connection)
                {
                    string GetAllBugStatus = @"Select Id, Status From BugStatus";
                    conn.Open();
                    var result = await conn.QueryAsync<BugStatus>(GetAllBugStatus);
                    return result;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return null;
        }

        public async Task<BugModel> GetBugDetails(int ProjectId, int BugId)
        {
            BugModel BugDetails = null;
            try
            {
                using (IDbConnection conn = Connection)
                {
                    string GetDetails = @"  Select Projects.Name, ProjectBugs.Id, ProjectBugs.ProjectId, ProjectBugs.Title, ProjectBugs.Description, 
                                            ProjectBugs.CreatedOn, ProjectBugs.BugPriorityId, ProjectBugs.BugStatusId
                                            from ProjectBugs
                                            Inner Join Projects
                                            on Projects.Id = ProjectBugs.ProjectId
                                            Where ProjectBugs.ProjectId = @ProjectId And ProjectBugs.Id = @BugId";
                    conn.Open();

                    var Details = await conn.QueryAsync(GetDetails, new
                    {
                        ProjectId = ProjectId,
                        BugId = BugId
                    });

                    if (Details != null)
                    {
                        var Detail = Details.FirstOrDefault();

                        string AssignedTo = @"  Select UserProjectBugMaps.ApplicationUserId
                                                From UserProjectBugMaps
                                                Where UserProjectBugMaps.ProjectBugId= @BugId";

                        var AssignedResult = await conn.QueryAsync(AssignedTo, new
                        {
                            BugId = BugId
                        });

                        BugDetails = new BugModel
                        {
                            Id = Detail.Id,
                            ProjectId = Detail.ProjectId,
                            ProjectName = Detail.Name,
                            Title = Detail.Title,
                            Description = Detail.Description,
                            CreatedOn = Detail.CreatedOn,
                            PriorityId = Detail.BugPriorityId,
                            BugStatusId = Detail.BugStatusId
                        };

                        if (AssignedResult != null && AssignedResult.Any())
                        {
                            var AssignedMemberIds = new List<string>();
                            foreach (var member in AssignedResult)
                            {
                                AssignedMemberIds.Add(member.ApplicationUserId);
                            }
                            BugDetails.AssignedToIds = AssignedMemberIds;
                        }
                        return BugDetails;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return BugDetails;
        }
        public async Task<IEnumerable<BugPriority>> GetBugPriorityList()
        {
            IEnumerable<BugPriority> result = null;
            try
            {
                using (IDbConnection conn = Connection)
                {
                    string QueryGetAllBugPriority = @"SELECT Id, Name FROM BugPriority";

                    conn.Open();
                    result = await conn.QueryAsync<BugPriority>(QueryGetAllBugPriority);
                    return result;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return result;
        }

        public async Task<bool> MarkResolve(int Id)
        {
            try
            {
                using (IDbConnection conn = Connection)
                {
                    string query = @"Update ProjectBugs 
                                     Set ResolvedOn = @ResolveDate, BugStatusId = @BugStatus
                                     Where Id=@BugId";

                    var rowsAffected = await conn.ExecuteAsync(query, new
                    {
                        ResolveDate = DateTime.Now,
                        BugStatus = BugStatusEnum.Resolved,
                        BugId = Id
                    });
                    return rowsAffected > 0;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return false;
        }

        public async Task<int> TotalBugs(int ProjectId)
        {
            try
            {
                using (IDbConnection conn = Connection)
                {
                    string GetTotalBugsCount = @"Select count(*)
                                                From ProjectBugs
                                                Where ProjectId = @ProjectId";
                    var count = await conn.QueryFirstAsync<int>(GetTotalBugsCount, new
                    {
                        ProjectId = ProjectId
                    });
                    return count;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return 0;
        }

        public async Task<int> GetResolvedBugCount()
        {
            try
            {
                using(IDbConnection conn = Connection)
                {
                    string query = @"   Select Count(*) 
                                        From ProjectBugs 
                                        Where BugStatusId = 3";
                    var result = await conn.QueryFirstAsync<int>(query);
                    return result;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return 0;
        }
        public async Task<bool> UpdateBugDetails(BugModel model)
        {
            try
            {
                using (IDbConnection conn = Connection)
                {
                    string updateDetails = @" Update ProjectBugs
                                              Set Title = @Title, 
                                              Description = @Description,
                                              BugPriorityId = @BugPriorityId
                                              Where Id = @BugId and ProjectId = @ProjectId";

                    var rowsAffected = await conn.ExecuteAsync(updateDetails, new
                    {
                        Title = model.Title,
                        Description = model.Description,
                        BugPriorityId = model.PriorityId,
                        BugId = model.Id,
                        ProjectId = model.ProjectId
                    });

                    if (rowsAffected > 0)
                    {
                        string DeleteOldMapping = @"Delete From UserProjectBugMaps
                                                        Where ProjectBugId = @BugId";
                        var rowsDeleted = await conn.ExecuteAsync(DeleteOldMapping, new
                        {
                            BugId = model.Id
                        });
                        if (rowsDeleted > 0)
                        {
                            var isUpdated = await UpdatedBugStatus(model.Id, BugStatusEnum.Pending, conn);
                            if (isUpdated == false) return isUpdated;
                        }

                        if (model.AssignedToIds != null && model.AssignedToIds.Count > 0)
                        {
                            var isMapAdded = await AddUserProjectBugMap(model, conn, model.Id);
                            return isMapAdded;
                        }
                    }
                    return rowsAffected > 0;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return false;
        }

        private async Task<bool> AddUserProjectBugMap(BugModel model, IDbConnection conn, int BugId)
        {
            try
            {
                string insertToUserProjectBugMap = @" Insert Into UserProjectBugMaps(ProjectBugId, ApplicationUserId) 
                                                      Values(@ProjectBugId, @ApplicationUserId)";

                int sizeOfTeam = model.AssignedToIds.Count;
                var addMaps = new dynamic[sizeOfTeam];
                for (int i = 0; i < sizeOfTeam; i++)
                {
                    addMaps[i] = new { ProjectBugId = BugId, ApplicationUserId = model.AssignedToIds[i] };
                }
                var rowsAdded = await conn.ExecuteAsync(insertToUserProjectBugMap, addMaps);

                if (rowsAdded > 0)
                {
                    return await UpdatedBugStatus(BugId, BugStatusEnum.Assigned, conn);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return false;
        }

        private async Task<bool> UpdatedBugStatus(int BugId, BugStatusEnum Status, IDbConnection conn)
        {
            try
            {
                string updateBugStatus = @" Update ProjectBugs
                                            Set BugStatusId = @BugStatus
                                            Where Id =  @BugId";

                var updatedRows = await conn.ExecuteAsync(updateBugStatus, new
                {
                    BugStatus = Status,
                    BugId = BugId
                });

                return updatedRows > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return false;
        }


    }
}