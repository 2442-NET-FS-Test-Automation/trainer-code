/*
This script is intended to be cleanly re-runnable. I should be able to 
run it top to bottom and recreate my database in place - this is for demo purposes. 

Im going to break this script into sections, denoted by comments.
*/


IF DB_ID(N'LibraryDB') IS NULL
    CREATE DATABASE LibraryDB;
GO

-- At the top of my file, before any commands/statements. I want to make sure,
-- They run in the correct database.
USE LibraryDB;
GO -- GO is MS SQL Server specific - it is a batching statement. It's telling
-- the underlying SQL Server instance that is reading (running) this file 
-- "execute everything above this GO, and after any existing GO above, as one 
-- batch of statements."

/*

USE LibraryDB;
GO -- First batch 

CREATE TABLE AUTHOR{ }
CREATE TABLE BOOK { }
...
GO -- second batch 

... and so on and so forth

*/
-- ===================================================
-- Section 1 - DDL - Defining our Database
-- CREATE - Creating new tables, schemas, databases: NOT INDIVIDUAL ROWS
-- DROP - Deletes a table, or schema, or database entirely. 
-- TRUNCATE - Deletes all data in a table - preserves structure (columns + constraints)
-- ALTER - Used to edit the structure of an existing table (add columns, tweak constraints, etc)
-- ===================================================
-- I will come back once we've created out tables, and include a DROP section
DROP TABLE IF EXISTS dbo.Loan;
DROP TABLE IF EXISTS dbo.Book;
DROP TABLE IF EXISTS dbo.Member;
DROP TABLE IF EXISTS dbo.Author;
GO


-- The first thing I want to do, is create my tables. 
-- Table names are technically pre-pended by the schema
-- We have a schema already - MS SQL Server creates a default schema called "dbo"
-- "Database Owner". If I create Author without specifying a schema, SQL Server
-- associates it with this default dbo schema. So the full table name becomes:
-- dbo.Author
CREATE TABLE dbo.Author
(   -- Column-name data-type constraints (optional)
    -- In MS SQL Server, identity lets us define a PK that automatically increments
    AuthorId INT IDENTITY(1,1) NOT NULL,
    FirstName VARCHAR(50) NOT NULL,
    LastName VARCHAR(50) NOT NULL,
    BirthYear INT NULL, -- NULL here signifies that we INTEND for this to maybe be null

    -- After I define my columns, datatypes and basic constraints 
    -- I can optionally add some named constraints. If I don't name constraints,
    -- nothing breaks BUT I can make my life easier and make error messages more 
    -- functional/readable by explicitly naming my constraints
    CONSTRAINT PK_Author PRIMARY KEY (AuthorId),

    -- When someone tries to add an Author, make sure that BirthYear is either NULL, OR between 300 and 2050
    CONSTRAINT CK_Author_BirthYear CHECK (BirthYear IS NULL OR BirthYear BETWEEN 300 AND 2050)
    -- If you THINK that you might need to alter a table's constraints,
    -- you should name them. It makes running ALTER TABLE commands easier later
);
GO -- including my GO batch statement for MS SQL Server
-- SELECT * FROM dbo.Author;
USE LibraryDB;
CREATE TABLE dbo.Member 
(   
    -- This is perhaps faster but not best practice. For example, lets say I want to change from MemberID as my PK
    -- to email as my PK. By doing an in-line non-named constraint, I've shot myself in the foot. It is much harder
    -- to ALTER our table later on, to play with the constraint. 
    MemberId INT IDENTITY(1,1) NOT NULL PRIMARY KEY, -- Fun fact, Identity will not reuse any numbers even if deleted
    FirstName VARCHAR(50) NOT NULL,
    LastName VARCHAR(50) NOT NULL,
    Email VARCHAR(125) NOT NULL UNIQUE,
    -- Using a DEFAULT constraint, if no value is provided, the built in GETDATE() function gets a value to satisfy the column
    JoinedDate DATE NOT NULL DEFAULT (GETDATE())
);
GO 

