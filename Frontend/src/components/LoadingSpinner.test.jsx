import { render, screen } from "@testing-library/react";
import LoadingSpinner from "./LoadingSpinner";

describe("LoadingSpinner", () => {
  it("viser loading tekst", () => {
    render(<LoadingSpinner />);
    expect(screen.getByText(/indlæser varer/i)).toBeInTheDocument();
  });

  it("matcher snapshot", () => {
    const { asFragment } = render(<LoadingSpinner />);
    expect(asFragment()).toMatchSnapshot();
  });
});