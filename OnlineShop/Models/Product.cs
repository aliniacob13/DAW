using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace OnlineShop.Models;
public class Product
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Titlul produsului este obligatoriu.")]
    [StringLength(100)]
    public string Title { get; set; }

    [Required(ErrorMessage = "Descrierea este obligatorie.")]
    public string Description { get; set; }

    // Stocăm calea imaginii (ex: "/images/produse/laptop.jpg")
    [Required(ErrorMessage = "Imaginea este obligatorie.")]
    public string ImageUrl { get; set; }

    // Preț > 0. Folosim 'decimal' pentru bani (precizie mai bună decât double)
    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Prețul trebuie să fie mai mare decât 0.")]
    [Column(TypeName = "decimal(18,2)")] // Configurare pentru baza de date SQL
    public decimal Price { get; set; }

    // Stoc >= 0
    [Required]
    [Range(0, int.MaxValue, ErrorMessage = "Stocul nu poate fi negativ.")]
    public int Stock { get; set; }

    // Flag pentru aprobare (implicit false)
    public bool IsApproved { get; set; } = false;

    // --- RELAȚII ---

    // Cheia externă către Categorie
    [Required(ErrorMessage = "Selectarea unei categorii este obligatorie.")]
    public int CategoryId { get; set; }
    public Category Category { get; set; }

    // Lista de review-uri
    public ICollection<Review> Reviews { get; set; } = new List<Review>();

    // --- CALCUL AUTOMAT RATING ---
    
    // Această proprietate nu se salvează în baza de date ([NotMapped]),
    // ci se calculează on-the-fly când afișezi produsul.
    [NotMapped]
    public double AverageRating
    {
        get
        {
            if (Reviews == null || Reviews.Count == 0)
            {
                return 0; // Rating inițial 0
            }
            
            // Calculăm media doar pentru review-urile care au rating (cele cu null sunt ignorate)
            var ratings = Reviews.Where(r => r.Rating.HasValue).Select(r => r.Rating.Value);
            
            if (!ratings.Any()) return 0;

            return Math.Round(ratings.Average(), 1); // Returnăm media cu o zecimală (ex: 4.5)
        }
    }
}