using Metal_Mate_MVC.Data;
using Metal_Mate_MVC.Models;
using Microsoft.EntityFrameworkCore;

namespace Metal_Mate_MVC.Services
{
    public interface IAlertRequestService
    {
        Task<List<AlertRequest>> GetAllAlertRequestsAsync();
        Task<List<AlertRequest>> GetForUserAsync(string userId);
        Task AddAsync(AlertRequest alertRequest);
        Task SaveAsync(AlertRequest alertRequest);
        Task<AlertRequest?> GetByIdAsync(int id, string userId);
        Task<bool> DeleteAsync(int id, string userId);
    }

    public class AlertRequestService : IAlertRequestService
    {
        private readonly ApplicationDbContext _context;

        public AlertRequestService(ApplicationDbContext context)
        {
            _context = context;
        }


        public async Task<List<AlertRequest>> GetAllAlertRequestsAsync()
        {
            return await _context.AlertRequests
                    .ToListAsync();
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

        public async Task<AlertRequest?> GetByIdAsync(int id, string userId)
        {
            return await _context.AlertRequests
                .FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId);
        }

        public async Task SaveAsync(AlertRequest alertRequest)
        {
            _context.Update(alertRequest);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> DeleteAsync(int id, string userId)
        {
            var alertRequest = await _context.AlertRequests
                .FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId);

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
