using Cinema.Models;
using System.ComponentModel.DataAnnotations;

public class Promotion
{
    public int Id { get; set; }

    [Required]
    public string Code { get; set; }

    [Range(0, 100)]
    public decimal Discount { get; set; }

    public bool IsValid { get; set; }

    [Required]
    public DateTime? ValidTo { get; set; }


    [Range(1, int.MaxValue)]
    public int MaxUsage { get; set; }

    [Required]
    public int MovieId { get; set; }
    public Movie? Movie { get; set; }
}
