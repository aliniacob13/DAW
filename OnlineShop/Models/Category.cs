namespace OnlineShop.Models;
using System.ComponentModel.DataAnnotations;
public class Category
{
    public int Id { get; set; }
    [Required(ErrorMessage = "Numele categoriei este obligatoriu!")]
    [StringLength(50)]
    public string Name { get; set; } = string.Empty;
    
    public ICollection<Product> Products { get; set; }
}