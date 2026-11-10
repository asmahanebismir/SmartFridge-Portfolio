using SmartFridgeBackend.Database.Repositories;
using SmartFridgeBackend.DTOs.Groceries;
using ItemModel = SmartFridgeBackend.Database.Models.Item;
using UserModel = SmartFridgeBackend.Database.Models.User;

namespace SmartFridgeBackend.Services;

public class GroceriesService : IGroceriesService
{
    private readonly IRepository<ItemModel> _itemRepository;
    private readonly IRepository<UserModel> _userRepository;

    public GroceriesService(
        IRepository<ItemModel> itemRepository,
        IRepository<UserModel> userRepository
    )
    {
        _itemRepository = itemRepository;
        _userRepository = userRepository;
    }

    private async Task<UserModel?> GetUserByFirebaseUid(string firebaseUid)
    {
        var users = await _userRepository.FindAsync(u => u.AuthId == firebaseUid);
        return users.FirstOrDefault();
    }

    public async Task<List<GroceriesResponse>> GetMineAsync(string firebaseUid)
    {
        var user = await GetUserByFirebaseUid(firebaseUid);
        if (user == null) return new List<GroceriesResponse>();

        var items = await _itemRepository.FindAsync(i => i.UserId == user.UserId);

        return items.Select(i => new GroceriesResponse
        {
            Id = i.ItemId!,
            Name = i.Name!,
            Amount = i.Amount,
            Barcode = i.Barcode,
            ImageUrl = i.ImageUrl
        }).ToList();
    }

    public async Task<GroceriesResponse?> AddAsync(string firebaseUid, CreateGroceriesRequest request)
    {
        var user = await GetUserByFirebaseUid(firebaseUid);
        if (user == null) return null;

        var item = new ItemModel
        {
            UserId = user.UserId,
            Name = request.Name,
            Amount = request.Amount,
            Barcode = request.Barcode,
            ImageUrl = request.ImageUrl
        };

        await _itemRepository.InsertAsync(item);

        return new GroceriesResponse
        {
            Id = item.ItemId!,
            Name = item.Name!,
            Amount = item.Amount,
            Barcode = item.Barcode,
            ImageUrl = item.ImageUrl
        };
    }

    public async Task<bool> DeleteAsync(string firebaseUid, string grocerieId)
    {
        var user = await GetUserByFirebaseUid(firebaseUid);
        if (user == null) return false;

        var items = await _itemRepository.FindAsync(i => i.ItemId == grocerieId && i.UserId == user.UserId);
        var item = items.FirstOrDefault();

        if (item == null) return false;

        await _itemRepository.DeleteAsync(grocerieId);
        return true;
    }

    public async Task<bool> UpdateAmountAsync(string firebaseUid, string grocerieId, int amount)
    {
        var user = await GetUserByFirebaseUid(firebaseUid);
        if (user == null) return false;

        var items = await _itemRepository.FindAsync(i => i.ItemId == grocerieId && i.UserId == user.UserId);
        var item = items.FirstOrDefault();

        if (item == null) return false;

        item.Amount = amount;

        await _itemRepository.ReplaceAsync(grocerieId, item);
        return true;
    }
}