import { BookCard } from "../../src/components/BookCard";
import type { InventoryItem } from "../../src/types";

// Memory router is a react router - same behavior, you can render components in it 
// but it doesn't interact with the Browser URL bar OR history. Used for testing - like
// what we're doing now. 
import { MemoryRouter } from "react-router-dom"; 

// Component testing: cy.mount renders ONE component into a real browser tab - 
// no SPA dev server, no API no db. This is a unit test - though we are testing
// rendering behavior. 

describe("BookCard (component)", () => {

    // Arrange: an inventory item we can reuse for the props of the card
    const item: InventoryItem = { sku: "BK-001", name: "Clean Code", currentStock: 5};

    it("renders name, sku and stock", () => {
        // BoolCard renders a <Link> so it needs a router context. MemoryRouter
        // provides one without a real URL bar. If we had RTL unit tests outside
        // of cypress we'd do the same thing

        // This is that mount command we configured
        cy.mount(
            <MemoryRouter>
                <BookCard item={item} />
            </MemoryRouter>
        );

        cy.contains("h3", "Clean Code");
        cy.contains("dd", "BK-001");
        cy.contains("dd", "5");
    });

    it("marks a zero-stock item with the out class", () => {
        // This never hits the db so we provide our own zero stock item
        cy.mount(
            <MemoryRouter>
                <BookCard item = {{ sku: "BK-001", name: "Clean Code", currentStock: 0}} />
            </MemoryRouter>
        );

        cy.get("dd.out").should("have.text", "0");
    });

    it("hides the stock line in compact mode", () => {
        cy.mount(
        <MemoryRouter>
            <BookCard item={item} compact />
        </MemoryRouter>,
        );

        cy.contains("dt", "In stock").should("not.exist");
    });
    
});