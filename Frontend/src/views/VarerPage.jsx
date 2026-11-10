import { useEffect, useState } from "react";
import Layout from "../components/Layout";
import ErrorMessage from "../components/ErrorMessage";
import LoadingSpinner from "../components/LoadingSpinner";
import ScannerInput from "../components/ScannerInput";
import TilføjVareModal from "../components/TilføjVareModal";
import VarelisteComponent from "../components/VarelisteComponent";
import { resendVerificationEmail, getMe } from "../services/authService";
import { getFavoriteRecipes } from "../services/recipeService";
import { useVarerViewModel } from "../viewmodels/useVarerViewModel";
import {
  cardStyle,
  favoriteItemStyle,
  mutedTextStyle,
  primaryButtonStyleSmall,
  sectionTitleStyle,
} from "../styles/appStyles";

function VarerPage() {
  const vm = useVarerViewModel();
  const [favoriteRecipes, setFavoriteRecipes] = useState([]);
  const [favoritesError, setFavoritesError] = useState("");
  const [verifyMessage, setVerifyMessage] = useState("");
  const [verifyError, setVerifyError] = useState("");
  const [emailVerified, setEmailVerified] = useState(false);

  useEffect(() => {
    loadFavorites();
    loadMe();
  }, []);

  async function loadFavorites() {
    try {
      setFavoritesError("");
      const data = await getFavoriteRecipes();
      setFavoriteRecipes(data);
    } catch (error) {
      console.error(error);
      setFavoritesError("Kunne ikke hente favoritopskrifter");
    }
  }

  async function handleResendVerification() {
    try {
      setVerifyMessage("");
      setVerifyError("");
      await resendVerificationEmail();
      setVerifyMessage("Verifikationsmail er sendt");
    } catch (error) {
      console.error(error);
      setVerifyError(error.message || "Kunne ikke sende verifikationsmail");
    }
  }

  async function loadMe()
  {
    try 
    {
      const me = await getMe();
      setEmailVerified(me.emailVerified);
    }
    catch (error)
    {
      console.error(error);
    }
  }

  return (
    <Layout title="Varer">
      {vm.error && <ErrorMessage message={vm.error} />}
      {favoritesError && <ErrorMessage message={favoritesError} />}

      {!emailVerified && (// Show button if email is not verified. 
        <div style={{ marginBottom: "16px" }}>
          <button
            type="button"
            onClick={handleResendVerification}
            style={primaryButtonStyleSmall}
          >
            Send verifikationsmail igen
          </button>
        </div>
      )}

      {verifyMessage && (
        <p style={{ margin: "0 0 16px 0", color: "#166534", fontWeight: 600 }}>
          {verifyMessage}
        </p>
      )}

      {verifyError && (
        <p style={{ margin: "0 0 16px 0", color: "#991b1b", fontWeight: 600 }}>
          {verifyError}
        </p>
      )}

      <div
        style={{
          display: "grid",
          gridTemplateColumns: "2fr 1fr",
          gap: "24px",
        }}
      >
        <section style={cardStyle}>
          <h2 style={sectionTitleStyle}>Tilføj varer</h2>
          <p style={mutedTextStyle}>
            Scan stregkoden med USB-scanner. Hvis scanneren ikke virker, kan du
            indtaste stregkoden manuelt eller tilføje varen via formularen.
          </p>

          <div style={{ marginTop: "20px", display: "grid", gap: "16px" }}>
            <ScannerInput
              scannerInput={vm.scannerInput}
              setScannerInput={vm.setScannerInput}
              onSubmit={vm.handleScannerSubmit}
              onKeyDown={vm.handleScannerKeyDown}
              error={vm.barcodeError}
              isLoading={vm.isLookingUpBarcode}
            />

            <div>
              <button
                type="button"
                onClick={() => vm.setShowModal(true)}
                style={primaryButtonStyleSmall}
              >
                Tilføj vare
              </button>
            </div>
          </div>
        </section>

        <section style={cardStyle}>
          <h2 style={sectionTitleStyle}>Favoritopskrifter</h2>
          <p style={mutedTextStyle}>
            Disse opskrifter vises også på favorit-siden.
          </p>

          <div style={{ marginTop: "16px", display: "grid", gap: "10px" }}>
            {favoriteRecipes.length > 0 ? (
              favoriteRecipes.map((recipe, index) => (
                <div
                  key={recipe.recipeId ?? recipe.id ?? recipe.favoriteId ?? index}
                  style={favoriteItemStyle}
                >
                  <span>{recipe.title}</span>
                </div>
              ))
            ) : (
              <p style={mutedTextStyle}>Ingen favoritopskrifter endnu.</p>
            )}
          </div>
        </section>
      </div>

      {vm.loading ? (
        <section style={{ marginTop: "24px" }}>
          <LoadingSpinner />
        </section>
      ) : (
        <VarelisteComponent
          items={vm.filteredItems}
          onDelete={vm.removeItem}
          search={vm.search}
          onSearchChange={vm.setSearch}
        />
      )}

      {vm.showModal && (
        <TilføjVareModal
          onClose={() => vm.setShowModal(false)}
          onAdd={vm.handleAddItem}
        />
      )}
    </Layout>
  );
}

export default VarerPage;