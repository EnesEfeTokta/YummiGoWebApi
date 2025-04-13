// Models/LoginModel.cs
using System.ComponentModel.DataAnnotations;
namespace YummiGoWebApi.Models
{
    // Giriş yapmak için API'ye gönderilen veri
    public class LoginModel
    {
        [Required(ErrorMessage = "Kullanıcı adı zorunludur.")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Şifre zorunludur.")]
        public string Password { get; set; } = string.Empty;
    }
}