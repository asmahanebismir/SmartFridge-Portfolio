using SmartFridgeBackend.DTOs.Groceries;

namespace SmartFridgeBackend.Services;

public interface IGroceriesService
{
    Task<List<GroceriesResponse>> GetMineAsync(string firebaseUid);
    Task<GroceriesResponse?> AddAsync(string firebaseUid, CreateGroceriesRequest request);
    Task<bool> DeleteAsync(string firebaseUid, string grocerieId);
    Task<bool> UpdateAmountAsync(string firebaseUid, string grocerieId, int amount);
}