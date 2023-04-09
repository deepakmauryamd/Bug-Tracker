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
using Microsoft.Extensions.DependencyInjection;
using BugTracker.BusinessLogic;

namespace BugTracker.Controllers
{
    [Authorize]
    public class ProjectBugController : Controller
    {
        private readonly IProjectRepository _projectRepo;
        private readonly IBugRepository _bugRepo;
        private readonly IServiceProvider _container;

        public ProjectBugController(IProjectRepository projectRepo, IBugRepository bugRepo, IServiceProvider container)
        {
            _projectRepo = projectRepo;
            _bugRepo = bugRepo;
            _container = container;
        }

        [HttpGet("/AllBugs/{projectId}", Name = "AllBugsActionMethod")]
        public async Task<ActionResult> AllBugs(uint projectId, string successMessage, int pageNumber = 1, int pageSize = 5)
        {
            if (projectId == 0)
            {
                return BadRequest();
            }
            var projectLogic = _container.GetRequiredService<IProject>();
            ProjectModel project = await projectLogic.GetProjectDetailById(projectId);
            if (project == null || project.Id <= 0)
            {
                return BadRequest();
            }
            int excludeRecords = (pageNumber * pageSize) - pageSize;
            var BugList = await _bugRepo.GetAllBugs((int)projectId, pageSize, excludeRecords) ?? new List<BugModel>();
            int totalBugCount = await _bugRepo.TotalBugs((int)projectId);
            ViewBag.ProjectName = project.Name;
            ViewBag.ProjectId = projectId;
            ViewBag.SuccessMessage = successMessage;
            var result = new PagedResult<BugModel>
            {
                Data = BugList.ToList(),
                TotalItems = totalBugCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
            return View(result);

        }

        [HttpGet("/AddBug/{projectId}")]
        public async Task<ActionResult> AddBug(uint projectId, string message = null)
        {
            var projectLogic = _container.GetRequiredService<IProject>();
            ProjectModel project = await projectLogic.GetProjectDetailById(projectId);
            if (project == null)
            {
                return BadRequest();
            }
            BugModel model = new BugModel
            {
                ProjectName = project.Name,
                ProjectId = (int)projectId,
            };

            var bugPriorityList = await _bugRepo.GetBugPriorityList();
            var teamMembers = await projectLogic.GetTeamMembersByProjectId(projectId);

            ViewBag.bugPriorityList = bugPriorityList;
            ViewBag.teamMembers = new SelectList(teamMembers, "Id", "Name");

            ViewBag.message = message;

            return View(model);
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
                    string successMessage = "Bug Resolved successfully";
                    return RedirectToRoute("AllBugsActionMethod", new { pid = Pid, successMessage = successMessage }); ;
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
                    return RedirectToRoute("AllBugsActionMethod", new { pid = Pid, successMessage = SuccessMessage }); ;
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
                    string message = "Edit successful";
                    return RedirectToRoute("AllBugsActionMethod", new { pid = Pid, successMessage = message });
                }
            }
            return View(model);
        }
    }
}