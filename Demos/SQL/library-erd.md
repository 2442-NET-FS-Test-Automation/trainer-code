# LibraryDB — ERD (Mermaid)

Schema for our Library ERD so far.

```mermaid
erDiagram
    AUTHOR ||--o{ BOOK : writes
    BOOK   ||--o{ LOAN : "loaned in"
    MEMBER ||--o{ LOAN : borrows

    AUTHOR {
        int      AuthorId PK
        varchar  FirstName
        varchar  LastName
        int      BirthYear
    }
    BOOK {
        int      BookId PK
        varchar  Title
        char     ISBN UK
        int      PublishedYear
        varchar  Category
        int      AuthorId FK
        int      TotalCopies
        int      AvailableCopies
    }
    MEMBER {
        int      MemberId PK
        varchar  FirstName
        varchar  LastName
        varchar  Email UK
        date     JoinedDate
    }
    LOAN {
        int      LoanId PK
        int      BookId FK
        int      MemberId FK
        date     LoanDate
        date     DueDate
        date     ReturnDate
    }
```

Notes:
- `UK` = "unique keys" (ISBN, Email). `FK` lines = `Book.AuthorId`, `Loan.BookId`, `Loan.MemberId`.
- `Category` on `BOOK` + single author per book = deliberate simplifications (Wed normalization demo fixes both).
