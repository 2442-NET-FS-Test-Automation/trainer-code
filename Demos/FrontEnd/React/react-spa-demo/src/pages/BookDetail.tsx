import { useEffect, useState } from "react";
import { Link, useParams } from "react-router-dom";
import { getInventoryItem } from "../api/inventory";
import type { InventoryItem, FetchState } from "../types";

// BookDetail will use a route with a URL parameter - useParams reads that `:sku` from the path
export function BookDetail() {

    // grabbing sku from URL path
    const { sku } = useParams<{ sku: string}>();
    const [item, setItem] = useState<InventoryItem | null>(null);
    const [fState, setFState] = useState<FetchState>("idle");

    // useEffect- this time we have a dependency. The effect (the api call we make)
    // depends on "sku" - if a user navigates ta different sku, useEffect re-runs
    useEffect(() => {
        
        if (!sku) return;
        let active = true;
        setFState("loading");

        getInventoryItem(sku)
            .then((data) => {
                if (!active) return;
                setItem(data);
                setFState("loaded");
            })
            .catch(() => {
                if (active) setFState("failed");
            });
        
        return () => {
            active = false;
        };
    }, [sku]);

    if (fState === "idle" || fState === "loading") return <p>Loading...</p>
    
    if (fState === "failed" || !item)
        return (
            <p>
                Book {sku} not found. <Link to="/">Back to catalog</Link>
            </p>
        );
    
    return (
        <article>
            <p>
                <Link to="/">&larr; Back to catalog</Link>
            </p>
            <h2>{item.name}</h2>
            <p>SKU: {item.sku}</p>
            <p>In stock: {item.currentStock}</p>
        </article>
    )
}