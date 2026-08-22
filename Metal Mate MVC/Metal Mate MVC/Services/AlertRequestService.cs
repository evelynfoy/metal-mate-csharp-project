using Metal_Mate_MVC.Data;
using Metal_Mate_MVC.Models;
using Microsoft.EntityFrameworkCore;

namespace Metal_Mate_MVC.Services
{
    public interface IAlertRequestService
    {
        Task<List<AlertRequest>> GetForUserAsync(string userId);
        Task AddAsync(AlertRequest alertRequest);
        Task SaveAsync(AlertRequest alertRequest);
        Task<AlertRequest?> GetByIdAsync(int id);
        Task<bool> DeleteAsync(int id);
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

        public async Task AddAsync(AlertRequest alertRequest)
        {
            _context.Add(alertRequest);
            await _context.SaveChangesAsync();
        }

        public async Task<AlertRequest?> GetByIdAsync(int id)
        {
            return await _context.AlertRequests.FindAsync(id);
        }

        public async Task SaveAsync(AlertRequest alertRequest)
        {
            _context.Update(alertRequest);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var alertRequest = await _context.AlertRequests.FindAsync(id);

            if (alertRequest == null)
            {
                return false;
            }

            _context.AlertRequests.Remove(alertRequest);
            await _context.SaveChangesAsync();

            return true;
        }

    }
}
