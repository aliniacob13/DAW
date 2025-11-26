    using ArticlesApp.Data;
using ArticlesApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace ArticlesApp.Controllers
{
    public class ArticlesController(ApplicationDbContext context) : Controller
    {
        private readonly ApplicationDbContext db = context;

        // Se afiseaza lista tuturor articolelor impreuna cu categoria 
        // din care fac parte
        // HttpGet implicit
        public IActionResult Index()
        {
            var articles = db.Articles
                             .Include(a => a.Category)
                             .OrderByDescending(a => a.Date);

            // ViewBag.OriceDenumireSugestiva
            ViewBag.Articles = articles;

            if (TempData.ContainsKey("message"))
            {
                ViewBag.Message = TempData["message"];
            }

            return View();
        }

        // Se afiseaza un singur articol in functie de id-ul sau 
        // impreuna cu categoria din care face parte
        // In plus sunt preluate si toate comentariile asociate unui articol
        // HttpGet implicit
        public IActionResult Show(int id)
        {
            Article? article = db.Articles
                                 .Include(a => a.Category)
                                 .Include(a => a.Comments)
                                 .Where(a => a.Id == id)
                                 .FirstOrDefault();

            if (article is null)
            {
                return NotFound();

                // sau se foloseste TempData
                // TempData["message"] = "Articolul nu exista!";
                // return RedirectToAction("Index");
            }

            return View(article);
        }


        // Se afiseaza formularul in care se vor completa datele unui articol
        // impreuna cu selectarea categoriei din care face parte
        // HttpGet implicit
        public IActionResult New()
        {
            Article article = new Article();

            article.Categ = GetAllCategories();

            return View(article);
        }

        // Se adauga articolul in baza de date
        [HttpPost]
        public IActionResult New(Article article)
        {
            article.Date = DateTime.Now;

            if (ModelState.IsValid)
            {
                db.Articles.Add(article);
                db.SaveChanges();
                TempData["message"] = "Articolul a fost adaugat";
                return RedirectToAction("Index");
            }

            else
            {
                article.Categ = GetAllCategories();
                return View(article);
            }
        }


        // Se editeaza un articol existent in baza de date impreuna cu categoria din care face parte
        // Categoria se selecteaza dintr-un dropdown
        // HttpGet implicit
        // Se afiseaza formularul impreuna cu datele aferente articolului din baza de date
        public IActionResult Edit(int id)
        {

            Article? article = db.Articles
                                .Include(a => a.Category)
                                .Where(art => art.Id == id)
                                .FirstOrDefault();

            if (article is null)
            {
                return NotFound();

               // sau se foloseste TempData
               // TempData["message"] = "Articolul nu exista!";
               // return RedirectToAction("Index");
            }

            article.Categ = GetAllCategories();

            return View(article);

        }

        // Se adauga articolul modificat in baza de date
        [HttpPost]
        public IActionResult Edit(int id, Article requestArticle)
        {
            Article? article = db.Articles.Find(id);

            if(article is null)
            {
                return NotFound();
            }

            else
            {
                if (ModelState.IsValid)
                {
                    article.Title = requestArticle.Title;
                    article.Content = requestArticle.Content;
                    article.Date = DateTime.Now;
                    article.CategoryId = requestArticle.CategoryId;
                    TempData["message"] = "Articolul a fost modificat";
                    db.SaveChanges();
                    return RedirectToAction("Index");

                }
                else
                {
                    requestArticle.Categ = GetAllCategories();
                    return View(requestArticle);
                }
            }   
        }


        // Se sterge un articol din baza de date 
        [HttpPost]
        public ActionResult Delete(int id)
        {
            Article? article = db.Articles.Find(id);

            if (article is null)
            {
                return NotFound();
            }

            db.Articles.Remove(article);

            try
            {
                db.SaveChanges();
                TempData["message"] = "Articolul a fost sters";
            }
            catch(DbUpdateException)
            {
                TempData["message"] = "Nu se poate sterge articolul";
            }

            return RedirectToAction("Index");
        }

        // Adaugarea unui comentariu asociat unui articol in baza de date
        [HttpPost]
        public IActionResult Show([FromForm] Comment comment)
        {
            comment.Date = DateTime.Now;

            if (ModelState.IsValid)
            {
                db.Comments.Add(comment);
                db.SaveChanges();
                return Redirect("/Articles/Show/" + comment.ArticleId);
            }
            else
            {
                Article? art = db.Articles
                                .Include(a => a.Category)
                                .Include(a => a.Comments)
                                .Where(art => art.Id == comment.ArticleId)
                                .FirstOrDefault();

                if (art is null)
                {
                    return NotFound();
                }

                //return Redirect("/Articles/Show/" + comm.ArticleId);

                return View(art);
            }

        }


            [NonAction]
        public IEnumerable<SelectListItem> GetAllCategories()
        {
            // generam o lista de tipul SelectListItem fara elemente
            var selectList = new List<SelectListItem>();

            // extragem toate categoriile din baza de date
            var categories = from cat in db.Categories
                             select cat;

            // iteram prin categorii
            foreach (var category in categories)
            {
                // adaugam in lista elementele necesare pentru dropdown
                // id-ul categoriei si denumirea acesteia
                selectList.Add(new SelectListItem
                {
                    Value = category.Id.ToString(),
                    Text = category.CategoryName
                });
            }
            /* Sau se poate implementa astfel: 
             * 
            foreach (var category in categories)
            {
                var listItem = new SelectListItem();
                listItem.Value = category.Id.ToString();
                listItem.Text = category.CategoryName;

                selectList.Add(listItem);
             }*/


            // returnam lista de categorii
            return selectList;
        }

        // Metoda utilizata pentru exemplificarea Layout-ului
        // Am adaugat un nou Layout in Views -> Shared -> numit _LayoutNou.cshtml
        // Aceasta metoda are un View asociat care utilizeaza noul layout creat
        // in locul celui default generat de framework numit _Layout.cshtml
        public IActionResult IndexNou()
        {
            return View();
        }
    }
}
