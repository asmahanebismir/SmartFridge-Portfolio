import { BrowserRouter, Route, Routes } from "react-router-dom";
import ProtectedRoute from "../components/ProtectedRoute";
import CreateUserPage from "../views/CreateUserPage";
import FavoritterPage from "../views/FavoritterPage";
import LoginPage from "../views/LoginPage";
import OpskrifterPage from "../views/OpskrifterPage";
import VarerPage from "../views/VarerPage";

function AppRoutes({ favoriteRecipes, favoriteRecipeIds, onToggleFavorite }) {
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/" element={<LoginPage />} />
        <Route path="/opret-bruger" element={<CreateUserPage />} />

        <Route
          path="/varer"
          element={
            <ProtectedRoute>
              <VarerPage favoriteRecipes={favoriteRecipes} />
            </ProtectedRoute>
          }
        />

        <Route
          path="/favoritter"
          element={
            <ProtectedRoute>
              <FavoritterPage
                favoriteRecipes={favoriteRecipes}
                favoriteRecipeIds={favoriteRecipeIds}
                onToggleFavorite={onToggleFavorite}
              />
            </ProtectedRoute>
          }
        />

        <Route
          path="/opskrifter"
          element={
            <ProtectedRoute>
              <OpskrifterPage
                favoriteRecipeIds={favoriteRecipeIds}
                onToggleFavorite={onToggleFavorite}
              />
            </ProtectedRoute>
          }
        />
      </Routes>
    </BrowserRouter>
  );
}

export default AppRoutes;