USE ProjectBankingApp;

-- ==========================
-- Customers
-- ==========================
CREATE TABLE Customers (
    id              UNIQUEIDENTIFIER   NOT NULL CONSTRAINT DF_Customers_Id DEFAULT NEWID(),
    username        VARCHAR(50)        NOT NULL,
    password        VARCHAR(255)       NOT NULL,
    name            VARCHAR(100)       NOT NULL,
    phone_number    VARCHAR(20)        NOT NULL,
    email           VARCHAR(100)       NOT NULL,
    CONSTRAINT PK_Customers PRIMARY KEY (id),
    CONSTRAINT UQ_Customers_Username UNIQUE (username),
	CONSTRAINT UQ_Customers_PhoneNumber UNIQUE (phone_number),
    CONSTRAINT UQ_Customers_Email UNIQUE (email)
);

-- ==========================
-- Admins
-- ==========================
CREATE TABLE Admins (
    id              UNIQUEIDENTIFIER   NOT NULL CONSTRAINT DF_Admins_Id DEFAULT NEWID(),
    username        VARCHAR(50)        NOT NULL,
    password        VARCHAR(255)       NOT NULL,
    name            VARCHAR(100)       NOT NULL,
    phone_number    VARCHAR(20)        NOT NULL,
    email           VARCHAR(100)       NOT NULL,
    CONSTRAINT PK_Admins PRIMARY KEY (id),
    CONSTRAINT UQ_Admins_Username UNIQUE (username),
	CONSTRAINT UQ_Admins_PhoneNumber UNIQUE (phone_number),
    CONSTRAINT UQ_Admins_Email UNIQUE (email)
);

-- ==========================
-- Checking Accounts
-- ==========================
CREATE TABLE CheckingAccounts (
    id              UNIQUEIDENTIFIER   NOT NULL CONSTRAINT DF_CheckingAccounts_Id DEFAULT NEWID(),
    account_number  VARCHAR(20)        NOT NULL,
    routing_number  VARCHAR(20)        NOT NULL,
    balance         DECIMAL(18,2)      NOT NULL CONSTRAINT DF_CheckingAccounts_Balance DEFAULT 0,
    date_open       DATETIME2          NOT NULL CONSTRAINT DF_CheckingAccounts_DateOpen DEFAULT SYSDATETIME(),
    status          VARCHAR(20)        NOT NULL CONSTRAINT DF_CheckingAccounts_Status DEFAULT 'Active',
    customer_id     UNIQUEIDENTIFIER   NOT NULL,
    CONSTRAINT PK_CheckingAccounts PRIMARY KEY (id),
    CONSTRAINT UQ_CheckingAccounts_AccountNumber UNIQUE (account_number),
    CONSTRAINT FK_CheckingAccounts_Customers FOREIGN KEY (customer_id) REFERENCES Customers(id),
    CONSTRAINT CK_CheckingAccounts_Status CHECK (status IN ('Active', 'Inactive', 'Closed')),
    CONSTRAINT CK_CheckingAccounts_Balance CHECK (balance >= 0)
);

