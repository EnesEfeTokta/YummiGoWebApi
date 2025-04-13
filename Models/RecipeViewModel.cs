// Models/RecipeViewModel.cs
using System.Collections.Generic;
namespace YummiGoWebApi.Models
{
    public class RecipeViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<string> Ingredients { get; set; } = new List<string>();
        public List<string> Steps { get; set; } = new List<string>();
        public string Category { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public string? VideoUrl { get; set; }
        public int? Calories { get; set; }
        public int? Protein { get; set; }
        public int? Carbs { get; set; }
        public int? Fat { get; set; }
        public int? CookingTimeInMinutes { get; set; }

        // YENİ EKLENEN ALANLAR
        public int LikeCount { get; set; } = 0; // Beğeni sayısı
        public bool IsLikedByCurrentUser { get; set; } = false; // O anki kullanıcı beğendi mi?
    }
}