DECLARE @Counter INT = 1;
DECLARE @ProductName NVARCHAR(100);
DECLARE @Description NVARCHAR(MAX);
DECLARE @BasePrice DECIMAL(10, 2);
DECLARE @InStock INT;
DECLARE @MinStock INT;
DECLARE @ProductID INT;

-- Arrays voor opties
DECLARE @Sizes TABLE (Size NVARCHAR(10));
INSERT INTO @Sizes (Size) VALUES ('S'), ('M'), ('L'), ('XL');

DECLARE @Colors TABLE (Color NVARCHAR(20));
INSERT INTO @Colors (Color) VALUES ('Red'), ('Blue'), ('Green'), ('Black');

DECLARE @CustomText TABLE (TextOption NVARCHAR(50));
INSERT INTO @CustomText (TextOption) VALUES ('Enabled=true'), ('MaxLength=20'), ('PricePerCharacter=0.10');

WHILE @Counter <= 5000
BEGIN
    -- Genereer random waarden voor het product
    SET @ProductName = 'Product ' + CAST(@Counter AS NVARCHAR(10));
    SET @Description = 'Description for ' + @ProductName;
    SET @BasePrice = CAST((RAND() * 100) AS DECIMAL(10, 2)); -- Random prijs tussen 0 en 100
    SET @InStock = CAST((RAND() * 1000) AS INT); -- Random voorraad tussen 0 en 1000
    SET @MinStock = CAST((RAND() * 100) AS INT); -- Random minimale voorraad tussen 0 en 100

    -- Voeg het product toe
    INSERT INTO Products (Name, Description, BasePrice, InStock, MinStock)
    VALUES (@ProductName, @Description, @BasePrice, @InStock, @MinStock);

    -- Haal het ProductID op van het zojuist toegevoegde product
    SET @ProductID = SCOPE_IDENTITY();

    -- Voeg opties toe voor het product
    -- Optie: Size
    INSERT INTO ProductOptions (ProductID, OptionType, OptionValue)
    SELECT @ProductID, 'Size', Size FROM @Sizes;

    -- Optie: Color
    INSERT INTO ProductOptions (ProductID, OptionType, OptionValue)
    SELECT @ProductID, 'Color', Color FROM @Colors;

    -- Optie: CustomText
    INSERT INTO ProductOptions (ProductID, OptionType, OptionValue)
    SELECT @ProductID, 'CustomText', TextOption FROM @CustomText;

    -- Verhoog de teller
    SET @Counter = @Counter + 1;
END;