-- ==========================
-- Saving Accounts
-- ==========================
CREATE TABLE SavingAccounts (
    id                UNIQUEIDENTIFIER  NOT NULL CONSTRAINT DF_SavingAccounts_Id DEFAULT NEWID(),
    account_number    VARCHAR(20)       NOT NULL,
    routing_number    VARCHAR(20)       NOT NULL,
    balance           DECIMAL(18,2)     NOT NULL CONSTRAINT DF_SavingAccounts_Balance DEFAULT 0,
    interest_rate     DECIMAL(5,2)      NOT NULL CONSTRAINT DF_SavingAccounts_InterestRate DEFAULT 0.0001,
    withdrawal_limit  DECIMAL(18,2)     NOT NULL CONSTRAINT DF_SavingAccounts_WithdrawlLimit DEFAULT 5000,
    date_open         DATETIME2         NOT NULL CONSTRAINT DF_SavingAccounts_DateOpen DEFAULT SYSDATETIME(),
    status            VARCHAR(20)       NOT NULL CONSTRAINT DF_SavingAccounts_Status DEFAULT 'Active',
    customer_id       UNIQUEIDENTIFIER  NOT NULL,
    CONSTRAINT PK_SavingAccounts PRIMARY KEY (id),
    CONSTRAINT UQ_SavingAccounts_AccountNumber UNIQUE (account_number),
    CONSTRAINT FK_SavingAccounts_Customers FOREIGN KEY (customer_id) REFERENCES Customers(id),
    CONSTRAINT CK_SavingAccounts_Status CHECK (status IN ('Active', 'Inactive', 'Closed')),
    CONSTRAINT CK_SavingAccounts_Balance CHECK (balance >= 0)
);

-- ==========================
-- Transactions
-- ==========================
CREATE TABLE Transactions (
    id                    UNIQUEIDENTIFIER  NOT NULL CONSTRAINT DF_Transactions_Id DEFAULT NEWID(),
    transaction_type      VARCHAR(20)       NOT NULL,
    amount                DECIMAL(18,2)     NOT NULL,
    transaction_date      DATETIME2         NOT NULL CONSTRAINT DF_Transactions_Date DEFAULT SYSDATETIME(),
    saving_account_id     UNIQUEIDENTIFIER  NULL,
    checking_account_id   UNIQUEIDENTIFIER  NULL,
    customer_id           UNIQUEIDENTIFIER  NOT NULL,
    CONSTRAINT PK_Transactions PRIMARY KEY (id),
    CONSTRAINT FK_Transactions_SavingAccounts FOREIGN KEY (saving_account_id) REFERENCES SavingAccounts(id),
    CONSTRAINT FK_Transactions_CheckingAccounts FOREIGN KEY (checking_account_id) REFERENCES CheckingAccounts(id),
    CONSTRAINT FK_Transactions_Customers FOREIGN KEY (customer_id) REFERENCES Customers(id),
    CONSTRAINT CK_Transactions_Type CHECK (transaction_type IN ('Withdraw', 'Deposit', 'Transfer')),
    CONSTRAINT CK_Transactions_AccountRef CHECK (
        (saving_account_id IS NOT NULL AND checking_account_id IS NULL)
        OR (saving_account_id IS NULL AND checking_account_id IS NOT NULL)
    )
);

-- ==========================
-- Cheque Book Requests
-- ==========================
CREATE TABLE ChequeBookRequests (
    id                    UNIQUEIDENTIFIER  NOT NULL CONSTRAINT DF_ChequeBookRequests_Id DEFAULT NEWID(),
    request_date          DATETIME2         NOT NULL CONSTRAINT DF_ChequeBookRequests_Date DEFAULT SYSDATETIME(),
    status                VARCHAR(20)       NOT NULL CONSTRAINT DF_ChequeBookRequests_Status DEFAULT 'Pending',
    checking_account_id   UNIQUEIDENTIFIER  NOT NULL,
    customer_id           UNIQUEIDENTIFIER  NOT NULL,
    admin_id              UNIQUEIDENTIFIER  NULL,
    CONSTRAINT PK_ChequeBookRequests PRIMARY KEY (id),
    CONSTRAINT FK_ChequeBookRequests_CheckingAccounts FOREIGN KEY (checking_account_id) REFERENCES CheckingAccounts(id),
    CONSTRAINT FK_ChequeBookRequests_Customers FOREIGN KEY (customer_id) REFERENCES Customers(id),
    CONSTRAINT FK_ChequeBookRequests_Admins FOREIGN KEY (admin_id) REFERENCES Admins(id),
    CONSTRAINT CK_ChequeBookRequests_Status CHECK (status IN ('Pending', 'Approved', 'Rejected'))
);