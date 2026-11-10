export function saveAuth(token, email) {
  localStorage.setItem("secret", token);
  localStorage.setItem("email", email);
}

export function clearAuth() {
  localStorage.removeItem("secret");
  localStorage.removeItem("email");
}

export function getToken() {
  return localStorage.getItem("secret");
}