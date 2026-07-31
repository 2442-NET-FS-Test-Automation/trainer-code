// First E2E testing spec: we test the whole app in one assert. If this passes,
// the SPA served, React mounted our components (for the catalog page), axios 
// reached the API, the API reached SQL Server, it all returned - and the



// describe.... it pattern 
// catalog rendered real data
describe("catalog smoke test", () => {
    it("loads the catalog from the live API", () => {
        cy.visit("/"); // go to the catalog (in our case home) page
        // Find the h1 header, make sure it has text "Library"
        cy.get("h1").should("have.text", "Library");
        
        // Because Cypress runs in the browser alongside your app - we can 
        // have it retry things easily. By default it'll retry for 4s until it gives up
        cy.get("article.card").should("have.length.at.least", 1);

    });
});