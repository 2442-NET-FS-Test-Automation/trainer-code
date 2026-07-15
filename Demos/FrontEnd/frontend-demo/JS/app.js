// Using hard coded items until we call our API
// The same catalog shape js/app.js renders - and GET /api/Inventory returns.
// let catalogItems = [
//     { sku: "BK-101", name: "Clean Code",                price: 29.99, currentStock: 12 },
//     { sku: "BK-102", name: "The Pragmatic Programmer",  price: 34.99, currentStock: 7 },
//     { sku: "BK-103", name: "Design Patterns",           price: 44.99, currentStock: 3 },
//     { sku: "BK-104", name: "Refactoring",               price: 39.99, currentStock: 0 },
// ];

// Switching over to an empty array - we will populate this via fetch
let catalogItems = []

async function loadCatalog() {

    const container = document.querySelector("#catalog-cards");

    // We've been using .innerHTML, this lets us swap out an elements innerHTML wholesale
    // we can also just create elements in memory - and then append to what's rendering
    // in an element. 

    const loading = document.createElement("p"); // create a <p></p> element
    loading.className = "hint"; // <p class="hint"></p>
    loading.textContent = "loading..." // <p class="hint">..loading</p>


    container.innerHTML = "";
    container.appendChild(loading);

    try{
        // Doing our fetch the "proper way"
        const response = await fetch(`${API}/api/Inventory`); // sending my GET
        
        // We need to make sure we got a 200-family code back
        // If we got back a 400/500 - alert the user, break out of function
        if (!response.ok) {
            container.innerHTML = `<p class="hint">API said ${response.status}</p>`;
            return;
        }

        catalogItems = await response.json(); // deserializing a body is an async function in JS
        renderCards(catalogItems);

    } catch (err) {
        // Fetch doesn't throw errors for 400/500s. This is for stuff like
        // "CORS Policy rejected us" and "API isn't even on"
        console.error(err); // log error to browser console
        container.innerHTML = `<p class="hint">cannot reach the API. Is it on?</p>`;
    }


}


// Lets do some rendering 
function renderCards(items) {
    // In JS the HTML page that the JS is running against is treated as an object
    // called the DOM.

    // I want to grab the element that contains my cards
    const container = document.getElementById("catalog-cards");

    if (items.length === 0) {
        container.innerHTML = `<p class="hint">nothing matches</p>`;
        return;
    }

    // Using a map to generate a whole block of HTML for each item in our array
    container.innerHTML = items.map(item => `
            <article class="card" data-sku="${item.sku}">
                <h3>${item.name}</h3>
                <dl>
                    <dt>SKU</dt><dd>${item.sku}</dd>
                    
                    <dt>In Stock</dt><dd>${item.currentStock}</dd>
                </dl>
                <button class="price-btn" data-sku="${item.sku}">Supplier price</button>
                <p class="supplier-price"></p>
            </article>
        `
    ).join("");
}

// Event listener section

// Event listeners let us "listen" for certain actions/states in the HTML page
// Elements loading, buttons being clicked, even hovering over a certain element. 

// Adding an event listener for our cards. This will handle every click 
// event that happens within the card. The card has a button - when it's clicked
// the event bubbles up - and we can catch it at it's parent/container
document.querySelector("#catalog-cards").addEventListener("click", (e) => {
    if (e.target.matches(".price-btn")) {
        console.log("clicked", e.target.dataset.sku); // eventually we will call the API here
    }

});

// Filtering with the search bar - as the user types filter the list of displayed cards
document.querySelector("#search").addEventListener("input", (e) => {
    // Grab the string value in the search bar
    const search = e.target.value.trim().toLowerCase();

    // Allowing for search by name or sku using a filter. 
    renderCards(catalogItems.filter(item => 
        item.name.toLowerCase().includes(search) || item.sku.toLowerCase().includes(search)
    ));

});

document.addEventListener("DOMContentLoaded", () => {
    loadCatalog();
    //renderCards(catalogItems);
    // fetch(`${API}/api/Inventory`)
    // .then(res => res.json()) 
    // .then(items => { 
    //     catalogItems = items;
    //     items.map(item => item.price = 5)
    // renderCards(items); });
});

// Transient/temp - we will wrap this in methods later
// This will be a promise chain example
// fetch(`${API}/api/Inventory`)
//     .then(res => res.json()) 
//     .then(items => { catalogItems = items; renderCards(items) });

