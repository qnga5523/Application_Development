using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using GCH211211.Data;
using GCH211211.Models;
using Microsoft.AspNetCore.Authorization;

namespace GCH211211.Controllers
{
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Products
        public async Task<IActionResult> Index()
        {
            var products = _context.Product.ToList();
            var categories = _context.Category.ToList();
            ViewBag.Categories = categories;
            return View(products);
        }

        // GET: Products/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            var products = _context.Product.ToList();
            var item = products.Find(p => p.Id == id);
            return View(item);
        }
        [Authorize(Roles = "Admin")]
        [HttpGet]

        // GET: Products/Create
        public IActionResult Create()
        {
            var categories = _context.Category.ToList();
            ViewBag.Categories = categories;
            return View();
        }

        // POST: Products/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Price,Quantity,Description,Image,CategoryId")] Product product)
        {
            if (ModelState.IsValid)
            {
                TempData["Message"] = "Create product successfully";
                _context.Product.Add(product);
                _context.SaveChanges();
                return Redirect("/products");
            }
            return View();
        }
        [Authorize(Roles = "Admin")]
        [HttpGet]
        // GET: Products/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            var products = _context.Product.ToList();
            var item = products.Find(p => p.Id == id);
            var categories = _context.Category.ToList();
            ViewBag.Categories = categories;
            return View(item);
        }

        // POST: Products/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]

        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Price,Quantity,Description,Image,CategoryId")] Product product)
        {
            if (ModelState.IsValid)
            {
                TempData["Message"] = "Update product successfully";
                _context.Product.Update(product);
                _context.SaveChanges();
                return Redirect("/products");

            }
            return View();
        }

        [HttpGet]
        // GET: Products/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Product
                .Include(p => p.Category)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // POST: Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Product.FindAsync(id);
            if (product != null)
            {
                _context.Product.Remove(product);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        public IActionResult SortDESC()
        {
            var products = _context.Product.OrderByDescending(p => p.Quantity).ToList();
            var categories = _context.Category.ToList();
            ViewBag.Categories = categories;
            return View("Index", products);
        }
        public IActionResult SortASC()
        {
            var products = _context.Product.OrderBy(p => p.Quantity).ToList();
            var categories = _context.Category.ToList();
            ViewBag.Categories = categories;
            return View("Index", products);
        }
        private bool ProductExists(int id)
        {
            return _context.Product.Any(e => e.Id == id);
        }
    }
}
