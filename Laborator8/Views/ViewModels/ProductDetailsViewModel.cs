using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ArticlesApp.Models.ViewModels
{
    public class ProductDetailsViewModel
    {
        public Product Product { get; set; } = null!;
        public List<Review> Reviews { get; set; } = new();

        public ReviewInputModel NewReview { get; set; } = new();
    }

    public class ReviewInputModel : IValidatableObject
    {
        public int ProductId { get; set; }

        [Range(1, 5, ErrorMessage = "Ratingul trebuie sa fie intre 1 si 5.")]
        public int? Rating { get; set; }

        public string? Content { get; set; }

        // Validare custom: nu permitem review complet gol
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (!Rating.HasValue && string.IsNullOrWhiteSpace(Content))
            {
                yield return new ValidationResult(
                    "Trebuie sa completezi cel putin rating sau un comentariu.",
                    new[] { nameof(Rating), nameof(Content) });
            }
        }
    }
}