-- =============================================
-- 1. KHỞI TẠO DATABASE
-- =============================================
USE master;
GO

IF EXISTS (SELECT * FROM sys.databases WHERE name = 'CookingShareDB')
BEGIN
    ALTER DATABASE CookingShareDB SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE CookingShareDB;
END
GO

CREATE DATABASE CookingShareDB;
GO

USE CookingShareDB;
GO

-- =============================================
-- 2. QUẢN LÝ NGƯỜI DÙNG & HỆ THỐNG
-- =============================================

CREATE TABLE ACCOUNT (
    ID INT IDENTITY(1,1) PRIMARY KEY,
    UserName VARCHAR(50) UNIQUE NOT NULL, 
    Email VARCHAR(100) UNIQUE,          
    Phone VARCHAR(15) UNIQUE,  
    Password VARCHAR(255) NOT NULL,
    Role INT DEFAULT 2, 
    Status INT DEFAULT 1, 
    RegistDate DATETIME DEFAULT GETDATE(),
    ResetToken VARCHAR(100) NULL,
    TokenExpiry DATETIME NULL
);

CREATE TABLE PROFILE (
    AccountID INT PRIMARY KEY REFERENCES ACCOUNT(ID) ON DELETE CASCADE,
    FullName NVARCHAR(100) NOT NULL,
    Avatar NVARCHAR(MAX) DEFAULT 'default-avatar.png',
    Height FLOAT,
    Weight FLOAT, 
    Gender NVARCHAR(10), 
    BirthDay DATE,
    Lifestyle NVARCHAR(50),
    CaloDaily FLOAT DEFAULT 0 
);

-- =============================================
-- 3. QUẢN LÝ NỘI DUNG CHÍNH (CÔNG THỨC)
-- =============================================

CREATE TABLE CATEGORY (
    ID INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL,
    ImageURL NVARCHAR(MAX) 
);

CREATE TABLE TAG (
    ID INT IDENTITY(1,1) PRIMARY KEY,
    TagName NVARCHAR(50) NOT NULL,
    TagType NVARCHAR(30) 
);

CREATE TABLE RECIPE (
    ID INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(200) NOT NULL,
    Describe NVARCHAR(MAX),
    MainImage NVARCHAR(MAX),
    CookTime INT, 
    PrepTime INT, 
    Servings INT DEFAULT 2, 
    Difficulty NVARCHAR(20), 
    Status INT DEFAULT 0, 
    RejectReason NVARCHAR(MAX), 
    Views INT DEFAULT 0,
    CreateDate DATETIME DEFAULT GETDATE(),
    AccountID INT REFERENCES ACCOUNT(ID), 
    CategoryID INT REFERENCES CATEGORY(ID)    
);

CREATE TABLE RECIPE_TAG_MAP (
    RecipeID INT REFERENCES RECIPE(ID) ON DELETE CASCADE,
    TagID INT REFERENCES TAG(ID) ON DELETE CASCADE,
    PRIMARY KEY (RecipeID, TagID)
);

-- =============================================
-- 4. CHI TIẾT CÔNG THỨC & QUY TRÌNH
-- =============================================

CREATE TABLE UNIT (
    ID INT IDENTITY(1,1) PRIMARY KEY,
    UnitName NVARCHAR(20) NOT NULL 
);

CREATE TABLE INGREDIENT (
    ID INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL,
    Calo FLOAT DEFAULT 0, 
    Protein FLOAT DEFAULT 0, 
    Fat FLOAT DEFAULT 0, 
    Sugar FLOAT DEFAULT 0 
);

CREATE TABLE RECIPE_DETAIL (
    RecipeID INT REFERENCES RECIPE(ID) ON DELETE CASCADE,
    IngredientID INT REFERENCES INGREDIENT(ID),
    Quantity FLOAT NOT NULL,
    UnitID INT REFERENCES UNIT(ID),
    PRIMARY KEY (RecipeID, IngredientID)
);

