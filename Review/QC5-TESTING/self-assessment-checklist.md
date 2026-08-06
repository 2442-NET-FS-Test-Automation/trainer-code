# QC-6 Test Automation — Self-Assessment Checklist

Every objective from `qc-criteria/QC-6-Test-Automation.md`, reproduced verbatim and grouped by
section and tier (92 rows: 46 Must / 30 Should / 16 Nice). Work it top to bottom; anything you
cannot check, take back to the study guide's cluster for that section, then to the source note.

Scope annotations (indented lines) mark the rows whose hands-on half lands after this exam's
material window; the objective itself is still fair game at the annotated depth.

---

## 1. Testing Philosophy

### Must know
- [ ] Can I understand the testing process.
- [ ] Can I understand the importance of testing.
- [ ] Can I define and differentiate between positive and negative testing
- [ ] Can I describe Quality Assurance and Quality Control
- [ ] Can I differentiate between Automated and Manual Testing
- [ ] Can I explain what software requirements are and their importance in testing.

### Should know
- [ ] Can I understand and be able to talk about Testing Principles
- [ ] Can I analyze a sample test scenario and determine if it applies positive or negative testing principles.
- [ ] Can I be able to describe a testing mindset from a Tester's perspective
- [ ] Can I describe and explain the various objectives of testing

### Nice to Have
- [ ] Can I understand the difference between defect, error, failure
- [ ] Can I explain the difference between verification and validation.

## 2. Designing Test Cases Principles and Practice

### Must know
- [ ] Can I implement testing principles in the context of test case design and execution
- [ ] Can I describe common testing principles and their relevance across the testing lifecycle
- [ ] Can I explain the structure and purpose of the testing pyramid
- [ ] Can I apply black-box testing techniques to validate application behavior
- [ ] Can I apply white-box testing techniques to verify internal logic and flow
- [ ] Can I recognize appropriate general testing techniques to utilize based on documented requirements
- [ ] Can I design test cases optimized to minimize time and effort required for creation while maintaining high coverage
- [ ] Can I design structured test cases using appropriate techniques aligned with an RTM
- [ ] Can I create test data determined by test objectives and test objects
- [ ] Can I organize test data into appropriate storage solutions determined by the test object

### Should know
- [ ] Can I perform equivalence partitioning to organize and optimize test data
- [ ] Can I use domain knowledge and past defect patterns to hypothesize and document likely failure points  for error guess testing
- [ ] Can I perform structured and repeatable error guess testing
- [ ] Can I design exploratory test cases based on error guessing insights
- [ ] Can I plan and document exploratory testing strategies to catch edge-case defects
- [ ] Can I perform structured and repeatable exploratory testing
- [ ] Can I document findings from exploratory testing and map them to requirements in the RTM
- [ ] Can I understand the benefits and drawbacks of exploratory and error guess testing

### Nice to Have
- [ ] Can I walk through a testing scenario and select optimal techniques and practices
- [ ] Can I review test data to look for out of data resources and areas for data organization improvements
- [ ] Can I assess test case efficacy and recommend changes based on new Project Evaluation requirements
- [ ] Can I communicate objectives, risk, and risk mitigation strategies to stakeholders

## 3. Testing and Logging .NET Applications

### Must know
- [ ] Can I serialize/deserialize objects to common formats such as bitstreams or JSON.
- [ ] Can I do basic file I/O. Reading and writing to a text file.
- [ ] Can I use a testing framework to create meaningful unit tests for an application.
- [ ] Can I do file I/O - serializing objects to a file to persist data.
- [ ] Can I use logging frameworks to record vital events within a running application.
- [ ] Can I distinguish between unit and integration testing.

### Should know
- [ ] Can I understand the purpose and value of test-driven development (TDD).
  - *Scope: theory row — covered at note depth (`xunit-fundamentals.md`); the demos practiced
    test-after, so answer from the red-green-refactor loop and its value, not a claimed habit.*
- [ ] Can I explain and apply Fact and Theory attributes in xUnit.
- [ ] Can I use appropriate assertion types (e.g., equality, null, boolean) in unit tests.
- [ ] Can I use mocking frameworks to isolate dependencies in tests.

