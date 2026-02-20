DELETE FROM INVOICES;
DELETE FROM APPOINTMENTS;
DELETE FROM REQUESTS;
DELETE FROM REQUESTORS;


CREATE TABLE IF NOT EXISTS InterpreterPhones (
    Id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    InterpreterId INTEGER NOT NULL,
    HomePhone TEXT NULL,
    BusinessPhone TEXT NULL,
    MobilePhone TEXT NULL,
    CONSTRAINT FK_InterpreterPhones_Interpreters_InterpreterId
        FOREIGN KEY (InterpreterId) REFERENCES Interpreters (Id)
        ON DELETE CASCADE
);

CREATE UNIQUE INDEX IF NOT EXISTS IX_InterpreterPhones_InterpreterId
    ON InterpreterPhones (InterpreterId);

    ALTER TABLE Interpreters ADD COLUMN HomePhone TEXT NULL;
ALTER TABLE Interpreters ADD COLUMN BusinessPhone TEXT NULL;
ALTER TABLE Interpreters ADD COLUMN MobilePhone TEXT NULL;