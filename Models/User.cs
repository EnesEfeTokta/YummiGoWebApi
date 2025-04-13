// Models/User.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YummiGoWebApi.Models
{
    [Table("Users")]
    public class User
    {
        [Key]
        [Column("UserId")]
        public int Id { get; set; }

        [Required]
        [Column("Username")]
        public string Username { get; set; } = string.Empty;

        [Required]
        [Column("Email")]
        public string Email { get; set; } = string.Empty;

        [Required]
        [Column("Password")]
        public string Password { get; set; } = string.Empty; // GÜVENLİK RİSKİ: Hash'lenmeli!

        // ProfilePictureUrl alanı burada YOKTU.
    }
}