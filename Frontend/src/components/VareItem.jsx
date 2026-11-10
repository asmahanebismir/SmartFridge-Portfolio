import { dangerButtonStyle, listItemStyle } from "../styles/appStyles";

function VareItem({ item, onDelete }) {
  const itemAmount = item.amount ?? item.quantity;

  return (
    <div
      style={{
        ...listItemStyle,
        display: "flex",
        justifyContent: "space-between",
        alignItems: "center",
        gap: "16px",
        flexWrap: "wrap",
      }}
    >
      <div style={{ display: "flex", alignItems: "center", gap: "16px" }}>
        {item.imageUrl && (
          <img
            src={item.imageUrl}
            alt={item.name}
            style={{
              width: "72px",
              height: "72px",
              objectFit: "cover",
              borderRadius: "12px",
              border: "1px solid #e5e7eb",
              background: "#fff",
            }}
          />
        )}

        <div>
          <div style={{ fontSize: "18px", fontWeight: 700 }}>{item.name}</div>

          {itemAmount && (
            <div style={{ color: "#374151", marginTop: "4px" }}>
              Antal: {itemAmount}
            </div>
          )}

          {item.barcode && (
            <div style={{ color: "#374151", marginTop: "4px" }}>
              Stregkode: {item.barcode}
            </div>
          )}
        </div>
      </div>

      <div style={{ display: "flex", gap: "10px", flexWrap: "wrap" }}>
        <button
          type="button"
          onClick={() => onDelete(item.id)}
          style={dangerButtonStyle}
        >
          Fjern
        </button>
      </div>
    </div>
  );
}

export default VareItem;