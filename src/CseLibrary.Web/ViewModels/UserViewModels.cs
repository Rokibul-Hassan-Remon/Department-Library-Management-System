using System.ComponentModel.DataAnnotations;

namespace CseLibrary.Web.ViewModels;

public class CreateUserViewModel
{
    [Required, Display(Name = "User name")]
    public string UserName { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Display(Name = "Full name")]
    public string? FullName { get; set; }

    [Required, MinLength(6), DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    public IList<string> SelectedRoles { get; set; } = new List<string>();
    public IList<string> AvailableRoles { get; set; } = new List<string>();
}

public class EditUserViewModel
{
    public string Id { get; set; } = string.Empty;

    [Display(Name = "User name")]
    public string? UserName { get; set; }

    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Display(Name = "Full name")]
    public string? FullName { get; set; }

    [Display(Name = "Active")]
    public bool IsActive { get; set; }

    public string? PhotoPath { get; set; } // display only; the upload is a separate form file

    public IList<string> SelectedRoles { get; set; } = new List<string>();
    public IList<string> AvailableRoles { get; set; } = new List<string>();
}

public class ResetPasswordViewModel
{
    public string Id { get; set; } = string.Empty;
    public string? UserName { get; set; }

    [Required, MinLength(6), DataType(DataType.Password), Display(Name = "New password")]
    public string NewPassword { get; set; } = string.Empty;

    [Required, DataType(DataType.Password), Display(Name = "Confirm new password")]
    [Compare(nameof(NewPassword), ErrorMessage = "Passwords do not match.")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
