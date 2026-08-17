using Metal_Mate_MVC.Data;
using Metal_Mate_MVC.Models;
using Microsoft.EntityFrameworkCore;

namespace Metal_Mate_MVC.Services
{
    public interface IAlertRequestService
    {
        Task<List<AlertRequest>> GetForUserAsync(string userId);
    }

    public class AlertRequestService : IAlertRequestService
    {
        private readonly ApplicationDbContext _context;

        public AlertRequestService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<AlertRequest>> GetForUserAsync(string userId)
        {
            return await _context.AlertRequests
                    .Include(a => a.User)
                    .Where(ar => ar.UserId == userId)
                    .ToListAsync();
        }
    }
}
