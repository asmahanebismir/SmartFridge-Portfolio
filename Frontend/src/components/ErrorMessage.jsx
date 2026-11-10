function ErrorMessage({ message }) {
  return (
    <div
      style={{
        background: "#fee2e2",
        color: "#991b1b",
        border: "1px solid #fecaca",
        borderRadius: "14px",
        padding: "16px 18px",
        marginBottom: "24px",
      }}
    >
      {message}
    </div>
  );
}

export default ErrorMessage;