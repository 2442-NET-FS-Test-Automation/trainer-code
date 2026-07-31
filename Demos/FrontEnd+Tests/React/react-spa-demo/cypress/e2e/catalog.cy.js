describe("catalog filtering and sorting", () => {

    // We can, within our describe block have multiple it() tests
    // we can also have a beforeEach() - some sort of setup function
    // as well as an afterEach() - teardowns, and a few others.
    beforeEach(() => {
        // Before each test....
        cy.visit("/"); // make sure we're on the catalog page
        cy.get("article.card").should("have.length.at.least", 3); // make sure we render cards
    });

    it("filters by name as the user types", () => {
        // We grab things on screen via cy.get() - then we can either act on them
        // to simulate user behavior OR assert something about them, with .should()
        // Find our search text input - type "clean" into it
        cy.get('input[type="search"][placeholder="Filter by name..."]').type("clean");

        // Now we can assert things about what the UI SHOULD be reflecting. 
        cy.get("article.card").should("have.length", 1);
        cy.get("article.card h3 a").should("contain.text", "Clean Code");
    })

    it("shows empty state for bad search", () => {
        // Find the search, type these letters
        cy.get('input[type="search"]').type("zzz");

        cy.contains('No books match "zzz".')
    });

    it("sorts Z-A and back", () => {
        // Default order is A-Z on mount
        // meaning our first card should be the "Clean Code" book's
        cy.get("article.card h3 a").first().should("contain.text", "Clean Code");

        // The button labels the direction it should switch to. 
        // Find that button and click it as a user would
        cy.contains("button", "Sort Z-A").click();

        // Now that we've reversed the sort, so we're now in descending alphabetical order...
        cy.get("article.card h3 a").first().should("contain.text", "The Pragmatic Programmer");

        // And lets make sure we can transition back to the original order
        cy.contains("button", "Sort A-Z").click();
        cy.get("article.card h3 a").first().should("contain.text", "Clean Code");
    });

    it("links every card to its detail route", () => {
        // Lets do some chaining - multiple asserts

        // First - find all article elements with the card class attribute
        // then - only look at the first one.
        // within the article element's inner HTML, find the a element inside the h3
        // then assert with should, then another with and()
        cy.get("article.card").first().find("h3 a")
            .should("have.attr", "href")
            .and("include", "/inventory/")
    })

})