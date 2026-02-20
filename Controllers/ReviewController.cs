using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using JonyBalls3.Services;
using JonyBalls3.Models;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using JonyBalls3.Data;

namespace JonyBalls3.Controllers
{
    [Authorize]
    public class ReviewController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ReviewController> _logger;

        public ReviewController(
            ApplicationDbContext context,
            ILogger<ReviewController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Review/ForContractor/5
        public async Task<IActionResult> ForContractor(int id)
        {
            var contractor = await _context.ContractorProfiles
                .Include(c => c.User)
                .Include(c => c.Reviews)
                    .ThenInclude(r => r.User)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (contractor == null)
            {
                return NotFound();
            }

            return View(contractor);
        }

        // POST: Review/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int contractorId, int rating, string comment, int? projectId)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                // Проверяем, не оставлял ли пользователь уже отзыв этому подрядчику
                var existingReview = await _context.Reviews
                    .FirstOrDefaultAsync(r => r.ContractorId == contractorId && r.UserId == userId);

                if (existingReview != null)
                {
                    return Json(new { success = false, message = "Вы уже оставляли отзыв этому подрядчику" });
                }

                var review = new Review
                {
                    ContractorId = contractorId,
                    UserId = userId,
                    ProjectId = projectId,
                    Rating = rating,
                    Comment = comment,
                    CreatedAt = DateTime.Now
                };

                _context.Reviews.Add(review);

                // Обновляем рейтинг подрядчика
                var contractor = await _context.ContractorProfiles
                    .Include(c => c.Reviews)
                    .FirstOrDefaultAsync(c => c.Id == contractorId);

                if (contractor != null)
                {
                    contractor.Rating = (contractor.Reviews.Sum(r => r.Rating) + rating) / (contractor.Reviews.Count() + 1.0);
                    contractor.ReviewsCount = contractor.Reviews.Count() + 1;
                }

                await _context.SaveChangesAsync();

                return Json(new { 
                    success = true, 
                    message = "Отзыв добавлен",
                    newRating = contractor?.Rating ?? 0,
                    newCount = contractor?.ReviewsCount ?? 0
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при создании отзыва");
                return Json(new { success = false, message = "Ошибка сервера" });
            }
        }

        // POST: Review/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var review = await _context.Reviews
                    .Include(r => r.Contractor)
                    .FirstOrDefaultAsync(r => r.Id == id);

                if (review == null)
                {
                    return NotFound();
                }

                // Проверяем, что пользователь является автором отзыва
                if (review.UserId != userId)
                {
                    return Forbid();
                }

                _context.Reviews.Remove(review);

                // Пересчитываем рейтинг подрядчика
                var contractor = review.Contractor;
                var remainingReviews = await _context.Reviews
                    .Where(r => r.ContractorId == contractor.Id && r.Id != id)
                    .ToListAsync();

                if (remainingReviews.Any())
                {
                    contractor.Rating = remainingReviews.Average(r => r.Rating);
                    contractor.ReviewsCount = remainingReviews.Count;
                }
                else
                {
                    contractor.Rating = 0;
                    contractor.ReviewsCount = 0;
                }

                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Отзыв удален" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при удалении отзыва");
                return Json(new { success = false, message = "Ошибка сервера" });
            }
        }
    }
}