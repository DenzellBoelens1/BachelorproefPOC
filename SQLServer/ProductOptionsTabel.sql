CREATE TABLE ProductOptions (
    OptionID INT PRIMARY KEY IDENTITY(1,1),
    ProductID INT FOREIGN KEY REFERENCES Products(ProductID),
    OptionType NVARCHAR(50) NOT NULL, -- Bijv. "Size", "Color", "CustomText"
    OptionValue NVARCHAR(100) NOT NULL -- Bijv. "S", "Red", "MaxLength=20"
);