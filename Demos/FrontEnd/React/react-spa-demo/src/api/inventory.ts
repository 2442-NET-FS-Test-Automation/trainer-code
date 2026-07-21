import { api } from "./client";
import type { InventoryItem } from "../types";

// Here lives the catalog data call to the api.

export async function getInventory(): Promise<InventoryItem[]> {

    // Axios is really nice to us - it makes the call and parses the response from JSON
    // all in one
    const response = await api.get<InventoryItem[]>("/api/Inventory");
    return response.data; 
}