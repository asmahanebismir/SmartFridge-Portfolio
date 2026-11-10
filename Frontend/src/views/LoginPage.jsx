import { useLoginViewModel } from "../viewmodels/useLoginViewModel";
import {
  inputStyle,
  primaryButtonStyle,
  secondaryAuthButtonStyle,
} from "../styles/appStyles";

function LoginPage() {
  const vm = useLoginViewModel();

  return (
    <div
      style={{
        minHeight: "100vh",
        display: "flex",
        justifyContent: "center",
        alignItems: "center",
        backgroundColor: "#e9ecef",
        padding: "24px",
      }}
    >
      <div
        style={{
          width: "900px",
          maxWidth: "100%",
          padding: "100px 90px",
          background: "white",
          borderRadius: "20px",
          boxShadow: "0 12px 40px rgba(0,0,0,0.15)",
        }}
      >
        <h1
          style={{
            marginBottom: "50px",
            fontSize: "64px",
            textAlign: "center",
            fontWeight: "700",
          }}
        >
          SmartFridge
        </h1>

        <form
          onSubmit={vm.handleSubmit}
          style={{
            display: "flex",
            flexDirection: "column",
            gap: "24px",
          }}
        >
          <input
            type="email"
            placeholder="Email"
            value={vm.email}
            onChange={(e) => vm.setEmail(e.target.value)}
            style={inputStyle}
          />

          <input
            type="password"
            placeholder="Kodeord"
            value={vm.password}
            onChange={(e) => vm.setPassword(e.target.value)}
            style={inputStyle}
          />

          {vm.message && (
            <p style={{ margin: 0, color: "#166534", fontWeight: 600 }}>
              {vm.message}
            </p>
          )}

          {vm.error && (
            <p style={{ margin: 0, color: "#991b1b", fontWeight: 600 }}>
              {vm.error}
            </p>
          )}

          <button type="submit" style={primaryButtonStyle}>
            Log ind
          </button>

          <button
            type="button"
            onClick={vm.handleForgotPassword}
            style={secondaryAuthButtonStyle}
          >
            Glemt kodeord
          </button>

          <button
            type="button"
            onClick={vm.goToCreateUser}
            style={secondaryAuthButtonStyle}
          >
            Opret bruger
          </button>
        </form>
      </div>
    </div>
  );
}

export default LoginPage;