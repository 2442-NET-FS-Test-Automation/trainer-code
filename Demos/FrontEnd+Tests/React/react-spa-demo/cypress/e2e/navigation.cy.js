// Lets test our routing - how a user would experience it

describe("navigation", () => {

    it("goes from a card to the detail page and back", () => {
        cy.visit("/");
        cy.get("article.card").should("have.length.at.least", 1);

        // Default sort is A-Z, so given our data the first book is Clean Code
        cy.get("article.card h3 a").first().click();
        cy.url().should("include", "/inventory/BK-001"); // asserting something about the URL
        cy.contains("SKU: BK-001");
        cy.contains("In stock: ");

        // We aren't logged in - and anonymous users see text prompting them 
        // to log in to see supplier price, lets check for that
        cy.contains("Sign in to see supplier prices");

        cy.contains("a", "Back to catalog").click();
        cy.url().should("not.include", "/inventory");
        cy.contains("h2", "Catalog");
    });

    it("serves the static About page route", () => {
        cy.visit("/about");
        cy.contains("h2", "About");
        cy.contains("the client half of our web app.")
    })

    it("shows the not-found page for a bad route", () => {
        cy.visit("/no-such-page");
        cy.contains("Page not found");
    });



});