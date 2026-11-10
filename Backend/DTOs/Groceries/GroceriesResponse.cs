namespace SmartFridgeBackend.DTOs.Groceries;

public class GroceriesResponse
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public int Amount { get; set; }
    public string? Barcode { get; set; }
    public string? ImageUrl { get; set; }
}