import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { vi } from "vitest";
import ScannerInput from "./ScannerInput";

describe("ScannerInput", () => {
  it("viser inputfelt og knap", () => {
    render(
      <ScannerInput
        scannerInput=""
        setScannerInput={vi.fn()}
        onSubmit={vi.fn()}
        onKeyDown={vi.fn()}
        error=""
        isLoading={false}
      />
    );

    expect(
      screen.getByPlaceholderText(/fx 5701234567890/i)
    ).toBeInTheDocument();

    expect(
      screen.getByRole("button", { name: /tilføj stregkode/i })
    ).toBeInTheDocument();
  });

  it("kalder setScannerInput når brugeren skriver i feltet", async () => {
    const user = userEvent.setup();
    const setScannerInput = vi.fn();

    render(
      <ScannerInput
        scannerInput=""
        setScannerInput={setScannerInput}
        onSubmit={vi.fn()}
        onKeyDown={vi.fn()}
        error=""
        isLoading={false}
      />
    );

    const input = screen.getByPlaceholderText(/fx 5701234567890/i);
    await user.type(input, "5701234567890");

    expect(setScannerInput).toHaveBeenCalled();
  });

  it("kalder onSubmit når brugeren klikker på knappen", async () => {
    const user = userEvent.setup();
    const onSubmit = vi.fn();

    render(
      <ScannerInput
        scannerInput="5701234567890"
        setScannerInput={vi.fn()}
        onSubmit={onSubmit}
        onKeyDown={vi.fn()}
        error=""
        isLoading={false}
      />
    );

    await user.click(
      screen.getByRole("button", { name: /tilføj stregkode/i })
    );

    expect(onSubmit).toHaveBeenCalledTimes(1);
  });

  it("viser fejlbesked når error er sat", () => {
    render(
      <ScannerInput
        scannerInput=""
        setScannerInput={vi.fn()}
        onSubmit={vi.fn()}
        onKeyDown={vi.fn()}
        error="Ugyldigt stregkodeformat"
        isLoading={false}
      />
    );

    expect(
      screen.getByText(/ugyldigt stregkodeformat/i)
    ).toBeInTheDocument();
  });

  it("viser 'Henter...' når isLoading er true", () => {
    render(
      <ScannerInput
        scannerInput=""
        setScannerInput={vi.fn()}
        onSubmit={vi.fn()}
        onKeyDown={vi.fn()}
        error=""
        isLoading={true}
      />
    );

    expect(screen.getByRole("button", { name: /henter/i })).toBeInTheDocument();
  });
});