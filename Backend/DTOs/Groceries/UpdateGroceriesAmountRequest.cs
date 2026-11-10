using System.ComponentModel.DataAnnotations;
using System.Dynamic;

namespace SmartFridgeBackend.DTOs.Groceries;

public class UpdateGroceriesAmountRequest
{
    [Range(0,9999)]
    public int Amount { get; set; }
}