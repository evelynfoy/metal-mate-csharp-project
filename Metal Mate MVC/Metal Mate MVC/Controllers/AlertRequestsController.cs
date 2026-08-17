using Metal_Mate_MVC.Data;
using Metal_Mate_MVC.Models;
using Metal_Mate_MVC.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Metal_Mate_MVC.Services;

namespace Metal_Mate_MVC.Controllers
{
    [Authorize]
    public class AlertRequestsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AlertRequestsController> _logger;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IAlertRequestService _alertRequestService;

        public AlertRequestsController(ApplicationDbContext context,
                                       ILogger<AlertRequestsController> logger,
                                       UserManager<ApplicationUser> userManager,
                                       IAlertRequestService alertRequestService)
        {
            _context = context;
            _logger = logger;
            _userManager = userManager;
            _alertRequestService = alertRequestService;
        }

        // GET: AlertRequests
        public async Task<IActionResult> Index()
        {
            var model = new AlertRequestIndexViewModel();

            try
            {
                var user = await _userManager.GetUserAsync(User);

                if (user == null)
                {
                    _logger.LogError("The user is null. User: {UserName}", User.Identity?.Name);
                    model.ErrorMessage = "Your alert requests are temporarily unavailable. Please try again later.";
                    return View(model);
                }

                model.AlertRequests =
                    await _alertRequestService.GetForUserAsync(user.Id);

                
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving alert requests.");
                model.ErrorMessage = "Your alert requests are temporarily unavailable. Please try again later.";
            }

            return View(model);
        }

        // GET: AlertRequests/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var alertRequest = await _context.AlertRequests
                .Include(a => a.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (alertRequest == null)
            {
                return NotFound();
            }

            return View(alertRequest);
        }

        // GET: AlertRequests/Create
        public IActionResult Create()
        {
            var model = new AlertRequest
            {
                Metal = string.Empty, 
                Currency = string.Empty,
                Operator = ComparisonOperator.GreaterThan,
                Value = 0,
                IsEnabled = true 
            };
            return View();
        }

        // POST: AlertRequests/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AlertRequestViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return Unauthorized();
            }

            var alertRequest = new AlertRequest
            {
                Metal = model.Metal,
                Currency = model.Currency,
                Operator = model.Operator,
                Value = model.Value,
                IsEnabled = model.IsEnabled,
                UserId = user.Id
            };

            alertRequest.UserId = user.Id;
            alertRequest.User = user;

            foreach (var modelState in ModelState)
            {
                foreach (var error in modelState.Value.Errors)
                {
                    Console.WriteLine($"{modelState.Key}: {error.ErrorMessage}");
                }
            }

            ModelState.Remove(nameof(alertRequest.UserId));
            ModelState.Remove(nameof(alertRequest.User));

            if (ModelState.IsValid)
            {
                _context.Add(alertRequest);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(alertRequest);
        }

        // GET: AlertRequests/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var alertRequest = await _context.AlertRequests.FindAsync(id);
            if (alertRequest == null)
            {
                return NotFound();
            }
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", alertRequest.UserId);
            return View(alertRequest);
        }

        // POST: AlertRequests/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Metal,Currency,Operator,Value,IsEnabled,UserId")] AlertRequest alertRequest)
        {
            if (id != alertRequest.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(alertRequest);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AlertRequestExists(alertRequest.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", alertRequest.UserId);
            return View(alertRequest);
        }

        // GET: AlertRequests/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var alertRequest = await _context.AlertRequests
                .Include(a => a.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (alertRequest == null)
            {
                return NotFound();
            }

            return View(alertRequest);
        }

        // POST: AlertRequests/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var alertRequest = await _context.AlertRequests.FindAsync(id);
            if (alertRequest != null)
            {
                _context.AlertRequests.Remove(alertRequest);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AlertRequestExists(int id)
        {
            return _context.AlertRequests.Any(e => e.Id == id);
        }
    }
}
