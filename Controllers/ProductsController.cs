using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Pecoraro_Lillian_HW5.DAL;
using Pecoraro_Lillian_HW5.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pecoraro_Lillian_HW5.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ProductsController : Controller
    {
        private readonly AppDbContext _context;

        public ProductsController(AppDbContext context)
        {
            _context = context;
        }

        // ---------- Helper to get all suppliers for listbox ----------
        private MultiSelectList GetAllSuppliers(Product? product = null)
        {
            var allSuppliers = _context.Suppliers
                                       .OrderBy(s => s.SupplierName)
                                       .ToList();

            List<int> selectedSuppliers = new List<int>();

            if (product != null && product.Suppliers != null)
            {
                selectedSuppliers = product.Suppliers
                                           .Select(s => s.SupplierID)
                                           .ToList();
            }

            return new MultiSelectList(allSuppliers, "SupplierID", "SupplierName", selectedSuppliers);
        }

        // ---------- Everyone can see the product list ----------
        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            // For index, scalars only – no Include needed
            var products = await _context.Products.ToListAsync();
            return View(products);
        }

        // ---------- Everyone can see product details ----------
        [AllowAnonymous]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var product = await _context.Products
                                        .Include(p => p.Suppliers)
                                        .FirstOrDefaultAsync(m => m.ProductID == id);

            if (product == null) return NotFound();

            return View(product);
        }

        // ---------- Admin: GET Create ----------
        public IActionResult Create()
        {
            ViewBag.AllSuppliers = GetAllSuppliers();
            return View();
        }

        // ---------- Admin: POST Create ----------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("ProductID,Name,Description,Price,ProductType")] Product product,
            int[] SelectedSuppliers)
        {
            if (ModelState.IsValid == false)
            {
                ViewBag.AllSuppliers = GetAllSuppliers(product);
                return View(product);
            }

            // Attach suppliers
            product.Suppliers = new List<Supplier>();

            if (SelectedSuppliers != null)
            {
                foreach (int supplierID in SelectedSuppliers)
                {
                    Supplier? supplier = await _context.Suppliers.FindAsync(supplierID);
                    if (supplier != null)
                    {
                        product.Suppliers.Add(supplier);
                    }
                }
            }

            _context.Add(product);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // ---------- Admin: GET Edit ----------
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var product = await _context.Products
                                        .Include(p => p.Suppliers)
                                        .FirstOrDefaultAsync(p => p.ProductID == id);

            if (product == null) return NotFound();

            ViewBag.AllSuppliers = GetAllSuppliers(product);
            return View(product);
        }

        // ---------- Admin: POST Edit ----------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            [Bind("ProductID,Name,Description,Price,ProductType")] Product product,
            int[] SelectedSuppliers)
        {
            if (id != product.ProductID) return NotFound();

            if (ModelState.IsValid == false)
            {
                // Re-load product with suppliers for multiselect
                var badProduct = await _context.Products
                                               .Include(p => p.Suppliers)
                                               .FirstOrDefaultAsync(p => p.ProductID == id);
                ViewBag.AllSuppliers = GetAllSuppliers(badProduct);
                return View(product);
            }

            // Get product from DB with suppliers
            var dbProduct = await _context.Products
                                          .Include(p => p.Suppliers)
                                          .FirstOrDefaultAsync(p => p.ProductID == id);

            if (dbProduct == null) return NotFound();

            // Update scalar properties
            dbProduct.Name = product.Name;
            dbProduct.Description = product.Description;
            dbProduct.Price = product.Price;
            dbProduct.ProductType = product.ProductType;

            // Update suppliers list
            dbProduct.Suppliers.Clear();

            if (SelectedSuppliers != null)
            {
                foreach (int supplierID in SelectedSuppliers)
                {
                    Supplier? supplier = await _context.Suppliers.FindAsync(supplierID);
                    if (supplier != null)
                    {
                        dbProduct.Suppliers.Add(supplier);
                    }
                }
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.ProductID == id);
        }
    }
}