import { describe, it, expect, beforeAll } from "vitest";
import { loginUser } from "../services/authService";
import { saveAuth } from "../services/storageService";
import { addItem, deleteItem, getItems } from "../services/itemService";


const TEST_EMAIL = process.env.TEST_EMAIL;
const TEST_PASSWORD = process.env.TEST_PASSWORD;


describe("Frontend-backend integration - Groceries", () => {
  beforeAll(async () => {
    const result = await loginUser(TEST_EMAIL, TEST_PASSWORD);
    saveAuth(result.token, TEST_EMAIL);
  });

  it("kan hente varer fra rigtig backend", async () => {
    const items = await getItems();
    expect(Array.isArray(items)).toBe(true);
  });

  it("kan tilføje en vare via rigtig backend", async () => {
    const uniqueName = `Integration Test Vare ${Date.now()}`;
    const uniqueBarcode = String(Date.now()).slice(0, 13);

    const created = await addItem({
      name: uniqueName,
      amount: 1,
      barcode: uniqueBarcode,
      imageUrl: "",
    });

    expect(created).toBeTruthy();
    expect(created.name).toBe(uniqueName);

    const items = await getItems();
    const addedItem = items.find((item) => item.name === uniqueName);

    expect(addedItem).toBeTruthy();
    expect(addedItem.barcode).toBe(uniqueBarcode);
  });

  it("kan slette en vare via rigtig backend", async () => {
    const uniqueName = `Slet Test Vare ${Date.now()}`;
    const uniqueBarcode = String(Date.now()).slice(0, 13);

    const created = await addItem({
      name: uniqueName,
      amount: 1,
      barcode: uniqueBarcode,
      imageUrl: "",
    });

    const createdId = created?.id;
    expect(createdId).toBeTruthy();

    await deleteItem(createdId);

    const items = await getItems();
    const deletedItem = items.find((item) => item.id === createdId);

    expect(deletedItem).toBeUndefined();
  });
});
