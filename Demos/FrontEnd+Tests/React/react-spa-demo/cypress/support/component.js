// Component testing support - runs before EVERY COMPONENT SPEC
// same as e2e.js but for component tests. 

import "./commands";
import "@cypress/code-coverage/support"
import { mount } from "cypress/react";

// We need the React app's real stylesheets
import "../../src/App.css"
import "../../src/index.css"

Cypress.Commands.add("mount", mount);