CREATE TABLE STEPTODO (
    ID INT IDENTITY(1,1) PRIMARY KEY,
    RecipeID INT REFERENCES RECIPE(ID) ON DELETE CASCADE,
    StepOrder INT NOT NULL,
    Content NVARCHAR(MAX) NOT NULL,
    ImageURL NVARCHAR(MAX) 
);

-- =============================================
-- 5. TƯƠNG TÁC & MARKETING
-- =============================================

CREATE TABLE ALLERGY (
    AccountID INT REFERENCES ACCOUNT(ID) ON DELETE CASCADE,
    IngredientID INT REFERENCES INGREDIENT(ID) ON DELETE CASCADE,
    PRIMARY KEY (AccountID, IngredientID)
);

CREATE TABLE COMMENT (
    ID INT IDENTITY(1,1) PRIMARY KEY,
    Content NVARCHAR(MAX),
    Rating INT CHECK (Rating >= 1 AND Rating <= 5), 
    CreateDate DATETIME DEFAULT GETDATE(),
    Status INT DEFAULT 1, 
    AccountID INT REFERENCES ACCOUNT(ID),
    RecipeID INT REFERENCES RECIPE(ID) ON DELETE CASCADE
);

CREATE TABLE TIP (
    ID INT IDENTITY(1,1) PRIMARY KEY,
    Title NVARCHAR(255) NOT NULL,
    Content NVARCHAR(MAX),
    ImageURL NVARCHAR(MAX),
    CreateDate DATETIME DEFAULT GETDATE(),
    AccountID INT REFERENCES ACCOUNT(ID)
);

CREATE TABLE BANNER (
    ID INT IDENTITY(1,1) PRIMARY KEY,
    Title NVARCHAR(100),
    ImageURL NVARCHAR(MAX) NOT NULL,
    LinkURL NVARCHAR(MAX),
    Position INT DEFAULT 1, 
    IsActive BIT DEFAULT 1
);

CREATE TABLE SYSTEM_SETTING (
    ID INT IDENTITY(1,1) PRIMARY KEY,
    SettingKey VARCHAR(50) UNIQUE, 
    SettingValue NVARCHAR(MAX)
);
GO

-- =============================================
-- 6. THÊM DỮ LIỆU MẪU (SEED DATA)
-- =============================================

INSERT INTO ACCOUNT (UserName, Email, Phone, Password, Role, Status) VALUES 
('admin', 'admin@cookingshare.com', '0901234567', '123456', 1, 1),
('hoanganh', 'hoanganh@gmail.com', '0987654321', '123456', 2, 1),
('bepxinh', 'bepxinh@yahoo.com', '0911222333', '123456', 2, 1);

INSERT INTO PROFILE (AccountID, FullName, Height, Weight, Gender, CaloDaily) VALUES 
(1, N'Quản Trị Viên', NULL, NULL, N'Nam', 2000),
(2, N'Lê Hoàng Anh', 175, 70, N'Nam', 2300),
(3, N'Bếp Xinh', 158, 48, N'Nữ', 1800);

INSERT INTO CATEGORY (Name) VALUES 
(N'Món Mặn'), (N'Món Chay'), (N'Món Canh'), (N'Salad - Healthy'), (N'Ăn Vặt');

INSERT INTO TAG (TagName, TagType) VALUES 
(N'Eat Clean', 'Diet'), (N'Keto', 'Diet'), (N'Dễ làm', 'Difficulty'), (N'Truyền thống', 'Style');

INSERT INTO UNIT (UnitName) VALUES 
(N'gam'), (N'kg'), (N'ml'), (N'muỗng canh'), (N'muỗng cafe'), (N'quả'), (N'tép'), (N'củ'), (N'bó'), (N'con');

