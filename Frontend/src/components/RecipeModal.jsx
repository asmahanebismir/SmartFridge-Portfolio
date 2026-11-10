import {
  secondaryButtonStyle,
} from "../styles/appStyles";

function RecipeModal({ recipe, onClose, isFavorite = false, onToggleFavorite }) {
  if (!recipe) return null;

  return (
    <div
      onClick={onClose}
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
        <h2 style={{ marginTop: 0 }}>{recipe.title}</h2>

        <p style={{ margin: "8px 0 20px 0", color: "#374151" }}>
          {recipe.timeMinutes} min • {recipe.servings} personer
        </p>

        <h3>Ingredienser</h3>
        <ul>
          {recipe.ingredients?.map((ingredient, index) => (
            <li key={index}>
              {ingredient.amount} {ingredient.unit} {ingredient.name}
            </li>
          ))}
        </ul>

        <h3>Fremgangsmåde</h3>
        <ol>
          {recipe.steps?.map((step, index) => (
            <li key={index}>{step}</li>
          ))}
        </ol>

        <div style={{ display: "flex", gap: "12px", marginTop: "20px", flexWrap: "wrap" }}>
          {onToggleFavorite && (
            <button
              type="button"
              style={secondaryButtonStyle}
              onClick={onToggleFavorite}
            >
              {isFavorite ? "Fjern favorit" : "Tilføj favorit"}
            </button>
          )}

          <button
            type="button"
            style={secondaryButtonStyle}
            onClick={onClose}
          >
            Luk
          </button>
        </div>
      </div>
    </div>
  );
}

export default RecipeModal;