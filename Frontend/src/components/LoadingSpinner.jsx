import { cardStyle } from "../styles/appStyles";

function LoadingSpinner() {
  return (
    <div style={{ ...cardStyle, textAlign: "center" }}>
      <p style={{ margin: 0, fontSize: "18px" }}>Indlæser varer...</p>
    </div>
  );
}

export default LoadingSpinner;