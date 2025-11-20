using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Pecoraro_Lillian_HW5.DAL;
using Pecoraro_Lillian_HW5.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Pecoraro_Lillian_HW5.Controllers
{
    [Authorize]
    public class OrderDetailsController : Controller
    {
        private readonly AppDbContext _context;

        public OrderDetailsController(AppDbContext context)
        {
            _context = context;
        }

        // ---------- Helper: all products for dropdown ----------
        private SelectList GetAllProducts()
        {
            var products = _context.Products
                                   .OrderBy(p => p.Name)
                                   .ToList();
            return new SelectList(products, "ProductID", "Name");
        }

        // ---------- Index ----------
        public async Task<IActionResult> Index(int? orderID)
        {
            if (orderID == null)
            {
                return View("Error", new string[] { "No order specified." });
            }

            var orderDetails = await _context.OrderDetails
                                             .Include(od => od.Product)
                                             .Include(od => od.Order)
                                             .Where(od => od.Order.OrderID == orderID)
                                             .ToListAsync();

            ViewBag.OrderID = orderID;
            return View(orderDetails);
        }

        // ---------- GET: Create ----------
        public async Task<IActionResult> Create(int orderID)
        {
            var order = await _context.Orders
                                      .Include(o => o.Customer)
                                      .FirstOrDefaultAsync(o => o.OrderID == orderID);

            if (order == null) return NotFound();

            OrderDetail od = new OrderDetail();
            od.Order = order;

            ViewBag.AllProducts = GetAllProducts();
            return View(od);
        }

        // ---------- POST: Create ----------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int OrderID, int SelectedProduct, int Quantity)
        {
            Console.WriteLine("👉 POST /OrderDetails/Create HIT");
            Console.WriteLine($"OrderID = {OrderID}, SelectedProduct = {SelectedProduct}, Qty = {Quantity}");

            // Load order
            var order = await _context.Orders
                                      .Include(o => o.OrderDetails)
                                      .FirstOrDefaultAsync(o => o.OrderID == OrderID);

            if (order == null)
            {
                Console.WriteLine("❌ Order not found");
                return NotFound();
            }

            // Load product
            var product = await _context.Products.FindAsync(SelectedProduct);

            if (product == null)
            {
                Console.WriteLine("❌ Product not found");
                ModelState.AddModelError("", "You must select a product.");
                ViewBag.AllProducts = GetAllProducts();
                return View();
            }

            // Create OD manually
            OrderDetail od = new OrderDetail
            {
                Order = order,
                Product = product,
                Quantity = Quantity,
                ProductPrice = product.Price,
                ExtendedPrice = product.Price * Quantity
            };

            Console.WriteLine("👉 Adding OD...");
            _context.OrderDetails.Add(od);
            await _context.SaveChangesAsync();
            Console.WriteLine("✔️ OD Saved!");

            // Redirect back to order details
            return RedirectToAction("Details", "Orders", new { id = order.OrderID });
        }

        // ---------- GET: Edit ----------
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var orderDetail = await _context.OrderDetails
                                            .Include(od => od.Product)
                                            .Include(od => od.Order)
                                            .FirstOrDefaultAsync(od => od.OrderDetailID == id);

            if (orderDetail == null) return NotFound();

            return View(orderDetail);
        }

        // ---------- POST: Edit ----------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, OrderDetail orderDetail)
        {
            if (id != orderDetail.OrderDetailID) return NotFound();

            if (!ModelState.IsValid)
            {
                return View(orderDetail);
            }

            var dbOD = await _context.OrderDetails
                                     .Include(od => od.Product)
                                     .Include(od => od.Order)
                                     .FirstOrDefaultAsync(od => od.OrderDetailID == id);

            if (dbOD == null) return NotFound();

            dbOD.Quantity = orderDetail.Quantity;
            dbOD.ProductPrice = dbOD.Product.Price;
            dbOD.ExtendedPrice = dbOD.Quantity * dbOD.ProductPrice;

            await _context.SaveChangesAsync();

            return RedirectToAction("Details", "Orders", new { id = dbOD.Order.OrderID });
        }

        // ---------- GET: Delete ----------
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var orderDetail = await _context.OrderDetails
                                            .Include(od => od.Product)
                                            .Include(od => od.Order)
                                            .FirstOrDefaultAsync(m => m.OrderDetailID == id);

            if (orderDetail == null) return NotFound();

            return View(orderDetail);
        }

        // ---------- POST: Delete ----------
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var orderDetail = await _context.OrderDetails
                                            .Include(od => od.Order)
                                            .FirstOrDefaultAsync(od => od.OrderDetailID == id);

            if (orderDetail == null) return NotFound();

            int orderID = orderDetail.Order.OrderID;

            _context.OrderDetails.Remove(orderDetail);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", "Orders", new { id = orderID });
        }
    }
}