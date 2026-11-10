import { useEffect, useMemo, useState } from "react";
import {
  addItem,
  deleteItem,
  getItems,
  updateItemAmount,
} from "../services/itemService";
import { fetchProductByBarcode } from "../services/productService";
import { isValidBarcode, parseBarcode } from "../utils/barcodeUtils";

//BArcode commands: states
const BARCODE_COMMANDS = {
  AMOUNT: "amount",
  DELETE: "delete",
};

export function useVarerViewModel() {
  const [search, setSearch] = useState("");
  const [items, setItems] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [showModal, setShowModal] = useState(false);
  const [scannerInput, setScannerInput] = useState("");
  const [barcodeError, setBarcodeError] = useState("");
  const [isLookingUpBarcode, setIsLookingUpBarcode] = useState(false);
  const [scannerMode, setScannerMode] = useState(null);

  async function loadItems() {
    try {
      setLoading(true);
      setError("");
      const data = await getItems();
      setItems(data);
    } catch (error) {
      console.error(error);
      setError("Kunne ikke hente varer fra API");
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => {
    loadItems();
  }, []);

  const filteredItems = useMemo(() => {
    return items.filter((item) =>
      item.name.toLowerCase().includes(search.toLowerCase())
    );
  }, [items, search]);

  const handleAddItem = async (newItem) => {
    try {
      setError("");

      await addItem({
        name: newItem.name,
        amount: newItem.quantity ?? newItem.amount ?? 1,
        barcode: newItem.barcode ?? "",
        imageUrl: newItem.imageUrl ?? "",
      });

      await loadItems();
    } catch (error) {
      console.error(error);
      setError("Kunne ikke tilføje vare");
    }
  };

  const removeItem = async (id) => {
    try {
      setError("");
      await deleteItem(id);
      await loadItems();
    } catch (error) {
      console.error(error);
      setError("Kunne ikke slette vare");
    }
  };

  const handleScannerSubmit = async () => {
    const input = scannerInput.trim();

    if (!input) {
      setBarcodeError("Indtast eller scan en stregkode");
      return;
    }

    const command = input.toLowerCase();

    if (command === BARCODE_COMMANDS.AMOUNT) {
      setScannerMode(BARCODE_COMMANDS.AMOUNT);
      setBarcodeError("Scan nu varen du vil ændre antal på");
      setScannerInput("");
      return;
    }

    if (command === BARCODE_COMMANDS.DELETE) {
      setScannerMode(BARCODE_COMMANDS.DELETE);
      setBarcodeError("Scan nu varen du vil slette");
      setScannerInput("");
      return;
    }

    const parsedBarcode = parseBarcode(input);

    if (!parsedBarcode) {
      setBarcodeError("Indtast eller scan en stregkode");
      return;
    }

    if (!isValidBarcode(parsedBarcode)) {
      setBarcodeError("Ugyldigt stregkodeformat. Brug 8, 12 eller 13 cifre.");
      return;
    }

    try {
      setIsLookingUpBarcode(true);
      setBarcodeError("");

      const existingItem = items.find((item) => item.barcode === parsedBarcode);

      if (scannerMode === BARCODE_COMMANDS.DELETE) {
        if (!existingItem) {
          setBarcodeError("Varen blev ikke fundet og kan derfor ikke slettes");
          return;
        }

        await deleteItem(existingItem.id);
        await loadItems();

        setScannerMode(null);
        setScannerInput("");
        setBarcodeError("Varen blev slettet");
        return;
      }

      if (scannerMode === BARCODE_COMMANDS.AMOUNT) {
        if (!existingItem) {
          setBarcodeError("Varen blev ikke fundet og kan derfor ikke redigeres");
          return;
        }

        const currentAmount = existingItem.amount ?? existingItem.quantity ?? 1;

        const newAmount = prompt(
          `Indtast nyt antal for ${existingItem.name}`,
          currentAmount
        );

        if (newAmount === null) {
          setScannerMode(null);
          setScannerInput("");
          return;
        }

        const parsedAmount = Number(newAmount);

        if (!Number.isInteger(parsedAmount) || parsedAmount < 0) {
          setBarcodeError("Antal skal være et helt tal på 0 eller derover");
          return;
        }

        await updateItemAmount(existingItem.id, parsedAmount);
        await loadItems();

        setScannerMode(null);
        setScannerInput("");
        setBarcodeError("Antal blev opdateret");
        return;
      }

      const product = await fetchProductByBarcode(parsedBarcode);

      if (existingItem) {
        const currentAmount = existingItem.amount ?? existingItem.quantity ?? 0;
        await updateItemAmount(existingItem.id, currentAmount + 1);
      } else {
        await addItem({
          name: product?.name || `Ukendt vare (${parsedBarcode})`,
          amount: 1,
          barcode: parsedBarcode,
          imageUrl: product?.imageUrl || "",
        });
      }

      await loadItems();
      setScannerInput("");
    } catch (error) {
      console.error(error);
      setBarcodeError("Kunne ikke udføre scanner-handlingen");
    } finally {
      setIsLookingUpBarcode(false);
    }
  };

  const handleScannerKeyDown = (e) => {
    if (e.key === "Enter") {
      e.preventDefault();
      handleScannerSubmit();
    }
  };

  return {
    search,
    setSearch,
    filteredItems,
    loading,
    error,
    showModal,
    setShowModal,
    scannerInput,
    setScannerInput,
    barcodeError,
    isLookingUpBarcode,
    handleAddItem,
    removeItem,
    handleScannerSubmit,
    handleScannerKeyDown,
    loadItems,
  };
}