import { useEffect, useState } from "react";
import Layout from "../components/Layout";
import { getItems } from "../services/itemService";
import { useRecipeViewModel } from "../viewmodels/useRecipeViewModel";
import {
  cardStyle,
  primaryButtonStyleSmall,
  secondaryButtonStyle,
} from "../styles/appStyles";

function OpskrifterPage() {
  const vm = useRecipeViewModel();
  const [items, setItems] = useState([]);

  useEffect(() => {
    vm.loadFavorites();
    loadItems();
  }, []);

  async function loadItems() {
    try {
      const data = await getItems();
      setItems(data);
    } catch (error) {
      console.error(error);
    }
  }

  const availableIngredients = items.map((item) => item.name);

  return (
    <Layout title="Opskriftsforslag">
      <div style={{ display: "grid", gap: "16px" }}>
        <div style={{ display: "flex", gap: "12px", flexWrap: "wrap" }}>
          <button
            type="button"
            style={primaryButtonStyleSmall}
            onClick={() =>
              vm.generateRecipeList({
                ingredients: availableIngredients,
                preferences: "",
                maxMinutes: 30,
              })
            }
          >
            Generér opskrifter
          </button>
        </div>

        {vm.loading && (
          <p style={{ margin: 0, color: "#374151" }}>Henter opskrifter...</p>
        )}

        {vm.error && (
          <p style={{ margin: 0, color: "crimson" }}>{vm.error}</p>
        )}

        {vm.recipes.map((recipe, index) => (
          <div
            key={`${recipe.title}-${index}`}
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
                  onClick={() =>
                    vm.isFavorite(recipe)
                      ? vm.removeFavoriteByRecipe(recipe)
                      : vm.addFavorite(recipe)
                  }
                >
                  {vm.isFavorite(recipe) ? "Fjern favorit" : "Tilføj favorit"}
                </button>

                <button
                  type="button"
                  style={primaryButtonStyleSmall}
                  onClick={() => vm.openRecipe(recipe)}
                >
                  Se opskrift
                </button>
              </div>
            </div>
          </div>
        ))}
      </div>

      {vm.selectedRecipe && (
        <div
          onClick={vm.closeRecipe}
          style={{
            position: "fixed",
            inset: 0,
            background: "rgba(0,0,0,0.45)",
            display: "flex",
            alignItems: "center",
            justifyContent: "center",
            padding: "24px",
            zIndex: 1000,
          }}
        >
          <div
            onClick={(e) => e.stopPropagation()}
            style={{
              width: "100%",
              maxWidth: "700px",
              maxHeight: "85vh",
              overflowY: "auto",
              background: "white",
              borderRadius: "16px",
              padding: "24px",
              boxShadow: "0 20px 50px rgba(0,0,0,0.2)",
            }}
          >
            <h2>{vm.selectedRecipe.title}</h2>
            <p>
              {vm.selectedRecipe.timeMinutes} min • {vm.selectedRecipe.servings} personer
            </p>

            <h3>Ingredienser</h3>
            <ul>
              {vm.selectedRecipe.ingredients?.map((ingredient, index) => (
                <li key={index}>
                  {ingredient.amount} {ingredient.unit} {ingredient.name}
                </li>
              ))}
            </ul>

            <h3>Fremgangsmåde</h3>
            <ol>
              {vm.selectedRecipe.steps?.map((step, index) => (
                <li key={index}>{step}</li>
              ))}
            </ol>

            <div style={{ display: "flex", gap: "12px", marginTop: "20px" }}>
              <button
                type="button"
                style={secondaryButtonStyle}
                onClick={() =>
                  vm.isFavorite(vm.selectedRecipe)
                    ? vm.removeFavoriteByRecipe(vm.selectedRecipe)
                    : vm.addFavorite(vm.selectedRecipe)
                }
              >
                {vm.isFavorite(vm.selectedRecipe) ? "Fjern favorit" : "Tilføj favorit"}
              </button>

              <button
                type="button"
                style={secondaryButtonStyle}
                onClick={vm.closeRecipe}
              >
                Luk
              </button>
            </div>
          </div>
        </div>
      )}
    </Layout>
  );
}

export default OpskrifterPage;