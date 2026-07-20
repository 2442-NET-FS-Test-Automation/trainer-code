// TS has interfaces as well as classes
// We will use interfaces ALOT as type contracts
// for incoming data from our API, mirroring our ASP.NET API's DTOs
// The main pain point is we need the field names to be 1:1

// Mirror InventoryDTO (GET /api/Inventory)
export interface InventoryItem {
    sku: Sku; //we'll change this later....
    name: string;
    currentStock: number;
}

// Export just lets us import into other TS files later on.
export interface SupplierPrice {
    sku: Sku; //this one too
    supplierPrice: number;
}

// More TS types 

// We can use type aliases to give a custom name to any type
// Useful for documenting semantic meaning/intent
export type Sku = string; // aliasing strings as Skus

// Union type: a type that allows a variable to be one of several types
// useful for creating custom types to allow for multiple parameter types in a method
// can also use it like an enum
export type FetchState = "idle" | "loading" | "loaded" | "failed"

// Enums - lets use a numeric enum to list out our error codes 
// that we can expect to get back from API

export const HttpStatus = {
    Ok : 200,
    Created : 201,
    NoContent : 204,
    BadRequest : 400,
    Unauthorized : 401,
    Forbidden : 403,
    NotFound : 404
} as const;
export type HttpStatus = typeof HttpStatus[keyof typeof HttpStatus];

// String enum - same logic as C# enums, same as the one above
export const SortDirection = {
    Ascending : "asc",
    Descending : "desc"
} as const;
export type SortDirection = typeof SortDirection[keyof typeof SortDirection];

// Utility types - we can derive types from existing types 
// saves you time typing

// Partial<T>: just makes every field optional
export type InventoryPatch = Partial<InventoryItem>; 

// Omit<T, K>: Take a type and shave off a property. In our case the server assigns a sku
// so we don't need to send it
export type NewInventoryItem = Omit<InventoryItem, "sku">;

