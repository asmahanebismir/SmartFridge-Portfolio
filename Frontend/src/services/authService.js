import { getToken } from "./storageService";

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL;

async function parseResponse(response, fallbackMessage) {
  const text = await response.text();

  let data = null;
  try {
    data = text ? JSON.parse(text) : null;
  } catch {
    data = null;
  }

  if (!response.ok) {
    throw new Error(data?.error || data?.fejl || text || fallbackMessage);
  }

  return data;
}

export async function loginUser(email, password) {
  const response = await fetch(`${API_BASE_URL}/api/auth/login`, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify({
      email: email.trim(),
      password: password.trim(),
    }),
  });

  return parseResponse(response, "Login fejlede");
}

export async function registerUser(email, password) {
  const response = await fetch(`${API_BASE_URL}/api/auth/register`, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify({
      email: email.trim(),
      password: password.trim(),
    }),
  });

  return parseResponse(response, "Kunne ikke oprette bruger");
}

export async function forgotPassword(email) {
  const response = await fetch(`${API_BASE_URL}/api/auth/forgot-password`, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify({
      email: email.trim(),
    }),
  });

  return parseResponse(response, "Kunde ikke sende reset-mail");
}

export async function resendVerificationEmail() {
  const token = getToken();

  if (!token) {
    throw new Error("Brugeren er ikke logget ind");
  }

  const response = await fetch(
    `${API_BASE_URL}/api/auth/Resend Verification Email`,
    {
      method: "POST",
      headers: {
        Authorization: `Bearer ${token}`,
      },
    }
  );

  return parseResponse(response, "Kunne ikke sende verifikationsmail");
}

// To hide the email verification button in "vare siden" we have to get the isverified bool from backend.
export async function getMe()
{
  const token = getToken();

  if(!token) 
  {
    throw new Error("Brugeren er ikke logget ind");
  }

  const response = await fetch(`${API_BASE_URL}/api/auth/me`, {
    headers: {
      Authorization: `Bearer ${token}`,
    },
  });

  return parseResponse(response, "Kunne ikke hente brugerinfo");
}