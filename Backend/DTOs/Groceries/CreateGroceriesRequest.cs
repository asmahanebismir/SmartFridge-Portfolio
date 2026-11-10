using System.ComponentModel.DataAnnotations;

namespace SmartFridgeBackend.DTOs.Groceries;

public class CreateGroceriesRequest
{
    [Required]
    [MinLength(1)]
    [MaxLength(100)]
    public string Name { get; set; } = "";

    [Range(1,9999)]
    public int Amount { get; set; }

[MaxLength(100)] // Lidt i tvivl om længderne af stregkoderne lige nu. Retter det hvis det bliver et problem. 
    public string? Barcode { get; set; }

    [MaxLength(500)]
    public string? ImageUrl { get; set; }
}