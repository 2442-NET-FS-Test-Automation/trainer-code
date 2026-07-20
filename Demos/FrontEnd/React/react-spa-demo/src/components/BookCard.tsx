import type { InventoryItem } from "../types";

// This file will hold our first component with TSX and 
// type checked arguments called Props. Components have a hierarchy
// A child component is simply a component that renders inside another component. 
// These Props (arguments for the component's function) flow from Parent -> Child
// Because we are using TS we need to create interfaces for our props
interface BookCardProps {
    item: InventoryItem;
    compact?: boolean;
}

export function BookCard( {item, compact = false}: BookCardProps ) {

    // This component is responsible for rendering Book Cards
    // Just all the data about a given book. Additionally, I want the option
    // to have both a "full" and "compact" mode for each BookCard

    // Components return JSX (in our case TSX): HTML like syntax,
    // that allows us to drop in TS expressions and code using { }
    return(
        <article className="card">
            <h3>{item.name}</h3>
            <dl>
                <dt>SKU</dt>
                <dd>{item.sku}</dd>
                {/* Here we can conditionally render based on the value of compact*/}
                {!compact && (
                    <>
                        <dt>In stock</dt>
                        <dd className={item.currentStock === 0 ? "out" : ""}>
                            {item.currentStock}
                        </dd>
                    </>
                )}

            </dl>
        </article>
    )
}

