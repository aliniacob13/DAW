using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace ArticlesApp.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        // Pret > 0
        [Column(TypeName = "decimal(10,2)")]
        [Range(0.01, 999999)]
        public decimal Price { get; set; }

        // Stoc ≥ 0
        [Range(0, int.MaxValue)]
        public int Stock { get; set; }

        // Numele fisierului de imagine, de exemplu: "produs1.jpg"
        public string? ImagePath { get; set; }

        // Legatura la categorie
        [Required]
        public int CategoryId { get; set; }
        public Category Category { get; set; } = null!;

        // Review uri asociate produsului
        public ICollection<Review> Reviews { get; set; } = new List<Review>();

        // Scor calculat automat ca medie a ratingurilor (0 daca nu exista ratinguri)
        [NotMapped]
        public double AverageRating
        {
            get
            {
                var ratings = Reviews
                    .Where(r => r.Rating.HasValue)
                    .Select(r => r.Rating!.Value)
                    .ToList();

                if (ratings.Count == 0)
                {
                    return 0;
                }

                return ratings.Average();
            }
        }
    }
}