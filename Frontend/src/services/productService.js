export async function fetchProductByBarcode(barcode) {
  const response = await fetch(
    `https://world.openfoodfacts.org/api/v2/product/${barcode}.json`
  );

  if (!response.ok) {
    throw new Error("Kunne ikke slå stregkoden op i Open Food Facts");
  }

  const data = await response.json();

  if (!data || data.status === 0 || !data.product) {
    return null;
  }

  const product = data.product;

  return {
    name:
      product.product_name_da ||
      product.product_name ||
      product.generic_name_da ||
      product.generic_name ||
      product.brands ||
      "Ukendt vare",
    barcode,
    imageUrl:
      product.image_front_url ||
      product.image_url ||
      product.image_small_url ||
      "",
  };
}