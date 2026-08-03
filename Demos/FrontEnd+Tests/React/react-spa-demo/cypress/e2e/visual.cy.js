// Visual regression - assert what the page looks like not what the DOM says
// cy.CompareSnapshot screenshots the page and diffs the pixels. 
// This lets you know if some change in the .css or something breaks the page
// visually. 

// This suit will be pinned to ONE browser: each browser's rendering engine  differs
// a chrome screenshot will fail against a baseline Opera screenshot

describe("catalog visual regression", {browser: "electron"}, () => {

    it("catalog page matches the baseline", () => {
        // Stub the network because we need those pixels to be deterministic
        // if the db changes our render changes if we are calling the db
        cy.intercept("GET", "**/api/Inventory", {fixture: "../fixtures/inventory.json"}).as("getInventory");

        cy.visit("/");
        cy.wait("@getInventory");
        cy.get("article.card").should("have.length", 3);

        // While we're here - lets use that task that we set up inside of setupNodeEvents 
        // the browser can't print to our CLI - we can use that task to print out to our console
        // when this runs
        cy.task("log", "visual: comparing catalog-stubbed against the committed baseline image");

        
        // cy.document().then((doc) => {
        //     const style = doc.createElement("style");
        //     style.innerHTML = ".card h3 a { color: red; }";
        //     doc.head.appendChild(style);
        // })

        // First run writes the baseline image, every later run compares against it
        cy.compareSnapshot("catalog-stubbed");

    });
})