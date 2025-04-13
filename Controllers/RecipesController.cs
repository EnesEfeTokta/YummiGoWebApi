// Controllers/RecipesController.cs
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using YummiGoWebApi.Data;
using YummiGoWebApi.Models;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Threading.Tasks;

namespace YummiGoWebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RecipesController : ControllerBase
    {
        private readonly DataContext _context;

        public RecipesController(DataContext context)
        {
            _context = context;
        }

        // GET: /api/recipes?category=&search=&searchField=&sortBy=&limit=&ingredients=
        [HttpGet]
        public async Task<ActionResult<IEnumerable<RecipeViewModel>>> GetRecipes(
            [FromQuery] string? category = null,
            [FromQuery] string? search = null,
            [FromQuery] string? searchField = null, // "title", "ingredients", "description", "category", "all"
            [FromQuery] string sortBy = "date_desc",  // "date_desc", "date_asc", "title_asc", "title_desc", "likes_desc"
            [FromQuery] List<string>? ingredients = null,
            [FromQuery] int? limit = null)
        {
            int? loggedInUserId = GetLoggedInUserId();
            var query = _context.Recipes.AsQueryable();

            // Kategori filtresi
            if (!string.IsNullOrWhiteSpace(category))
            {
                query = query.Where(r => r.Category.ToLower() == category.ToLower());
            }

            // Arama filtresi
            if (!string.IsNullOrWhiteSpace(search))
            {
                string searchTerm = search.ToLower().Trim();
                string field = searchField?.ToLowerInvariant() ?? "all";

                switch (field)
                {
                    case "title":
                        query = query.Where(r => r.Title.ToLower().Contains(searchTerm));
                        break;
                    case "ingredients":
                        query = query.Where(r => r.Ingredients.ToLower().Contains(searchTerm));
                        break;
                    case "description":
                        query = query.Where(r => r.Description.ToLower().Contains(searchTerm));
                        break;
                    case "category":
                        query = query.Where(r => r.Category.ToLower().Contains(searchTerm));
                        break;
                    default:
                        query = query.Where(r =>
                            r.Title.ToLower().Contains(searchTerm) ||
                            r.Description.ToLower().Contains(searchTerm) ||
                            r.Ingredients.ToLower().Contains(searchTerm) ||
                            r.Category.ToLower().Contains(searchTerm));
                        break;
                }
            }

            // Malzeme filtresi (liste içindeki tüm malzemeleri içermeli)
            if (ingredients != null && ingredients.Count > 0)
            {
                var requiredIngredients = ingredients.Select(i => i.ToLower().Trim())
                                                     .Where(i => !string.IsNullOrEmpty(i))
                                                     .ToList();
                foreach (var reqIng in requiredIngredients)
                {
                    query = query.Where(r => r.Ingredients.ToLower().Contains(reqIng));
                }
            }

            // Sıralama
            switch (sortBy?.ToLowerInvariant())
            {
                case "title_asc":
                    query = query.OrderBy(r => r.Title);
                    break;
                case "title_desc":
                    query = query.OrderByDescending(r => r.Title);
                    break;
                case "date_asc":
                    query = query.OrderBy(r => r.Id);
                    break;
                default:
                    query = query.OrderByDescending(r => r.Id);
                    break;
            }

            // Basit limit (sayfalama yerine)
            if (limit.HasValue && limit > 0)
            {
                query = query.Take(limit.Value);
            }

            var recipesFromDb = await query.ToListAsync();
            var recipeViewModels = new List<RecipeViewModel>();
            foreach (var recipe in recipesFromDb)
            {
                recipeViewModels.Add(await MapRecipeToViewModelAsync(recipe, loggedInUserId));
            }

            return Ok(recipeViewModels);
        }

        // GET: /api/recipes/{id}
        [HttpGet("{id}", Name = "GetRecipe")]
        public async Task<ActionResult<RecipeViewModel>> GetRecipe(int id)
        {
            int? loggedInUserId = GetLoggedInUserId();
            var recipe = await _context.Recipes.FirstOrDefaultAsync(r => r.Id == id);
            if (recipe == null)
            {
                return NotFound(new { Message = $"ID {id} olan tarif bulunamadı." });
            }

            var recipeViewModel = await MapRecipeToViewModelAsync(recipe, loggedInUserId);
            return Ok(recipeViewModel);
        }

        // POST: /api/recipes
        [HttpPost]
        public async Task<ActionResult<RecipeViewModel>> CreateRecipe([FromBody] RecipeCreateModel recipeModel)
        {
            int? userId = GetLoggedInUserId();
            if (!userId.HasValue)
                return Unauthorized(new { Message = "Tarif eklemek için giriş yapmalısınız." });

            var newRecipe = new Recipe
            {
                Title = recipeModel.Title,
                Description = recipeModel.Description,
                Ingredients = recipeModel.Ingredients,
                Steps = recipeModel.Steps,
                Category = recipeModel.Category,
                ImageUrl = recipeModel.ImageUrl,
                VideoUrl = recipeModel.VideoUrl,
                Calories = recipeModel.Calories,
                Protein = recipeModel.Protein,
                Carbs = recipeModel.Carbs,
                Fat = recipeModel.Fat,
                CookingTimeInMinutes = recipeModel.CookingTimeInMinutes,
                UserId = userId.Value
            };

            _context.Recipes.Add(newRecipe);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                Console.WriteLine($"Kayıt Hatası: {ex.InnerException?.Message ?? ex.Message}");
                if (ex.InnerException is Npgsql.PostgresException pgEx && pgEx.SqlState == "23505")
                {
                    return StatusCode(StatusCodes.Status500InternalServerError,
                        new { Message = "Kayıt eklenirken birincil anahtar çakışması.", Detail = pgEx.Message });
                }
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { Message = "Tarif kaydedilirken veritabanı hatası oluştu.", Detail = ex.InnerException?.Message });
            }

            var recipeViewModel = await MapRecipeToViewModelAsync(newRecipe, userId);
            return CreatedAtAction(nameof(GetRecipe), new { id = newRecipe.Id }, recipeViewModel);
        }

        // PUT: /api/recipes/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRecipe(int id, [FromBody] RecipeCreateModel recipeUpdateModel)
        {
            int? loggedInUserId = GetLoggedInUserId();
            if (!loggedInUserId.HasValue)
                return Unauthorized(new { Message = "Giriş yapmanız gerekli." });

            var recipeToUpdate = await _context.Recipes.FindAsync(id);
            if (recipeToUpdate == null)
                return NotFound(new { Message = "Güncellenecek tarif bulunamadı." });

            if (recipeToUpdate.UserId != loggedInUserId.Value)
                return Forbid();

            recipeToUpdate.Title = recipeUpdateModel.Title;
            recipeToUpdate.Description = recipeUpdateModel.Description;
            recipeToUpdate.Ingredients = recipeUpdateModel.Ingredients;
            recipeToUpdate.Steps = recipeUpdateModel.Steps;
            recipeToUpdate.Category = recipeUpdateModel.Category;
            recipeToUpdate.ImageUrl = recipeUpdateModel.ImageUrl;
            recipeToUpdate.VideoUrl = recipeUpdateModel.VideoUrl;
            recipeToUpdate.Calories = recipeUpdateModel.Calories;
            recipeToUpdate.Protein = recipeUpdateModel.Protein;
            recipeToUpdate.Carbs = recipeUpdateModel.Carbs;
            recipeToUpdate.Fat = recipeUpdateModel.Fat;
            recipeToUpdate.CookingTimeInMinutes = recipeUpdateModel.CookingTimeInMinutes;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!RecipeExists(id))
                    return NotFound();
                else
                    throw;
            }
            catch (DbUpdateException ex)
            {
                Console.WriteLine($"Güncelleme Hatası: {ex.InnerException?.Message ?? ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { Message = "Tarif güncellenirken veritabanı hatası oluştu.", Detail = ex.InnerException?.Message });
            }

            return NoContent();
        }

        // DELETE: /api/recipes/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRecipe(int id)
        {
            int? loggedInUserId = GetLoggedInUserId();
            if (!loggedInUserId.HasValue)
                return Unauthorized(new { Message = "Giriş yapmalısınız." });

            var recipeToDelete = await _context.Recipes.FindAsync(id);
            if (recipeToDelete == null)
                return NotFound(new { Message = "Silinecek tarif bulunamadı." });

            if (recipeToDelete.UserId != loggedInUserId.Value)
                return Forbid();

            _context.Recipes.Remove(recipeToDelete);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                Console.WriteLine($"Silme Hatası: {ex.InnerException?.Message ?? ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { Message = "Tarif silinirken veritabanı hatası oluştu.", Detail = ex.InnerException?.Message });
            }
            return NoContent();
        }

        // POST: /api/recipes/{id}/like
        [HttpPost("{id}/like")]
        public async Task<IActionResult> LikeRecipe(int id)
        {
            int? loggedInUserId = GetLoggedInUserId();
            if (!loggedInUserId.HasValue)
                return Unauthorized(new { Message = "Beğenmek için giriş yapmalısınız." });

            if (!await _context.Recipes.AnyAsync(r => r.Id == id))
                return NotFound(new { Message = "Beğenilecek tarif bulunamadı." });

            bool alreadyLiked = await _context.RecipeLikes.AnyAsync(rl => rl.RecipeId == id && rl.UserId == loggedInUserId.Value);
            if (alreadyLiked)
                return Ok(new { Message = "Tarif zaten beğenildi." });

            var newLike = new RecipeLike { RecipeId = id, UserId = loggedInUserId.Value };
            _context.RecipeLikes.Add(newLike);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                Console.WriteLine($"Like Hatası: {ex}");
                if (ex.InnerException is Npgsql.PostgresException pgEx && pgEx.SqlState == "23505")
                    return Ok(new { Message = "Tarif zaten beğenildi (eş zamanlı istek)." });
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { Message = "Beğenme işlemi sırasında hata oluştu." });
            }
            return Ok(new { Message = "Tarif beğenildi." });
        }

        // DELETE: /api/recipes/{id}/like
        [HttpDelete("{id}/like")]
        public async Task<IActionResult> UnlikeRecipe(int id)
        {
            int? loggedInUserId = GetLoggedInUserId();
            if (!loggedInUserId.HasValue)
                return Unauthorized(new { Message = "Giriş yapmalısınız." });

            var likeToDelete = await _context.RecipeLikes.FirstOrDefaultAsync(rl => rl.RecipeId == id && rl.UserId == loggedInUserId.Value);
            if (likeToDelete == null)
                return Ok(new { Message = "Tarif zaten beğenilmemiş." });

            _context.RecipeLikes.Remove(likeToDelete);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                Console.WriteLine($"Unlike Hatası: {ex}");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { Message = "Beğeni geri alınırken hata oluştu." });
            }
            return Ok(new { Message = "Tarif beğenisi geri alındı." });
        }

        // GET: /api/recipes/categories
        [HttpGet("categories")]
        public async Task<ActionResult<IEnumerable<string>>> GetCategories()
        {
            try
            {
                var categories = await _context.Recipes
                    .Select(r => r.Category)
                    .Distinct()
                    .OrderBy(c => c)
                    .ToListAsync();
                return Ok(categories);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Kategori Hatası: {ex}");
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "Kategoriler alınırken hata oluştu." });
            }
        }

        // Yardımcı metotlar
        private bool RecipeExists(int id) => _context.Recipes.Any(e => e.Id == id);

        private int? GetLoggedInUserId()
        {
            if (HttpContext?.Session == null)
                return null;
            string? userIdString = HttpContext.Session.GetString("UserId");
            return int.TryParse(userIdString, out int userId) ? userId : null;
        }

        private async Task<RecipeViewModel> MapRecipeToViewModelAsync(Recipe recipe, int? loggedInUserId)
        {
            int likeCount = await _context.RecipeLikes.CountAsync(rl => rl.RecipeId == recipe.Id);
            bool isLiked = loggedInUserId.HasValue && await _context.RecipeLikes.AnyAsync(rl => rl.RecipeId == recipe.Id && rl.UserId == loggedInUserId.Value);

            return new RecipeViewModel
            {
                Id = recipe.Id,
                Title = recipe.Title,
                Description = recipe.Description,
                Ingredients = recipe.Ingredients?.Split('\n', StringSplitOptions.RemoveEmptyEntries).ToList() ?? new List<string>(),
                Steps = recipe.Steps?.Split('\n', StringSplitOptions.RemoveEmptyEntries).ToList() ?? new List<string>(),
                Category = recipe.Category,
                ImageUrl = recipe.ImageUrl,
                VideoUrl = recipe.VideoUrl,
                Calories = recipe.Calories,
                Protein = recipe.Protein,
                Carbs = recipe.Carbs,
                Fat = recipe.Fat,
                CookingTimeInMinutes = recipe.CookingTimeInMinutes,
                LikeCount = likeCount,
                IsLikedByCurrentUser = isLiked
            };
        }
    }
}
