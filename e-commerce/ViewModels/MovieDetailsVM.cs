using Cinema.Models;
using System.Collections.Generic;

namespace Cinema.ViewModels
{
    public class MovieDetailsVM
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public double Price { get; set; }
        public bool Status { get; set; }
        public string MainImage { get; set; } = string.Empty;
        public List<MovieImage> SubImages { get; set; } = new();
        public List<MovieSchedule> Schedules { get; set; } = new();
        public string? CinemaName { get; set; }
        public int CinemasCount { get; set; }
    }
}
