// Lets create an actual program - eventually 
// this will hit our API
// For now, we can use static data

// Notice we import from a .js file - that's the runtime path
// because what actually runs is /dist/demo.js
import { InventoryItem, HttpStatus, SortDirection, SupplierPrice,
    FetchState, Sku
 } from "./types.js";
import { ApiClient } from "./ts-client.js";

// For now, lets create what our API will hand back to us
const catalog: InventoryItem[] = [
    { sku: "BK-001", name: "Clean Code", currentStock: 5 },
    { sku: "BK-002", name: "Dune", currentStock: 3 },
    { sku: "BK-003", name: "Refactoring", currentStock: 8 }
];

// Lets see our first TS function 
// We type arguments and we type the return
function printCatalog(items: InventoryItem[]): void {
    for (const item of items) {
        console.log(`${item.sku} ${item.name} ${item.currentStock}`);
    }
}

console.log("catalog: ");
printCatalog(catalog); //calling our function

// Enums are RUNTIME objects - interfaces and aliases are NOT
// In TS numeric enums actually map both ways
console.log(HttpStatus.Unauthorized); // 401
console.log(HttpStatus[401]); // Unauthorized

// Lets use our client
const api = new ApiClient();


const liveCatalog = await api.getJson("/api/Inventory");
console.log(liveCatalog);