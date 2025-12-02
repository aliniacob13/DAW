using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace OnlineShop.Models;
public class Review : IValidatableObject
{
    public int Id { get; set; }

    // Rating de la 1 la 5. Este nullable (int?) pentru că nu e obligatoriu.
    [Range(1, 5, ErrorMessage = "Ratingul trebuie să fie între 1 și 5.")]
    public int? Rating { get; set; }

    // Comentariul text, opțional
    public string? Comment { get; set; }

    // Data la care a fost lăsat review-ul (opțional, dar recomandat)
    public DateTime DatePosted { get; set; } = DateTime.Now;

    // --- RELAȚII (Foreign Key) ---
    public int ProductId { get; set; }
    
    public string? UserId { get; set; }
    public virtual ApplicationUser? User { get; set; }
    [ValidateNever]
    public Product Product { get; set; }

    // --- VALIDARE PERSONALIZATĂ ---
    // Această metodă se apelează automat când dai save/post
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        // Dacă nu avem NICI rating, NICI comentariu => Eroare
        if (Rating == null && string.IsNullOrWhiteSpace(Comment))
        {
            yield return new ValidationResult(
                "Nu se poate posta un review gol, trebuie cel putin adaugata o nota sau un comentariu."
            );
        }
    }
}