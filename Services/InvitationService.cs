using Microsoft.EntityFrameworkCore;
using JonyBalls3.Data;
using JonyBalls3.Models;

namespace JonyBalls3.Services
{
    public class InvitationService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<InvitationService> _logger;

        public InvitationService(
            ApplicationDbContext context,
            ILogger<InvitationService> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Создать приглашение
        public async Task<Invitation> CreateInvitationAsync(int projectId, int contractorId, string message, string userId)
        {
            var invitation = new Invitation
            {
                ProjectId = projectId,
                ContractorId = contractorId,
                Message = message,
                Status = InvitationStatus.Pending,
                SentAt = DateTime.Now
            };

            _context.Invitations.Add(invitation);
            await _context.SaveChangesAsync();

            // Создаем системное сообщение в чате
            var project = await _context.Projects
                .Include(p => p.Contractor)
                .FirstOrDefaultAsync(p => p.Id == projectId);

            if (project != null)
            {
                var chatMessage = new ChatMessage
                {
                    ProjectId = projectId,
                    SenderId = userId,
                    ReceiverId = project.Contractor?.UserId,
                    Message = $"📨 Приглашение отправлено: {message}",
                    SentAt = DateTime.Now
                };
                _context.ChatMessages.Add(chatMessage);
                await _context.SaveChangesAsync();
            }

            return invitation;
        }

        // Получить приглашения для подрядчика
        public async Task<List<Invitation>> GetInvitationsForContractorAsync(int contractorId)
        {
            return await _context.Invitations
                .Include(i => i.Project)
                .ThenInclude(p => p.User)
                .Where(i => i.ContractorId == contractorId)
                .OrderByDescending(i => i.SentAt)
                .ToListAsync();
        }

        // Получить приглашения для проекта
        public async Task<List<Invitation>> GetInvitationsForProjectAsync(int projectId)
        {
            return await _context.Invitations
                .Include(i => i.Contractor)
                .ThenInclude(c => c.User)
                .Where(i => i.ProjectId == projectId)
                .OrderByDescending(i => i.SentAt)
                .ToListAsync();
        }

        // Принять приглашение
        public async Task<bool> AcceptInvitationAsync(int invitationId, string userId)
        {
            var invitation = await _context.Invitations
                .Include(i => i.Project)
                .Include(i => i.Contractor)
                .FirstOrDefaultAsync(i => i.Id == invitationId);

            if (invitation == null || invitation.Status != InvitationStatus.Pending)
                return false;

            // Проверяем, что это тот подрядчик
            if (invitation.Contractor.UserId != userId)
                return false;

            invitation.Status = InvitationStatus.Accepted;
            invitation.RespondedAt = DateTime.Now;

            // Привязываем подрядчика к проекту
            var project = invitation.Project;
            project.ContractorId = invitation.ContractorId;
            project.UpdatedAt = DateTime.Now;

            // Создаем сообщение в чате
            var chatMessage = new ChatMessage
            {
                ProjectId = project.Id,
                SenderId = userId,
                ReceiverId = project.UserId,
                Message = "✅ Приглашение принято! Теперь можно обсуждать детали.",
                SentAt = DateTime.Now
            };
            _context.ChatMessages.Add(chatMessage);

            await _context.SaveChangesAsync();
            return true;
        }

        // Отклонить приглашение
        public async Task<bool> RejectInvitationAsync(int invitationId, string userId)
        {
            var invitation = await _context.Invitations
                .Include(i => i.Project)
                .Include(i => i.Contractor)
                .FirstOrDefaultAsync(i => i.Id == invitationId);

            if (invitation == null || invitation.Status != InvitationStatus.Pending)
                return false;

            // Проверяем, что это тот подрядчик
            if (invitation.Contractor.UserId != userId)
                return false;

            invitation.Status = InvitationStatus.Rejected;
            invitation.RespondedAt = DateTime.Now;

            // Создаем сообщение в чате
            var chatMessage = new ChatMessage
            {
                ProjectId = invitation.ProjectId,
                SenderId = userId,
                ReceiverId = invitation.Project.UserId,
                Message = "❌ Приглашение отклонено",
                SentAt = DateTime.Now
            };
            _context.ChatMessages.Add(chatMessage);

            await _context.SaveChangesAsync();
            return true;
        }

        // Отменить приглашение (для заказчика)
        public async Task<bool> CancelInvitationAsync(int invitationId, string userId)
        {
            var invitation = await _context.Invitations
                .Include(i => i.Project)
                .FirstOrDefaultAsync(i => i.Id == invitationId);

            if (invitation == null || invitation.Status != InvitationStatus.Pending)
                return false;

            // Проверяем, что это владелец проекта
            if (invitation.Project.UserId != userId)
                return false;

            invitation.Status = InvitationStatus.Cancelled;
            invitation.RespondedAt = DateTime.Now;

            await _context.SaveChangesAsync();
            return true;
        }

        // Получить количество ожидающих приглашений для подрядчика
        public async Task<int> GetPendingCountAsync(int contractorId)
        {
            return await _context.Invitations
                .Where(i => i.ContractorId == contractorId && i.Status == InvitationStatus.Pending)
                .CountAsync();
        }

        // Проверить, отправлял ли уже приглашение
        public async Task<bool> HasExistingInvitationAsync(int projectId, int contractorId)
        {
            return await _context.Invitations
                .AnyAsync(i => i.ProjectId == projectId && 
                               i.ContractorId == contractorId && 
                               i.Status == InvitationStatus.Pending);
        }
    }
}