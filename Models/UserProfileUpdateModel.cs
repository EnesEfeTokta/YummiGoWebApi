// Models/UserProfileUpdateModel.cs
using System.ComponentModel.DataAnnotations;

namespace YummiGoWebApi.Models
{
    // Sadece Email güncelleme için
    public class UserProfileUpdateModel
    {
        [Required(ErrorMessage = "E-posta adresi zorunludur.")]
        [EmailAddress(ErrorMessage = "Geçersiz e-posta adresi formatı.")]
        public string Email { get; set; } = string.Empty;

        // Username alanı burada YOKTU.
    }
}