import { describe, it, expect } from "vitest";
import { loginUser } from "../services/authService";

describe("Frontend-backend integration - Auth", () => {
  it("kan logge ind mod rigtig backend", async () => {

    const email = process.env.TEST_EMAIL;
    const password = process.env.TEST_PASSWORD;


    const result = await loginUser(email, password);

    expect(result).toBeTruthy();
    expect(result.token).toBeTruthy();
  });
});
