using ArticlesApp.Data;
using ArticlesApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ArticlesApp.Controllers
{
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET /Products
        public async Task<IActionResult> Index()
        {
            var produse = await _context.Products
                .Include(p => p.Category)
                .ToListAsync();

            return View(produse);
        }

        // GET /Products/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var produs = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Reviews)
                    .ThenInclude(r => r.User)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (produs == null)
                return NotFound();

            var model = new ProductDetailsViewModel
            {
                Product = produs,
                Reviews = produs.Reviews
                    .OrderByDescending(r => r.Id)
                    .ToList()
            };

            return View(model);
        }

        // GET /Products/New
        public async Task<IActionResult> New()
        {
            await LoadCategoriesInViewBag();
            return View(new Product());
        }

        // POST /Products/New
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> New(Product produs)
        {
            if (!ModelState.IsValid)
            {
                await LoadCategoriesInViewBag();
                return View(produs);
            }

            

            _context.Products.Add(produs);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET /Products/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var produs = await _context.Products.FindAsync(id);
            if (produs == null)
                return NotFound();

            await LoadCategoriesInViewBag();
            return View(produs);
        }

        // POST /Products/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Product produs)
        {
            if (id != produs.Id)
                return NotFound();

            if (!ModelState.IsValid)
            {
                await LoadCategoriesInViewBag();
                return View(produs);
            }

            try
            {
                _context.Update(produs);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                var exista = await _context.Products.AnyAsync(p => p.Id == id);
                if (!exista)
                    return NotFound();

                throw;
            }

            return RedirectToAction(nameof(Index));
        }

        // GET /Products/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var produs = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (produs == null)
                return NotFound();

            return View(produs);
        }

        // POST /Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var produs = await _context.Products.FindAsync(id);
            if (produs != null)
            {
                _context.Products.Remove(produs);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // helper pentru dropdown ul de categorii
        private async Task LoadCategoriesInViewBag()
        {
            var categorii = await _context.Categories
                .OrderBy(c => c.CategoryName)
                .ToListAsync();

            ViewBag.Categories = new SelectList(categorii, "Id", "CategoryName");
        }
        // private async Task RecalculateAverageRatingAsync(int productId)
        // {
        //     var product = await _context.Products
        //         .Include(p => p.Reviews)
        //         .FirstOrDefaultAsync(p => p.Id == productId);
        //
        //     if (product == null)
        //         return;
        //
        //     var ratings = product.Reviews
        //         .Where(r => r.Rating.HasValue)
        //         .Select(r => r.Rating!.Value)
        //         .ToList();
        //
        //     product.AverageRating = ratings.Count > 0
        //         ? ratings.Average()
        //         : (double?)null;
        //
        //     await _context.SaveChangesAsync();
        // }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddReview(int productId, int? rating, string? content)
        {
            // dacă nu există nici rating, nici text, nu salvăm nimic
            if (!rating.HasValue && string.IsNullOrWhiteSpace(content))
            {
                // poți pune mesaj de eroare dacă vrei
                return RedirectToAction(nameof(Details), new { id = productId });
            }

            var product = await _context.Products.FindAsync(productId);
            if (product == null)
                return NotFound();

            var review = new Review
            {
                ProductId = productId,
                Rating = rating,
                Content = content,
                // dacă ai Identity și vrei userul logat:
                // UserId = _userManager.GetUserId(User)
            };

            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();

            //await RecalculateAverageRatingAsync(productId);

            return RedirectToAction(nameof(Details), new { id = productId });
        }
    }
}