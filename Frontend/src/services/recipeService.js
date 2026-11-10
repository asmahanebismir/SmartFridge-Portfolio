//import { auth } from "../firebase";
import { getToken } from "./storageService";

const API_BASE_URL = `${import.meta.env.VITE_API_BASE_URL}/api/recipes`;

async function getAuthHeaders() {
  const token = getToken();

  if (!token) {
    throw new Error("Brugeren er ikke logget ind");
  }

  

  return {
    "Content-Type": "application/json",
    Authorization: `Bearer ${token}`,
  };
}

export async function generateRecipes({ ingredients, preferences, maxMinutes }) {
  const headers = await getAuthHeaders();

  const response = await fetch(`${API_BASE_URL}/generate`, {
    method: "POST",
    headers,
    body: JSON.stringify({
      ingredients,
      preferences,
      maxMinutes,
    }),
  });

  if (!response.ok) {
    throw new Error("Kunne ikke generere opskrifter");
  }

  return response.json();
}

export async function saveFavoriteRecipe(recipe) {
  const headers = await getAuthHeaders();

  const response = await fetch(`${API_BASE_URL}/favorites`, {
    method: "POST",
    headers,
    body: JSON.stringify({
      title: recipe.title,
      servings: recipe.servings,
      timeMinutes: recipe.timeMinutes,
      ingredients: recipe.ingredients,
      steps: recipe.steps,
    }),
  });

  if (!response.ok) {
    throw new Error("Kunne ikke gemme favorit");
  }

  return response.json();
}

export async function getFavoriteRecipes() {
  const headers = await getAuthHeaders();

  const response = await fetch(`${API_BASE_URL}/favorites`, {
    method: "GET",
    headers,
  });

  if (!response.ok) {
    throw new Error("Kunne ikke hente favoritter");
  }

  return response.json();
}

export async function deleteFavoriteRecipe(recipeId) {
  const headers = await getAuthHeaders();

  const response = await fetch(`${API_BASE_URL}/favorites/${recipeId}`, {
    method: "DELETE",
    headers,
  });

  if (!response.ok) {
    throw new Error("Kunne ikke slette favorit");
  }
}