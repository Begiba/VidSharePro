// VidSharePro.API/Models/UploadVideoForm.cs
using System.ComponentModel.DataAnnotations;

namespace VidSharePro.API.Models;

public class UploadVideoForm
{
    [Required]
    public string Title { get; set; } = null!;

    [Required]
    public IFormFile File { get; set; } = null!;
}