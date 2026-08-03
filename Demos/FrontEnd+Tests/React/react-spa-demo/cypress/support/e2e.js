// Anything inside of this file loads before every test spec file (cy.js)
// Global hooks and commands live here
import "./commands";
import "@cypress/code-coverage/support"
import compareSnapshotCommand from "cypress-image-diff-js";

compareSnapshotCommand();