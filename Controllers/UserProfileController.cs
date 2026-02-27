using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using JonyBalls3.Models;
using JonyBalls3.Services;
using System.Security.Claims;

namespace JonyBalls3.Controllers
{
    [Authorize]
    public class UserProfileController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly IWebHostEnvironment _environment;
        private readonly ProjectService _projectService;
        private readonly ILogger<UserProfileController> _logger;

        public UserProfileController(
            UserManager<User> userManager,
            IWebHostEnvironment environment,
            ProjectService projectService,
            ILogger<UserProfileController> logger)
        {
            _userManager = userManager;
            _environment = environment;
            _projectService = projectService;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId);
            
            if (user == null) return NotFound();

            var projects = await _projectService.GetUserProjectsAsync(userId);
            
            var viewModel = new UserProfileViewModel
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email ?? "",
                PhoneNumber = user.PhoneNumber,
                AboutMe = user.AboutMe,
                City = user.City,
                BirthDate = user.BirthDate,
                AvatarUrl = user.AvatarUrl,
                CreatedAt = user.CreatedAt,
                ProjectsCount = projects.Count,
                CompletedProjectsCount = projects.Count(p => p.Status == ProjectStatus.Completed)
            };

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> Edit()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId);
            
            if (user == null) return NotFound();

            var viewModel = new UserProfileViewModel
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email ?? "",
                PhoneNumber = user.PhoneNumber,
                AboutMe = user.AboutMe,
                City = user.City,
                BirthDate = user.BirthDate,
                AvatarUrl = user.AvatarUrl
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UserProfileViewModel model)
        {
            if (ModelState.IsValid)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var user = await _userManager.FindByIdAsync(userId);
                
                if (user == null) return NotFound();

                user.FirstName = model.FirstName;
                user.LastName = model.LastName;
                user.AboutMe = model.AboutMe;
                user.City = model.City;
                user.BirthDate = model.BirthDate;
                user.PhoneNumber = model.PhoneNumber;

                if (model.AvatarFile != null && model.AvatarFile.Length > 0)
                {
                    var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads/avatars");
                    Directory.CreateDirectory(uploadsFolder);

                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(model.AvatarFile.FileName);
                    var filePath = Path.Combine(uploadsFolder, fileName);
                    
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.AvatarFile.CopyToAsync(stream);
                    }

                    if (!string.IsNullOrEmpty(user.AvatarUrl))
                    {
                        var oldFilePath = Path.Combine(_environment.WebRootPath, user.AvatarUrl.TrimStart('/'));
                        if (System.IO.File.Exists(oldFilePath)) System.IO.File.Delete(oldFilePath);
                    }

                    user.AvatarUrl = "/uploads/avatars/" + fileName;
                }

                var result = await _userManager.UpdateAsync(user);
                
                if (result.Succeeded)
                {
                    TempData["Success"] = "Профиль обновлен";
                    return RedirectToAction(nameof(Index));
                }
            }
            return View(model);
        }
    }
}