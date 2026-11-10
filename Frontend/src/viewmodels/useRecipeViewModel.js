import { useMemo, useState } from "react";
import {
  generateRecipes,
  getFavoriteRecipes,
  saveFavoriteRecipe,
  deleteFavoriteRecipe,
} from "../services/recipeService";

function getRecipeColor(missingCount) {
  if (missingCount === 0) return "#d1fae5"; // grøn
  if (missingCount <= 2) return "#fef3c7"; // gul
  return "#fee2e2"; // rød
}

function normalizeText(text) {
  return (text || "").toLowerCase().trim();
}

function calculateMissingCount(recipeIngredients, availableIngredients) {
  const available = availableIngredients.map(normalizeText);

  const missingIngredients = (recipeIngredients || []).filter((ingredient) => {
    const ingredientName = normalizeText(ingredient.name);

    const exists = available.some(
      (item) =>
        ingredientName.includes(item) || item.includes(ingredientName)
    );

    return !exists;
  });

  return missingIngredients.length;
}

export function useRecipeViewModel() {
  const [recipes, setRecipes] = useState([]);
  const [favorites, setFavorites] = useState([]);
  const [selectedRecipe, setSelectedRecipe] = useState(null);
  const [loading, setLoading] = useState(false);
  const [favoritesLoading, setFavoritesLoading] = useState(false);
  const [error, setError] = useState("");

  async function loadFavorites() {
    try {
      setFavoritesLoading(true);
      setError("");
      const data = await getFavoriteRecipes();
      setFavorites(data);
    } catch (err) {
      console.error(err);
      setError("Kunne ikke hente favoritter");
    } finally {
      setFavoritesLoading(false);
    }
  }

  async function generateRecipeList({ ingredients, preferences, maxMinutes }) {
    try {
      setLoading(true);
      setError("");

      const data = await generateRecipes({
        ingredients,
        preferences,
        maxMinutes,
      });

      const recipesWithColor = data.map((recipe) => {
        const missingCount = calculateMissingCount(
          recipe.ingredients,
          ingredients
        );

        return {
          ...recipe,
          missingCount,
          color: getRecipeColor(missingCount),
        };
      });

      setRecipes(recipesWithColor);
    } catch (err) {
      console.error(err);
      setError("Kunne ikke generere opskrifter");
    } finally {
      setLoading(false);
    }
  }

  function openRecipe(recipe) {
    setSelectedRecipe(recipe);
  }

  function closeRecipe() {
    setSelectedRecipe(null);
  }

  async function addFavorite(recipe) {
    try {
      setError("");

      await saveFavoriteRecipe({
        title: recipe.title,
        servings: recipe.servings,
        timeMinutes: recipe.timeMinutes,
        ingredients: recipe.ingredients,
        steps: recipe.steps,
      });

      await loadFavorites();
    } catch (err) {
      console.error(err);
      setError("Kunne ikke gemme favorit");
    }
  }

  function findMatchingFavorite(recipe) {
    return favorites.find(
      (fav) =>
        fav.title === recipe.title &&
        fav.servings === recipe.servings &&
        fav.timeMinutes === recipe.timeMinutes
    );
  }

  async function removeFavoriteByRecipe(recipe) {
    try {
      setError("");

      const favorite = findMatchingFavorite(recipe);
      if (!favorite) return;

      const favoriteId =
        favorite.recipeId ?? favorite.id ?? favorite.favoriteId;

      if (!favoriteId) {
        throw new Error("Kunne ikke finde favorit-id");
      }

      await deleteFavoriteRecipe(favoriteId);
      await loadFavorites();
    } catch (err) {
      console.error(err);
      setError("Kunne ikke fjerne favorit");
    }
  }

  const favoriteKeys = useMemo(() => {
    return new Set(
      favorites.map((fav) =>
        JSON.stringify({
          title: fav.title,
          servings: fav.servings,
          timeMinutes: fav.timeMinutes,
        })
      )
    );
  }, [favorites]);

  function isFavorite(recipe) {
    const key = JSON.stringify({
      title: recipe.title,
      servings: recipe.servings,
      timeMinutes: recipe.timeMinutes,
    });

    return favoriteKeys.has(key);
  }

  return {
    recipes,
    favorites,
    selectedRecipe,
    loading,
    favoritesLoading,
    error,
    loadFavorites,
    generateRecipeList,
    openRecipe,
    closeRecipe,
    addFavorite,
    removeFavoriteByRecipe,
    isFavorite,
  };
}