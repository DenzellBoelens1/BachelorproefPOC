SET NOCOUNT ON;
DECLARE @i INT = 1;

WHILE @i <= 500
BEGIN
    DECLARE @Username NVARCHAR(50) = 'User' + CAST(@i AS NVARCHAR);
    DECLARE @Email NVARCHAR(100) = 'user' + CAST(@i AS NVARCHAR) + '@example.com';
    DECLARE @PasswordHash NVARCHAR(255) = 'hashedpassword' + CAST(@i AS NVARCHAR); -- Simulatie van een hash

    -- Gebruiker invoegen
    INSERT INTO Users (Username, PasswordHash, Email, CreatedAt)
    VALUES (@Username, @PasswordHash, @Email, GETDATE());

    -- Profiel invoegen met een referentie naar de zojuist aangemaakte gebruiker
    DECLARE @UserID INT = SCOPE_IDENTITY();
    INSERT INTO Profiles (UserID, FirstName, LastName, PhoneNumber, AddressStreet, AddressCity, AddressZipCode, AddressCountry, DateOfBirth, LastLogin, PreferencesNewsletter)
    VALUES (
        @UserID, 
        'Voornaam' + CAST(@i AS NVARCHAR), 
        'Achternaam' + CAST(@i AS NVARCHAR), 
        '+31 6 123456' + CAST((@i % 10) AS NVARCHAR), 
        'Straat ' + CAST(@i AS NVARCHAR), 
        'Stad ' + CAST((@i % 50) + 1 AS NVARCHAR), 
        '1234AB', 
        'Nederland', 
        DATEADD(YEAR, -20 - (@i % 30), GETDATE()), 
        NULL, 
        (@i % 2) -- Om de nieuwsbriefvoorkeur af te wisselen
    );

    SET @i = @i + 1;
END;
