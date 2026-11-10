using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using SmartFridgeBackend.DTOs.Recipes;

namespace SmartFridgeBackend.Services;

public class OpenAiRecipeGenerationService : IRecipeGenerationService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    public OpenAiRecipeGenerationService(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }

    public async Task<List<GeneratedRecipeResponse>> GenerateRecipesAsync(GenerateRecipesRequest request)
    {
        var apiKey = _configuration["OpenAI:ApiKey"];
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new InvalidOperationException("OpenAI API key is missing.");
        }

        var client = _httpClientFactory.CreateClient("openai");
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", apiKey);

        var userPrompt = // Hmmmmmm. Vi skal have lavet "rigtige" opskrifter og der skal også være ingredienser der ikke er tilgængelige.
        $@"
        Du er en dansk opskriftsassistent.

        Brugerens ingredienser:
        {string.Join(", ", request.Ingredients)}

        Lav 10 realistiske og forskellige hverdagsopskrifter.

        Krav:
        - Opskrifterne skal være realistiske retter.
        - Alle opskrifter skal være forskellige.
        - Ingen dubletter.
        - Brug varierede retstyper.
        - Hver opskrift skal have:
        - title
        - servings
        - time_minutes
        - ingredients
        - steps
        - Alle ingredients skal have:
        - name
        - amount
        - unit
        - Unit må aldrig være tom.
        - Hver opskrift skal have mindst 3 steps.

        Returnér kun JSON der matcher schema.
        ";
        //Der SKAL være 3 opskrifter hvor der KUN bruges nogle eller alle ingredienser der er beskrevet og 7 hvor der skal være 1 til 5 ingredienser ud over de beskrevede. 

        var payload = new
        {
            model = "gpt-4.1-mini", // alternatives: "gpt-4o-mini" // gpt-4.1 // gpt-4o // o4-mini
            temperature = 0.4, // For more stabile answers.
            input = new object[]
            {
                new { role = "user", content = userPrompt }
            },
            text = new
            {
                format = new
                {
                    type = "json_schema",
                    name = "recipes_schema",
                    strict = true,
                    schema = new
                    {
                        type = "object",
                        additionalProperties = false,
                        required = new[] { "recipes" },
                        properties = new
                        {
                            recipes = new
                            {
                                type = "array",
                                minItems = 10,
                                maxItems = 10,
                                items = new
                                {
                                    type = "object",
                                    additionalProperties = false,
                                    required = new[] { "title", "servings", "time_minutes", "ingredients", "steps" },
                                    properties = new
                                    {
                                        title = new { type = "string" },
                                        servings = new { type = "integer", minimum = 1, maximum = 12 },
                                        time_minutes = new { type = "integer", minimum = 1, maximum = 300 },
                                        ingredients = new
                                        {
                                            type = "array",
                                            minItems = 1,
                                            items = new
                                            {
                                                type = "object",
                                                additionalProperties = false,
                                                required = new[] { "name", "amount", "unit" },
                                                properties = new
                                                {
                                                    name = new { type = "string" },
                                                    amount = new { type = "number" },
                                                    unit = new { type = "string" }
                                                }
                                            }
                                        },
                                        steps = new
                                        {
                                            type = "array",
                                            minItems = 3,
                                            items = new { type = "string" }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        };

        var json = JsonSerializer.Serialize(payload);
        string responseBody;

        try
        {
            using var httpContent = new StringContent(json, Encoding.UTF8, "application/json");

            using var response = await client.PostAsync("responses", httpContent);
            responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new ApplicationException(
                    $"OpenAI API error: {(int)response.StatusCode} {response.ReasonPhrase}\n{responseBody}");
            }
        }
        catch (TaskCanceledException)
        {
            throw new ApplicationException(
                "AI-opskriftsgenerering tog for lang tid. Prøv igen med færre ingredienser.");
        }
        catch (HttpRequestException ex)
        {
            throw new ApplicationException(
                $"Fejl ved forbindelse til OpenAI API: {ex.Message}");
        }


        using var document = JsonDocument.Parse(responseBody);

        var outputText = ExtractFirstOutputText(document.RootElement);
        if (outputText is null)
        {
            throw new ApplicationException("Could not find output_text in OpenAI response.");
        }

        var parsed = JsonSerializer.Deserialize<RecipesWrapper>(outputText, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (parsed?.Recipes is null || parsed.Recipes.Count != 10)
        {
            throw new ApplicationException("Model did not return exactly 10 recipes.");
        }

        var recipes = parsed.Recipes.Select(r => new GeneratedRecipeResponse
        {
            Title = r.Title,
            Servings = r.Servings,
            TimeMinutes = r.Time_Minutes,
            Ingredients = r.Ingredients.Select(i => new GeneratedIngredientResponse
            {
                Name = i.Name,
                Amount = i.Amount,
                Unit = i.Unit
            }).ToList(),
            Steps = r.Steps
        }).ToList();

        var userIngredients = request.Ingredients
            .Select(i => i.ToLower().Trim())
            .ToHashSet();

        foreach (var recipe in recipes)
        {
            var missingIngredients = recipe.Ingredients
                .Where(i => !userIngredients.Contains(i.Name.ToLower().Trim()))
                .Count();

            recipe.MissingIngredients = missingIngredients;
        }
        return recipes
            .OrderBy(r => r.MissingIngredients) // Order the recipes by missing ingredients. 0 missing first and so on.
            .ThenBy(r => r.TimeMinutes) // If the same amount of missing ingredients, the least amount of time for making it is first. 1. 25 min - 2. 30 min and so on.
            .ToList();
    }

    private static string? ExtractFirstOutputText(JsonElement root)
    {
        if (!root.TryGetProperty("output", out var output) || output.ValueKind != JsonValueKind.Array)
            return null;

        foreach (var item in output.EnumerateArray())
        {
            if (!item.TryGetProperty("content", out var content) || content.ValueKind != JsonValueKind.Array)
                continue;

            foreach (var c in content.EnumerateArray())
            {
                if (c.TryGetProperty("type", out var type) &&
                    type.GetString() == "output_text" &&
                    c.TryGetProperty("text", out var text))
                {
                    return text.GetString();
                }
            }
        }

        return null;
    }

    private class RecipesWrapper
    {
        public List<RecipeDto> Recipes { get; set; } = new();
    }

    private class RecipeDto
    {
        public string Title { get; set; } = "";
        public int Servings { get; set; }
        public int Time_Minutes { get; set; }
        public List<IngredientDto> Ingredients { get; set; } = new();
        public List<string> Steps { get; set; } = new();
    }

    private class IngredientDto
    {
        public string Name { get; set; } = "";
        public double Amount { get; set; }
        public string Unit { get; set; } = "";
    }
}