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
                var user = await _userManager.GetUserAsync(User);

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

            if(ev == null)
                return NotFound();

            return View(ev);
        }


        // GET: Events/Edit
        public async Task<IActionResult> Edit(int id)
        {
            var ev = await _context.Events
                .FindAsync(id);

            if(ev == null)
                return NotFound();

            var user = await _userManager.GetUserAsync(User);
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
            if(!ModelState.IsValid)
                return View(vm);

            var ev = await _context.Events
                .FindAsync(id);

            if(ev == null)
                return NotFound();

            var user = await _userManager.GetUserAsync(User);

            if(ev.OrganizerId != user.Id)
                return Forbid();

            ev.Title = vm.Title;
            ev.Description = vm.Description;
            ev.StartDateTime = vm.StartDateTime;
            ev.EndDateTime = vm.EndDateTime;
            ev.Location = vm.Location;
            ev.MaxParticipants = vm.MaxParticipants;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // DELETE: Events/Delete
        public async Task<IActionResult> Delete(int id)
        {
            var ev = await _context.Events
                .Include(e => e.Organizer)
                .FirstOrDefaultAsync(e => e.Id == id);

            if(ev == null) 
                return NotFound();

            var user = await _userManager.GetUserAsync(User);

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

            var user = await _userManager.GetUserAsync(User);

            if (ev.OrganizerId != user.Id)
                return Forbid();

            _context.Events.Remove(ev);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        //POST Events/Join
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Join(int id)
        {
            var user = await _userManager.GetUserAsync(User);

            var ev = await _context.Events
                .Include(e => e.Participants)
                .FirstOrDefaultAsync(e => e.Id == id);

            if(ev == null)
                return NotFound();

            bool alreadyJoined = ev.Participants.Any(p => p.UserId == user.Id);

            if (alreadyJoined)
                return RedirectToAction(nameof(Details), new { id });

            if (ev.MaxParticipants.HasValue && ev.Participants.Count >= ev.MaxParticipants)
            {
                return RedirectToAction(nameof(Details), new { id });
            }

            var participants = new EventParticipant
            {
                EventId = ev.Id,
                UserId = user.Id
            };

            _context.EventParticipants.Add(participants);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Details), new { id });
        }
    }
}
