// Testing our admin create/delete workflow
// but using our commands. we will log in programmatically with a database reset
// first
describe("admin form", () => {
    
    beforeEach(() => {
        cy.resetInventory();
        cy.fixture("users.json").then((users) => {
            cy.login(users.admin.username, users.admin.password);
        })
        cy.visit("/admin");
        cy.contains("h2", "Admin - ada"); // this only renders if we pass the route guard
    });

    it("creates a book, then deletes via quick-find copy", () => {
        cy.get('input[placeholder="SKU"]').type("BK-E2E");
        cy.get('input[placeholder="Name"]').type("Cypress E2E book");
        // The price and stock inputs have no placeholder - grab the form's
        // number inputs in document order (price first, then stock)
        cy.get('.admin-form input[type="number"]').eq(0).clear().type("19.99");
        cy.get('.admin-form input[type="number"]').eq(1).clear().type("7");
        cy.contains("button", "Create").click();

        cy.contains("Created BK-E2E - Cypress E2E book");

        // Next... lets use that uncontrolled useRef form... because why not
        cy.get('input[placeholder="Quick SKU (Uncontrolled)"]').type("BK-E2E");
        cy.contains("button", "Copy into form").click();

        cy.get('input[placeholder="SKU"]').should("have.value", "BK-E2E");

        cy.contains("button", "Delete by sku").click();
        cy.contains("Deleted BK-E2E");
    });

    it("surfaces the failure message when creation fails", () => {
        // A negative price violates the DTO (on the API) [Range] check
        // the API response with a 400, we should see that surfaced to the user
        cy.get('input[placeholder="SKU"]').type("BK-E2E");
        cy.get('input[placeholder="Name"]').type("Cypress E2E book");
        cy.get('.admin-form input[type="number"]').eq(0).clear().type("-19.99");
        cy.get('.admin-form input[type="number"]').eq(1).clear().type("7");
        cy.contains("button", "Create").click();

        cy.contains("Create failed - check fields, you may lack admin role.");
    });
});

