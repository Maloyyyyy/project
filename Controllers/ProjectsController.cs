using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using JonyBalls3.Services;
using JonyBalls3.Models;

namespace JonyBalls3.Controllers
{
    [Authorize] // Только для авторизованных
    public class ProjectsController : Controller
    {
        private readonly ProjectService _projectService;

        public ProjectsController(ProjectService projectService)
        {
            _projectService = projectService;
        }

        public IActionResult Index()
        {
            var projects = _projectService.GetAllProjects();
            return View(projects);
        }

        public IActionResult Details(int id)
        {
            var project = _projectService.GetProjectById(id);
            if (project == null)
            {
                return NotFound();
            }
            return View(project);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(SimpleProject project)
        {
            if (ModelState.IsValid)
            {
                _projectService.AddProject(project);
                return RedirectToAction("Index");
            }
            return View(project);
        }

        public IActionResult Edit(int id)
        {
            var project = _projectService.GetProjectById(id);
            if (project == null)
            {
                return NotFound();
            }
            return View(project);
        }

        [HttpPost]
        public IActionResult Edit(SimpleProject project)
        {
            if (ModelState.IsValid)
            {
                _projectService.UpdateProject(project);
                return RedirectToAction("Details", new { id = project.Id });
            }
            return View(project);
        }

        public IActionResult Delete(int id)
        {
            var project = _projectService.GetProjectById(id);
            if (project == null)
            {
                return NotFound();
            }
            return View(project);
        }

        [HttpPost, ActionName("Delete")]
        public IActionResult DeleteConfirmed(int id)
        {
            _projectService.DeleteProject(id);
            return RedirectToAction("Index");
        }
    }
}
