using System.ComponentModel.DataAnnotations;

namespace NotificationSystem.Api.Contracts.Requests;

public class CreateUserRequest
{
    [Required(ErrorMessage = "Name is required.")]
    [MinLength(2, ErrorMessage = "Name must be at least 2 characters.")]
    [MaxLength(100, ErrorMessage = "Name must not exceed 100 characters.")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Email is invalid.")]
    [MaxLength(254, ErrorMessage = "Email must not exceed 254 characters.")] // RFC 5321 limit
    public string Email { get; set; } = string.Empty;
}
