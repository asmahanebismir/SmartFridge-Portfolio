import { describe, it, expect, vi, beforeEach } from "vitest";
import { render, screen, fireEvent } from "@testing-library/react";
import "@testing-library/jest-dom";
import { MemoryRouter } from "react-router-dom";
import OpskrifterPage from "../OpskrifterPage";

const mockViewModel = {
  recipes: [],
  selectedRecipe: null,
  loading: false,
  error: "",
  loadFavorites: vi.fn(),
  generateRecipeList: vi.fn(),
  openRecipe: vi.fn(),
  closeRecipe: vi.fn(),
  addFavorite: vi.fn(),
  removeFavoriteByRecipe: vi.fn(),
  isFavorite: vi.fn(() => false),
};

vi.mock("../../components/Layout", () => ({
  default: ({ children, title }) => (
    <div>
      <h1>{title}</h1>
      {children}
    </div>
  ),
}));

vi.mock("../../viewmodels/useRecipeViewModel", () => ({
  useRecipeViewModel: () => mockViewModel,
}));

function renderPage() {
  return render(
    <MemoryRouter>
      <OpskrifterPage />
    </MemoryRouter>
  );
}

describe("Use case tests - Opskrifter", () => {
  beforeEach(() => {
    vi.clearAllMocks();
    mockViewModel.recipes = [];
    mockViewModel.selectedRecipe = null;
    mockViewModel.loading = false;
    mockViewModel.error = "";
    mockViewModel.isFavorite = vi.fn(() => false);
  });

  it("UC6: brugeren kan se knappen 'Generér opskrifter'", () => {
    renderPage();

    expect(screen.getByText("Opskriftsforslag")).toBeInTheDocument();
    expect(
      screen.getByRole("button", { name: /generér opskrifter/i })
    ).toBeInTheDocument();
  });

  it("UC6: brugeren kan klikke på 'Generér opskrifter'", () => {
    renderPage();

    fireEvent.click(
      screen.getByRole("button", { name: /generér opskrifter/i })
    );

    expect(mockViewModel.generateRecipeList).toHaveBeenCalledTimes(1);
  });

  it("UC6: systemet viser genererede opskriftsforslag", () => {
    mockViewModel.recipes = [
      {
        title: "Pasta med tomat",
        timeMinutes: 20,
        servings: 2,
        color: "#d1fae5",
        missingCount: 0,
        ingredients: [],
        steps: [],
      },
    ];

    renderPage();

    expect(screen.getByText("Pasta med tomat")).toBeInTheDocument();
    expect(screen.getByText(/20 min/i)).toBeInTheDocument();
    expect(
      screen.getByText(/manglende ingredienser: 0/i)
    ).toBeInTheDocument();
  });

  it("UC6: systemet viser fejlbesked hvis opskrifter ikke kan hentes", () => {
    mockViewModel.error = "Kunne ikke generere opskrifter";

    renderPage();

    expect(
      screen.getByText("Kunne ikke generere opskrifter")
    ).toBeInTheDocument();
  });

  it("UC7: brugeren kan vælge en opskrift via 'Se opskrift'", () => {
    const recipe = {
      title: "Pasta med tomat",
      timeMinutes: 20,
      servings: 2,
      color: "#d1fae5",
      missingCount: 0,
      ingredients: [],
      steps: [],
    };

    mockViewModel.recipes = [recipe];

    renderPage();

    fireEvent.click(screen.getByRole("button", { name: /se opskrift/i }));

    expect(mockViewModel.openRecipe).toHaveBeenCalledWith(recipe);
  });

  it("UC7: systemet viser opskriftsdetaljer for den valgte opskrift", () => {
    mockViewModel.selectedRecipe = {
      title: "Pasta med tomat",
      timeMinutes: 20,
      servings: 2,
      ingredients: [{ name: "Pasta", amount: 200 }],
      steps: ["Kog pasta", "Lav sauce"],
    };

    renderPage();

    expect(screen.getByText("Ingredienser")).toBeInTheDocument();
    expect(screen.getByText("Fremgangsmåde")).toBeInTheDocument();
    expect(screen.getByText("Kog pasta")).toBeInTheDocument();
  });

  it("UC7: brugeren kan tilføje en valgt opskrift til favoritter", () => {
    const recipe = {
      title: "Pasta med tomat",
      timeMinutes: 20,
      servings: 2,
      ingredients: [],
      steps: [],
    };

    mockViewModel.selectedRecipe = recipe;
    mockViewModel.isFavorite = vi.fn(() => false);

    renderPage();

    fireEvent.click(screen.getByRole("button", { name: /tilføj favorit/i }));

    expect(mockViewModel.addFavorite).toHaveBeenCalledWith(recipe);
  });
});