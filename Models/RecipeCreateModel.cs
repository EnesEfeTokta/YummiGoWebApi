// Models/RecipeCreateModel.cs
using System.ComponentModel.DataAnnotations;
namespace YummiGoWebApi.Models
{
    // Yeni tarif eklemek/güncellemek için API'ye gönderilen veri
    public class RecipeCreateModel
    {
        [Required(ErrorMessage = "Başlık zorunludur.")]
        [StringLength(150)]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Açıklama zorunludur.")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Malzemeler zorunludur.")]
        public string Ingredients { get; set; } = string.Empty; // '\n' ile ayrılmış

        [Required(ErrorMessage = "Adımlar zorunludur.")]
        public string Steps { get; set; } = string.Empty;       // '\n' ile ayrılmış

        [Required(ErrorMessage = "Kategori zorunludur.")]
        [StringLength(50)]
        public string Category { get; set; } = string.Empty;

        [Required(ErrorMessage = "Görsel URL'si zorunludur.")]
        [Url]
        public string ImageUrl { get; set; } = string.Empty;

        [Url]
        public string? VideoUrl { get; set; }

        [Range(0, int.MaxValue)]
        public int? Calories { get; set; }

        [Range(0, int.MaxValue)]
        public int? Protein { get; set; }

        [Range(0, int.MaxValue)]
        public int? Carbs { get; set; }

        [Range(0, int.MaxValue)]
        public int? Fat { get; set; }

        [Range(0, int.MaxValue)]
        public int? CookingTimeInMinutes { get; set; }
        // UserId burada olmaz, session'dan alınır.
    }
}