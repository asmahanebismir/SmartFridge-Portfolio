import { useState } from "react";
import { fetchProductByBarcode } from "../services/productService";
import { isValidBarcode, parseBarcode } from "../utils/barcodeUtils";
import {
  closeButtonStyle,
  inputStyle,
  labelStyle,
  modalOverlayStyle,
  modalStyle,
  mutedTextStyle,
  primaryButtonStyleSmall,
  secondaryButtonStyle,
} from "../styles/appStyles";

function TilføjVareModal({ onClose, onAdd }) {
  const [name, setName] = useState("");
  const [quantity, setQuantity] = useState("");
  const [barcode, setBarcode] = useState("");
  const [isSaving, setIsSaving] = useState(false);

  const handleSubmit = async (e) => {
    e.preventDefault();

    const parsedBarcode = parseBarcode(barcode);

    if (!quantity.trim() || !parsedBarcode) {
      alert("Udfyld antal og stregkode");
      return;
    }

    if (!isValidBarcode(parsedBarcode)) {
      alert("Ugyldigt stregkodeformat. Brug 8, 12 eller 13 cifre.");
      return;
    }

    try {
      setIsSaving(true);

      const product = await fetchProductByBarcode(parsedBarcode);

      onAdd({
        id: Date.now(),
        name: name.trim() || product?.name || `Ukendt vare (${parsedBarcode})`,
        quantity: quantity.trim(),
        barcode: parsedBarcode,
      });

      setName("");
      setQuantity("");
      setBarcode("");
      onClose();
    } catch (error) {
      console.error(error);

      onAdd({
        id: Date.now(),
        name: name.trim() || `Ukendt vare (${parsedBarcode})`,
        quantity: quantity.trim(),
        barcode: parsedBarcode,
      });

      setName("");
      setQuantity("");
      setBarcode("");
      onClose();
    } finally {
      setIsSaving(false);
    }
  };

  return (
    <div style={modalOverlayStyle}>
      <div style={modalStyle}>
        <div
          style={{
            display: "flex",
            justifyContent: "space-between",
            alignItems: "center",
            gap: "16px",
            marginBottom: "20px",
            flexWrap: "wrap",
          }}
        >
          <h2 style={{ margin: 0, fontSize: "30px" }}>Tilføj vare</h2>

          <button type="button" onClick={onClose} style={closeButtonStyle}>
            Luk
          </button>
        </div>

        <form onSubmit={handleSubmit} style={{ display: "grid", gap: "16px" }}>
          <div>
            <label style={labelStyle}>Navn</label>
            <input
              type="text"
              placeholder="Skriv varenavn (valgfrit)"
              value={name}
              onChange={(e) => setName(e.target.value)}
              style={inputStyle}
            />
          </div>

          <div>
            <label style={labelStyle}>Antal</label>
            <input
              type="number"
              placeholder="Skriv antal"
              value={quantity}
              onChange={(e) => setQuantity(e.target.value)}
              style={inputStyle}
            />
          </div>

          <div>
            <label style={labelStyle}>Stregkode</label>
            <input
              type="text"
              placeholder="Fx 5701234567890"
              value={barcode}
              onChange={(e) => setBarcode(e.target.value)}
              style={inputStyle}
            />
          </div>

          <p style={{ ...mutedTextStyle, marginTop: "-4px" }}>
            Du kan enten scanne med USB-stregkodescanner eller skrive stregkoden manuelt.
          </p>

          <div style={{ display: "flex", gap: "12px", flexWrap: "wrap" }}>
            <button type="submit" style={primaryButtonStyleSmall} disabled={isSaving}>
              {isSaving ? "Henter vare..." : "Gem vare"}
            </button>

            <button
              type="button"
              onClick={onClose}
              style={secondaryButtonStyle}
              disabled={isSaving}
            >
              Annuller
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}

export default TilføjVareModal;