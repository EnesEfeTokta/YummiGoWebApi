// Models/Recipe.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YummiGoWebApi.Models
{
    [Table("Recipes")]
    public class Recipe
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Başlık zorunludur.")]
        [StringLength(150)]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Açıklama zorunludur.")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Malzemeler zorunludur.")]
        public string Ingredients { get; set; } = string.Empty; // '\n' ile ayrılmış

        [Required(ErrorMessage = "Adımlar zorunludur.")]
        public string Steps { get; set; } = string.Empty; // '\n' ile ayrılmış

        [Required(ErrorMessage = "Kategori zorunludur.")]
        [StringLength(50)]
        public string Category { get; set; } = string.Empty;

        [Required(ErrorMessage = "Görsel URL'si zorunludur.")]
        [Url]
        public string ImageUrl { get; set; } = string.Empty;

        [Url]
        public string? VideoUrl { get; set; } // Nullable

        [Range(0, int.MaxValue)]
        public int? Calories { get; set; } // Nullable

        [Range(0, int.MaxValue)]
        public int? Protein { get; set; } // Nullable

        [Range(0, int.MaxValue)]
        public int? Carbs { get; set; } // Nullable

        [Range(0, int.MaxValue)]
        public int? Fat { get; set; } // Nullable

        [Range(0, int.MaxValue)]
        public int? CookingTimeInMinutes { get; set; } // Nullable

        // İlişki
        [Required]
        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual User? User { get; set; } // Nullable Navigation Property
    }
}