// Models/UserViewModel.cs
namespace YummiGoWebApi.Models
{
    public class UserViewModel
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        // ProfilePictureUrl alanı burada YOKTU.
    }
}