-- Book is our largest table so far
USE LibraryDB;
CREATE TABLE dbo.Book
(
    -- Columns + some constraints
    BookId INT IDENTITY(1,1) NOT NULL,
    Title VARCHAR(200) NOT NULL, --UNIQUE
    ISBN CHAR(13) NOT NULL,
    PublishedYear INT NULL,
    -- Creating a named constraint inline, useful for the constraint code isn't super long
    CategoryName VARCHAR(60) NOT NULL CONSTRAINT DF_Book_CategoryName DEFAULT ('General'),
    AuthorId INT NOT NULL, -- This will be a foreign key, we'll set the FK constraint below
    TotalCopies INT NOT NULL CONSTRAINT DF_Book_TotalCopies DEFAULT(1),
    AvailableCopies INT NOT NULL CONSTRAINT DF_Book_AvailableCopies DEFAULT(1),
    -- More named constraints below
    CONSTRAINT PK_Book PRIMARY KEY (BookId),
    CONSTRAINT UQ_Book_ISBN UNIQUE (ISBN),

    -- Setting our first Foreign Key constraint 
    -- We need to tell the SQL engine, what column in this table is getting the FK constraint
    -- as well as what column in another existing table that FK points to
    -- When I set a FK constraint, I can optionally set the delete behavior via ON DELETE
    -- CASCADE - VERY RISKY - If an author is deleted, all their books go too.
    -- SET NULL - Low risk - If an author is deleted, this AuthorId in Book is set to null. (requires a nullable column)
    -- RESTRICT - SAFE - Default behavior, blocks deletion of an author if any books reference it
    -- NO ACTION - SAFE - Same as restrict in MS SQL Server
    -- SET DEFAULT - Low risk - Requires a default value constraint, will set the FK column to that value if the author is deleted
    CONSTRAINT FK_Book_Author FOREIGN KEY (AuthorId) REFERENCES dbo.Author (AuthorId) ON DELETE CASCADE,

    -- The final thing I want to do is enforce some logical rules about Available and Total copies
    -- AvailableCopies CANNOT exceed TotalCopies
    CONSTRAINT CK_Book_Copies CHECK (TotalCopies >= AvailableCopies)
)
GO

-- Loan -- two foreign keys
-- Represents the library loaning a book to a member
USE LibraryDB;
CREATE TABLE dbo.Loan
(
    LoanId INT IDENTITY(1,1) NOT NULL, -- PK
    BookId INT NOT NULL, -- FK
    MemberId INT NOT NULL, -- FK 
    -- Date stamp for when the book was lent to the member
    LoanDate DATE NOT NULL CONSTRAINT DF_Loan_LoanDate DEFAULT(GETDATE()),
    DueDate DATE NOT NULL,
    ReturnDate DATE NULL, -- This will remain NULL until the book is actually returned

    -- More named constraints below
    CONSTRAINT PK_Loan PRIMARY KEY (LoanId),
    -- Note: Technically, FK columns don't have to match the column in the table they are a PK in. 
    CONSTRAINT FK_Loan_Book FOREIGN KEY (BookId) REFERENCES dbo.Book (BookId),
    CONSTRAINT FK_Loan_Member FOREIGN KEY (MemberId) REFERENCES dbo.Member (MemberId),
    CONSTRAINT CK_Loan_Dates CHECK (DueDate >= LoanDate) -- DueDate has to be in the future
);
GO 

-- Now that I've created the tables, using CREATE (DDL), how can I edit the tables themselves?

-- Let's add a column to an existing table, lets use dbo.Book
-- We can set constraints for this new column in line as well
ALTER TABLE dbo.Book ADD Edition INT NOT NULL CONSTRAINT DF_Book_Edition DEFAULT (1);

-- We can get more granular, and not just add a new column, we can edit things about existing columns
-- I can use this ALTER TABLE + ALTER COLUMN to add or edit constraints 
ALTER TABLE dbo.Book ALTER COLUMN Title VARCHAR(250) NOT NULL;
GO

-- Ideally, we would never have to ALTER stuff. When possible, do it in CREATE. In the real world, 
-- you will need to ALTER things about the tables in a schema. Once you have data in a table, you're
-- stuck ALTER-ing it. Prior to giving a table any data, it is often easier to drop the table and
-- edit the CREATE statement for it. 

