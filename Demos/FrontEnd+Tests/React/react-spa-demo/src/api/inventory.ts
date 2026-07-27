import { api } from "./client";
import type { InventoryItem, SupplierPrice } from "../types";

// Here lives the catalog data call to the api.

export interface CreateInventoryBody {
    sku: string;
    name: string;
    price: number;
    currentStock: number;
}

export async function getInventory(): Promise<InventoryItem[]> {

    // Axios is really nice to us - it makes the call and parses the response from JSON
    // all in one
    const response = await api.get<InventoryItem[]>("/api/Inventory");
    return response.data; 
}

export async function getInventoryItem(sku: string): Promise<InventoryItem> {
    const response = await api.get<InventoryItem>(`/api/Inventory/${sku}`);
    return response.data;
}

// GET /api/Inventory/{sku}/supplier-price - requires ANY signed in user. No token -> 401
export async function getSupplierPrice(sku: string): Promise<SupplierPrice> {
    const response = await api.get<SupplierPrice>(`/api/Inventory/${sku}/supplier-price`);
    return response.data;
}

// Finally - two calls that SHOULD be admin-only
export async function createBook(body:CreateInventoryBody): Promise<InventoryItem> {
    const response = await api.post<InventoryItem>("/api/Inventory", body);
    return response.data;
}

export async function deleteBook(sku: string): Promise<void> {
    await api.delete(`/api/Inventory/${sku}`);
}

