import { useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import { clearAuth } from "../services/storageService";
import { hamburgerLineStyle, menuLinkStyle } from "../styles/appStyles";

function Layout({ children, title }) {
  const [menuOpen, setMenuOpen] = useState(false);
  const navigate = useNavigate();

  const handleLogout = () => {
    clearAuth();
    navigate("/");
  };

  return (
    <div style={{ minHeight: "100vh", backgroundColor: "#f3f4f6" }}>
      <header
        style={{
          background: "white",
          borderBottom: "1px solid #e5e7eb",
          padding: "18px 32px",
          display: "flex",
          justifyContent: "space-between",
          alignItems: "center",
          position: "relative",
          zIndex: 100,
        }}
      >
        <div style={{ fontSize: "28px", fontWeight: 700 }}>SmartFridge</div>

        <div style={{ position: "relative" }}>
          <button
            type="button"
            onClick={() => setMenuOpen(!menuOpen)}
            style={{
              width: "48px",
              height: "48px",
              borderRadius: "12px",
              border: "1px solid #d1d5db",
              background: "white",
              cursor: "pointer",
              display: "flex",
              flexDirection: "column",
              justifyContent: "center",
              alignItems: "center",
              gap: "5px",
            }}
          >
            <span style={hamburgerLineStyle}></span>
            <span style={hamburgerLineStyle}></span>
            <span style={hamburgerLineStyle}></span>
          </button>

          {menuOpen && (
            <div
              style={{
                position: "absolute",
                top: "58px",
                right: 0,
                width: "220px",
                background: "white",
                border: "1px solid #e5e7eb",
                borderRadius: "14px",
                boxShadow: "0 12px 30px rgba(0,0,0,0.12)",
                padding: "10px",
                display: "flex",
                flexDirection: "column",
                gap: "8px",
                zIndex: 1000,
              }}
            >
              <Link to="/varer" style={menuLinkStyle} onClick={() => setMenuOpen(false)}>
                Varer
              </Link>

              <Link to="/favoritter" style={menuLinkStyle} onClick={() => setMenuOpen(false)}>
                Favoritopskrifter
              </Link>

              <Link to="/opskrifter" style={menuLinkStyle} onClick={() => setMenuOpen(false)}>
                Opskrifter
              </Link>

              <button
                type="button"
                onClick={handleLogout}
                style={{
                  color: "#111827",
                  textDecoration: "none",
                  fontSize: "16px",
                  fontWeight: 600,
                  padding: "12px 14px",
                  borderRadius: "10px",
                  background: "#f3f4f6",
                  display: "block",
                  border: "none",
                  cursor: "pointer",
                  textAlign: "left",
                }}
              >
                Log ud
              </button>
            </div>
          )}
        </div>
      </header>

      <main style={{ padding: "32px" }}>
        <div style={{ maxWidth: "1200px", margin: "0 auto" }}>
          <h1 style={{ fontSize: "36px", marginBottom: "24px" }}>{title}</h1>
          {children}
        </div>
      </main>
    </div>
  );
}

export default Layout;