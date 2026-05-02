using System.ComponentModel.DataAnnotations;

namespace Gym.Client.Models;

public sealed class UpdateMemberModel
{
    [Required]
    [StringLength(50)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string LastName { get; set; } = string.Empty;

    [Required]
    public DateTime DateOfBirth { get; set; }

    [Required]
    [StringLength(20)]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required]
    [Url]
    public string ImageUrl { get; set; } = string.Empty;

    [Required]
    public DateTime JoinDate { get; set; }

    [StringLength(500)]
    public string? Notes { get; set; }
}
