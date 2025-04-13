using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using YummiGoWebApi.Data;
using YummiGoWebApi.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace YummiGoWebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly DataContext _context;
        // IWebHostEnvironment burada YOKTU.

        public UserController(DataContext context) // Constructor eski hali
        {
            _context = context;
        }

        // --- Kayıt ---
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            if (await _context.Users.AnyAsync(u => u.Username == model.Username || u.Email == model.Email))
                return BadRequest(new { Message = "Bu kullanıcı adı veya e-posta adresi zaten kullanılıyor." });

            var newUser = new User { Username = model.Username, Email = model.Email, Password = model.Password };
            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            var userViewModel = new UserViewModel { Id = newUser.Id, Username = newUser.Username, Email = newUser.Email }; // ProfilePictureUrl YOK
            return Ok(new { Message = "Kullanıcı başarıyla kaydedildi.", User = userViewModel });
        }

        // --- Giriş ---
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == model.Username && u.Password == model.Password);
            if (user == null) return Unauthorized("Kullanıcı adı veya şifresi hatalı.");

            HttpContext.Session.SetString("UserId", user.Id.ToString());
            HttpContext.Session.SetString("Username", user.Username);

            var userViewModel = new UserViewModel { Id = user.Id, Username = user.Username, Email = user.Email }; // ProfilePictureUrl YOK
            return Ok(new { Message = "Giriş başarılı.", User = userViewModel });
        }

        // --- Oturum Kapatma --- (Aynı kalabilir)
        [HttpPost("logout")]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return Ok(new { Message = "Oturum başarıyla kapatıldı." });
        }

        // --- Giriş Yapan Kullanıcının Bilgilerini Getirme ---
        [HttpGet("me")]
        public async Task<IActionResult> GetCurrentUser()
        {
            string? userIdString = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId)) return Unauthorized(new { Message = "Bu işlemi yapmak için giriş yapmalısınız." });

            var user = await _context.Users.FindAsync(userId);
            if (user == null) { HttpContext.Session.Clear(); return Unauthorized(new { Message = "Oturumunuzla ilişkili kullanıcı bulunamadı." }); }

            var userViewModel = new UserViewModel { Id = user.Id, Username = user.Username, Email = user.Email }; // ProfilePictureUrl YOK
            return Ok(userViewModel);
        }

        // --- Giriş Yapan Kullanıcının Kendi Tariflerini Getirme --- (Aynı kalabilir veya geliştirilebilir)
        [HttpGet("me/recipes")]
        public async Task<ActionResult<IEnumerable<object>>> GetMyRecipes() // Dönen tipi object veya basit DTO yapalım
        {
            string? userIdString = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int loggedInUserId)) return Unauthorized(new { Message = "Bu işlemi yapmak için giriş yapmalısınız." });

            var myRecipesBasic = await _context.Recipes
                .Where(r => r.UserId == loggedInUserId)
                .Select(r => new
                {
                    r.Id,
                    r.Title,
                    r.Description,
                    r.ImageUrl,
                    r.Category
                })
                .ToListAsync();
            return Ok(myRecipesBasic);
        }

        // --- Giriş Yapan Kullanıcının Profilini Güncelleme (Sadece Email) ---
        [HttpPut("me")]
        public async Task<IActionResult> UpdateProfile([FromBody] UserProfileUpdateModel model) // Model sadece Email içeriyor
        {
            string? userIdString = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int loggedInUserId))
                return Unauthorized(new { Message = "Bu işlemi yapmak için giriş yapmalısınız." });

            var userToUpdate = await _context.Users.FindAsync(loggedInUserId);
            if (userToUpdate == null)
            {
                HttpContext.Session.Clear();
                return Unauthorized(new { Message = "Oturumunuzla ilişkili kullanıcı bulunamadı." });
            }

            // Sadece Email kontrolü
            bool emailExists = await _context.Users.AnyAsync(u => u.Email == model.Email && u.Id != loggedInUserId);
            if (emailExists)
            {
                ModelState.AddModelError(nameof(model.Email), "Bu e-posta adresi zaten başka bir kullanıcı tarafından kullanılıyor.");
                return ValidationProblem(ModelState);
            }

            userToUpdate.Email = model.Email; // Sadece Email güncelleniyor
            // Username güncellemesi YOK

            try { await _context.SaveChangesAsync(); }
            catch (DbUpdateConcurrencyException) { throw; }

            var updatedUserViewModel = new UserViewModel
            {
                Id = userToUpdate.Id,
                Username = userToUpdate.Username, // Kullanıcı adı değişmedi
                Email = userToUpdate.Email
            };
            return Ok(updatedUserViewModel);
        }

        // UploadProfilePicture metodu burada YOKTU.
    }
}