### Nice to Have
- [ ] Can I use theory testing and multiple assertion types to verify a methods function with unit testing.
- [ ] Can I create and use stubs to simulate external behavior in tests.

## 4. Testing Applications with Cypress

### Must know
- [ ] Can I install and configure Cypress in a development environment
- [ ] Can I understand Cypress test structure (describe, it, before/after hooks)
- [ ] Can I write basic Cypress tests for UI interactions (clicks, typing, assertions)
- [ ] Can I use Cypress commands for element selection and traversal effectively
- [ ] Can I test form submissions and validate expected outcomes with Cypress
- [ ] Can I implement Cypress fixtures and custom commands for reusable test logic
- [ ] Can I debug failing tests using Cypress Test Runner and browser developer tools
- [ ] Can I integrate Cypress tests into CI/CD pipelines
  - *Scope: awareness depth for this sitting — exit-code contract, app-up-first, failure
    artifacts, sample workflow shape (`cypress-advanced.md`, "Running in a CI pipeline"). The
    hands-on pipeline lands in Week 10 (deferral on record, 2026-07-27).*
- [ ] Can I handle asynchronous behavior and API requests within Cypress tests
- [ ] Can I apply best practices for organizing and maintaining a Cypress test suite

### Should know
- [ ] Can I use Cypress intercepts to stub, spy, and mock network requests
- [ ] Can I leverage Cypress plugins to extend functionality (e.g., code coverage)
- [ ] Can I use Cypress for cross-browser testing
- [ ] Can I integrate visual regression testing into Cypress workflows
- [ ] Can I apply Cypress testing strategies for component-level testing
- [ ] Can I collaborate using Cypress dashboards for test reporting and insights

### Nice to Have
- [ ] Can I implement advanced Cypress patterns for data-driven testing
- [ ] Can I combine Cypress with other testing frameworks/tools (e.g., Playwright, Jest) for hybrid strategies
- [ ] Am I aware of how to develop Cypress plugins for project-specific needs
- [ ] Can I apply Cypress to accessibility testing scenarios

## 5. Web Automation with Selenium

### Must know
- [ ] Can I understand the components of the Selenium ecosystem
- [ ] Can I understand the common use-cases for Selenium WebDriver
- [ ] Can I incorporate Selenium WebDriver into a project utilizing the automated Driver management
- [ ] Can I use navigation methods to control browser flow
- [ ] Can I utilize appropriate locator strategies for finding web elements using Selenium
- [ ] Can I utilize appropriate find methods for accessing web elements in Selenium scripts
- [ ] Can I utilize the Select class to interact with select elements in the browser
- [ ] Can I interact with web elements to simulate user actions in the browser
- [ ] Can I understand the resources available to Selenium to validate element state
- [ ] Can I understand and implement implicit waits
- [ ] Can I understand and implement explicit waits
- [ ] Can I understand and implement fluent waits
- [ ] Can I organize code using the page object model design pattern
- [ ] Can I debug and troubleshoot common Selenium errors

### Should know
- [ ] Can I use option classes to customize browser behavior
- [ ] Can I capture screenshots during code execution
- [ ] Can I perform complex user interactions using the Actions API
- [ ] Can I manage browser window contexts during code execution
- [ ] Can I handle browser alerts during code execution
- [ ] Can I apply Xpath functions to locate dynamic elements
- [ ] Can I understand and apply absolute Xpath expressions
- [ ] Can I understand and apply relative Xpath expressions

### Nice to Have
- [ ] Can I understand the use cases and advantages of Selenium IDE
- [ ] Can I understand the use and benefits of Selenium Grid for distributed testing
- [ ] Can I manually configure and instantiate a Selenium WebDriver object
- [ ] Can I evaluate the pros and cons of using Selenium for automated testing

---

## Not yet covered (taught in Week 10)

Nothing on the rubric is wholly untaught for this exam. The one split row is annotated in place
above: **Cypress CI/CD integration (Must)** is covered at awareness depth in the notes; the
hands-on pipeline is Week 10 material (`out-of-scope-register.md` carries the record). Coverage
gates/thresholds and CI-attached test reporting — which the rubric does not name — are also
Week 10 and live only in the register.
