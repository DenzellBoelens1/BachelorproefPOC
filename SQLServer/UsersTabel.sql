CREATE TABLE Users (
    UserID INT PRIMARY KEY IDENTITY(1,1), -- Unieke identifier voor de gebruiker
    Username NVARCHAR(50) NOT NULL UNIQUE, -- Gebruikersnaam (moet uniek zijn)
    PasswordHash NVARCHAR(255) NOT NULL, -- Gehasht wachtwoord
    Email NVARCHAR(100) NOT NULL UNIQUE, -- E-mailadres (moet uniek zijn)
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE() -- Datum van registratie
);