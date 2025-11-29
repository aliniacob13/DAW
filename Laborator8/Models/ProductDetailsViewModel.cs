using System.Collections.Generic;

namespace ArticlesApp.Models
{
    // ViewModel pentru pagina de detalii produs:
    //  - produsul în sine
    //  - lista de review uri existente
    //  - un review nou care se completeaza în formular
    public class ProductDetailsViewModel
    {
        public Product Product { get; set; } = null!;

        public IList<Review> Reviews { get; set; } = new List<Review>();

        // Folosit pentru formularul de "Adauga review"
        public Review NewReview { get; set; } = new Review();
    }
}