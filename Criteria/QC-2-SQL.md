# QC 2 (SQL) Criteria

## SQL

| Priority | Objective | Example / Explanation |
| :--- | :--- | :--- |
| Must know | Articulate the reasons for using a Relational Database Model to represent data. | Provides structured data organization, enforces data integrity, supports complex queries via SQL, and reduces data redundancy. |
| Must know | Explain the usage of the sublanguages in SQL | DDL defines schema; DML modifies data; DCL manages access and permissions; TCL manages transactions. |
| Must know | Understand the role of a primary key in a data-set. | A unique identifier for each record in a table, ensuring no duplicate rows exist and enabling efficient retrieval. |
| Must know | Construct SQL statements using DDL (CREATE, DROP, ALTER, TRUNCATE) keywords to generate tables | `CREATE TABLE Users (ID int, Name varchar(50));` |
| Must know | Describe and utilize constraints in table creation. (Unique, Not Null, Primary Key, Foreign Key, Auto Incrementing, Default, Check) | `CREATE TABLE Orders (ID int PRIMARY KEY, UserID int NOT NULL);` |
| Must know | Construct SQL statements using DML(Insert, Update, Delete) keywords to manipulate pre-existing data within tables | `INSERT INTO Users (ID, Name) VALUES (1, 'Alice');` |
| Must know | Describe the difference between DROP, DELETE, and TRUNCATE functionality | DROP removes the table entity; TRUNCATE empties the table structure quickly; DELETE removes specific rows based on a WHERE condition. |
| Must know | Understand the role of a foreign key in a data-set. | A field in one table that uniquely identifies a row of another table, establishing a referential link between them. |
| Must know | Create a valid schema for a given data-set | `CREATE TABLE Dept (ID int PK); CREATE TABLE Emp (ID int PK, DeptID int FK);` |
| Must know | Perform basic DDL operations, such as creating, dropping, or truncating tables. | `DROP TABLE Users;` |
| Must know | Understand difference between aggregate and scalar functions | Aggregate functions return a single value calculated from multiple rows (e.g., SUM). Scalar functions return a single value based on a single input value (e.g., UPPER). |
| Must know | Utilize the GROUP BY clause. | `SELECT DeptID, COUNT(*) FROM Emp GROUP BY DeptID;` |
| Must know | Demonstrate the ability to filter records using the WHERE clause and operators | `SELECT * FROM Users WHERE Age >= 18;` |
| Must know | Understand basic types of joins and demonstrate usage in select statements (inner, left/right outer, full outer, equi) | `SELECT * FROM TableA INNER JOIN TableB ON TableA.id = TableB.id;` |
| Must know | Describe the purpose of transactions in a database and when they are used. | Groups a sequence of operations into a single logical unit of work to guarantee either complete execution or complete rollback. |
| Must know | Understand database consistency and utilize transactions to ensure data consistency in a set of SQL commands. | `BEGIN TRANSACTION; UPDATE Accounts SET Balance = Balance - 100 WHERE ID = 1; COMMIT;` |
| Must know | Be able to create a User Defined Function | `CREATE FUNCTION GetTotal() RETURNS INT AS BEGIN RETURN (SELECT SUM(Amount) FROM Sales) END;` |
| Must know | Be able to create and call a Stored Procedure | `CREATE PROCEDURE GetUsers AS SELECT * FROM Users; EXEC GetUsers;` |
| Must know | Create and use views to store the results of a SQL query | `CREATE VIEW ActiveUsers AS SELECT * FROM Users WHERE Status = 'Active';` |
| Must know | Normalize a database schema from unnormalized form (0NF) to Third Normal Form (3NF), providing step-by-step justification. | 1NF: Eliminate repeating groups. 2NF: Remove partial dependencies. 3NF: Remove transitive dependencies. |
| Must know | Describe referential integrity | A property ensuring that a foreign key value must correspond to a valid primary key value in the referenced table, preventing orphaned records. |
| Must know | Explain the ACID properties (Atomicity, Consistency, Isolation, Durability) and their importance in transaction management. | Atomicity (all or nothing), Consistency (valid state transitions), Isolation (concurrent execution control), Durability (permanent storage of committed data). |
| Should know | Demonstrate how to identify valid candidate keys for a primary key of an entity | Identify all attributes or combinations of attributes that uniquely identify a record (e.g., SSN, Email). One is selected as the Primary Key. |
| Should know | Read and understand ERD (Entity Relationship Diagram) | A visual blueprint depicting entities (tables), attributes (columns), and their relational connections. |
| Should know | Explain the concept of multiplicity in database relationships. | Defines the cardinality between entities, such as 1:1 (one-to-one), 1:N (one-to-many), or M:N (many-to-many). |
| Should know | Accurately describe database schemas, including tables, fields, and the relationships between them. | A logical architecture detailing tables, columns, data types, constraints, and relational mapping via primary and foreign keys. |
| Should know | Identify and implement common data typessuch as varchar, decimal, integer, and char. | `Price DECIMAL(10,2), Age INT` |
| Should know | Translate real-world problem descriptions into ER diagrams and implement them as a working relational schema. | Converting business logic into structured tables, applying appropriate normalization, and resolving M:N relationships. |
| Should know | Utilize the HAVING clause to filter aggregated query results | `SELECT DeptID, COUNT(*) FROM Emp GROUP BY DeptID HAVING COUNT(*) > 5;` |
| Should know | Understand when to use subqueries versus joins in SQL logic | Use joins to combine data sets; use subqueries for filtering against calculated aggregates or temporary sets not explicitly joined. |
| Should know | Identify and use commonly used aggregate functions (e.g., COUNT(), SUM(), AVG(), MIN(), MAX()) to summarize data in queries. | `SELECT AVG(Salary) FROM Employees;` |
| Should know | Utilize column aliases to enhance readability and clarity of SQL queries. | `SELECT FirstName AS [First Name] FROM Users;` |
| Should know | Identify and compare different isolation levels (Read Uncommitted, Read Committed, Repeatable Read, Serializable) and their trade-offs. | Higher isolation levels reduce concurrency phenomena (dirty reads, phantom reads) but decrease system performance and throughput. |
| Should know | Explain the benefits and potential drawbacks of database normalization, including impacts on performance and data integrity. | Benefits: Reduces redundancy, prevents insertion/deletion anomalies. Drawbacks: Increases query complexity and execution time due to required joins. |
| Should know | Utilize cascades to define what happens to related tables during DML operations | `FOREIGN KEY (DeptID) REFERENCES Dept(ID) ON DELETE CASCADE` |
| Should know | Configure triggers to execute the corresponding stored procedures when certain events occur | `CREATE TRIGGER AfterInsertUser ON Users AFTER INSERT AS EXEC LogNewUser;` |
| Should know | Describe database indexing and its benefits | Indexes are data structures (such as B-trees) that improve data retrieval speed on a table at the cost of additional storage and slower write operations. |
| Should know | Describe triggers and their use in automating tasks. | Stored procedures automatically executed by the database engine in response to specific DML events (INSERT, UPDATE, DELETE). |
| Nice to Have | Explain how to modify a table structure after creation using ALTER TABLE with examples (e.g., adding a column, modifying a data type). | `ALTER TABLE Users ADD Email varchar(100);` |
| Nice to Have | Recognize and explain less common key types beyond primary and foreign keys, such as candidate keys or composite keys. | Composite key: A primary key consisting of two or more columns to guarantee uniqueness when a single column is insufficient. |
| Nice to Have | Identify and implement advanced data types beyond the basics, such as BIGINT for handling very large integers | `TransactionID BIGINT` |
| Nice to Have | Capable of implementing bridge tables to handle many-to-many relationships between entities. | `CREATE TABLE StudentCourse (StudentID int, CourseID int);` |
| Nice to Have | Utilize subquery structure to execute a select statement. | `SELECT * FROM Emp WHERE DeptID IN (SELECT ID FROM Dept WHERE Name = 'Sales');` |
| Nice to Have | Utilize set operations between multiple select statement | `SELECT Name FROM TableA UNION SELECT Name FROM TableB;` |
| Nice to Have | Understand and know how to utilize Window Functions | `SELECT Name, Salary, ROW_NUMBER() OVER(PARTITION BY DeptID ORDER BY Salary DESC) FROM Employees;` |
| Nice to Have | Utilize Common Table Expressions (CTEs) | `WITH CTE AS (SELECT * FROM Users WHERE Age > 30) SELECT * FROM CTE;` |
