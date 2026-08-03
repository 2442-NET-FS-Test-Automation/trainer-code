import { SearchBar } from "../../src/components/SearchBar";

// We are going to use cy.spy() - a test double. Same functionality 
// as moq.verify(). We can record every call made to it, who called it, what arguments,
// how many times, etc - and assert on that afterwards. 

describe("SearchBar (component)", () => {

    it("renders the value passed in by parent", () => {
        // Mounting searchbar with an empty OnChange
        cy.mount(<SearchBar value="clean" onChange={() => {}} />)

        cy.get("input[type=search]").should("have.value", "clean");
    });

    it("reports every keystroke to the parent", () => {
        // Creating our spy method
        const onChange = cy.spy().as("onChange");

        // mounting our searchbar with our spy() onChange
        cy.mount(<SearchBar value="" onChange={onChange} />)
        

        cy.get("input[type=search]").type("dune");

        cy.get("@onChange").should("have.callCount", 4);
        cy.get("@onChange").should("have.been.calledWith", "d");
        cy.get("@onChange").should("have.been.calledWith", "e");
    });

})