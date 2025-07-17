using Eventure.Data;
using Eventure.Models;
using Eventure.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Eventure.Controllers
{
    [Authorize]
    public class EventsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public EventsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Events 
        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            var events = await _context.Events
                .Include(e => e.Organizer)
                .OrderBy(e => e.StartDateTime)
                .AsNoTracking()
                .ToListAsync();

            return View(events);
        }

        // GET: Events/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Events/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(EventCreateViewModel vm)
        {
            if (ModelState.IsValid)
            {
                var user = await GetCurrentUserAsync();

                var newEvent = new Event
                {
                    Title = vm.Title,
                    Description = vm.Description,
                    StartDateTime = vm.StartDateTime,
                    EndDateTime = vm.EndDateTime,
                    Location = vm.Location,
                    MaxParticipants = vm.MaxParticipants,
                    OrganizerId = user.Id
                };

                _context.Add(newEvent);
                await _context.SaveChangesAsync();

                TempData["Message"] = "Wydarzenie zostało utworzone.";
                TempData["MessageType"] = "success";
                return RedirectToAction(nameof(Index));
            }
            return View(vm);
        }

        // GET: Events/Details
        public async Task<IActionResult> Details(int id)
        {
            var ev = await _context.Events
                .Include(e => e.Organizer)
                .Include(e => e.Participants)
                    .ThenInclude(p => p.User)
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id == id);

            if (ev == null)
                return NotFound();

            return View(ev);
        }


        // GET: Events/Edit
        public async Task<IActionResult> Edit(int id)
        {
            var ev = await _context.Events
                .FindAsync(id);

            if (ev == null)
                return NotFound();

            var user = await GetCurrentUserAsync();
            if (ev.OrganizerId != user.Id)
                return Forbid();

            var vm = new EventCreateViewModel
            {
                Title = ev.Title,
                Description = ev.Description,
                StartDateTime = ev.StartDateTime,
                EndDateTime = ev.EndDateTime,
                Location = ev.Location,
                MaxParticipants = ev.MaxParticipants,
            };

            return View(vm);
        }

        // POST: Events/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EventCreateViewModel vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            var ev = await _context.Events
                .FindAsync(id);

            if (ev == null)
                return NotFound();

            var user = await GetCurrentUserAsync();

            if (ev.OrganizerId != user.Id)
                return Forbid();

            ev.Title = vm.Title;
            ev.Description = vm.Description;
            ev.StartDateTime = vm.StartDateTime;
            ev.EndDateTime = vm.EndDateTime;
            ev.Location = vm.Location;
            ev.MaxParticipants = vm.MaxParticipants;

            await _context.SaveChangesAsync();

            TempData["Message"] = "Wydarzenie zostało zaktualizowane.";
            TempData["MessageType"] = "success";
            return RedirectToAction(nameof(Index));
        }

        // DELETE: Events/Delete
        public async Task<IActionResult> Delete(int id)
        {
            var ev = await _context.Events
                .Include(e => e.Organizer)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (ev == null)
                return NotFound();

            var user = await GetCurrentUserAsync();

            if (ev.OrganizerId != user.Id)
                return Forbid();

            return View(ev);
        }

        // POST: Events/Delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var ev = await _context.Events.FindAsync(id);

            if (ev == null)
                return NotFound();

            var user = await GetCurrentUserAsync();

            if (ev.OrganizerId != user.Id)
                return Forbid();

            _context.Events.Remove(ev);
            await _context.SaveChangesAsync();

            TempData["Message"] = "Wydarzenie zostało usunięte.";
            TempData["MessageType"] = "success";
            return RedirectToAction(nameof(Index));
        }

        //POST Events/Join
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Join(int id)
        {
            var user = await GetCurrentUserAsync();

            var ev = await _context.Events
                .Include(e => e.Participants)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (ev == null)
                return NotFound();

            bool alreadyJoined = ev.Participants.Any(p => p.UserId == user.Id);

            if (alreadyJoined)
            {
                TempData["Message"] = "Już bierzesz udział w tym wydarzeniu.";
                TempData["MessageType"] = "warning";
                return RedirectToAction(nameof(Details), new { id });
            }

            if (ev.MaxParticipants.HasValue && ev.Participants.Count >= ev.MaxParticipants)
            {
                TempData["Message"] = "Brak miejsc – wydarzenie jest pełne.";
                TempData["MessageType"] = "danger";
                return RedirectToAction(nameof(Details), new { id });
            }

            var participants = new EventParticipant
            {
                EventId = ev.Id,
                UserId = user.Id
            };

            _context.EventParticipants.Add(participants);
            await _context.SaveChangesAsync();

            TempData["Message"] = "Pomyślnie dołączono do wydarzenia.";
            TempData["MessageType"] = "success";
            return RedirectToAction(nameof(Details), new { id });
        }

        //POST Events/Leave
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Leave(int id)
        {
            var user = await GetCurrentUserAsync();

            var participant = await _context.EventParticipants
                .FirstOrDefaultAsync(p => p.EventId == id && p.UserId == user.Id);

            if (participant == null)
            {
                TempData["Message"] = "Nie jesteś uczestnikiem tego wydarzenia.";
                TempData["MessageType"] = "warning";
                return RedirectToAction(nameof(Details), new { id });
            }

            _context.EventParticipants.Remove(participant);
            await _context.SaveChangesAsync();

            TempData["Message"] = "Pomyślnie opuściłeś wydarzenie.";
            TempData["MessageType"] = "success";
            return RedirectToAction(nameof(Details), new { id });
        }

        //GET Events/MyEvents
        public async Task<IActionResult> MyEvents()
        {
            var user = await GetCurrentUserAsync();

            var organizedEvents = await _context.Events
                .Where(e => e.OrganizerId == user.Id)
                .OrderByDescending(e => e.StartDateTime)
                .AsNoTracking()
                .ToListAsync();

            var joinedEvents = await _context.EventParticipants
                .Where(p => p.UserId == user.Id)
                .Include(p => p.Event)
                    .ThenInclude(e => e.Organizer)
                .OrderByDescending(p => p.Event.StartDateTime)
                .AsNoTracking()
                .ToListAsync();

            var vm = new MyEventsViewModel
            {
                OrganizedEvents = organizedEvents,
                JoinedEvents = joinedEvents.Select(p => p.Event).ToList(),
            };

            return View(vm);
        }

        private Task<ApplicationUser> GetCurrentUserAsync()
        {
            return _userManager.GetUserAsync(User);
        }
}
}
