// We can create custom Cypress commands. We can then use them 
// like we do cy.get() or cy.visit()

// We define some logic once, we can can resuse it everywhere

// Lets write one that resets the database. Because we put that endpoint
// inside of the minimal API - I have it running. And we will need
// to send a post request to its localhost:5101 URL
Cypress.Commands.add("resetInventory", () => {
    // NOTE: the minimal API's route is literally "/inventory/rest" (typo in Program.cs)
    cy.request("POST", "http://localhost:5101/inventory/rest")
});

// Lets make one to do a programmatic login. 
// Same thing that our login test did - but no UI walking. We just send the request 
// and get that token back so our app (and other tests can use it.)
// We still test the login form inside of login.cy.js - but any test that needs auth
// can just skip that - and call this instead
Cypress.Commands.add("login", (username, password) => {
    cy.request("POST", "http://localhost:5137/auth/login", { username, password })
        .then(({ body }) => {
            window.localStorage.setItem("library.token", body.token)
        });
});