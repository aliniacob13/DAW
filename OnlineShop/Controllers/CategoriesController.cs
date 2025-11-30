using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using OnlineShop.Models;
namespace OnlineShop.Controllers;
using Microsoft.EntityFrameworkCore;
using OnlineShop.Data;
public class CategoriesController : Controller
{
    private readonly ApplicationDbContext _context; // Baza de date

    public CategoriesController(ApplicationDbContext context)
    {
        _context = context;
    }

    // PASUL 1: Afișează formularul gol (GET)
    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }

    // PASUL 2: Primește datele completate și le salvează (POST)
    [HttpPost]
    [ValidateAntiForgeryToken] // Securitate
    public async Task<IActionResult> Create(Category category)
    {
        // Verificare custom pentru cerința "Nume Unic"
        bool existaDeja = _context.Categories.Any(c => c.Name == category.Name);
        if (existaDeja)
        {
            ModelState.AddModelError("Name", "Această categorie există deja!");
        }
        
        ModelState.Remove("Products");
        
        if (ModelState.IsValid)
        {
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index)); // Te întoarce la listă
        }
        
        
        
        // Dacă sunt erori, reafișează formularul cu mesajele de eroare
        return View(category);
    }
    
    // GET: Categories/Index
    public async Task<IActionResult> Index()
    {
        // Simplu: Luăm toate categoriile din bază
        return View(await _context.Categories.ToListAsync());
    }
    
    // 1. GET: Afișează formularul cu datele existente
    [HttpGet]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound(); // Dacă nu primim ID, dăm eroare
        }

        // Căutăm categoria în baza de date după ID
        var category = await _context.Categories.FindAsync(id);

        if (category == null)
        {
            return NotFound(); // Dacă ID-ul nu există în bază
        }

        return View(category); // Trimitem categoria găsită către View
    }

// 2. POST: Primește datele modificate și le salvează
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Category category)
    {
        if (id != category.Id)
        {
            return NotFound(); // Verificare de securitate
        }
        ModelState.Remove("Products");
        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(category); // Marchează obiectul ca modificat
                await _context.SaveChangesAsync(); // Trimite UPDATE în SQL
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Categories.Any(e => e.Id == category.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return RedirectToAction(nameof(Index));
        }
        return View(category);
    }
    // 1. GET: Pagina de confirmare
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var category = await _context.Categories
            .FirstOrDefaultAsync(m => m.Id == id);

        if (category == null) return NotFound();

        return View(category);
    }

// 2. POST: Ștergerea efectivă
// Observă: Numele metodei este DeleteConfirmed, dar ActionName este "Delete"
// Facem asta pentru că C# nu permite două metode cu aceiași parametri (int id).
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var category = await _context.Categories.FindAsync(id);
        if (category != null)
        {
            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
        }
    
        return RedirectToAction(nameof(Index));
    }
}