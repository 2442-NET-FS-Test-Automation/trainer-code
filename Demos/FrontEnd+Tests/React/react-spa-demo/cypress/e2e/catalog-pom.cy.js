import { CatalogPage } from "../pages/CatalogPage";

// The same filter and sort ideas that catalog.cy.js tests - rewritten 
// with the CatalogPage.js Page Object. The assertions are identical 
// SELECTORS have been moved into CatalogPage

describe("catalog via a page object", () => {

    // Creating a CatalogPage object to call our methods
    const catalog = new CatalogPage();

    it("filters through the page object", () => {
        catalog.visit().search("Clean");
        catalog.cards().should("have.length", 1);
        catalog.firstTitle().should("contain.text", "Clean Code");
    });

    it("sorts through the page object", () => {
        catalog.visit().toggleSort();
        catalog.firstTitle().should("contain.text", "The Pragmatic Programmer");
    });

});