import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { registerUser } from "../services/authService";
import { saveAuth } from "../services/storageService";

export function useCreateUserViewModel() {
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [repeatPassword, setRepeatPassword] = useState("");
  const navigate = useNavigate();

  const handleCreateUser = async (e) => {
    e.preventDefault();

    if (!email.trim() || !password.trim() || !repeatPassword.trim()) {
      alert("Udfyld alle felter");
      return;
    }

    if (password !== repeatPassword) {
      alert("Kodeordene matcher ikke");
      return;
    }

    try {
      const data = await registerUser(email, password);
      saveAuth(data.token, data.email || email.trim());
      navigate("/varer");
    } catch (error) {
      console.error(error);
      alert(error.message || "Kunne ikke oprette bruger");
    }
  };

  return {
    email,
    password,
    repeatPassword,
    setEmail,
    setPassword,
    setRepeatPassword,
    handleCreateUser,
    goToLogin: () => navigate("/"),
  };
}