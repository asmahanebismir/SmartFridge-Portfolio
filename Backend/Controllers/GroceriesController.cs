using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartFridgeBackend.DTOs.Groceries;
using SmartFridgeBackend.Services;

namespace SmartFridgeBackend.Controllers;

[ApiController]
[Route("api/groceries")]
[Authorize(Policy = "EmailVerified")]
public class GroceriesController : ControllerBase
{
    private readonly IGroceriesService _groceriesService;
    private readonly ILogger<GroceriesController> _logger;

    public GroceriesController(IGroceriesService groceriesService, ILogger<GroceriesController> logger)
    {
        _groceriesService = groceriesService;
        _logger = logger;
    }

    private string? GetFirebaseUid()
    {
        return User.FindFirst("uid")?.Value;
    }

    [HttpGet]
    public async Task<IActionResult> GetMine()
    {
        var uid = GetFirebaseUid();
        if(uid == null)
        {
            _logger.LogWarning("Unauthorized request to get groceries.");
            return Unauthorized(new { error = "Du skal være logget ind" });
        } 

        var groceries = await _groceriesService.GetMineAsync(uid);
        _logger.LogInformation("User {FirebaseUid} fetched groceries", uid);
        return Ok(groceries);
    }

    [HttpPost("add")]
    public async Task<IActionResult> Add([FromBody] CreateGroceriesRequest request)
    {
        if (!ModelState.IsValid)
        return ValidationProblem(ModelState);

        var uid = GetFirebaseUid();
        if(uid == null)
        {
            _logger.LogWarning("Unauthorized request to add grocery");
            return Unauthorized(new { error = "Du skal være logget ind" });
        }

        var created = await _groceriesService.AddAsync(uid, request);
        if (created == null)
        {
            _logger.LogWarning("Grocery could not be added for user {FirebaseUid}", uid);
            return Unauthorized(new { error = "Du skal være logget ind" });
        }
        _logger.LogInformation("User {FirebaseUid} added grocery {GroceriesId}", uid, created.Id);
        return Ok(created);
    }

    [HttpDelete("{groceriesId}")]
    public async Task<IActionResult> Delete(string groceriesId)
    {
        var uid = GetFirebaseUid();
        if (uid == null)
        {
            _logger.LogWarning("Unauthorized request to delete grocery");
            return Unauthorized(new { error = "Du skal være logget ind" });
        }

        var success = await _groceriesService.DeleteAsync(uid, groceriesId);

        if (!success)
        {
            _logger.LogWarning("Grocery {GroceriesId} not found for user {FirebaseUid}", groceriesId, uid);
            return NotFound(new { error = "Varen blev ikke fundet"});
        }
        _logger.LogInformation("User {FirebaseUid} deleted grocery {GroceriesId}", uid, groceriesId);
        return NoContent();
    }

    [HttpPut("{groceriesId}/amount")]
    public async Task<IActionResult> UpdateAmount(string groceriesId, [FromBody] UpdateGroceriesAmountRequest request)
    {
        if(!ModelState.IsValid) return ValidationProblem(ModelState);

        var uid = GetFirebaseUid();
        if (uid == null)
        {
            _logger.LogWarning("Unauthorized request to edit grocery");
            return Unauthorized(new { error = "Du skal være logget ind" });
        }

        var success = await _groceriesService.UpdateAmountAsync(uid, groceriesId, request.Amount);

        if(!success)
        {
            _logger.LogWarning("Grocery {GroceriesId} not found for user {FirebaseUid}", groceriesId, uid);
            return NotFound(new { error = "Varen blev ikke fundet"});
        }
        _logger.LogInformation("User {FirebaseUid} edited grocery {GroceriesId} amount to {Amount}", uid, groceriesId, request.Amount);
        return NoContent();
    }
}