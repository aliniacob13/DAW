using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OnlineShop.Models;
using OnlineShop.Data;
using OnlineShop.ViewModels;
namespace OnlineShop.Controllers;
public class ProductsController : Controller
{
    private readonly ApplicationDbContext db;
    private readonly IWebHostEnvironment whe;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    public ProductsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, IWebHostEnvironment webHostEnvironment)
    {
        db = context;
        _userManager=userManager;
        _roleManager=roleManager;
        whe = webHostEnvironment;
    }

    // Injectăm ambele servicii în constructor
    
    // ==========================================
    // PASUL 1: Afișarea formularului (GET)
    // ==========================================
    public IActionResult Create()
    {
        // Trebuie să trimitem lista de categorii către View pentru a popula Dropdown-ul
        // SelectList are 3 argumente: Lista sursă, Ce salvăm (Id), Ce afișăm (Name)
        ViewBag.CategoryId = new SelectList(db.Categories, "Id", "Name");
        
        return View();
    }

    [Authorize(Roles="Editor,Admin")]
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
        product.UserId = _userManager.GetUserId(User);
        if (ModelState.IsValid)
        {
            // -- 2. SALVARE FIZICĂ A IMAGINII --
            
            // Generăm un nume unic fișierului pentru a evita duplicatele
            // Ex: "laptop.jpg" devine "guid-unic-laptop.jpg"
            string uniqueFileName = Guid.NewGuid().ToString() + "_" + imageFile.FileName;
            
            // Compunem calea unde o salvăm: wwwroot/images
            string uploadsFolder = Path.Combine(whe.WebRootPath, "images");
            
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

            db.Add(product);
            TempData["message"] = "Produsul a fost adaugat cu succes!";
            await db.SaveChangesAsync();
            return RedirectToAction(nameof(Index)); // Ne întoarcem la listă
        }

        // Dacă ceva a mers prost, reîncărcăm lista de categorii și reafisam formularul
        ViewBag.CategoryId = new SelectList(db.Categories, "Id", "Name", product.CategoryId);
        return View(product);
    }
    [Authorize(Roles="User,Editor,Admin")]
    public async Task<IActionResult> Index()
    {
        var produse = await db.Products
            .Include(a=>a.Category)
            .ToListAsync(); 

        return View(produse);
    }
   
    //Se sterge un produs din baza de date
    [HttpPost]
    [Authorize] // oricine logat poate ajunge aici, verificam rolurile in interior
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var product = await db.Products.FindAsync(id);

        if (product is null)
        {
            return NotFound();
        }

        var currentUserId = _userManager.GetUserId(User);

        // Admin sau Editor pot sterge ORICE produs
        // User simplu poate sterge DOAR produsul lui
        if (User.IsInRole("Admin") ||
            User.IsInRole("Editor") ||
            product.UserId == currentUserId)
        {
            db.Products.Remove(product);
            await db.SaveChangesAsync();

            TempData["message"] = "Produsul a fost sters.";
            TempData["message type"] = "alert-success";
            return RedirectToAction("Index");
        }
        else
        {
            TempData["message"] = "Nu aveti dreptul sa stergeti acest produs.";
            TempData["message type"] = "alert-danger";
            return RedirectToAction("Index");
        }
    }

    public IActionResult IndexNou()
    {
        return View();
    }

    public IEnumerable<SelectListItem> GetAllCategories()
    {
        //generam o lista de tipul SelectListItem fara elemente
        var selecList=new List<SelectListItem>();
        //extragem toate categoriile din baza de date
        var categories=db.Categories.ToList();
        //pentru fiecare categorie adaugam un element in lista
        foreach (var category in categories)
        {
            selecList.Add(new SelectListItem{Value=category.Id.ToString(),Text=category.Name});
        }
        return selecList;
    }
    
    
    [HttpGet]
    [Authorize(Roles = "Editor,Admin")]
    public IActionResult Edit(int id)
{
    var produs = db.Products
        .Include(p => p.Category)
        .FirstOrDefault(p => p.Id == id);

    if (produs == null)
    {
        return NotFound();
    }

    var vm = new ProductEditViewModel
    {
        Id = produs.Id,
        Title = produs.Title,
        Description = produs.Description,
        Price = produs.Price,
        Stock = produs.Stock,
        CategoryId = produs.CategoryId,
        ImageUrlCurenta = produs.ImageUrl
    };

    ViewBag.CategoryId = new SelectList(db.Categories, "Id", "Name", vm.CategoryId);
    if(produs.UserId == _userManager.GetUserId(User) || User.IsInRole("Admin"))
    {return View(vm);}
    else
    {
        TempData["message"] = "Nu aveti dreptul sa faceti modifcari asupra unui articol care nu va apartine";
        return RedirectToAction("Index");
    }
}
[Authorize]
[HttpPost]
public async Task<IActionResult> Edit(ProductEditViewModel model)
{
    var produs = db.Products.FirstOrDefault(p => p.Id == model.Id);
    if (produs == null)
    {
        return NotFound();
    }

    // verificare drepturi: proprietar sau admin
    if (produs.UserId == _userManager.GetUserId(User) || User.IsInRole("Admin"))
    {
        // validare minima: categoria obligatorie daca vrei sa impui asta la edit
        if (model.CategoryId == null)
        {
            ModelState.AddModelError("CategoryId", "Selectati o categorie");
        }

        if (!ModelState.IsValid)
        {
            ViewBag.CategoryId = new SelectList(db.Categories, "Id", "Name", model.CategoryId);
            return View(model);
        }

        // LOGICA DE "PATCH"
        if (!string.IsNullOrWhiteSpace(model.Title))
            produs.Title = model.Title;

        if (!string.IsNullOrWhiteSpace(model.Description))
            produs.Description = model.Description;

        if (model.Price.HasValue)
            produs.Price = model.Price.Value;

        if (model.Stock.HasValue)
            produs.Stock = model.Stock.Value;

        if (model.CategoryId.HasValue)
            produs.CategoryId = model.CategoryId.Value;

        // Imagine noua optionala
        if (model.ImagineNoua != null && model.ImagineNoua.Length > 0)
        {
            string extensie = Path.GetExtension(model.ImagineNoua.FileName).ToLower();
            string[] permise = { ".jpg", ".jpeg", ".png", ".gif" };

            if (!permise.Contains(extensie))
            {
                ModelState.AddModelError("ImagineNoua", "Format invalid pentru imagine.");
                ViewBag.CategoryId = new SelectList(db.Categories, "Id", "Name", model.CategoryId);
                return View(model);
            }

            string numeUnic = Guid.NewGuid() + "_" + model.ImagineNoua.FileName;
            string folder = Path.Combine(whe.WebRootPath, "images");

            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            string caleFisier = Path.Combine(folder, numeUnic);

            using var fisierStream = new FileStream(caleFisier, FileMode.Create);
            await model.ImagineNoua.CopyToAsync(fisierStream);

            produs.ImageUrl = "/images/" + numeUnic;
        }

        await db.SaveChangesAsync();

        TempData["message"] = "Produsul a fost modificat";
        TempData["messagetype"] = "alert-success";

        return RedirectToAction("Index");
    }
    else
    {
        TempData["message"] = "Nu aveti dreptul sa faceti modificari asupra unui produs care nu va apartine";
        TempData["messagetype"] = "alert-danger";
        return RedirectToAction("Index");
    }
}

    [HttpGet]
    public IActionResult Show(int id)
    {
        Product? produs = db.Products
            .Include(a => a.Category)
            .Include(a => a.Reviews)
            .Where(a => a.Id == id)
            .FirstOrDefault();
        if (produs is null)
        {
            return NotFound();
        }
        return View(produs);   
    }
}