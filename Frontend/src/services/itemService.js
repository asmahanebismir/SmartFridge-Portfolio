// import { auth } from "../firebase";
import { getToken } from "./storageService";

const API_BASE_URL = `${import.meta.env.VITE_API_BASE_URL}/api/groceries`;

async function getAuthHeaders() {
  const token = getToken();

  if (!token) {
    throw new Error("Brugeren er ikke logget ind");
  }


  return {
    "Content-Type": "application/json",
    Authorization: `Bearer ${token}`,
  };
}

export async function getItems() {
  const headers = await getAuthHeaders();

  const response = await fetch(API_BASE_URL, {
    method: "GET",
    headers,
  });

  if (!response.ok) {
    throw new Error("Kunne ikke hente varer");
  }

  return await response.json();
}

export async function addItem({ name, amount, barcode, imageUrl }) {
  const headers = await getAuthHeaders();

  const response = await fetch(`${API_BASE_URL}/add`, {
    method: "POST",
    headers,
    body: JSON.stringify({
      name,
      amount,
      barcode,
      imageUrl,
    }),
  });

  if (!response.ok) {
    const errorText = await response.text();
    throw new Error(errorText || "Kunne ikke tilføje vare");
  }

  return await response.json();
}

export async function deleteItem(groceriesId) {
  const headers = await getAuthHeaders();

  const response = await fetch(`${API_BASE_URL}/${groceriesId}`, {
    method: "DELETE",
    headers,
  });

  if (!response.ok) {
    const errorText = await response.text();
    throw new Error(errorText || "Kunne ikke slette vare");
  }
}

export async function updateItemAmount(groceriesId, amount) {
  const headers = await getAuthHeaders();

  const response = await fetch(`${API_BASE_URL}/${groceriesId}/amount`, {
    method: "PUT",
    headers,
    body: JSON.stringify({ amount }),
  });

  if (!response.ok) {
    const errorText = await response.text();
    throw new Error(errorText || "Kunne ikke opdatere antal");
  }

  return true;
}