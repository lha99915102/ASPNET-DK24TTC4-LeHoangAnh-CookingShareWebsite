-- =============================================
-- 1. KHỞI TẠO DATABASE
-- =============================================
USE master;
GO
IF EXISTS (SELECT * FROM sys.databases WHERE name = 'CookingShareDB')
    DROP DATABASE CookingShareDB;
GO
CREATE DATABASE CookingShareDB;
GO
USE CookingShareDB;
GO

-- =============================================
-- 2. TẠO CÁC BẢNG (TABLES)
-- =============================================

-- 2.1. Bảng Danh Mục
CREATE TABLE CATEGORY (
    ID INT IDENTITY(1,1) PRIMARY KEY,
    NAME NVARCHAR(100) NOT NULL
);

-- 2.2. Bảng Người Dùng (ACCOUNT)
-- Username là BẮT BUỘC. Email/SĐT là tùy chọn để đăng nhập thêm.
CREATE TABLE ACCOUNT (
    ID INT IDENTITY(1,1) PRIMARY KEY,
    UserName VARCHAR(50) UNIQUE NOT NULL, 
    -- Email và SĐT được phép NULL (nếu người dùng chưa cập nhật)
    -- NHƯNG nếu đã nhập thì phải UNIQUE (Không trùng với người khác)
    Email VARCHAR(100) UNIQUE,         
    Phone VARCHAR(15) UNIQUE, 
    Password VARCHAR(255) NOT NULL,
    Role INT DEFAULT 2, -- 1: Admin, 2: User
    RegistDate DATETIME DEFAULT GETDATE(),
    Status BIT DEFAULT 1
);

-- 2.3. Bảng Hồ Sơ Sức Khỏe (PROFILE)
CREATE TABLE PROFILE (
    ID INT PRIMARY KEY REFERENCES ACCOUNT(ID) ON DELETE CASCADE,
    Name NVARCHAR(100) NOT NULL,
    
    -- Các chỉ số sức khỏe
    Hight FLOAT, 
    Weight FLOAT, 
    Gender NVARCHAR(10), 
    Birth INT,
    LifeStype NVARCHAR(50), 
    Calo FLOAT,
);

-- 2.4. Bảng Nguyên Liệu
CREATE TABLE INGREDIENT (
    ID INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL,
    Unit NVARCHAR(20) DEFAULT N'gam', 
    Calo FLOAT DEFAULT 0, 
    Protein FLOAT DEFAULT 0, 
    Fat FLOAT DEFAULT 0, 
    Sugar FLOAT DEFAULT 0 
);

-- 2.5. Bảng Dị Ứng
CREATE TABLE ALLERGY (
AccountID INT REFERENCES Account(ID) ON DELETE CASCADE,
IngredientID int REFERENCES Ingredient(ID) ON DELETE CASCADE,
PRIMARY KEY (AccountID, IngredientID)
);

-- 2.5. Bảng Công Thức
CREATE TABLE RECIPE (
    ID INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(200) NOT NULL,
    Describe NTEXT,
    Images NVARCHAR(MAX),
    Cooktime INT, 
    Diet INT, 
    CreateDate DATETIME DEFAULT GETDATE(),
    Views INT DEFAULT 0,
    TrangThai BIT DEFAULT 1, 
    AccountID INT REFERENCES Account(ID), 
    CategoryID INT REFERENCES Category(ID)    
);

-- 2.6. Bảng Chi tiết công thức
CREATE TABLE RECIPEDEAILS (
   RecipeID int REFERENCES RECIPE(ID) ON DELETE CASCADE,
   IngredientID int REFERENCES Ingredient(ID),
   Quantity float NOT NULL,
   Unit NVarchar(20),
   PRIMARY KEY (RecipeID, IngredientID)
);

-- 2.7. Bảng bước thực hiện
CREATE TABLE STEPTODO(
    ID Int Identity(1,1) PRIMARY KEY,
    RecipeID Int REFERENCES Recipe(ID) ON DELETE CASCADE,
    Orders int NOT NULL,
    Content NText NOT NULL
);

-- 2.8. Bảng Bài viết
CREATE TABLE ARTICLE(
    ID Int Identity(1,1) PRIMARY KEY,
    Content NText,
    Images NVarchar(MAX),
    PostDate Datetime Default 0,
    DisLike Int Default 0,
    AccountID Int REFERENCES Account(ID) ON DELETE CASCADE,
    RecipeID Int REFERENCES Recipe(ID)
);