INSERT INTO INGREDIENT (Name, Calo, Protein, Fat, Sugar) VALUES 
(N'Thịt ba chỉ heo', 518, 16, 50, 0),
(N'Ức gà', 165, 31, 3.6, 0),
(N'Thịt bò', 250, 26, 15, 0),
(N'Cá lóc', 97, 18, 2.7, 0),
(N'Đậu hũ', 76, 8, 4.8, 1.9),
(N'Trứng gà', 155, 13, 11, 1.1),
(N'Tôm', 99, 24, 0.3, 0.2),
(N'Rau muống', 30, 3, 0.2, 3),
(N'Cà chua', 18, 0.9, 0.2, 3.9),
(N'Hành tây', 40, 1.1, 0.1, 9),
(N'Tỏi', 149, 6, 0.5, 33),
(N'Sả', 81, 1.2, 0.2, 19),
(N'Nước mắm', 40, 5, 0, 4),
(N'Đường', 387, 0, 0, 100),
(N'Dầu ăn', 884, 0, 100, 0),
(N'Khoai tây', 77, 2, 0.1, 17),
(N'Dưa leo', 15, 0.6, 0.1, 3.6),
(N'Thịt heo băm', 247, 17, 20, 0),
(N'Bí đỏ', 26, 1, 0.1, 6),
(N'Mực', 92, 15, 1.4, 0);

INSERT INTO RECIPE (Name, Describe, CookTime, PrepTime, Servings, Difficulty, Status, AccountID, CategoryID) VALUES 
(N'Thịt kho tàu', N'Món ăn truyền thống ngày Tết', 60, 15, 4, N'Trung bình', 1, 2, 1),
(N'Canh chua cá lóc', N'Thanh mát, giải nhiệt mùa hè', 30, 15, 4, N'Trung bình', 1, 3, 3),
(N'Gà xào sả ớt', N'Đậm đà, cay nồng đưa cơm', 20, 10, 2, N'Dễ', 1, 2, 1),
(N'Salad ức gà', N'Món ăn Eat Clean giảm cân hiệu quả', 10, 10, 1, N'Dễ', 1, 3, 4),
(N'Đậu hũ sốt cà chua', N'Món chay thanh đạm, dễ làm', 15, 5, 2, N'Dễ', 1, 3, 2),
(N'Bò lúc lắc', N'Thịt bò mềm mọng nước', 25, 15, 2, N'Trung bình', 1, 2, 1),
(N'Rau muống xào tỏi', N'Món ăn quốc dân', 10, 5, 2, N'Dễ', 1, 3, 2),
(N'Trứng chiên nước mắm', N'Nhanh, gọn, lẹ cho ngày bận rộn', 5, 2, 2, N'Dễ', 1, 2, 1),
(N'Canh bí đỏ thịt bằm', N'Bổ dưỡng, ngọt nước', 20, 10, 3, N'Dễ', 1, 3, 3),
(N'Mực xào chua ngọt', N'Mực giòn sần sật', 15, 10, 2, N'Trung bình', 1, 2, 1),
(N'Tôm rim mặn ngọt', N'Tôm săn chắc, đậm vị', 20, 10, 4, N'Dễ', 1, 3, 1),
(N'Khoai tây chiên', N'Món ăn vặt giòn rụm', 20, 10, 2, N'Dễ', 1, 2, 5),
(N'Salad cà chua dưa leo', N'Giải ngán cho bữa tiệc', 5, 5, 2, N'Dễ', 1, 3, 4),
(N'Sườn xào chua ngọt', N'Chua chua ngọt ngọt hấp dẫn', 40, 15, 4, N'Khó', 1, 2, 1),
(N'Canh cải xanh cá rô', N'Ngọt mát, dân dã', 30, 20, 4, N'Trung bình', 1, 3, 3),
(N'Gà luộc lá chanh', N'Gà ta da vàng giòn', 45, 10, 4, N'Dễ', 1, 2, 1),
(N'Bò xào hành tây', N'Bò mềm không bị dai', 15, 10, 3, N'Dễ', 1, 3, 1),
(N'Nấm rơm kho tiêu', N'Món chay mặn mà', 20, 10, 2, N'Dễ', 1, 2, 2),
(N'Cơm chiên dương châu', N'Tận dụng cơm nguội', 15, 10, 2, N'Trung bình', 1, 3, 1),
(N'Bắp cải xào tỏi', N'Rau củ xào giòn ngọt', 10, 5, 2, N'Dễ', 1, 2, 2);

