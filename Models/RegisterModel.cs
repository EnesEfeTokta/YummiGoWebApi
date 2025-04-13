// Models/RegisterModel.cs
using System.ComponentModel.DataAnnotations;
namespace YummiGoWebApi.Models
{
    // Yeni kullanıcı kaydetmek için API'ye gönderilen veri
    public class RegisterModel
    {
        [Required(ErrorMessage = "Kullanıcı adı zorunludur.")]
        [MinLength(3)]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "E-posta adresi zorunludur.")]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Şifre zorunludur.")]
        [MinLength(6)]
        public string Password { get; set; } = string.Empty;
    }
}