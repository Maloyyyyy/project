using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using JonyBalls3.Models;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using JonyBalls3.Data;

namespace JonyBalls3.Controllers
{
    [Authorize]
    public class ChatController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ChatController> _logger;

        public ChatController(ApplicationDbContext context, ILogger<ChatController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            var projects = await _context.Projects
                .Include(p => p.User)
                .Include(p => p.Contractor).ThenInclude(c => c.User)
                .Include(p => p.ChatMessages)
                .Where(p => p.UserId == userId || (p.Contractor != null && p.Contractor.UserId == userId))
                .OrderByDescending(p => p.ChatMessages.Any() ? p.ChatMessages.Max(m => m.SentAt) : p.CreatedAt)
                .ToListAsync();

            var chatRooms = new List<ChatRoomViewModel>();

            foreach (var project in projects)
            {
                User? otherUser = project.UserId == userId ? project.Contractor?.User : project.User;
                if (otherUser == null) continue;

                var lastMessage = project.ChatMessages.OrderByDescending(m => m.SentAt).FirstOrDefault();
                var unreadCount = project.ChatMessages.Count(m => m.ReceiverId == userId && !m.IsRead);

                chatRooms.Add(new ChatRoomViewModel
                {
                    ProjectId = project.Id,
                    ProjectName = project.Name,
                    OtherUserId = otherUser.Id,
                    OtherUserName = otherUser.FullName,
                    OtherUserAvatar = otherUser.AvatarUrl ?? "",
                    LastMessage = lastMessage?.Message ?? "Нет сообщений",
                    LastMessageTime = lastMessage?.SentAt ?? project.CreatedAt,
                    UnreadCount = unreadCount,
                    IsContractor = project.UserId != userId
                });
            }

            return View(chatRooms);
        }

        public async Task<IActionResult> Room(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            var project = await _context.Projects
                .Include(p => p.User)
                .Include(p => p.Contractor).ThenInclude(c => c.User)
                .Include(p => p.ChatMessages).ThenInclude(m => m.Sender)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (project == null) return NotFound();
            if (project.UserId != userId && (project.Contractor == null || project.Contractor.UserId != userId)) return Forbid();

            var otherUser = project.UserId == userId ? project.Contractor?.User : project.User;
            if (otherUser == null) return NotFound();

            var unreadMessages = project.ChatMessages.Where(m => m.ReceiverId == userId && !m.IsRead).ToList();
            foreach (var msg in unreadMessages) msg.ReadAt = DateTime.Now;
            await _context.SaveChangesAsync();

            var messages = project.ChatMessages.OrderBy(m => m.SentAt)
                .Select(m => new ChatMessageViewModel
                {
                    Id = m.Id,
                    ProjectId = project.Id,
                    ProjectName = project.Name,
                    SenderId = m.SenderId,
                    SenderName = m.Sender?.FullName ?? "Пользователь",
                    SenderAvatar = m.Sender?.AvatarUrl ?? "",
                    Message = m.Message,
                    SentAt = m.SentAt,
                    IsRead = m.IsRead,
                    IsMine = m.SenderId == userId
                }).ToList();

            ViewBag.Project = project;
            ViewBag.OtherUser = otherUser;

            return View(messages);
        }

        [HttpPost]
        public async Task<IActionResult> Send([FromBody] SendMessageModel model)
        {
            try
            {
                var senderId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var project = await _context.Projects.Include(p => p.Contractor).FirstOrDefaultAsync(p => p.Id == model.ProjectId);
                if (project == null) return Json(new { success = false, message = "Проект не найден" });

                string receiverId = project.UserId == senderId ? project.Contractor?.UserId : project.UserId;
                if (string.IsNullOrEmpty(receiverId)) return Json(new { success = false, message = "Получатель не найден" });

                var message = new ChatMessage
                {
                    ProjectId = model.ProjectId,
                    SenderId = senderId,
                    ReceiverId = receiverId,
                    Message = model.Message,
                    SentAt = DateTime.Now
                };

                _context.ChatMessages.Add(message);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = new { id = message.Id, text = message.Message, senderId = message.SenderId, sentAt = message.SentAt } });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при отправке сообщения");
                return Json(new { success = false, message = "Ошибка сервера" });
            }
        }
    }

    public class SendMessageModel
    {
        public int ProjectId { get; set; }
        public string Message { get; set; } = "";
    }

    public class ChatMessageViewModel
    {
        public int Id { get; set; }
        public int ProjectId { get; set; }
        public string ProjectName { get; set; } = "";
        public string SenderId { get; set; } = "";
        public string SenderName { get; set; } = "";
        public string SenderAvatar { get; set; } = "";
        public string Message { get; set; } = "";
        public DateTime SentAt { get; set; }
        public bool IsRead { get; set; }
        public bool IsMine { get; set; }
    }

    public class ChatRoomViewModel
    {
        public int ProjectId { get; set; }
        public string ProjectName { get; set; } = "";
        public string OtherUserId { get; set; } = "";
        public string OtherUserName { get; set; } = "";
        public string OtherUserAvatar { get; set; } = "";
        public string LastMessage { get; set; } = "";
        public DateTime LastMessageTime { get; set; }
        public int UnreadCount { get; set; }
        public bool IsContractor { get; set; }
    }
}