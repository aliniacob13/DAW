using OnlineShop.Models;
using OnlineShop.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace OnlineShop.Controllers
{
    public class ReviewsController(ApplicationDbContext context) : Controller
    {
        private readonly ApplicationDbContext db = context;
        // Adaugarea unui review asociat unui produs in baza de date
        [HttpPost]
        public IActionResult Create(Review review)
        {
            // Product nu vine din formular, deci scoatem orice eroare automata de pe el
            ModelState.Remove("Product");

            review.DatePosted = DateTime.Now;

            if (!ModelState.IsValid)
            {
                // Reincarcam produsul cu review urile existente
                var product = db.Products
                    .Include(p => p.Reviews)
                    .FirstOrDefault(p => p.Id == review.ProductId);

                if (product == null)
                {
                    return NotFound();
                }

                // trimitem din nou pagina Products/Show cu erorile din ModelState
                return View("~/Views/Products/Show.cshtml", product);
            }

            db.Reviews.Add(review);
            db.SaveChanges();
            return RedirectToAction("Show", "Products", new { id = review.ProductId });
        }

        // Stergerea unui review din baza de date
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            Review? review = db.Reviews.Find(id);
            if (review is null)
            {
                return NotFound();
            }

            var productId = review.ProductId;

            db.Reviews.Remove(review);
            db.SaveChanges();
            return Redirect("/Products/Show/" + productId);
        }

        // Se editeaza un review existent
        [HttpGet]
        public IActionResult Edit(int id)
        {
            Review? review = db.Reviews.Find(id);
            if (review is null)
            {
                return NotFound();
            }
            return View(review);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Review requestedReview)
        {
            Review? review = db.Reviews.Find(id);
            if (review is null)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                // Daca nu e valid, ramanem pe pagina de editare si afisam erorile
                return View(requestedReview);
            }

            review.Comment = requestedReview.Comment;
            review.Rating = requestedReview.Rating;
            review.DatePosted = DateTime.Now;
            
            db.SaveChanges();

            return Redirect("/Products/Show/" + review.ProductId);
        }
    }
}