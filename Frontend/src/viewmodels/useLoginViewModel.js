import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { forgotPassword, loginUser } from "../services/authService";
import { saveAuth } from "../services/storageService";

export function useLoginViewModel() {
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState("");
  const [message, setMessage] = useState("");
  const navigate = useNavigate();

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError("");
    setMessage("");

    if (!email.trim() || !password.trim()) {
      setError("Udfyld email og kodeord");
      return;
    }

    try {
      const data = await loginUser(email, password);
      saveAuth(data.token, data.email || email.trim());
      navigate("/varer");
    } catch (error) {
      console.error(error);
      setError(error.message || "Kunne ikke logge ind");
    }
  };

  const handleForgotPassword = async () => {
    setError("");
    setMessage("");

    if (!email.trim()) {
      setError("Indtast din email først");
      return;
    }

    try {
      await forgotPassword(email);
      setMessage("Reset-mail er sendt til din email");
    } catch (error) {
      console.error(error);
      setError(error.message || "Kunne ikke sende reset-mail");
    }
  };

  return {
    email,
    password,
    error,
    message,
    setEmail,
    setPassword,
    handleSubmit,
    handleForgotPassword,
    goToCreateUser: () => navigate("/opret-bruger"),
  };
}