using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using SmartFridgeBackend.Database.Models;
using SmartFridgeBackend.DTOs;
using SmartFridgeBackend.Services;
using FirebaseAdmin.Auth;

namespace SmartFridgeBackend.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IMongoCollection<User> _users;
        private readonly FirebaseAuthService _firebaseAuthService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IMongoDatabase database, FirebaseAuthService firebaseAuthService, ILogger<AuthController> logger)
        {
            _users = database.GetCollection<User>("Users");
            _firebaseAuthService = firebaseAuthService;
            _logger = logger;
        }

        // UC1 — Opret Bruger
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] CreateUserRequest request)
        {
            if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
            {
                _logger.LogWarning("User did not fill out Email and Password");
                return BadRequest(new { fejl = "Email and Password is required" });
            }

            // Tjek om adgangskoden er den rigtige længde. Mellem 8 og 12 tegn
            if (request.Password.Length < 8 || request.Password.Length > 12)
            {
                _logger.LogWarning("User did not meet the password requirements");
                return BadRequest(new { fejl = "Password must be between 8 and 12 characters"});
            }

            // Tjek om det er ASCII tegn
            if(!request.Password.All(c => c <= 127))
            {
                _logger.LogWarning("User did not meet the password requirements");
                return BadRequest( new { fejl = "Password must be ASCII characters"});
            }

            var existingUser = await _users
                .Find(u => u.Email == request.Email)
                .FirstOrDefaultAsync();

            if (existingUser != null)
            {
                _logger.LogWarning("User using ({Email}) already exists", request.Email);
                return Conflict(new { fejl = "User already exists" });
            }

            var firebaseUser = await _firebaseAuthService.RegisterAsync(request.Email, request.Password);
            _logger.LogInformation("New user {FirebaseUid}", firebaseUser.Uid);
            var newUser = new User
            {
                AuthId = firebaseUser.Uid,
                Email = firebaseUser.Email,
                CreatedAt = DateTime.UtcNow
            };

            await _users.InsertOneAsync(newUser);

            var token = await _firebaseAuthService.LoginAsync(request.Email, request.Password);
            _logger.LogInformation("User {FirebaseUid} now logged in", firebaseUser.Uid);
            // Email verification
            try
            {
                await _firebaseAuthService.SendEmailVerificationAsync(token);
                _logger.LogInformation("Verification email sent to: {Email}", request.Email);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Firebase could not send verification email");
            }
            
            return StatusCode(201, new AuthResponse
            {
                Token = token,
                Email = newUser.Email
            });
        }

        // UC2 — Log Ind
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            {
                _logger.LogWarning("User did not fill out Email and Password");
                return BadRequest(new { error = "Email and password are required" });
            }
            
            try
            {
                var token = await _firebaseAuthService.LoginAsync(request.Email.Trim(), request.Password.Trim());
                
                var decodedToken = await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(token);
                var firebaseUid = decodedToken.Uid;

                var user = await _users
                    .Find(u => u.AuthId == firebaseUid)
                    .FirstOrDefaultAsync();

                if (user == null)
                {
                    _logger.LogWarning("User was not found in the backend database");
                    return NotFound(new { error = "User was not found in the backend database" });
                }
                _logger.LogInformation("User {FirebaseUid} is now logged in", firebaseUid);
                return Ok(new AuthResponse
                {
                    Token = token,
                    Email = user.Email
                });
            }
            catch (Exception ex) when (
                ex.Message.Contains("INVALID_LOGIN_CREDENTIALS") ||
                ex.Message.Contains("EMAIL_NOT_FOUND") ||
                ex.Message.Contains("INVALID_PASSWORD"))
            {
                _logger.LogWarning(ex, "Email or password is incorrect");
                return Unauthorized(new
                {
                    error = "Forkert email eller adgangskode"
                });
            }
        }

        // UC2 — Hent brugerdata via JWT token
        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> Me()
        {
            var firebaseUid = User.FindFirst("uid")?.Value;

            if (string.IsNullOrWhiteSpace(firebaseUid))
            {
                _logger.LogWarning("Unauthorized request to get me. Token not valid.");
                return Unauthorized(new { error = "Invalid or missing token" });
            }

            var user = await _users
                .Find(u => u.AuthId == firebaseUid)
                .FirstOrDefaultAsync();

            if (user == null)
            {
                _logger.LogWarning("User not found in the backend database");
                return NotFound(new { error = "User was not found" });
            }

            var firebaseUser = await FirebaseAuth.DefaultInstance.GetUserAsync(firebaseUid);
            _logger.LogInformation("User {FirebaseUid} sucessfully requested me", firebaseUid);
            return Ok(new MeResponse
            {
                UserId = user.UserId ?? "",
                Email = user.Email ?? "",
                CreatedAt = user.CreatedAt ?? DateTime.UtcNow,
                EmailVerified = firebaseUser.EmailVerified
            });
        }

        [HttpPost("Resend Verification Email")]
        [Authorize]
        public async Task<IActionResult> ResendVerification()
        {
            var authHeader = Request.Headers.Authorization.ToString();

            if (string.IsNullOrWhiteSpace(authHeader) || !authHeader.StartsWith("Bearer "))
            {
                _logger.LogWarning("Unauthorized request to get me. Token not valid.");
                return Unauthorized(new { error = "Invalid or missing token" });
            }

            var idToken = authHeader.Replace("Bearer ", "");

            try
            {
                await _firebaseAuthService.SendEmailVerificationAsync(idToken);
                _logger.LogInformation("Verification email sent for user {FirebaseUid}", User.FindFirst("uid")?.Value);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Firebase could not send verification email");
                return StatusCode(500, new { error = "Could not send verification email"});
            }

            
            return Ok(new
            {
                message = "Verification email sent"
            });
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email))
            {
                _logger.LogWarning("Email is required");
                return BadRequest(new { error = "Email is required" });
            }

            try
            {
                await _firebaseAuthService.SendPasswordResetEmailAsync(request.Email);
                _logger.LogInformation("Password reset email sent to {Email}", request.Email);
            }
            catch
            {
                _logger.LogWarning("Forgot password request for {Email} attempted", request.Email);
                // Vi skjuler fejlen, så man ikke kan gætte om en email findes i systemet
            }
            
            return Ok(new
            {
                message = "If the email exists, a password reset email has been sent."
            });
        }
    }
}