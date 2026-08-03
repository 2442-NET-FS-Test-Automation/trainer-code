import { defineConfig } from "cypress";
import registerCodeCoverage from "@cypress/code-coverage/task";
import getCompareSnapshotsPlugin from "cypress-image-diff-js/plugin";

export default defineConfig({
    e2e: {
        baseUrl: "http://localhost:5173",
        supportFile: "cypress/support/e2e.js",
        setupNodeEvents(on, config) {
            // we are going to use cy.task(): to write a tiny plugin
            // we will then use outside plugins to see code coverage, maybe do some other
            // stuff. Tasks must return something and null works. 
            on("task", {
                log(message) {
                    console.log(`[spec] ${message}`);
                    return null;
                }
            })
            // Adding code coverage to our Cypress tests
            // collects that window._coverage_ after every test
            // writes the coverage report when the test run ends
            registerCodeCoverage(on, config);
            
            // Adding our visual regression plugin
            getCompareSnapshotsPlugin(on, config);

            // A setupNodeEvents that touches config must return it
            return config;
        }
    },
    component: {
        // Component tests mount ONE component: Cypress spins up it's own Vite dev server
        // we don't need the actual SPA running, nor the API or database for component tests
        devServer: {
            framework: "react",
            bundler: "vite",
        }
    }
})