-- DROP and Truncate - please learn the difference
-- DROP: removes a table entirely. Data is lost, the structure is also gone. Like it never existed.
-- DROP TABLE dbo.Loan;

-- TRUNCATE: removes all the data (rows) in table, leaves behind the structure.
-- TRUNCATE TABLE dbo.Loan;

/*
========================================================
Section 2: DML + DQL - Reading and Writing (CRUD)
-- DML - Data Manipulation Language - Used for affecting rows in a table
-- INSERT - Used to insert new rows in an existing table
-- UPDATE - Used to update an existing row's information
-- DELETE - Used to remove a row

-- DQL - Data Query Language - Selecting rows. 
-- SELECT - Used to select a record or records. (This is where other SQL
-- keywords like GROUP BY, HAVING. WHERE, etc live.)
========================================================
*/ 

-- DML first - let's seed our database

-- Single row insertion. It is best practice - borderline mandatory - to explicitly list the columns 
-- you are inserting into 
INSERT INTO dbo.Author (FirstName, LastName, BirthYear) 
VALUES ('Robert', 'Martin', 1952);

-- We can also do a multi-row insert
INSERT INTO dbo.Author (FirstName, LastName, BirthYear) VALUES
    ('Martin', 'Fowler', 1963),
    ('Frank', 'Herbert', 1920),
    ('Kent', 'Beck', 1961); -- Final row in a multi-insert gets the semi-colon

-- SELECT every row from author to check our work
SELECT * FROM dbo.Author;

-- Now that we have authors, we can add books.
INSERT INTO dbo.Book (Title, ISBN, PublishedYear, CategoryName, AuthorId,
                             TotalCopies, AvailableCopies, Edition)
VALUES ('Clean Code', '9780132350885', 2008, 'Software', 1, 3, 3, 1);

SELECT * FROM dbo.Book;
GO 

INSERT INTO dbo.Author (FirstName, LastName, BirthYear) VALUES
    ('Robert',  'Martin',   1952),   -- 1
    ('Martin',  'Fowler',   1963),   -- 2
    ('Kent',    'Beck',     1961),   -- 3
    ('Erich',   'Gamma',    1961),   -- 4
    ('Andrew',  'Hunt',     1964),   -- 5
    ('David',   'Thomas',   1956);   -- 6
GO

INSERT INTO dbo.Member (FirstName, LastName, Email, JoinedDate) VALUES
    ('Ada',     'Lovelace', 'ada@example.com',     '2025-01-10'),  -- 1
    ('Grace',   'Hopper',   'grace@example.com',   '2025-02-02'),  -- 2
    ('Alan',    'Turing',   'alan@example.com',    '2025-02-20'),  -- 3
    ('Linus',   'Torvalds', 'linus@example.com',   '2025-03-15'),  -- 4
    ('Margaret','Hamilton', 'margaret@example.com','2025-04-01'),  -- 5
    ('Dennis',  'Ritchie',  'dennis@example.com',  '2025-05-05');  -- 6
GO

INSERT INTO dbo.Book (Title, ISBN, PublishedYear, CategoryDescription, AuthorId, TotalCopies, AvailableCopies, Edition) VALUES
    ('Clean Code',                         '9780132350884', 2008, 'Software',            1, 3, 3, 1),
    ('Clean Architecture',                 '9780134494166', 2017, 'Software',            1, 2, 2, 1),
    ('Refactoring',                        '9780201485677', 1999, 'Software',      2, 2, 1, 2),
    ('Patterns of Enterprise Application Architecture','9780321127426',2002,'Software', 2, 1, 1, 1),
    ('Test Driven Development',            '9780321146533', 2002, 'Testing',         3, 2, 2, 1),
    ('Extreme Programming Explained',      '9780321278654', 2004, 'Process',           3, 1, 0, 2),
    ('Design Patterns',                    '9780201633610', 1994, 'Software',          4, 2, 2, 1),
    ('The Pragmatic Programmer',           '9780201616224', 1999, 'Software',           5, 4, 3, 1),
    ('The Pragmatic Programmer 20th Anniv','9780135957059', 2019, 'Software',            5, 2, 2, 2),
    ('Programming Ruby',                   '9780974514055', 2004, 'Languages',           6, 1, 1, 1);
GO

