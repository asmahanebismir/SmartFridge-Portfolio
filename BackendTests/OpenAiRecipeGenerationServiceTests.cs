using System.Net;
using System.Text;
using Microsoft.Extensions.Configuration;
using Moq;
using SmartFridgeBackend.DTOs.Recipes;
using SmartFridgeBackend.Services;

namespace SmartFridgeBackend.BackendTests;

public class OpenAiRecipeGenerationServiceTests
{
    private readonly Mock<IHttpClientFactory> _httpClientFactoryMock;
    private readonly Mock<IConfiguration> _configurationMock;

    public OpenAiRecipeGenerationServiceTests()
    {
        _httpClientFactoryMock = new Mock<IHttpClientFactory>();
        _configurationMock = new Mock<IConfiguration>();
    }

    [Fact]
    public async Task GenerateRecipesAsync_MissingApiKey_ThrowsInvalidOperationException()
    {
        // Arrange
        _configurationMock
            .Setup(c => c["OpenAI:ApiKey"])
            .Returns((string?)null);

        var service = new OpenAiRecipeGenerationService(
            _httpClientFactoryMock.Object,
            _configurationMock.Object
        );

        var request = new GenerateRecipesRequest
        {
            Ingredients = new List<string> { "Eggs", "Milk" }
        };

        // Act + Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.GenerateRecipesAsync(request));
    }

    [Fact]
    public async Task GenerateRecipesAsync_OpenAiReturnsError_ThrowsApplicationException()
    {
        // Arrange
        _configurationMock
            .Setup(c => c["OpenAI:ApiKey"])
            .Returns("fake-api-key");

        var handler = new FakeHttpMessageHandler(_ =>
            new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                Content = new StringContent("bad request", Encoding.UTF8, "application/json")
            });

