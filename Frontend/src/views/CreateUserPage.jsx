import { useCreateUserViewModel } from "../viewmodels/useCreateUserViewModel";
import {
  inputStyle,
  primaryButtonStyle,
  secondaryAuthButtonStyle,
} from "../styles/appStyles";

function CreateUserPage() {
  const vm = useCreateUserViewModel();

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
            fontSize: "54px",
            textAlign: "center",
            fontWeight: "700",
          }}
        >
          Opret bruger
        </h1>

        <form
          onSubmit={vm.handleCreateUser}
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

          <input
            type="password"
            placeholder="Gentag kodeord"
            value={vm.repeatPassword}
            onChange={(e) => vm.setRepeatPassword(e.target.value)}
            style={inputStyle}
          />

          <button type="submit" style={primaryButtonStyle}>
            Opret bruger
          </button>

          <button
            type="button"
            onClick={vm.goToLogin}
            style={secondaryAuthButtonStyle}
          >
            Tilbage til login
          </button>
        </form>
      </div>
    </div>
  );
}

export default CreateUserPage;