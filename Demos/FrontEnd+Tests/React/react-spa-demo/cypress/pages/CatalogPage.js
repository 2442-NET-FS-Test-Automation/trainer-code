// Page Object Model (POM): One class per page of my SPA (or MPA, whatever)
// Selectors and page actions live in this file. A redesign to a page
// gets updated in one place, rather than hunting down dozens of cypress selectors. 
// Selenium also has a POM model that we will explore once we get to Selenium

export class CatalogPage {

    // First,  a method to test "readiness" (has the page loaded successfully)
    visit() {
        cy.visit("/");
        cy.get("article.card").should("have.length.at.least", 1);
        return this; // This "return this" is what allows for method chaining
        // example. .visit().get() - etc 
    }

    // Method for searching via our searchbar on the page
    search(text) {
        cy.get('input[type="search"][placeholder="Filter by name..."]').type(text);
        return this;
    }

    // Method for toggling the sort order
    toggleSort() {
        cy.get(".toolbar button").click();
        return this;
    }

    // Pure selection method - just get all the cards so we don't have to type
    // cy.get("article.card") over and over again
    cards() {
        return cy.get("article.card")
    }

    // Grab the first title that appears on the page in a BookCard
    firstTitle() {
        return cy.get("article.card h3 a").first();
    }


}