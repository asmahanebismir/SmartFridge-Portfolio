import VareItem from "./VareItem";
import {
  cardStyle,
  inputStyle,
  mutedTextStyle,
  sectionTitleStyle,
} from "../styles/appStyles";

function VarelisteComponent({ items, onDelete, search, onSearchChange }) {
  return (
    <section style={{ ...cardStyle, marginTop: "24px" }}>
      <div
        style={{
          display: "flex",
          justifyContent: "space-between",
          alignItems: "center",
          gap: "16px",
          flexWrap: "wrap",
          marginBottom: "20px",
        }}
      >
        <h2 style={{ ...sectionTitleStyle, margin: 0 }}>Vareliste</h2>

        <input
          type="text"
          placeholder="Søg efter vare"
          value={search}
          onChange={(e) => onSearchChange(e.target.value)}
          style={{ ...inputStyle, maxWidth: "320px" }}
        />
      </div>

      {items.length > 0 ? (
        <div style={{ display: "grid", gap: "12px" }}>
          {items.map((item) => (
            <VareItem key={item.id} item={item} onDelete={onDelete} />
          ))}
        </div>
      ) : (
        <p style={{ ...mutedTextStyle, fontSize: "17px" }}>
          Du har ingen varer endnu
        </p>
      )}
    </section>
  );
}

export default VarelisteComponent;