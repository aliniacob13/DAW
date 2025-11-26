using Microsoft.AspNetCore.Identity;

namespace ArticlesApp.Models;
//PASUL 1: useri si roluri
public class ApplicationUser : IdentityUser
{
    //pasul 6: useri si roluri
    //un user posteaza mai multe articole
    public virtual ICollection<Article>? Articles { get; set; }
    //un user posteaza mai multe comentarii
    public virtual ICollection<Comment>? Comments { get; set; } = [];
}