namespace SmartFridgeBackend.DTOs;

public class MeResponse
{
    public string UserId { get; set; } = "";
    public string Email { get; set; } = "";
    public DateTime CreatedAt { get; set; }
    public bool EmailVerified { get; set; }
}