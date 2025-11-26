using System.ComponentModel.DataAnnotations;

namespace ArticlesApp.Models
{
    public class Comment
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Continutul comentariului este obligatoriu")]
        public string Content { get; set; }

        public DateTime Date { get; set; }

        public int ArticleId { get; set; }
        
        //pasul 6: uesri si roluri
        //cheia externa (FK): un comentariu este postat de catre un user
        public string? UserId { get; set; }
        
        //proprietatea de navigatie
        //un comentariu este postat de catre un user
        public virtual ApplicationUser? User { get; set; }

        public virtual Article? Article { get; set; }
    }

}
