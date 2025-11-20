using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pecoraro_Lillian_HW5.DAL;
using Pecoraro_Lillian_HW5.Models;
using System.Linq;
using System.Threading.Tasks;

namespace Pecoraro_Lillian_HW5.Controllers
{
    [Authorize(Roles = "Admin")]
    public class SuppliersController : Controller
    {
        private readonly AppDbContext _context;

        public SuppliersController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Suppliers
        public async Task<IActionResult> Index()
        {
            var suppliers = await _context.Suppliers.ToListAsync();
            return View(suppliers);
        }

        // GET: Suppliers/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var supplier = await _context.Suppliers
                                         .Include(s => s.Products)
                                         .FirstOrDefaultAsync(m => m.SupplierID == id);

            if (supplier == null) return NotFound();

            return View(supplier);
        }

        // GET: Suppliers/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Suppliers/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("SupplierID,SupplierName,Email,PhoneNumber")] Supplier supplier)
        {
            if (ModelState.IsValid == false)
            {
                return View(supplier);
            }

            _context.Add(supplier);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Suppliers/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var supplier = await _context.Suppliers.FindAsync(id);
            if (supplier == null) return NotFound();

            return View(supplier);
        }

        // POST: Suppliers/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("SupplierID,SupplierName,Email,PhoneNumber")] Supplier supplier)
        {
            if (id != supplier.SupplierID) return NotFound();

            if (ModelState.IsValid == false)
            {
                return View(supplier);
            }

            try
            {
                _context.Update(supplier);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SupplierExists(supplier.SupplierID)) return NotFound();
                else throw;
            }

            return RedirectToAction(nameof(Index));
        }

        // 🚫 NO Delete methods per HW5

        private bool SupplierExists(int id)
        {
            return _context.Suppliers.Any(e => e.SupplierID == id);
        }
    }
}