CREATE TABLE Profiles (
    ProfileID INT PRIMARY KEY IDENTITY(1,1), -- Unieke identifier voor het profiel
    UserID INT NOT NULL FOREIGN KEY REFERENCES Users(UserID), -- Foreign key naar Users tabel
    FirstName NVARCHAR(50) NOT NULL, -- Voornaam
    LastName NVARCHAR(50) NOT NULL, -- Achternaam
    PhoneNumber NVARCHAR(20), -- Telefoonnummer
    AddressStreet NVARCHAR(100), -- Straatnaam en huisnummer
    AddressCity NVARCHAR(100), -- Stad
    AddressZipCode NVARCHAR(20), -- Postcode
    AddressCountry NVARCHAR(100), -- Land
    DateOfBirth DATE, -- Geboortedatum
    LastLogin DATETIME, -- Laatste login-datum
    PreferencesNewsletter BIT DEFAULT 0 -- Voorkeur voor nieuwsbrief (0 = nee, 1 = ja)
);
