using System;
using System.Linq;
using System.Threading.Tasks;
using BugTracker.Models;
using BugTracker.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using cloudscribe.Pagination.Models;
using System.Collections.Generic;

namespace BugTracker.Controllers
{
    [Authorize]
    public class ProjectBugController : Controller
    {
        private readonly IProjectRepository _projectRepo;
        private readonly IBugRepository _bugRepo;

        public ProjectBugController(IProjectRepository projectRepo, IBugRepository bugRepo)
        {
            _projectRepo = projectRepo;
            _bugRepo = bugRepo;
        }

        [HttpGet("/AllBugs/{pid}")]
        public async Task<ActionResult> AllBugs(int PId, string SuccessMessage, int pageNumber = 1, int pageSize = 5)
        {
            if (PId > 0)
            {
                var project = await _projectRepo.GetProjectNameById(PId);
                if (project != null)
                {
                    int excludeRecords = (pageNumber * pageSize) - pageSize;
                    var BugList = await _bugRepo.GetAllBugs(PId, pageSize, excludeRecords) ?? new List<BugModel>();
                    int totalBugCount = await _bugRepo.TotalBugs(PId);
                    ViewBag.ProjectName = project.Name;
                    ViewBag.ProjectId = PId;
                    ViewBag.SuccessMessage = SuccessMessage;
                    var result = new PagedResult<BugModel>
                    {
                        Data = BugList.ToList(),
                        TotalItems = totalBugCount,
                        PageNumber = pageNumber,
                        PageSize = pageSize
                    };
                    return View(result);
                }
            }
            return RedirectToAction("Index", "Home");
        }

        [HttpGet("/AddBug/{id}")]
        public async Task<ActionResult> AddBug(int Id, string message = null)
        {
            var project = await _projectRepo.GetProjectDetails(Id);
            if (project != null)
            {
                BugModel model = new BugModel();

                model.ProjectName = project.Name;
                model.ProjectId = Id;

                var bugPriorityList = await _bugRepo.GetBugPriorityList();
                var teamMembers = await _projectRepo.GetTeamMembers(Id);

                ViewBag.bugPriorityList = bugPriorityList;
                ViewBag.teamMembers = new SelectList(teamMembers, "Id", "Name");

                ViewBag.message = message;

                return View(model);
            }
            return RedirectToAction("Index", "Home");
        }

        [HttpPost("AddBug/{id}")]
        public async Task<ActionResult> AddBug(int Id, BugModel model)
        {
            if (ModelState.IsValid)
            {
                model.ProjectId = Id;
                model.CreatedOn = DateTime.Now;
                if (model.AssignedToIds != null && model.AssignedToIds[0] == null)
                {
                    model.AssignedToIds.RemoveAt(0);
                }
                var isBugAdded = await _bugRepo.AddBug(model);
                if (isBugAdded)
                {
                    return RedirectToAction(nameof(AllBugs), new { PId = Id });
                }
            }
            string message = "Something went wrong.";
            return RedirectToAction(nameof(AddBug), new { Id = Id, message = message });
        }

        [HttpGet("/MarkResolve/{pid}/{id}")]
        public async Task<ActionResult> MarkResolve(int Pid, int Id)
        {
            if (Id > 0)
            {
                var result = await _bugRepo.MarkResolve(Id);
                if (result)
                {
                    string SuccessMessage = "Bug Resolved successfully";
                    return RedirectToAction(nameof(AllBugs), new { Id = Pid, SuccessMessage = SuccessMessage });
                }
            }
            return RedirectToAction(nameof(AllBugs), new { Id = Pid });
        }

        [HttpGet("DeleteBug/{pid}/{id}")]
        public async Task<ActionResult> DeleteBug(int Pid, int Id)
        {
            if (Id > 0)
            {
                var result = await _bugRepo.DeleteBug(Id);
                if (result)
                {
                    string SuccessMessage = "Bug deleted successfully";
                    return RedirectToAction(nameof(AllBugs), new { Id = Pid, SuccessMessage = SuccessMessage });
                }
            }
            return RedirectToAction(nameof(AllBugs), new { Id = Pid });
        }

        [HttpGet("EditBug/{pid}/{id}")]
        public async Task<ActionResult> EditBug(int Pid, int Id)
        {
            if (Pid > 0 && Id > 0)
            {
                var BugDetails = await _bugRepo.GetBugDetails(Pid, Id);
                if (BugDetails != null)
                {
                    var bugPriorityList = await _bugRepo.GetBugPriorityList();
                    var teamMembers = await _projectRepo.GetTeamMembers(Pid);

                    ViewBag.bugPriorityList = bugPriorityList;

                    ViewBag.teamMembers = (teamMembers != null && teamMembers.Any())
                                        ? new SelectList(teamMembers, "Id", "Name", BugDetails.AssignedToIds)
                                        : null;

                    return View(BugDetails);
                }
            }
            return RedirectToAction(nameof(AllBugs), new { Id = Pid });
        }

        [HttpPost("EditBug/{pid}/{id}")]
        public async Task<ActionResult> EditBug(int Pid, int Id, BugModel model)
        {
            if (ModelState.IsValid)
            {
                model.ProjectId = Pid;
                model.Id = Id;
                if (model.AssignedToIds != null && model.AssignedToIds[0] == null)
                {
                    model.AssignedToIds.RemoveAt(0);
                }
                var isUpdated = await _bugRepo.UpdateBugDetails(model);
                if (isUpdated)
                {
                    return RedirectToAction(nameof(AllBugs), new { Id = Pid });
                }
            }
            return View(model);
        }
    }
}