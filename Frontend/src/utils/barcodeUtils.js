export function isValidBarcode(barcode) {
  const cleaned = barcode.trim();
  if (!/^\d+$/.test(cleaned)) return false;
  return cleaned.length === 8 || cleaned.length === 12 || cleaned.length === 13;
}

export function parseBarcode(barcode) {
  return barcode.trim().replace(/\s+/g, "");
}