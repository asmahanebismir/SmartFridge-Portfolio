import { render, screen } from "@testing-library/react";
import VarelisteComponent from "./VarelisteComponent";

describe("VarelisteComponent", () => {
  const items = [
    { id: 1, name: "Mælk", amount: 2, barcode: "5701111111111", imageUrl: "" },
    { id: 2, name: "Æg", amount: 6, barcode: "5702222222222", imageUrl: "" },
  ];

  it("viser varer i listen", () => {
    render(
      <VarelisteComponent
        items={items}
        onDelete={() => {}}
        search=""
        onSearchChange={() => {}}
      />
    );

    expect(screen.getByText("Mælk")).toBeInTheDocument();
    expect(screen.getByText("Æg")).toBeInTheDocument();
  });

  it("viser empty state", () => {
    render(
      <VarelisteComponent
        items={[]}
        onDelete={() => {}}
        search=""
        onSearchChange={() => {}}
      />
    );

    expect(screen.getByText(/du har ingen varer endnu/i)).toBeInTheDocument();
  });

  it("viser fjern-knapper", () => {
    render(
      <VarelisteComponent
        items={items}
        onDelete={() => {}}
        search=""
        onSearchChange={() => {}}
      />
    );

    const buttons = screen.getAllByRole("button", { name: /fjern/i });
    expect(buttons.length).toBe(2);
  });

  it("viser søgefelt", () => {
    render(
      <VarelisteComponent
        items={items}
        onDelete={() => {}}
        search=""
        onSearchChange={() => {}}
      />
    );

    expect(screen.getByPlaceholderText(/søg efter vare/i)).toBeInTheDocument();
  });

  it("matcher snapshot", () => {
    const { asFragment } = render(
      <VarelisteComponent
        items={items}
        onDelete={() => {}}
        search=""
        onSearchChange={() => {}}
      />
    );

    expect(asFragment()).toMatchSnapshot();
  });
});