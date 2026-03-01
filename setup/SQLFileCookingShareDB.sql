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
-- 2. QUẢN LÝ NGƯỜI DÙNG & HỆ THỐNG
-- =============================================

-- 2.1. Bảng Tài khoản
CREATE TABLE ACCOUNT (
    ID INT IDENTITY(1,1) PRIMARY KEY,
    UserName VARCHAR(50) UNIQUE NOT NULL, 
    Email VARCHAR(100) UNIQUE,          
    Phone VARCHAR(15) UNIQUE,  
    Password VARCHAR(255) NOT NULL,
    Role INT DEFAULT 2, -- 1: Admin, 2: User, 3: Moderator
    Status INT DEFAULT 1, -- 1: Active, 0: Locked, 2: Pending
    RegistDate DATETIME DEFAULT GETDATE(),
    -- Cột phục vụ tính năng Quên mật khẩu
    ResetToken VARCHAR(100) NULL,
    TokenExpiry DATETIME NULL
);

-- 2.2. Bảng Hồ sơ
CREATE TABLE PROFILE (
    AccountID INT PRIMARY KEY REFERENCES ACCOUNT(ID) ON DELETE CASCADE,
    FullName NVARCHAR(100) NOT NULL,
    Avatar NVARCHAR(MAX) DEFAULT 'default-avatar.png',
    Height FLOAT,
    Weight FLOAT, 
    Gender NVARCHAR(10), 
    BirthDay DATE,
    Lifestyle NVARCHAR(50),
    CaloDaily FLOAT DEFAULT 0 -- Calo cần thiết mỗi ngày
);

-- =============================================
-- 3. QUẢN LÝ NỘI DUNG CHÍNH (CÔNG THỨC)
-- =============================================

-- 3.1. Bảng Danh mục
CREATE TABLE CATEGORY (
    ID INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL,
    ImageURL NVARCHAR(MAX) -- Ảnh đại diện danh mục
);

-- 3.2. Bảng Tag & Chế độ ăn
CREATE TABLE TAG (
    ID INT IDENTITY(1,1) PRIMARY KEY,
    TagName NVARCHAR(50) NOT NULL,
    TagType NVARCHAR(30) -- VD: 'Diet', 'Season', 'Event'
);

-- 3.3. Bảng Công thức
CREATE TABLE RECIPE (
    ID INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(200) NOT NULL,
    Describe NVARCHAR(MAX),
    MainImage NVARCHAR(MAX),
    CookTime INT, -- Phút
    PrepTime INT, -- Phút (Sơ chế)
    Servings INT DEFAULT 2, -- Khẩu phần
    Difficulty NVARCHAR(20), -- Dễ, TB, Khó
    Status INT DEFAULT 0, -- 0: Chờ duyệt, 1: Đã đăng, 2: Từ chối
    RejectReason NVARCHAR(MAX), -- Lý do từ chối
    Views INT DEFAULT 0,
    CreateDate DATETIME DEFAULT GETDATE(),
    AccountID INT REFERENCES ACCOUNT(ID), 
    CategoryID INT REFERENCES CATEGORY(ID)    
);

-- 3.4. Bảng liên kết Công thức - Tag (N-N)
CREATE TABLE RECIPE_TAG_MAP (
    RecipeID INT REFERENCES RECIPE(ID) ON DELETE CASCADE,
    TagID INT REFERENCES TAG(ID) ON DELETE CASCADE,
    PRIMARY KEY (RecipeID, TagID)
);

-- =============================================
-- 4. CHI TIẾT CÔNG THỨC & QUY TRÌNH
-- =============================================

-- 4.1. Bảng Đơn vị tính
CREATE TABLE UNIT (
    ID INT IDENTITY(1,1) PRIMARY KEY,
    UnitName NVARCHAR(20) NOT NULL -- g, kg, muỗng, ml...
);

-- 4.2. Bảng Nguyên liệu (Calo tính trên 100g)
CREATE TABLE INGREDIENT (
    ID INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL,
    Calo FLOAT DEFAULT 0, 
    Protein FLOAT DEFAULT 0, 
    Fat FLOAT DEFAULT 0, 
    Sugar FLOAT DEFAULT 0 
);

-- 4.3. Chi tiết nguyên liệu trong món ăn
CREATE TABLE RECIPE_DETAIL (
    RecipeID INT REFERENCES RECIPE(ID) ON DELETE CASCADE,
    IngredientID INT REFERENCES INGREDIENT(ID),
    Quantity FLOAT NOT NULL,
    UnitID INT REFERENCES UNIT(ID),
    PRIMARY KEY (RecipeID, IngredientID)
);

-- 4.4. Các bước thực hiện
CREATE TABLE STEPTODO (
    ID INT IDENTITY(1,1) PRIMARY KEY,
    RecipeID INT REFERENCES RECIPE(ID) ON DELETE CASCADE,
    StepOrder INT NOT NULL,
    Content NVARCHAR(MAX) NOT NULL,
    ImageURL NVARCHAR(MAX) -- Ảnh minh họa cho từng bước
);

-- =============================================
-- 5. TƯƠNG TÁC & MARKETING
-- =============================================

-- 5.1. Bảng Dị ứng
CREATE TABLE ALLERGY (
    AccountID INT REFERENCES ACCOUNT(ID) ON DELETE CASCADE,
    IngredientID INT REFERENCES INGREDIENT(ID) ON DELETE CASCADE,
    PRIMARY KEY (AccountID, IngredientID)
);

-- 5.2. Bảng Bình luận & Đánh giá
CREATE TABLE COMMENT (
    ID INT IDENTITY(1,1) PRIMARY KEY,
    Content NVARCHAR(MAX),
    Rating INT CHECK (Rating >= 1 AND Rating <= 5), -- Đánh giá sao
    CreateDate DATETIME DEFAULT GETDATE(),
    Status INT DEFAULT 1, -- 1: Hiển thị, 0: Bị ẩn (vi phạm)
    AccountID INT REFERENCES ACCOUNT(ID),
    RecipeID INT REFERENCES RECIPE(ID) ON DELETE CASCADE
);

-- 5.3. Bảng Mẹo vặt
CREATE TABLE TIP (
    ID INT IDENTITY(1,1) PRIMARY KEY,
    Title NVARCHAR(255) NOT NULL,
    Content NVARCHAR(MAX),
    ImageURL NVARCHAR(MAX),
    CreateDate DATETIME DEFAULT GETDATE(),
    AccountID INT REFERENCES ACCOUNT(ID)
);

-- 5.4. Bảng Banner (Quảng cáo trang chủ)
CREATE TABLE BANNER (
    ID INT IDENTITY(1,1) PRIMARY KEY,
    Title NVARCHAR(100),
    ImageURL NVARCHAR(MAX) NOT NULL,
    LinkURL NVARCHAR(MAX),
    Position INT DEFAULT 1, -- Thứ tự hiển thị
    IsActive BIT DEFAULT 1
);

-- 5.5. Cấu hình hệ thống (Admin Settings)
CREATE TABLE SYSTEM_SETTING (
    ID INT IDENTITY(1,1) PRIMARY KEY,
    SettingKey VARCHAR(50) UNIQUE, -- VD: 'Logo', 'SiteName'
    SettingValue NVARCHAR(MAX)
);
GO