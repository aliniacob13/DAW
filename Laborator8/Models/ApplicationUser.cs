using Microsoft.AspNetCore.Identity;

namespace ArticlesApp.Models
{
    public class ApplicationUser : IdentityUser
    {
        // Produsele create sau propuse de utilizator
        public virtual ICollection<Product>? Products { get; set; } = [];

        // Review urile scrise de utilizator
        public virtual ICollection<Review>? Reviews { get; set; } = [];
    }
}