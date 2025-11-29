using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ArticlesApp.Models
{
    public class Category
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Numele categoriei este obligatoriu")]
        [StringLength(100)]
        public string CategoryName { get; set; } = string.Empty;

        // O categorie are mai multe produse
        public virtual ICollection<Product> Products { get; set; } = [];
    }
}