using SmartFridgeBackend.Database.Models;

namespace SmartFridgeBackend.Database;

// Indeholder testdata til databasen
public static class SeedData
{
    public static List<User> Users = new()
    {
        new User
        {
            UserId = null, // MongoDB genererer ID
            Email = "test1@example.com",
            AuthId = "firebase-uid-001",
        },
        new User
        {
            UserId = null,
            Email = "test2@example.com",
            AuthId = "firebase-uid-002",
        }
    };

    public static List<Item> Items = new()
    {
        new Item
        {
            ItemId = null,       // MongoDB genererer ID automatisk
            UserId = null,       // Bliver sat når varen tilknyttes en bruger
            Name = "Mælk",
            Barcode = "1234567890123"
        },
        new Item
        {
            ItemId = null,
            UserId = null,
            Name = "Æg",
            Barcode = null       // Ingen stregkode
        }
    };

    public static List<Recipe> Recipes = new()
    {
        new Recipe
        {
            RecipeId = null,          // MongoDB genererer ID automatisk
            Title = "Pasta med tomat",
            Ingredients = new List<RecipeIngredient>
            {
                new RecipeIngredient {Name = "Pasta", Amount = 200, Unit = "g" },
                new RecipeIngredient { Name = "Tomat", Amount = 2, Unit = "stk" },
                new RecipeIngredient { Name = "Salt", Amount = 1, Unit = "tsk" }
            },
            Steps = new List<string>
            {
                "Kog pasta",
                "Hak tomater",
                "Bland og server"
            }
        },
        new Recipe
        {
            RecipeId = null,
            Title = "Røræg",
            Ingredients = new List<RecipeIngredient>
            {
                new RecipeIngredient { Name = "Æg", Amount = 3, Unit = "stk" },
                new RecipeIngredient { Name = "Smør", Amount = 10, Unit = "g" },
                new RecipeIngredient { Name = "Salt", Amount = 1, Unit = "knivspids" }
            },
            Steps = new List<string>
            {
                "Pisk æg",
                "Smelt smør",
                "Steg æggene langsomt"
            }
        }
    };
}
