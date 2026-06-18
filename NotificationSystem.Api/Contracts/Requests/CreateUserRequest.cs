using System.ComponentModel.DataAnnotations;

namespace NotificationSystem.Api.Contracts.Requests;

public class CreateUserRequest
{
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Email is invalid.")]
    public string Email { get; set; } = string.Empty;
}
