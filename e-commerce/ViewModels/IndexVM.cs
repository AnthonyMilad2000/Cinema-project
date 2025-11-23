using Cinema.Models;
using System.Collections.Generic;

namespace Cinema.ViewModels
{
    public class IndexVM
    {
        public List<Movie> Movie { get; set; } = new();
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
    }


}
