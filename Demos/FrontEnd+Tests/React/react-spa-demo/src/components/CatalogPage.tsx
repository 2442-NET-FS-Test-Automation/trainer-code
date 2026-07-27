import { useEffect, useState } from "react";
import { BookCard } from "./BookCard";
// import { catalog } from "../data/catalog";
import { SortDirection, type FetchState, type InventoryItem } from "../types";
import { getInventory } from "../api/inventory";
import { SearchBar } from "./SearchBar";

// CatalogPage will be composed of many BookCards - they will be child components of 
// CatalogPage. CatalogPage will also own the State. 
export function CatalogPage() {
    // We are going to use a hook called useState to store information that will persist 
    // between component re-renders as well as be passed down to the child. 
    // Hooks are functions that allow you to do certain things based on the component lifecycle
    // Hook rules (this is from the React documentation)
    // 1. Only call on hooks from the top level (no loops, no nested functions, not inside a try-catch)
    // 2. Only call Hooks from react functions (i.e. function components and custom hooks you write)
    
    // Lets use that useState hook to store some info - today items holds static info
    // from a file. Tomorrow from the API
    //const [items] = useState(catalog);

    // Local UI state - here we will hold whether a user has toggled "compact" mode on a BookCard
    // and then pass it down as a prop. When the user toggles this value (via a button or something)
    // state is updated, React re-renders, and the cards render "compact"
    // const [compact, setCompact] = useState(false);
    const [items, setItems] = useState<InventoryItem[]>([]);
    const [fState, setFState] = useState<FetchState>("idle");

    // Search + sort state - lifted from SearchBar and shared with children as needed.
    const [userQuery, setUserQuery] = useState(""); // user search input
    const [dir, setDir] = useState<SortDirection>(SortDirection.Ascending);

    // useEffect is another hook that lets us hook into the mounting part of the lifecycle
    // It lets us run some code when the component mounts. We can use this to set state WITHOUT
    // triggering an immediate re-render. Subsequent calls to setState() for a given state value
    // will trigger re-renders as normal. 
    useEffect(() => {
        // Flag to allow for choosing the correct fState value later
        let active = true;
        
        setFState("loading"); 
        
        getInventory()
            .then((data) => {
                if(!active) return;
                setItems(data); // set state value for Items to our data from Api
                setFState("loaded");
            })
            .catch(() => {
                if (active) setFState("failed");
            });
        
        
        // useEffect needs a cleanup function to be returned
        // we still want to set active = false - we just do it as part of the cleanup
        return () => {
            active = false;
        }
    }, []);

    // We don't want to filter that items list that we got from useEffect in place.
    // we want state to stay as is - otherwise as we sort we will lose things we want 
    // from our state. We can derive that info and store it locally - not in state
    // for the purpose of rendering
    const visibleBooks = [...items]
        .filter((i) => i.name.toLowerCase().includes(userQuery.toLowerCase()))
        .sort((a, b) =>
            dir === SortDirection.Ascending
                ? a.name.localeCompare(b.name)
                : b.name.localeCompare(a.name)
        );

    // Conditional rendering based on branching return paths - totally allowed!
    if (fState === "idle" || fState === "loading")  return <p>Loading catalog...</p>

    if (fState === "failed")
        return <p>Could not reach the API. Is it running on :5173? Check CORS.</p>

    return (
        <section>
            <div className="toolbar">
                <h2>Catalog</h2>
                <SearchBar value={userQuery} onChange={setUserQuery}/>
                <button
                    type="button"
                    onClick={() => 
                        setDir((d) => // Clicking the button toggles the opposite sort direction
                            d === SortDirection.Ascending
                                ? SortDirection.Descending
                                : SortDirection.Ascending,
                        )
                    }
                > 
                    Sort {dir === SortDirection.Ascending ? "Z-A" : "A-Z"}
                </button>
            </div>

            {visibleBooks.length === 0 ? (
                <p>No books match "{userQuery}".</p>
            ) : (
                <div className="cards">
                    {visibleBooks.map((item) => (
                        <BookCard key={item.sku} item={item} />
                    ))}
                </div>
            )}
        </section>
    );


    // return(
    //     <>
    //         <div className="toolbar">
    //             <h2>Catalog</h2>    {/* When our button is clicked, invert the value of compact */}
    //             <button type="button" onClick={() => setCompact((c) => !c) }>
    //                 {compact ? "Show detail" : "Compact view"}
    //             </button>
    //         </div>

    //         {/* Here we can render our bookcards - we have an list that we don't know the size of
    //             because it comes from the API - from outside our front end react app. We will render 
    //             one BookCard per item in the items array.*/}
    //         <div className="cards">
    //             {   

    //                 items.map((item) => (
    //                     // The minimum to render a bookcard is a item and compact - its props
    //                     // Because we are rendering based on a list - we want to pass a key
    //                     // so react can track specific BookCard's
    //                     <BookCard key={item.sku} item={item} compact={compact}/>
    //                 ))
    //             }

    //         </div>
        
    //     </>
    // )
}