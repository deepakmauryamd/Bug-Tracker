using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using BugTracker.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using BugTracker.Repository;
using Microsoft.AspNetCore.Identity;
using BugTracker.Data;
using System.Collections;
using Microsoft.AspNetCore.Authorization;
using cloudscribe.Pagination.Models;

namespace BugTracker.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly IProjectRepository _projectRepo;
        private readonly IBugRepository _bugRepo;
        private readonly UserManager<ApplicationUser> _userManager;

        public HomeController(IProjectRepository projectRepo, IBugRepository bugRepo, UserManager<ApplicationUser> userManager)
        {
            _projectRepo = projectRepo;
            _bugRepo = bugRepo;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string message=null, int pageNumber=1, int pageSize=5)
        {
            int excludeRecords = (pageNumber * pageSize) - pageSize;
            var projects = await _projectRepo.GetProjects(pageSize, excludeRecords);
            if(projects != null && projects.Any())
            {
                foreach(var project in projects)
                {
                    project.TotalBugs = await _bugRepo.TotalBugs(project.Id);
                }
            }
            var details = await OverallDetails();
            ViewBag.totalResolvedBugsCount = details.Item2;
            ViewBag.totalBugsCount = details.Item3;
            ViewBag.Message = message;
            
            var result = new PagedResult<ProjectModel>{
                Data = projects.ToList(),
                TotalItems = details.Item1,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
            return View(result);
        }

        [Authorize(Roles = "Manager")]
        [HttpGet("/addproject")]
        public async Task<ActionResult> AddProject()
        {
            ViewBag.projectManagers = await GetProjctManagers();
            ViewBag.users = await GetTeamMembers();
            return View();
        }

        [Authorize(Roles = "Manager")]
        [HttpPost("/addproject")]
        public async Task<ActionResult> AddProject(ProjectModel model)
        {
            if (ModelState.IsValid)
            {
                bool isAdded = await _projectRepo.AddProject(model);
                if (isAdded)
                {
                    return RedirectToAction(nameof(Index));
                }
            }
            return View(model);
        }

        [Authorize(Roles = "Manager")]
        [HttpGet("/EditProject/{id}")]
        public async Task<ActionResult> EditProject(int Id, string message = null)
        {
            if (Id > 0)
            {
                var project = await _projectRepo.GetProjectDetails(Id);
                if (project != null)
                {
                    ViewBag.projectManagers = await GetProjctManagers(project.ProjectManagerId);

                    ViewBag.users = await GetTeamMembers(project.TeamMembersId);
                    ViewBag.message = message;
                    return View("Views/Home/EditProject.cshtml", project);
                }
            }
            return RedirectToAction("Index", "Home");
        }

        [Authorize(Roles = "Manager")]
        [HttpPost("/EditProject/{id}")]
        public async Task<ActionResult> EditProject(int Id, ProjectModel model)
        {
            if (ModelState.IsValid)
            {

                var isEditSuccess = await _projectRepo.EditProject(model);
                if (isEditSuccess)
                {
                    return RedirectToAction("Index", "Home");
                }
            }
            return RedirectToAction(nameof(EditProject), new { Id = model.Id, message = "Please Select any one team member" });
        }

        public async Task<ActionResult> DeleteProject(int Id)
        {
            if (Id > 0)
            {
                var isDeleted = await _projectRepo.DeleteProject(Id);
                if (isDeleted)
                {
                    string message = "Project deleted successfully";
                    return RedirectToAction(nameof(Index), new { message = message });
                }
            }
            return RedirectToAction("Index", "Home");
        }
        private async Task<SelectList> GetProjctManagers(Object selected = null)
        {
            var managerList = new List<UserModel>();
            var managers = await _userManager.GetUsersInRoleAsync("Manager");
            foreach (var manager in managers)
            {
                managerList.Add(new UserModel { Id = manager.Id, Name = manager.UserName });
            }
            var users = new SelectList(managerList, "Id", "Name", selected);
            return users;
        }

        private async Task<MultiSelectList> GetTeamMembers(IEnumerable selected = null)
        {
            var teamMemberList = new List<UserModel>();
            var users = await _userManager.GetUsersInRoleAsync("Developer");
            foreach (var user in users)
            {
                teamMemberList.Add(new UserModel { Id = user.Id, Name = user.UserName });
            }
            var teamMembers = new MultiSelectList(teamMemberList, "Id", "Name", selected);
            return teamMembers;
        }

        private async Task<Tuple<int, int, int>> OverallDetails()
        {
            var projects = await _projectRepo.AllProjects();
            int totalResolvedBugsCount =0;
            int totalBugsCount=0;
            if (projects != null && projects.Any())
            {     
                totalResolvedBugsCount = await _bugRepo.GetResolvedBugCount();
                foreach (var project in projects)
                {
                    totalBugsCount += await _bugRepo.TotalBugs(project.Id);
                }
            }
            var result = new Tuple<int, int , int>(projects.Count(), totalResolvedBugsCount, totalBugsCount);
            return result;
        }
        [AllowAnonymous]
        [Route("/notfoundpage")]
        public IActionResult NotFoundPage()
        {
            return View();
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}
