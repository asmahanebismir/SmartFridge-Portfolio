import { useState } from "react";

export function useFavoritesViewModel() {
  const [selectedRecipe, setSelectedRecipe] = useState(null);

  return {
    selectedRecipe,
    openRecipe: (recipe) => setSelectedRecipe(recipe),
    closeRecipe: () => setSelectedRecipe(null),
  };
}