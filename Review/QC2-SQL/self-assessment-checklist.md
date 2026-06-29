# QC-2 (SQL) — Self-Assessment Checklist

Every objective below is reproduced **verbatim** from `qc-criteria/QC-2-SQL.md`, grouped by priority tier
and topic. Read each as a self-question — *"Can I confidently do / explain this without notes?"* Tick only
what you can do unaided; every unticked box points you at the matching cluster in `study-guide.md`, task in
`drills.md`, and question in `mock-interview.md`.

## Must know

### Relational model & SQL sublanguages
- [ ] Articulate the reasons for using a Relational Database Model to represent data.
- [ ] Explain the usage of the sublanguages in SQL

### DDL & constraints
- [ ] Construct SQL statements using DDL (CREATE, DROP, ALTER, TRUNCATE) keywords to generate tables
- [ ] Describe and utilize constraints in table creation. (Unique, Not Null, Primary Key, Foreign Key, Auto Incrementing, Default, Check)
- [ ] Describe the difference between DROP, DELETE, and TRUNCATE functionality
- [ ] Perform basic DDL operations, such as creating, dropping, or truncating tables.

### DML & filtering
- [ ] Construct SQL statements using DML(Insert, Update, Delete) keywords to manipulate pre-existing data within tables
- [ ] Demonstrate the ability to filter records using the WHERE clause and operators

### Keys, schema & referential integrity
- [ ] Understand the role of a primary key in a data-set.
- [ ] Understand the role of a foreign key in a data-set.
- [ ] Create a valid schema for a given data-set
- [ ] Describe referential integrity

### Functions, grouping & joins
- [ ] Understand difference between aggregate and scalar functions
- [ ] Utilize the GROUP BY clause.
- [ ] Understand basic types of joins and demonstrate usage in select statements (inner, left/right outer, full outer, equi)

### Normalization
- [ ] Normalize a database schema from unnormalized form (0NF) to Third Normal Form (3NF), providing step-by-step justification.

### Transactions & ACID
- [ ] Describe the purpose of transactions in a database and when they are used.
- [ ] Understand database consistency and utilize transactions to ensure data consistency in a set of SQL commands.
- [ ] Explain the ACID properties (Atomicity, Consistency, Isolation, Durability) and their importance in transaction management.

### Views, functions & stored procedures
- [ ] Be able to create a User Defined Function
- [ ] Be able to create and call a Stored Procedure
- [ ] Create and use views to store the results of a SQL query

## Should know

### Keys, ERD & schema design
- [ ] Demonstrate how to identify valid candidate keys for a primary key of an entity
- [ ] Read and understand ERD (Entity Relationship Diagram)
- [ ] Explain the concept of multiplicity in database relationships.
- [ ] Accurately describe database schemas, including tables, fields, and the relationships between them.
- [ ] Identify and implement common data typessuch as varchar, decimal, integer, and char.
- [ ] Translate real-world problem descriptions into ER diagrams and implement them as a working relational schema.

### Functions, grouping & subqueries
- [ ] Utilize the HAVING clause to filter aggregated query results
- [ ] Understand when to use subqueries versus joins in SQL logic
- [ ] Identify and use commonly used aggregate functions (e.g., COUNT(), SUM(), AVG(), MIN(), MAX()) to summarize data in queries.
- [ ] Utilize column aliases to enhance readability and clarity of SQL queries.

### Transactions & isolation
- [ ] Identify and compare different isolation levels (Read Uncommitted, Read Committed, Repeatable Read, Serializable) and their trade-offs.

### Normalization trade-offs
- [ ] Explain the benefits and potential drawbacks of database normalization, including impacts on performance and data integrity.

### Constraints & automation
- [ ] Utilize cascades to define what happens to related tables during DML operations
- [ ] Configure triggers to execute the corresponding stored procedures when certain events occur
- [ ] Describe database indexing and its benefits
- [ ] Describe triggers and their use in automating tasks.

## Nice to Have

### DDL & data types
- [ ] Explain how to modify a table structure after creation using ALTER TABLE with examples (e.g., adding a column, modifying a data type).
- [ ] Identify and implement advanced data types beyond the basics, such as BIGINT for handling very large integers

### Keys & relationships
- [ ] Recognize and explain less common key types beyond primary and foreign keys, such as candidate keys or composite keys.
- [ ] Capable of implementing bridge tables to handle many-to-many relationships between entities.

### Subqueries & set operations
- [ ] Utilize subquery structure to execute a select statement.
- [ ] Utilize set operations between multiple select statement

### Window functions & CTEs
- [ ] Understand and know how to utilize Window Functions
- [ ] Utilize Common Table Expressions (CTEs)
