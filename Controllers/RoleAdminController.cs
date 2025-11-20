using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using Pecoraro_Lillian_HW5.Models;

namespace Pecoraro_Lillian_HW5.Controllers
{
    [Authorize(Roles = "Admin")]
    public class RoleAdminController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public RoleAdminController(UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // -------------------- INDEX --------------------
        public async Task<ActionResult> Index()
        {
            // FIX #1 – materialize roles & users first
            var roles = _roleManager.Roles.ToList();
            var users = _userManager.Users.ToList();

            List<RoleEditModel> model = new List<RoleEditModel>();

            foreach (IdentityRole role in roles)
            {
                List<AppUser> RoleMembers = new List<AppUser>();
                List<AppUser> RoleNonMembers = new List<AppUser>();

                foreach (AppUser user in users)
                {
                    if (await _userManager.IsInRoleAsync(user, role.Name))
                        RoleMembers.Add(user);
                    else
                        RoleNonMembers.Add(user);
                }

                model.Add(new RoleEditModel
                {
                    Role = role,
                    RoleMembers = RoleMembers,
                    RoleNonMembers = RoleNonMembers
                });
            }

            return View(model);
        }

        // -------------------- CREATE --------------------
        public ActionResult Create() => View();

        [HttpPost]
        public async Task<ActionResult> Create([Required] string name)
        {
            if (ModelState.IsValid)
            {
                IdentityResult result = await _roleManager.CreateAsync(new IdentityRole(name));

                if (result.Succeeded)
                    return RedirectToAction("Index");

                foreach (var error in result.Errors)
                    ModelState.AddModelError("", error.Description);
            }

            return View(name);
        }

        // -------------------- EDIT --------------------
        public async Task<ActionResult> Edit(string id)
        {
            IdentityRole role = await _roleManager.FindByIdAsync(id);
            if (role == null)
                return View("Error", new string[] { "Role Not Found" });

            // FIX #2 – materialize users up front
            var users = _userManager.Users.ToList();

            List<AppUser> RoleMembers = new List<AppUser>();
            List<AppUser> RoleNonMembers = new List<AppUser>();

            foreach (AppUser user in users)
            {
                if (await _userManager.IsInRoleAsync(user, role.Name))
                    RoleMembers.Add(user);
                else
                    RoleNonMembers.Add(user);
            }

            RoleEditModel rem = new RoleEditModel
            {
                Role = role,
                RoleMembers = RoleMembers,
                RoleNonMembers = RoleNonMembers
            };

            return View(rem);
        }

        [HttpPost]
        public async Task<ActionResult> Edit(RoleModificationModel rmm)
        {
            IdentityResult result;

            if (ModelState.IsValid)
            {
                if (rmm.IdsToAdd != null)
                {
                    foreach (string userId in rmm.IdsToAdd)
                    {
                        AppUser user = await _userManager.FindByIdAsync(userId);
                        result = await _userManager.AddToRoleAsync(user, rmm.RoleName);

                        if (!result.Succeeded)
                            return View("Error", result.Errors);
                    }
                }

                if (rmm.IdsToDelete != null)
                {
                    foreach (string userId in rmm.IdsToDelete)
                    {
                        AppUser user = await _userManager.FindByIdAsync(userId);
                        result = await _userManager.RemoveFromRoleAsync(user, rmm.RoleName);

                        if (!result.Succeeded)
                            return View("Error", result.Errors);
                    }
                }

                return RedirectToAction("Index");
            }

            return View("Error", new string[] { "Role Not Found" });
        }
    }
}