// cy.intercept(): subbing out the network seam 
// the SPA still runs, but when cypress detects an HTTP request 
// that we have provided a deterministic response to - it will 
// intercept it and return our dummy data
describe("catalog over a subbed network", () => {

    it("renders exactly the objects in our inventory fixture", () => {
        // Lets write that interceptor 
        // That final "as" is how we will refer to it in the rest of the test
        cy.intercept("GET", "**/api/Inventory", { fixture: "inventory.json"}).as("getInventory");

        cy.visit("/");
        cy.wait("@getInventory"); // waiting on that request 

        cy.get("article.card").should("have.length", 3);

        // We have a book in inventory.json that has a stock of 0
        // we can count on it being there, not like the db which could change
        cy.contains("article.card", "Stubbed Book Two")
            .find("dd.out")
            .should("have.text", "0");

    });

    it("shows the failure message if the API is down/dead", () => {
        // Forcing a 500 against the live API would be difficult
        // We can test for front end behavior when a 500 (or 400) occurs with one line
        cy.intercept("GET", "**/api/Inventory", { statusCode: 500, body: {}}).as("getInventory");

        cy.visit("/")
        cy.wait("@getInventory");

        cy.contains("Could not reach the API. Is it running on :5173? Check CORS.");
    });

});