import { render, screen } from "@testing-library/react";
import ErrorMessage from "./ErrorMessage";

describe("ErrorMessage", () => {
  it("viser fejlbesked", () => {
    render(<ErrorMessage message="Noget gik galt" />);

    expect(screen.getByText("Noget gik galt")).toBeInTheDocument();
  });

  it("matcher snapshot", () => {
    const { asFragment } = render(
      <ErrorMessage message="Fejl" />
    );

    expect(asFragment()).toMatchSnapshot();
  });
});