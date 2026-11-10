import { describe, it, expect, beforeAll } from "vitest";
import { loginUser } from "../services/authService";
import { saveAuth } from "../services/storageService";
import {
  getFavoriteRecipes,
  saveFavoriteRecipe,
  deleteFavoriteRecipe,
} from "../services/recipeService";


const TEST_EMAIL = process.env.TEST_EMAIL;
const TEST_PASSWORD = process.env.TEST_PASSWORD;


describe("Frontend-backend integration - Recipes", () => {
  beforeAll(async () => {
    const result = await loginUser(TEST_EMAIL, TEST_PASSWORD);
    saveAuth(result.token, TEST_EMAIL);
  });

  it("kan hente favoritopskrifter fra rigtig backend", async () => {
    const favorites = await getFavoriteRecipes();
    expect(Array.isArray(favorites)).toBe(true);
  });

  it("kan gemme og slette en favoritopskrift via rigtig backend", async () => {
    const uniqueTitle = `Integration Test Opskrift ${Date.now()}`;

    await saveFavoriteRecipe({
      title: uniqueTitle,
      servings: 2,
      timeMinutes: 15,
      ingredients: [{ name: "Pasta", amount: 200, unit: "g" }],
      steps: ["Kog pasta", "Server"],
    });

    const favorites = await getFavoriteRecipes();
    const savedFavorite = favorites.find((fav) => fav.title === uniqueTitle);

    expect(savedFavorite).toBeTruthy();

    const favoriteId =
      savedFavorite?.recipeId ?? savedFavorite?.id ?? savedFavorite?.favoriteId;

    expect(favoriteId).toBeTruthy();

    await deleteFavoriteRecipe(favoriteId);

    const updatedFavorites = await getFavoriteRecipes();
    const deletedFavorite = updatedFavorites.find(
      (fav) => fav.title === uniqueTitle
    );

    expect(deletedFavorite).toBeUndefined();
  });
});
