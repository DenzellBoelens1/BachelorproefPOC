CREATE TABLE Products (
    ProductID INT PRIMARY KEY IDENTITY(1,1),
    Name NVARCHAR(100) NOT NULL,
    Description NVARCHAR(MAX),
    BasePrice DECIMAL(10, 2) NOT NULL,
    InStock INT NOT NULL,
    MinStock INT NOT NULL
);