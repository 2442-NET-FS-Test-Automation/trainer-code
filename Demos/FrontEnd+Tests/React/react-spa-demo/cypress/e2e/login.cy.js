// Lets test the login flow via the real UI
describe("login", () => {

    // Before each test we want to make sure we're on the correct page
    beforeEach(() => {
        cy.visit("/login");
    });

    it("signs in the seeded admin and updates the header", () => {
        
        cy.contains("label", "Username").find("input").type("ada");
        cy.contains("label", "Password").find("input").type("pass123!");
        cy.contains("button", "Sign in").click();
        
        // On successful login - navigates back to catalog and updates the header
        cy.contains(".auth-box span", "ada (admin)");
        cy.contains("button", "Sign out");

        // Role-gated nav - only admins see the admin nav link. Ada is an admin
        cy.contains("nav a", "Admin");
    });

    it("shows the error message for bad credentials", () => {
        cy.contains("label", "Username").find("input").type("ada");
        cy.contains("label", "Password").find("input").type("wrong-password");
        cy.contains("button", "Sign in").click();

        cy.get("p.error").should("have.text", "Invalid username or password.");

        // We should still be on /login, and anonymous
        cy.url().should("include", "/login");
        cy.contains(".auth-box a", "Sign in");

    });



});