INSERT INTO RECIPE_DETAIL (RecipeID, IngredientID, Quantity, UnitID) VALUES 
(1, 1, 500, 1), (1, 6, 5, 6), (1, 13, 3, 4), (1, 14, 2, 4),
(3, 2, 300, 1), (3, 12, 2, 8), (3, 11, 3, 7), (3, 13, 2, 4), (3, 15, 1, 4),
(4, 2, 150, 1), (4, 9, 1, 6), (4, 17, 1, 6),
(5, 5, 200, 1), (5, 9, 2, 6), (5, 10, 1, 6);

INSERT INTO STEPTODO (RecipeID, StepOrder, Content) VALUES 
(3, 1, N'Gà rửa sạch, thái miếng vừa ăn. Sả, tỏi băm nhuyễn.'),
(3, 2, N'Ướp gà với 1/2 sả băm, nước mắm, tiêu trong 15 phút.'),
(3, 3, N'Phi thơm tỏi và sả còn lại, cho gà vào xào săn.'),
(3, 4, N'Thêm chút nước lọc, rim nhỏ lửa đến khi keo lại. Tắt bếp.'),
(4, 1, N'Ức gà luộc chín, xé phay hoặc thái hạt lựu.'),
(4, 2, N'Cà chua, dưa leo rửa sạch, thái lát mỏng.'),
(4, 3, N'Trộn đều các nguyên liệu với nước sốt mè rang hoặc dầu dấm.');

INSERT INTO RECIPE_TAG_MAP (RecipeID, TagID) VALUES 
(4, 1), (4, 3), (1, 4), (3, 3);

INSERT INTO ALLERGY (AccountID, IngredientID) VALUES (2, 7); 

INSERT INTO COMMENT (Content, Rating, AccountID, RecipeID) VALUES 
(N'Món gà xào này ướp ngon quá shop ơi!', 5, 3, 3),
(N'Mình thay đường bằng đường kiêng ăn vẫn rất cuốn.', 4, 2, 4);

INSERT INTO TIP (Title, Content, ImageURL, AccountID) VALUES 
(N'Cách rã đông thịt an toàn', N'Nên rã đông thịt trong ngăn mát tủ lạnh qua đêm thay vì ngâm nước nóng để tránh vi khuẩn sinh sôi.', 'tip-radong.jpg', 1),
(N'Mẹo luộc rau xanh mướt', N'Cho một chút muối vào nước luộc và vớt rau ra ngâm ngay vào tô nước đá lạnh trong 2 phút.', 'tip-luocrau.jpg', 2),
(N'Khử mùi tanh của cá', N'Sử dụng nước vo gạo hoặc rượu trắng pha gừng gập dập để rửa cá trước khi nấu.', 'tip-khutanh.jpg', 3);

INSERT INTO BANNER (Title, ImageURL, LinkURL, Position, IsActive) VALUES 
(N'Sự kiện: Vua Đầu Bếp Mùa Hè', 'banner-event.jpg', '/Cooksnap/Contest', 1, 1),
(N'Mẹo ăn Eat Clean giảm cân', 'banner-eatclean.jpg', '/Tag/Eat-Clean', 2, 1);

INSERT INTO SYSTEM_SETTING (SettingKey, SettingValue) VALUES 
('SiteName', 'CookingShare - Chia sẻ đam mê ẩm thực'),
('ContactEmail', 'support@cookingshare.com'),
('Hotline', '0988.123.456'),
('MaintenanceMode', 'false');
GO