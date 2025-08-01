﻿using Eventure.Models;
using Eventure.Services;
using Eventure.ViewModels.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Eventure.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IEventService _eventService;

        public AdminController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, IEventService eventService)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _eventService = eventService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> ManageUsers()
        {
            var users = await _userManager.Users.ToListAsync();
            var userViewModels = new List<UserViewModel>();

            foreach (var user in users)
            {
                userViewModels.Add(new UserViewModel
                {
                    Id = user.Id,
                    UserName = user.UserName,
                    Email = user.Email,
                    Roles = await _userManager.GetRolesAsync(user),
                    IsLockedOut = await _userManager.IsLockedOutAsync(user)
                });
            }
            return View(userViewModels);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LockUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                TempData["Message"] = "Użytkownik nie znaleziony.";
                TempData["MessageType"] = "danger";
                return NotFound();
            }

            if (user.Id == _userManager.GetUserId(User))
            {
                TempData["Message"] = "Nie możesz zablokować własnego konta.";
                TempData["MessageType"] = "danger";
                return RedirectToAction(nameof(ManageUsers));
            }

            var result = await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);

            if (result.Succeeded)
            {
                TempData["Message"] = $"Użytkownik {user.UserName} został zablokowany.";
                TempData["MessageType"] = "success";
            }
            else
            {
                TempData["Message"] = "Wystąpił błąd podczas blokowania użytkownika.";
                TempData["MessageType"] = "danger";
            }

            return RedirectToAction(nameof(ManageUsers));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UnlockUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                TempData["Message"] = "Użytkownik nie znaleziony.";
                TempData["MessageType"] = "danger";
                return NotFound();
            }

            var result = await _userManager.SetLockoutEndDateAsync(user, null);

            if (result.Succeeded)
            {
                TempData["Message"] = $"Użytkownik {user.UserName} został odblokowany.";
                TempData["MessageType"] = "success";
            }
            else
            {
                TempData["Message"] = "Wystąpił błąd podczas odblokowywania użytkownika.";
                TempData["MessageType"] = "danger";
            }

            return RedirectToAction(nameof(ManageUsers));
        }

        public async Task<IActionResult> ManageRoles(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var model = new ManageUserRolesViewModel
            {
                UserId = user.Id,
                UserName = user.UserName,
                Roles = new List<RoleViewModel>()
            };

            var allRoles = await _roleManager.Roles.ToListAsync();
            foreach (var role in allRoles)
            {
                var roleViewModel = new RoleViewModel
                {
                    RoleName = role.Name,
                    IsSelected = await _userManager.IsInRoleAsync(user, role.Name)
                };
                model.Roles.Add(roleViewModel);
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ManageRoles(ManageUserRolesViewModel model)
        {
            var user = await _userManager.FindByIdAsync(model.UserId);
            if (user == null)
            {
                return NotFound();
            }

            var userRoles = await _userManager.GetRolesAsync(user);

            foreach (var roleVm in model.Roles)
            {
                if (roleVm.IsSelected && !await _userManager.IsInRoleAsync(user, roleVm.RoleName))
                {
                    await _userManager.AddToRoleAsync(user, roleVm.RoleName);
                }
                else if (!roleVm.IsSelected && await _userManager.IsInRoleAsync(user, roleVm.RoleName))
                {
                    await _userManager.RemoveFromRoleAsync(user, roleVm.RoleName);
                }
            }

            TempData["Message"] = $"Role dla użytkownika {user.UserName} zostały zaktualizowane.";
            TempData["MessageType"] = "success";

            return RedirectToAction(nameof(ManageUsers));
        }
        public async Task<IActionResult> ManageEvents(string searchTitle, string location, int? categoryId, int pageNumber = 1)
        {
            int pageSize = 10;

            var paginatedEvents = await _eventService.GetFilteredEventsAsync(searchTitle, location, null, categoryId, pageNumber, pageSize);

            var categories = await _eventService.GetCategorySelectListAsync();
            ViewBag.Categories = new SelectList(categories, "Value", "Text", categoryId?.ToString());

            return View(paginatedEvents);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            var success = await _eventService.DeleteEventAsAdminAsync(id);

            if (success)
            {
                TempData["Message"] = "Wydarzenie zostało pomyślnie usunięte.";
                TempData["MessageType"] = "success";
            }
            else
            {
                TempData["Message"] = "Nie udało się usunąć wydarzenia (mogło już nie istnieć).";
                TempData["MessageType"] = "danger";
            }

            return RedirectToAction(nameof(ManageEvents));
        }
    }
}