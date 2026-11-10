import { useEffect, useState } from "react";
import Layout from "../components/Layout";
import RecipeModal from "../components/RecipeModal";
import { getFavoriteRecipes, deleteFavoriteRecipe } from "../services/recipeService";
import { getItems } from "../services/itemService";
import {
  cardStyle,
  primaryButtonStyleSmall,
  secondaryButtonStyle,
} from "../styles/appStyles";

function getRecipeColor(missingCount) {
  if (missingCount === 0) return "#d1fae5";
  if (missingCount <= 2) return "#fef3c7";
  return "#fee2e2";
}

function normalizeText(text) {
  return (text || "").toLowerCase().trim();
}

function calculateMissingCount(recipeIngredients, availableItems) {
  const available = availableItems.map((item) => normalizeText(item.name));

  const missingIngredients = (recipeIngredients || []).filter((ingredient) => {
    const ingredientName = normalizeText(ingredient.name);

    const exists = available.some(
      (itemName) =>
        ingredientName.includes(itemName) || itemName.includes(ingredientName)
    );

    return !exists;
  });

  return missingIngredients.length;
}

function FavoritterPage() {
  const [favorites, setFavorites] = useState([]);
  const [items, setItems] = useState([]);
  const [selectedRecipe, setSelectedRecipe] = useState(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");

  useEffect(() => {
    loadData();
  }, []);

  async function loadData() {
    try {
      setLoading(true);
      setError("");

      const [favoriteData, itemsData] = await Promise.all([
        getFavoriteRecipes(),
        getItems(),
      ]);

      const favoritesWithColor = favoriteData.map((recipe) => {
        const missingCount = calculateMissingCount(recipe.ingredients, itemsData);

        return {
          ...recipe,
          missingCount,
          color: getRecipeColor(missingCount),
        };
      });

      setFavorites(favoritesWithColor);
      setItems(itemsData);
    } catch (err) {
      console.error(err);
      setError("Kunne ikke hente favoritter");
    } finally {
      setLoading(false);
    }
  }

  async function handleRemoveFavorite(recipe) {
    try {
      setError("");

      const favoriteId = recipe.recipeId ?? recipe.id ?? recipe.favoriteId;
      if (!favoriteId) {
        throw new Error("Kunne ikke finde favorit-id");
      }

      await deleteFavoriteRecipe(favoriteId);
      await loadData();

      if (
        selectedRecipe &&
        (selectedRecipe.recipeId ?? selectedRecipe.id ?? selectedRecipe.favoriteId) === favoriteId
      ) {
        setSelectedRecipe(null);
      }
    } catch (err) {
      console.error(err);
      setError("Kunne ikke fjerne favorit");
    }
  }

  return (
    <Layout title="Favoritopskrifter">
      <div style={{ display: "grid", gap: "16px" }}>
        {loading && (
          <p style={{ margin: 0, color: "#374151" }}>Henter favoritter...</p>
        )}

        {error && (
          <p style={{ margin: 0, color: "crimson" }}>{error}</p>
        )}

        {!loading && favorites.length === 0 && (
          <div style={cardStyle}>
            <p style={{ fontSize: "18px", margin: 0 }}>
              Du har ingen favoritopskrifter endnu.
            </p>
          </div>
        )}

        {favorites.map((recipe, index) => (
          <div
            key={recipe.recipeId ?? recipe.id ?? recipe.favoriteId ?? `${recipe.title}-${index}`}
            style={{
              ...cardStyle,
              background: recipe.color,
              border: "1px solid rgba(0,0,0,0.06)",
            }}
          >
            <div
              style={{
                display: "flex",
                justifyContent: "space-between",
                alignItems: "center",
                gap: "16px",
                flexWrap: "wrap",
              }}
            >
              <div>
                <h3 style={{ margin: 0, fontSize: "24px" }}>{recipe.title}</h3>
                <p style={{ margin: "8px 0 0 0", color: "#374151" }}>
                  {recipe.timeMinutes} min • {recipe.servings} personer
                </p>
                <p style={{ margin: "8px 0 0 0", color: "#374151" }}>
                  Manglende ingredienser: {recipe.missingCount}
                </p>
              </div>

              <div style={{ display: "flex", gap: "10px", flexWrap: "wrap" }}>
                <button
                  type="button"
                  style={secondaryButtonStyle}
                  onClick={() => handleRemoveFavorite(recipe)}
                >
                  Fjern favorit
                </button>

                <button
                  type="button"
                  style={primaryButtonStyleSmall}
                  onClick={() => setSelectedRecipe(recipe)}
                >
                  Se opskrift
                </button>
              </div>
            </div>
          </div>
        ))}
      </div>

      <RecipeModal
        recipe={selectedRecipe}
        onClose={() => setSelectedRecipe(null)}
        isFavorite={true}
        onToggleFavorite={() => {
          if (selectedRecipe) {
            handleRemoveFavorite(selectedRecipe);
          }
        }}
      />
    </Layout>
  );
}

export default FavoritterPage;