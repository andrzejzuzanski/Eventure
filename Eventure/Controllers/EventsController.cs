using Eventure.Data;
using Eventure.Helpers;
using Eventure.Models;
using Eventure.Services;
using Eventure.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Eventure.Controllers
{
    [Authorize]
    public class EventsController : Controller
    {
        private readonly IEventService _eventService;
        private readonly IUserContextService _userContextService;
        private readonly ICommentService _commentService;
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public EventsController(IEventService eventService, IUserContextService userContextService, ICommentService commentService, ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _eventService = eventService;
            _userContextService = userContextService;
            _commentService = commentService;
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: Events 
        [AllowAnonymous]
        public async Task<IActionResult> Index(string searchTitle, string location, DateTime? startDate, int? categoryId, int pageNumber = 1)
        {
            int pageSize = 10;

            var paginated = await _eventService.GetFilteredEventsAsync(searchTitle, location, startDate, categoryId, pageNumber, pageSize);

            var categories = await _eventService.GetCategorySelectListAsync();

            ViewBag.Categories = new SelectList(categories, "Value", "Text", categoryId?.ToString());

            return View(paginated);
        }

        // GET: Events/Create
        public async Task<IActionResult> Create()
        {
            var vm = new EventCreateViewModel
            {
                Categories = await _eventService.GetCategorySelectListAsync()
            };

            return View(vm);
        }

        // POST: Events/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(EventCreateViewModel vm)
        {
            ModelState.Remove(nameof(vm.Categories));
            ModelState.Remove(nameof(vm.Latitude));
            ModelState.Remove(nameof(vm.Longitude));
            ModelState.Remove(nameof(vm.EventImage));

            if (ModelState.IsValid)
            {
                var user = await _userContextService.GetCurrentUserAsync();

                string imageUrl = null;
                if (vm.EventImage != null)
                {
                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "events");
                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + vm.EventImage.FileName;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    Directory.CreateDirectory(uploadsFolder);
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await vm.EventImage.CopyToAsync(fileStream);
                    }
                    imageUrl = "/uploads/events/" + uniqueFileName;
                }

                await _eventService.CreateEventAsync(vm, user.Id, imageUrl);

                TempData["Message"] = "Wydarzenie zostało utworzone.";
                TempData["MessageType"] = "success";
                return RedirectToAction(nameof(Index));
            }
            vm.Categories = await _eventService.GetCategorySelectListAsync();
            return View(vm);
        }

        // GET: Events/Details
        public async Task<IActionResult> Details(int id)
        {
            var ev = await _eventService.GetEventWithDetailsAsync(id);
            if (ev == null)
                return NotFound();

            var user = await _userContextService.GetCurrentUserAsync();
            bool isParticipant = user != null && (ev.OrganizerId == user.Id || ev.Participants.Any(p => p.UserId == user.Id));

            List<Comment> comments = new List<Comment>();
            if (isParticipant)
            {
                comments = await _commentService.GetCommentsForEventAsync(id);
            }

            var recommendedEvents = await _eventService.GetRecommendedEventsAsync(ev.CategoryId, ev.Id);

            var viewModel = new EventDetailsViewModel
            {
                Event = ev,
                RootComments = comments,
                IsUserParticipant = isParticipant,
                RecommendedEvents = recommendedEvents
            };

            return View(viewModel);
        }


        // GET: Events/Edit
        public async Task<IActionResult> Edit(int id)
        {
            var ev = await _eventService.GetEventByIdAsync(id);

            if (ev == null)
                return NotFound();

            var user = await _userContextService.GetCurrentUserAsync();
            if (!await _eventService.CanUserModifyEventAsync(id, user.Id))
                return Forbid();

            var vm = new EventCreateViewModel
            {
                Title = ev.Title,
                Description = ev.Description,
                StartDateTime = ev.StartDateTime,
                EndDateTime = ev.EndDateTime,
                Location = ev.Location,
                MaxParticipants = ev.MaxParticipants,
                CategoryId = ev.CategoryId,
                Latitude = ev.Latitude,
                Longitude = ev.Longitude,
                Categories = await _eventService.GetCategorySelectListAsync()
            };

            return View(vm);
        }

        // POST: Events/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EventCreateViewModel vm)
        {
            ModelState.Remove(nameof(vm.Categories));
            ModelState.Remove(nameof(vm.Latitude));
            ModelState.Remove(nameof(vm.Longitude));
            ModelState.Remove(nameof(vm.EventImage));

            if (!ModelState.IsValid)
            {
                vm.Categories = await _eventService.GetCategorySelectListAsync();
                return View(vm);
            }

            var user = await _userContextService.GetCurrentUserAsync();

            string imageUrl = null;
            if (vm.EventImage != null)
            {
                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "events");
                string uniqueFileName = Guid.NewGuid().ToString() + "_" + vm.EventImage.FileName;
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                Directory.CreateDirectory(uploadsFolder);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await vm.EventImage.CopyToAsync(fileStream);
                }
                imageUrl = "/uploads/events/" + uniqueFileName;
            }

            var isSuccess = await _eventService.UpdateEventAsync(id, vm, user.Id, imageUrl);

            if(!isSuccess)
                return NotFound();

            TempData["Message"] = "Wydarzenie zostało zaktualizowane.";
            TempData["MessageType"] = "success";
            return RedirectToAction(nameof(Index));
        }

        // DELETE: Events/Delete
        public async Task<IActionResult> Delete(int id)
        {
            var ev = await _eventService.GetEventWithDetailsAsync(id);

            if (ev == null)
                return NotFound();

            var user = await _userContextService.GetCurrentUserAsync();

            if (!await _eventService.CanUserModifyEventAsync(id, user.Id))
                return Forbid();

            return View(ev);
        }

        // POST: Events/Delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await _userContextService.GetCurrentUserAsync();
            var isSuccess = await _eventService.DeleteEventAsync(id, user.Id);

            if(!isSuccess)
                return NotFound();

            TempData["Message"] = "Wydarzenie zostało usunięte.";
            TempData["MessageType"] = "success";
            return RedirectToAction(nameof(Index));
        }

        //POST Events/Join
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Join(int id)
        {
            var user = await _userContextService.GetCurrentUserAsync();

            var (isSuccess, message) = await _eventService.JoinEventAsync(id, user.Id);

            if (isSuccess)
            {
                TempData["Message"] = message;
                TempData["MessageType"] = "success";
            }
            else
            {
                TempData["Message"] = message;
                TempData["MessageType"] = "danger";
            }
                return RedirectToAction(nameof(Details), new { id });
        }

        //POST Events/Leave
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Leave(int id)
        {
            var user = await _userContextService.GetCurrentUserAsync();
            var (isSuccess, message) = await _eventService.LeaveEventAsync(id, user.Id);

            if (isSuccess)
            {
                TempData["Message"] = "Pomyślnie opuściłeś wydarzenie.";
                TempData["MessageType"] = "success";
            }
            else
            {
                TempData["Message"] = "Nie jesteś uczestnikiem tego wydarzenia.";
                TempData["MessageType"] = "warning";
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpGet]
        public async Task<JsonResult> GetEventsForCalendar()
        {
            var events = await _context.Events
                .Select(e => new
                {
                    title = e.Title,
                    start = e.StartDateTime.ToString("yyyy-MM-ddTHH:mm:ss"),
                    end = e.EndDateTime.ToString("yyyy-MM-ddTHH:mm:ss"),
                    url = Url.Action("Details", "Events", new { id = e.Id })
                })
                .ToListAsync();

            return Json(events);
        }

        //GET Events/MyEvents
        public async Task<IActionResult> MyEvents()
        {
            var user = await _userContextService.GetCurrentUserAsync();
            var vm = await _eventService.GetUserEventsAsync(user.Id);

            return View(vm);
        }
        public IActionResult Calendar()
        {
            return View();
        }
    }
}
