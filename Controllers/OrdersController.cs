using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pecoraro_Lillian_HW5.DAL;
using Pecoraro_Lillian_HW5.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Pecoraro_Lillian_HW5.Controllers
{
    [Authorize]
    public class OrdersController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public OrdersController(AppDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Orders
        public async Task<IActionResult> Index()
        {
            var query = _context.Orders
                                .Include(o => o.Customer)
                                .Include(o => o.OrderDetails)
                                    .ThenInclude(od => od.Product);

            if (User.IsInRole("Admin"))
            {
                var allOrders = await query
                    .OrderByDescending(o => o.OrderDate)
                    .ToListAsync();

                return View(allOrders);
            }
            else
            {
                AppUser loggedIn = await _userManager.GetUserAsync(User);

                var myOrders = await query
                    .Where(o => o.Customer.Id == loggedIn.Id)
                    .OrderByDescending(o => o.OrderDate)
                    .ToListAsync();

                return View(myOrders);
            }
        }

        // GET: Orders/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var order = await _context.Orders
                                      .Include(o => o.Customer)
                                      .Include(o => o.OrderDetails)
                                          .ThenInclude(od => od.Product)
                                      .FirstOrDefaultAsync(m => m.OrderID == id);

            if (order == null) return NotFound();

            if (!User.IsInRole("Admin"))
            {
                AppUser loggedIn = await _userManager.GetUserAsync(User);
                if (order.Customer == null || order.Customer.Id != loggedIn.Id)
                {
                    return View("Error", new string[] { "You are not authorized to view this order." });
                }
            }

            return View(order);
        }

        // GET: Orders/Create
        [Authorize(Roles = "Customer")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Customer")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string OrderNotes)
        {
            Console.WriteLine("👉 POST /Orders/Create HIT");

            // Logged in user
            AppUser loggedIn = await _userManager.GetUserAsync(User);
            Console.WriteLine($"👉 LoggedIn User ID = {loggedIn?.Id}");

            // Create a new order manually (DO NOT USE MODEL BINDING)
            Order order = new Order
            {
                OrderNotes = OrderNotes,
                OrderDate = DateTime.Now,
                Customer = loggedIn
            };

            // Generate next number
            order.OrderNumber = _context.Orders.Any()
                ? _context.Orders.Max(o => o.OrderNumber) + 1
                : 70001;

            Console.WriteLine("👉 Adding order to DB...");
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();
            Console.WriteLine("👉 SaveComplete!");

            return RedirectToAction("Create", "OrderDetails", new { orderID = order.OrderID });
        }

        // GET: Orders/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var order = await _context.Orders
                                      .Include(o => o.Customer)
                                      .Include(o => o.OrderDetails)
                                          .ThenInclude(od => od.Product)
                                      .FirstOrDefaultAsync(o => o.OrderID == id);

            if (order == null) return NotFound();

            if (!User.IsInRole("Admin"))
            {
                AppUser loggedIn = await _userManager.GetUserAsync(User);
                if (order.Customer == null || order.Customer.Id != loggedIn.Id)
                {
                    return View("Error", new string[] { "You are not authorized to edit this order." });
                }
            }

            return View(order);
        }

        // POST: Orders/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("OrderID,OrderNotes")] Order order)
        {
            if (id != order.OrderID) return NotFound();

            if (ModelState.IsValid == false)
            {
                return View(order);
            }

            var dbOrder = await _context.Orders
                                        .Include(o => o.Customer)
                                        .FirstOrDefaultAsync(o => o.OrderID == id);

            if (dbOrder == null) return NotFound();

            if (!User.IsInRole("Admin"))
            {
                AppUser loggedIn = await _userManager.GetUserAsync(User);
                if (dbOrder.Customer == null || dbOrder.Customer.Id != loggedIn.Id)
                {
                    return View("Error", new string[] { "You are not authorized to edit this order." });
                }
            }

            dbOrder.OrderNotes = order.OrderNotes;

            await _context.SaveChangesAsync();
            return RedirectToAction("Details", new { id = dbOrder.OrderID });
        }

        private bool OrderExists(int id)
        {
            return _context.Orders.Any(e => e.OrderID == id);
        }
    }
}