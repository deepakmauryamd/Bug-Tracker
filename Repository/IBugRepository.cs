using System.Collections.Generic;
using System.Threading.Tasks;
using BugTracker.Data;
using BugTracker.Models;

namespace BugTracker.Repository
{
    public interface IBugRepository
    {
        Task<IEnumerable<BugPriority>> GetBugPriorityList();

        Task<bool> AddBug(BugModel model);

        Task<IEnumerable<BugModel>> GetAllBugs(int Id);

        Task<BugModel> GetBugDetails(int ProjectId, int BugId);

        Task<bool> MarkResolve(int Id);

        Task<bool> DeleteBug(int Id);

        Task<IEnumerable<BugStatus>> GetAllBugStatus(); 

        Task<bool> UpdateBugDetails(BugModel model);

        Task<int> TotalBugs(int ProjectId);

        Task<int> GetResolvedBugCount();
    }
}