        var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://api.openai.com/v1/")
        };

        _httpClientFactoryMock
            .Setup(f => f.CreateClient("openai"))
            .Returns(httpClient);

        var service = new OpenAiRecipeGenerationService(
            _httpClientFactoryMock.Object,
            _configurationMock.Object
        );

        var request = new GenerateRecipesRequest
        {
            Ingredients = new List<string> { "Eggs", "Milk" }
        };

        // Act + Assert
        var exception = await Assert.ThrowsAsync<ApplicationException>(() =>
            service.GenerateRecipesAsync(request));

        Assert.Contains("OpenAI API error", exception.Message);
    }

    [Fact]
    public async Task GenerateRecipesAsync_ResponseWithoutOutputText_ThrowsApplicationException()
    {
        // Arrange
        _configurationMock
            .Setup(c => c["OpenAI:ApiKey"])
            .Returns("fake-api-key");

        var responseJson = """
        {
          "output": [
            {
              "content": [
                {
                  "type": "not_output_text",
                  "text": "ignored"
                }
              ]
            }
          ]
        }
        """;

        var handler = new FakeHttpMessageHandler(_ =>
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(responseJson, Encoding.UTF8, "application/json")
            });

        var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://api.openai.com/v1/")
        };

        _httpClientFactoryMock
            .Setup(f => f.CreateClient("openai"))
            .Returns(httpClient);

        var service = new OpenAiRecipeGenerationService(
            _httpClientFactoryMock.Object,
            _configurationMock.Object
        );

        var request = new GenerateRecipesRequest
        {
            Ingredients = new List<string> { "Eggs", "Milk" }
        };

        // Act + Assert
        var exception = await Assert.ThrowsAsync<ApplicationException>(() =>
            service.GenerateRecipesAsync(request));

        Assert.Contains("Could not find output_text", exception.Message);
    }

    [Fact]
    public async Task GenerateRecipesAsync_ResponseWithWrongRecipeCount_ThrowsApplicationException()
    {
        // Arrange
        _configurationMock
            .Setup(c => c["OpenAI:ApiKey"])
            .Returns("fake-api-key");

        var innerJson = """
        {
          "recipes": [
            {
              "title": "Recipe 1",
              "servings": 2,
              "time_minutes": 15,
              "ingredients": [
                { "name": "Eggs", "amount": 2, "unit": "pcs" }
              ],
              "steps": ["Step 1", "Step 2", "Step 3"]
            }
          ]
        }
        """;

        var escapedInnerJson = innerJson.Replace("\"", "\\\"").Replace("\r", "").Replace("\n", "");

        var responseJson = $$"""
        {
          "output": [
            {
              "content": [
                {
                  "type": "output_text",
                  "text": "{{escapedInnerJson}}"
                }
              ]
            }
          ]
        }
        """;

        var handler = new FakeHttpMessageHandler(_ =>
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(responseJson, Encoding.UTF8, "application/json")
            });

        var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://api.openai.com/v1/")
        };

        _httpClientFactoryMock
            .Setup(f => f.CreateClient("openai"))
            .Returns(httpClient);

        var service = new OpenAiRecipeGenerationService(
            _httpClientFactoryMock.Object,
            _configurationMock.Object
        );

        var request = new GenerateRecipesRequest
        {
            Ingredients = new List<string> { "Eggs", "Milk" }
        };

        // Act + Assert
        var exception = await Assert.ThrowsAsync<ApplicationException>(() =>
            service.GenerateRecipesAsync(request));

        Assert.Contains("exactly 10 recipes", exception.Message);
    }

    [Fact]
    public async Task GenerateRecipesAsync_ValidResponse_ReturnsMappedRecipes()
    {
        // Arrange
        _configurationMock
            .Setup(c => c["OpenAI:ApiKey"])
            .Returns("fake-api-key");

        var recipesJson = """
        {
          "recipes": [
            {
              "title": "Recipe 1",
              "servings": 2,
              "time_minutes": 15,
              "ingredients": [
                { "name": "Eggs", "amount": 2, "unit": "pcs" }
              ],
              "steps": ["Step 1", "Step 2", "Step 3"]
            },
            {
              "title": "Recipe 2",
              "servings": 3,
              "time_minutes": 20,
              "ingredients": [
                { "name": "Milk", "amount": 1, "unit": "cup" }
              ],
              "steps": ["Step 1", "Step 2", "Step 3"]
            },
            {
              "title": "Recipe 3",
              "servings": 4,
              "time_minutes": 25,
              "ingredients": [
                { "name": "Flour", "amount": 100, "unit": "g" }
              ],
              "steps": ["Step 1", "Step 2", "Step 3"]
            },
            {
              "title": "Recipe 4",
              "servings": 2,
              "time_minutes": 30,
              "ingredients": [
                { "name": "Butter", "amount": 50, "unit": "g" }
              ],
              "steps": ["Step 1", "Step 2", "Step 3"]
            },
            {
              "title": "Recipe 5",
              "servings": 5,
              "time_minutes": 35,
              "ingredients": [
                { "name": "Cheese", "amount": 150, "unit": "g" }
              ],
              "steps": ["Step 1", "Step 2", "Step 3"]
            },
            {
              "title": "Recipe 6",
              "servings": 1,
              "time_minutes": 12,
              "ingredients": [
                { "name": "Tomato", "amount": 2, "unit": "pcs" }
              ],
              "steps": ["Step 1", "Step 2", "Step 3"]
            },
            {
              "title": "Recipe 7",
              "servings": 6,
              "time_minutes": 40,
              "ingredients": [
                { "name": "Chicken", "amount": 300, "unit": "g" }
              ],
              "steps": ["Step 1", "Step 2", "Step 3"]
            },
            {
              "title": "Recipe 8",
              "servings": 2,
              "time_minutes": 18,
              "ingredients": [
                { "name": "Rice", "amount": 200, "unit": "g" }
              ],
              "steps": ["Step 1", "Step 2", "Step 3"]
            },
            {
              "title": "Recipe 9",
              "servings": 3,
              "time_minutes": 22,
              "ingredients": [
                { "name": "Beans", "amount": 100, "unit": "g" }
              ],
              "steps": ["Step 1", "Step 2", "Step 3"]
            },
            {
              "title": "Recipe 10",
              "servings": 4,
              "time_minutes": 28,
              "ingredients": [
                { "name": "Potato", "amount": 3, "unit": "pcs" }
              ],
              "steps": ["Step 1", "Step 2", "Step 3"]
            }
          ]
        }
        """;

        var escapedRecipesJson = recipesJson.Replace("\"", "\\\"").Replace("\r", "").Replace("\n", "");

        var responseJson = $$"""
        {
          "output": [
            {
              "content": [
                {
                  "type": "output_text",
                  "text": "{{escapedRecipesJson}}"
                }
              ]
            }
          ]
        }
        """;

        var handler = new FakeHttpMessageHandler(_ =>
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(responseJson, Encoding.UTF8, "application/json")
            });

        var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://api.openai.com/v1/")
        };

        _httpClientFactoryMock
            .Setup(f => f.CreateClient("openai"))
            .Returns(httpClient);

        var service = new OpenAiRecipeGenerationService(
            _httpClientFactoryMock.Object,
            _configurationMock.Object
        );

        var request = new GenerateRecipesRequest
        {
            Ingredients = new List<string> { "Eggs", "Milk" }
        };

        // Act
        var result = await service.GenerateRecipesAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(10, result.Count);
        Assert.Equal("Recipe 1", result[0].Title);
        Assert.Equal(2, result[0].Servings);
        Assert.Equal(15, result[0].TimeMinutes);
        Assert.Single(result[0].Ingredients);
        Assert.Equal("Eggs", result[0].Ingredients[0].Name);
        Assert.Equal(2, result[0].Ingredients[0].Amount);
        Assert.Equal("pcs", result[0].Ingredients[0].Unit);
        Assert.Equal(3, result[0].Steps.Count);
    }

    private class FakeHttpMessageHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> _handler;

        public FakeHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> handler)
        {
            _handler = handler;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(_handler(request));
        }
    }
}