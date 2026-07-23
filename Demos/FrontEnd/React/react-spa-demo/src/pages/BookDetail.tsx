import { useEffect, useState } from "react";
import { Link, useParams } from "react-router-dom";
import { getInventoryItem, getSupplierPrice } from "../api/inventory";
import { useAuth } from "../auth/useAuth";
import type { InventoryItem, FetchState } from "../types";

// BookDetail will use a route with a URL parameter - useParams reads that `:sku` from the path
export function BookDetail() {

    // grabbing sku from URL path
    const { sku } = useParams<{ sku: string}>();
    const { status } = useAuth();
    const [item, setItem] = useState<InventoryItem | null>(null);
    const [fState, setFState] = useState<FetchState>("idle");

    // supplier price is a signed-in only call; we will hold its result (or failure/error) in local state
    // for rendering
    const [price, setPrice] = useState<number | null>(null);
    const [priceMsg, setPriceMessage] = useState<string | null>(null);


    // useEffect- this time we have a dependency. The effect (the api call we make)
    // depends on "sku" - if a user navigates ta different sku, useEffect re-runs
    useEffect(() => {
        
        if (!sku) return;
        let active = true;
        setFState("loading");
        setPrice(null);
        setPriceMessage(null);

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

    async function loadSupplierPrice() {

        // First, check to make sure we have a sku
        if (!sku) return;

        setPrice(null);

        try{
            const result = await getSupplierPrice(sku);
            setPrice(result.supplierPrice);
        }catch{
            setPriceMessage("Could not load the supplier price");
        }

    }



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

            {/* Role gated UI: the supplier price needs a token. Anonymous users get a prompt
                telling them to log in to see supplier price - logged in users get a button to grab the price */}

            {status === "authenticated" ? (
                 <>
                    <button type="button" onClick={loadSupplierPrice}>
                        Show supplier price
                    </button>
                    {price !== null && <p>Supplier price: ${price.toFixed(2)}</p>}
                    {priceMsg && <p className="error">{priceMsg}</p>}
                 </>
            ) : (
                <p> Sign in to see supplier prices</p>
            )}
        </article>
    )
}