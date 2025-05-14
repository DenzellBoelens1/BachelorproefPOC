SET NOCOUNT ON;
DECLARE @OrderCounter INT = 1;
DECLARE @UserID INT;
DECLARE @OrderID INT;
DECLARE @ProductID INT;
DECLARE @Quantity INT;
DECLARE @UnitPrice DECIMAL(10,2);
DECLARE @TotalPrice DECIMAL(10,2);
DECLARE @OrderItemID INT;
DECLARE @OptionID INT;

WHILE @OrderCounter <= 50000
BEGIN
    -- Random User kiezen
    SELECT TOP 1 @UserID = UserID FROM Users ORDER BY NEWID();

    -- Voeg order toe (zonder status)
    SET @TotalPrice = 0;
    INSERT INTO Orders (UserID, OrderDate, TotalPrice)
    VALUES (@UserID, DATEADD(DAY, -ABS(CHECKSUM(NEWID()) % 365), GETDATE()), 0);

    SET @OrderID = SCOPE_IDENTITY();

    -- Aantal producten in deze order (tussen 1 en 3)
    DECLARE @ItemsInOrder INT = 1 + ABS(CHECKSUM(NEWID()) % 3);
    DECLARE @ItemCounter INT = 1;

    WHILE @ItemCounter <= @ItemsInOrder
    BEGIN
        -- Kies een willekeurig product
        SELECT TOP 1 @ProductID = ProductID, @UnitPrice = BasePrice FROM Products ORDER BY NEWID();

        -- Kies een willekeurige hoeveelheid (1-10)
        SET @Quantity = 1 + ABS(CHECKSUM(NEWID()) % 10);

        -- Voeg product toe aan orderitems
        INSERT INTO OrderItems (OrderID, ProductID, Quantity, Price)
        VALUES (@OrderID, @ProductID, @Quantity, @UnitPrice);

        SET @OrderItemID = SCOPE_IDENTITY();

        -- Update de totale prijs
        SET @TotalPrice = @TotalPrice + (@UnitPrice * @Quantity);

        -- Kies exact 1 willekeurige optie voor het product
        SELECT TOP 1 @OptionID = OptionID FROM ProductOptions WHERE ProductID = @ProductID ORDER BY NEWID();

        -- Voeg optie toe aan OrderItemOptions
        INSERT INTO OrderItemOptions (OrderItemID, OptionID)
        VALUES (@OrderItemID, @OptionID);

        SET @ItemCounter = @ItemCounter + 1;
    END;

    -- Update de totaalprijs van de order
    UPDATE Orders SET TotalPrice = @TotalPrice WHERE OrderID = @OrderID;

    SET @OrderCounter = @OrderCounter + 1;
END;

SET NOCOUNT OFF;
