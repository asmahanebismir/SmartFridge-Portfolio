import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { vi } from "vitest";
import VareItem from "./VareItem";

describe("VareItem", () => {
  it("viser navn, antal og stregkode", () => {
    render(
      <VareItem
        item={{
          id: 1,
          name: "Mælk",
          amount: 2,
          barcode: "5701111111111",
          imageUrl: "",
        }}
        onDelete={() => {}}
      />
    );

    expect(screen.getByText("Mælk")).toBeInTheDocument();
    expect(screen.getByText("Antal: 2")).toBeInTheDocument();
    expect(screen.getByText("Stregkode: 5701111111111")).toBeInTheDocument();
  });

  it("kalder onDelete når man klikker på Fjern", async () => {
    const user = userEvent.setup();
    const onDelete = vi.fn();

    render(
      <VareItem
        item={{
          id: 1,
          name: "Mælk",
          amount: 2,
          barcode: "5701111111111",
          imageUrl: "",
        }}
        onDelete={onDelete}
      />
    );

    await user.click(screen.getByRole("button", { name: /fjern/i }));

    expect(onDelete).toHaveBeenCalledWith(1);
  });

  it("matcher snapshot", () => {
    const { asFragment } = render(
      <VareItem
        item={{
          id: 1,
          name: "Mælk",
          amount: 2,
          barcode: "5701111111111",
          imageUrl: "",
        }}
        onDelete={() => {}}
      />
    );

    expect(asFragment()).toMatchSnapshot();
  });
});