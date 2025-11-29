using System.ComponentModel.DataAnnotations;

namespace ArticlesApp.Models
{
    public class Review
    {
        public int Id { get; set; }

        // Legatura la produs
        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;

        // Rating optional, dar daca exista trebuie sa fie 1..5
        [Range(1, 5)]
        public int? Rating { get; set; }

        // Comentariu optional
        public string? Content { get; set; }

        // Legatura la utilizatorul care a scris review-ul
        public string? UserId { get; set; }
        public ApplicationUser? User { get; set; }
    }
}