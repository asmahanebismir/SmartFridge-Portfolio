import { useEffect, useRef } from "react";
import {
  inputStyle,
  labelStyle,
  primaryButtonStyleSmall,
  rowStyle,
} from "../styles/appStyles";

function ScannerInput({
  scannerInput,
  setScannerInput,
  onSubmit,
  onKeyDown,
  error,
  isLoading,
}) {
  const inputRef = useRef(null);

  useEffect(() => {
    inputRef.current?.focus();
  }, []);

  const handleSubmit = () => {
    onSubmit();
    inputRef.current?.focus();
  };

  const handleKeyDown = (e) => {
    onKeyDown?.(e);
  };

  return (
    <div>
      <label style={labelStyle}>Stregkode fra scanner / manuel input</label>

      <div style={rowStyle}>
        <input
          ref={inputRef}
          type="text"
          placeholder="Fx 5701234567890"
          value={scannerInput}
          onChange={(e) => setScannerInput(e.target.value)}
          onKeyDown={handleKeyDown}
          style={inputStyle}
        />
        <button
          type="button"
          onClick={handleSubmit}
          style={primaryButtonStyleSmall}
          disabled={isLoading}
        >
          {isLoading ? "Henter..." : "Tilføj stregkode"}
        </button>
      </div>

      {error && (
        <p style={{ margin: "12px 0 0 0", color: "#991b1b", fontWeight: 600 }}>
          {error}
        </p>
      )}
    </div>
  );
}

export default ScannerInput;