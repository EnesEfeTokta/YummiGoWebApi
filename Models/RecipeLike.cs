// Models/RecipeLike.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YummiGoWebApi.Models
{
    [Table("RecipeLikes")]
    public class RecipeLike
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        public int RecipeId { get; set; }

        [ForeignKey("UserId")]
        public virtual User? User { get; set; }

        [ForeignKey("RecipeId")]
        public virtual Recipe? Recipe { get; set; }
    }
}