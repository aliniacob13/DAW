using System.ComponentModel.DataAnnotations;

namespace ArticlesApp.Models
{
    public class ProductReview
    {
        public int Id { get; set; }

        [Required]
        public int ProductId { get; set; }
        public Product? Product { get; set; }

        // rating este opțional, 1–5
        [Range(1, 5, ErrorMessage = "Ratingul trebuie să fie între 1 și 5")]
        public int? Rating { get; set; }

        // textul review-ului (opțional după cerință)
        public string? Text { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // cine a lăsat review-ul
        public string? UserId { get; set; }
        public ApplicationUser? User { get; set; }
    }
}