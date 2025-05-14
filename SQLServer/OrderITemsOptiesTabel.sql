CREATE TABLE dbo.OrderItemOptions (
    OrderItemOptionID INT IDENTITY(1,1) PRIMARY KEY,
    OrderItemID       INT             NOT NULL 
                                CONSTRAINT FK_OrderItemOptions_OrderItems 
                                    FOREIGN KEY REFERENCES OrderItems(OrderItemID),
    OptionID          INT             NOT NULL 
                                CONSTRAINT FK_OrderItemOptions_ProductOptions 
                                    FOREIGN KEY REFERENCES ProductOptions(OptionID),
    OptionKey         NVARCHAR(50)    NOT NULL 
                                CONSTRAINT DF_OrderItemOptions_OptionKey 
                                    DEFAULT(''),
    OptionValue       NVARCHAR(255)   NOT NULL 
                                CONSTRAINT DF_OrderItemOptions_OptionValue 
                                    DEFAULT(''),
    CustomTextValue   NVARCHAR(255)   NULL
);
