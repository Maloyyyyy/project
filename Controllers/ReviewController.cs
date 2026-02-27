using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using JonyBalls3.Data;
using JonyBalls3.Models;
using System.Security.Claims;

namespace JonyBalls3.Controllers
{
    public class ReviewController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ReviewController> _logger;

        public ReviewController(ApplicationDbContext context, ILogger<ReviewController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [AllowAnonymous]
        public async Task<IActionResult> Contractor(int id)
        {
            var contractor = await _context.ContractorProfiles
                .Include(c => c.User)
                .Include(c => c.Reviews).ThenInclude(r => r.User)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (contractor == null) return NotFound();

            var viewModel = new ReviewListViewModel
            {
                ContractorId = contractor.Id,
                ContractorName = contractor.CompanyName,
                AverageRating = contractor.Rating,
                TotalReviews = contractor.ReviewsCount,
                Reviews = contractor.Reviews.Select(r => new ReviewViewModel
                {
                    Id = r.Id,
                    UserName = r.User?.FullName ?? "Пользователь",
                    UserAvatar = r.User?.AvatarUrl ?? "",
                    Rating = r.Rating,
                    Comment = r.Comment,
                    CreatedAt = r.CreatedAt,
                    IsRecommended = r.IsRecommended
                }).OrderByDescending(r => r.CreatedAt).ToList()
            };

            if (User.Identity?.IsAuthenticated == true)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                viewModel.CanAddReview = !viewModel.Reviews.Any(r => r.UserId == userId) && userId != contractor.UserId;
            }

            for (int i = 1; i <= 5; i++)
                viewModel.RatingDistribution[i] = viewModel.Reviews.Count(r => r.Rating == i);

            return View(viewModel);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ReviewViewModel model)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false, message = "Проверьте поля" });

            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var existing = await _context.Reviews.FirstOrDefaultAsync(r => r.ContractorId == model.ContractorId && r.UserId == userId);
                if (existing != null)
                    return Json(new { success = false, message = "Вы уже оставляли отзыв" });

                var contractor = await _context.ContractorProfiles.Include(c => c.Reviews).FirstOrDefaultAsync(c => c.Id == model.ContractorId);
                if (contractor == null)
                    return Json(new { success = false, message = "Подрядчик не найден" });

                var review = new Review
                {
                    ContractorId = model.ContractorId,
                    UserId = userId,
                    Rating = model.Rating,
                    Comment = model.Comment,
                    IsRecommended = model.IsRecommended,
                    CreatedAt = DateTime.Now
                };

                _context.Reviews.Add(review);
                contractor.Rating = (contractor.Reviews.Sum(r => r.Rating) + model.Rating) / (contractor.Reviews.Count + 1.0);
                contractor.ReviewsCount = contractor.Reviews.Count + 1;
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Отзыв добавлен" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка");
                return Json(new { success = false, message = "Ошибка сервера" });
            }
        }
    }

    public class ReviewViewModel
    {
        public int Id { get; set; }
        public int ContractorId { get; set; }
        public string UserId { get; set; } = "";
        public string UserName { get; set; } = "";
        public string UserAvatar { get; set; } = "";
        public int Rating { get; set; }
        public string Comment { get; set; } = "";
        public DateTime CreatedAt { get; set; }
        public bool IsRecommended { get; set; } = true;
    }

    public class ReviewListViewModel
    {
        public List<ReviewViewModel> Reviews { get; set; } = new();
        public double AverageRating { get; set; }
        public int TotalReviews { get; set; }
        public int ContractorId { get; set; }
        public string ContractorName { get; set; } = "";
        public bool CanAddReview { get; set; }
        public Dictionary<int, int> RatingDistribution { get; set; } = new();
    }
}