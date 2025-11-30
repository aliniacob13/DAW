using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OnlineShop.Models;
using OnlineShop.Data;
namespace OnlineShop.Controllers;
public class ProductsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _webHostEnvironment; // Serviciul pentru lucrul cu fișiere

    // Injectăm ambele servicii în constructor
    public ProductsController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
    {
        _context = context;
        _webHostEnvironment = webHostEnvironment;
    }

    // ==========================================
    // PASUL 1: Afișarea formularului (GET)
    // ==========================================
    public IActionResult Create()
    {
        // Trebuie să trimitem lista de categorii către View pentru a popula Dropdown-ul
        // SelectList are 3 argumente: Lista sursă, Ce salvăm (Id), Ce afișăm (Name)
        ViewBag.CategoryId = new SelectList(_context.Categories, "Id", "Name");
        
        return View();
    }

    // ==========================================
    // PASUL 2: Procesarea formularului (POST)
    // ==========================================
    [HttpPost]
    [ValidateAntiForgeryToken]
    // Primim obiectul Product SI fișierul separat (IFormFile imageFile)
    public async Task<IActionResult> Create(Product product, IFormFile? imageFile)
    {
        // -- 1. VALIDARE IMAGINE (Custom Logic) --
        
        // Verificăm dacă fișierul există
        if (imageFile == null || imageFile.Length == 0)
        {
            ModelState.AddModelError("ImageUrl", "Te rugăm să încarci o imagine.");
        }
        else
        {
            // Verificăm extensia (să fie doar imagine)
            ModelState.Remove("ImageUrl");
            string extension = Path.GetExtension(imageFile.FileName).ToLower();
            string[] permittedExtensions = { ".jpg", ".jpeg", ".png", ".gif" };
            
            if (!permittedExtensions.Contains(extension))
            {
                ModelState.AddModelError("ImageUrl", "Format invalid. Doar .jpg, .jpeg, .png, .gif sunt acceptate.");
            }

            // Verificăm dimensiunea (ex: max 5 MB) - Cerința ta din imagine
            if (imageFile.Length > 5 * 1024 * 1024) 
            {
                 ModelState.AddModelError("ImageUrl", "Imaginea este prea mare. Maxim 5MB.");
            }
        }

        // Deoarece Category este null (vine doar CategoryId din form), 
        // trebuie să scoatem eroarea de validare automată pentru obiectul Category
        ModelState.Remove("Category"); 

        if (ModelState.IsValid)
        {
            // -- 2. SALVARE FIZICĂ A IMAGINII --
            
            // Generăm un nume unic fișierului pentru a evita duplicatele
            // Ex: "laptop.jpg" devine "guid-unic-laptop.jpg"
            string uniqueFileName = Guid.NewGuid().ToString() + "_" + imageFile.FileName;
            
            // Compunem calea unde o salvăm: wwwroot/images
            string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images");
            
            // Dacă folderul nu există, îl creăm (opțional, dar bun pentru siguranță)
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

            // Copiem fișierul primit în calea definită
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await imageFile.CopyToAsync(fileStream);
            }

            // -- 3. SALVARE ÎN BAZA DE DATE --
            
            // Salvăm doar calea relativă în obiectul produs
            product.ImageUrl = "/images/" + uniqueFileName;

            _context.Add(product);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index)); // Ne întoarcem la listă
        }

        // Dacă ceva a mers prost, reîncărcăm lista de categorii și reafisam formularul
        ViewBag.CategoryId = new SelectList(_context.Categories, "Id", "Name", product.CategoryId);
        return View(product);
    }
    
    // GET: Products/Index
    public async Task<IActionResult> Index()
    {
        // CORECLIE: Trebuie să luăm PRODUSELE, nu Categoriile.
        // Include(p => p.Category) este obligatoriu ca să poți afișa @item.Category.Name în View
        var products = _context.Products.Include(p => p.Category);
        
        return View(await products.ToListAsync());
    }
    
    
}