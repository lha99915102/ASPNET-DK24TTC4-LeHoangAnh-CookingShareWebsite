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
    IDC INT IDENTITY(1,1) PRIMARY KEY,
    NAMEC NVARCHAR(100) NOT NULL
);

-- 2.2. Bảng Người Dùng (ACCOUNT)
-- Username là BẮT BUỘC. Email/SĐT là tùy chọn để đăng nhập thêm.
CREATE TABLE ACCOUNT (
    IDA INT IDENTITY(1,1) PRIMARY KEY,
    UserNameA VARCHAR(50) UNIQUE NOT NULL, 
    -- Email và SĐT được phép NULL (nếu người dùng chưa cập nhật)
    -- NHƯNG nếu đã nhập thì phải UNIQUE (Không trùng với người khác)
    EmailA VARCHAR(100) UNIQUE,         
    PhoneA VARCHAR(15) UNIQUE, 
    PasswordA VARCHAR(255) NOT NULL,
    RoleA INT DEFAULT 2, -- 1: Admin, 2: User
    RegistDateA DATETIME DEFAULT GETDATE(),
    StatusA BIT DEFAULT 1
);

-- 2.3. Bảng Hồ Sơ Sức Khỏe (PROFILE)
CREATE TABLE PROFILE (
    IDP INT PRIMARY KEY REFERENCES ACCOUNT(IDA) ON DELETE CASCADE,
    NameP NVARCHAR(100) NOT NULL,
    
    -- Các chỉ số sức khỏe
    HightP FLOAT, 
    WeightP FLOAT, 
    GenderP NVARCHAR(10), 
    BirthP INT,
    LifeStypeP NVARCHAR(50), 
    CaloP FLOAT,
);

-- 2.4. Bảng Nguyên Liệu
CREATE TABLE INGERDIENT (
    IDI INT IDENTITY(1,1) PRIMARY KEY,
    NameI NVARCHAR(100) NOT NULL,
    UnitI NVARCHAR(20) DEFAULT N'gam', 
    CaloI FLOAT DEFAULT 0, 
    ProteinI FLOAT DEFAULT 0, 
    FatI FLOAT DEFAULT 0, 
    SugarI FLOAT DEFAULT 0 
);

-- 2.5. Bảng Dị Ứng
CREATE TABLE ALLERGY (
ID INT
);

-- 2.5. Bảng Công Thức
CREATE TABLE RECIPE (
    ID INT IDENTITY(1,1) PRIMARY KEY,
    NameR NVARCHAR(200) NOT NULL,
    Describe NTEXT,
    ImageR NVARCHAR(MAX),
    Cooktime INT, 
    Diet INT, 
    CreateDate DATETIME DEFAULT GETDATE(),
    ViewR INT DEFAULT 0,
    TrangThai BIT DEFAULT 1, 
    AccountID INT REFERENCES Account(IDA), 
    CategoryID INT REFERENCES Category(IDC)    
);
