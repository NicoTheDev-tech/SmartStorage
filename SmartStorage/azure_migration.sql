IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260410211308_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetRoles] (
        [Id] nvarchar(450) NOT NULL,
        [Name] nvarchar(256) NULL,
        [NormalizedName] nvarchar(256) NULL,
        [ConcurrencyStamp] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetRoles] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260410211308_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetUsers] (
        [Id] nvarchar(450) NOT NULL,
        [UserName] nvarchar(256) NULL,
        [NormalizedUserName] nvarchar(256) NULL,
        [Email] nvarchar(256) NULL,
        [NormalizedEmail] nvarchar(256) NULL,
        [EmailConfirmed] bit NOT NULL,
        [PasswordHash] nvarchar(max) NULL,
        [SecurityStamp] nvarchar(max) NULL,
        [ConcurrencyStamp] nvarchar(max) NULL,
        [PhoneNumber] nvarchar(max) NULL,
        [PhoneNumberConfirmed] bit NOT NULL,
        [TwoFactorEnabled] bit NOT NULL,
        [LockoutEnd] datetimeoffset NULL,
        [LockoutEnabled] bit NOT NULL,
        [AccessFailedCount] int NOT NULL,
        CONSTRAINT [PK_AspNetUsers] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260410211308_InitialCreate'
)
BEGIN
    CREATE TABLE [Clients] (
        [Id] int NOT NULL IDENTITY,
        [UserId] nvarchar(max) NOT NULL,
        [FullName] nvarchar(max) NOT NULL,
        [PreferredName] nvarchar(max) NOT NULL,
        [Email] nvarchar(450) NOT NULL,
        [Phone] nvarchar(450) NOT NULL,
        [Address] nvarchar(max) NOT NULL,
        [IdNumber] nvarchar(450) NOT NULL,
        [RegistrationDate] datetime2 NOT NULL,
        CONSTRAINT [PK_Clients] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260410211308_InitialCreate'
)
BEGIN
    CREATE TABLE [StorageUnits] (
        [Id] int NOT NULL IDENTITY,
        [UnitNumber] nvarchar(450) NULL,
        [Size] nvarchar(max) NULL,
        [MonthlyRate] decimal(18,2) NOT NULL,
        [IsActive] bit NOT NULL,
        [Location] nvarchar(max) NULL,
        [ClimateControl] nvarchar(max) NULL,
        CONSTRAINT [PK_StorageUnits] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260410211308_InitialCreate'
)
BEGIN
    CREATE TABLE [Vehicles] (
        [Id] int NOT NULL IDENTITY,
        [RegistrationNumber] nvarchar(450) NULL,
        [Model] nvarchar(max) NULL,
        [Type] nvarchar(max) NULL,
        [Capacity] decimal(18,2) NOT NULL,
        [PurchaseDate] datetime2 NOT NULL,
        [LastMaintenanceDate] datetime2 NULL,
        [NextMaintenanceDate] datetime2 NULL,
        [Status] int NOT NULL,
        CONSTRAINT [PK_Vehicles] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260410211308_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetRoleClaims] (
        [Id] int NOT NULL IDENTITY,
        [RoleId] nvarchar(450) NOT NULL,
        [ClaimType] nvarchar(max) NULL,
        [ClaimValue] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260410211308_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetUserClaims] (
        [Id] int NOT NULL IDENTITY,
        [UserId] nvarchar(450) NOT NULL,
        [ClaimType] nvarchar(max) NULL,
        [ClaimValue] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260410211308_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetUserLogins] (
        [LoginProvider] nvarchar(450) NOT NULL,
        [ProviderKey] nvarchar(450) NOT NULL,
        [ProviderDisplayName] nvarchar(max) NULL,
        [UserId] nvarchar(450) NOT NULL,
        CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY ([LoginProvider], [ProviderKey]),
        CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260410211308_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetUserRoles] (
        [UserId] nvarchar(450) NOT NULL,
        [RoleId] nvarchar(450) NOT NULL,
        CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY ([UserId], [RoleId]),
        CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260410211308_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetUserTokens] (
        [UserId] nvarchar(450) NOT NULL,
        [LoginProvider] nvarchar(450) NOT NULL,
        [Name] nvarchar(450) NOT NULL,
        [Value] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY ([UserId], [LoginProvider], [Name]),
        CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260410211308_InitialCreate'
)
BEGIN
    CREATE TABLE [Assets] (
        [Id] int NOT NULL IDENTITY,
        [ClientId] int NULL,
        [Name] nvarchar(max) NULL,
        [Description] nvarchar(max) NULL,
        [Category] nvarchar(max) NULL,
        [StartingPrice] decimal(18,2) NOT NULL,
        [CurrentBid] decimal(18,2) NULL,
        [AuctionStartDate] datetime2 NOT NULL,
        [AuctionEndDate] datetime2 NOT NULL,
        [Status] int NOT NULL,
        CONSTRAINT [PK_Assets] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Assets_Clients_ClientId] FOREIGN KEY ([ClientId]) REFERENCES [Clients] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260410211308_InitialCreate'
)
BEGIN
    CREATE TABLE [Bookings] (
        [Id] int NOT NULL IDENTITY,
        [BookingNumber] nvarchar(max) NULL,
        [ClientId] int NOT NULL,
        [StorageUnitId] int NOT NULL,
        [StartDate] datetime2 NOT NULL,
        [EndDate] datetime2 NOT NULL,
        [TotalAmount] decimal(18,2) NOT NULL,
        [Status] int NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_Bookings] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Bookings_Clients_ClientId] FOREIGN KEY ([ClientId]) REFERENCES [Clients] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Bookings_StorageUnits_StorageUnitId] FOREIGN KEY ([StorageUnitId]) REFERENCES [StorageUnits] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260410211308_InitialCreate'
)
BEGIN
    CREATE TABLE [Drivers] (
        [Id] int NOT NULL IDENTITY,
        [UserId] nvarchar(max) NULL,
        [FullName] nvarchar(max) NULL,
        [LicenseNumber] nvarchar(450) NULL,
        [Phone] nvarchar(max) NULL,
        [IsAvailable] bit NOT NULL,
        [AssignedVehicleId] int NULL,
        CONSTRAINT [PK_Drivers] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Drivers_Vehicles_AssignedVehicleId] FOREIGN KEY ([AssignedVehicleId]) REFERENCES [Vehicles] ([Id]) ON DELETE SET NULL
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260410211308_InitialCreate'
)
BEGIN
    CREATE TABLE [MaintenanceRecords] (
        [Id] int NOT NULL IDENTITY,
        [VehicleId] int NOT NULL,
        [ScheduledDate] datetime2 NOT NULL,
        [CompletedDate] datetime2 NULL,
        [ServiceType] nvarchar(max) NULL,
        [Description] nvarchar(max) NULL,
        [Cost] decimal(18,2) NOT NULL,
        [ServiceProvider] nvarchar(max) NULL,
        [Status] int NOT NULL,
        CONSTRAINT [PK_MaintenanceRecords] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_MaintenanceRecords_Vehicles_VehicleId] FOREIGN KEY ([VehicleId]) REFERENCES [Vehicles] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260410211308_InitialCreate'
)
BEGIN
    CREATE TABLE [Bids] (
        [Id] int NOT NULL IDENTITY,
        [AssetId] int NOT NULL,
        [BuyerId] int NOT NULL,
        [Amount] decimal(18,2) NOT NULL,
        [BidTime] datetime2 NOT NULL,
        [IsWinning] bit NOT NULL,
        CONSTRAINT [PK_Bids] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Bids_Assets_AssetId] FOREIGN KEY ([AssetId]) REFERENCES [Assets] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_Bids_Clients_BuyerId] FOREIGN KEY ([BuyerId]) REFERENCES [Clients] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260410211308_InitialCreate'
)
BEGIN
    CREATE TABLE [Contracts] (
        [Id] int NOT NULL IDENTITY,
        [ContractNumber] nvarchar(450) NOT NULL,
        [BookingId] int NOT NULL,
        [ClientId] int NOT NULL,
        [StartDate] datetime2 NOT NULL,
        [EndDate] datetime2 NOT NULL,
        [MonthlyRate] decimal(18,2) NOT NULL,
        [SecurityDeposit] decimal(18,2) NOT NULL,
        [TotalContractValue] decimal(18,2) NOT NULL,
        [TermsAndConditions] nvarchar(max) NOT NULL,
        [SpecialConditions] nvarchar(max) NOT NULL,
        [Status] int NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [AcceptedAt] datetime2 NULL,
        [ActivatedAt] datetime2 NULL,
        [AcceptedBy] nvarchar(max) NULL,
        [AcceptedIpAddress] nvarchar(max) NULL,
        CONSTRAINT [PK_Contracts] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Contracts_Bookings_BookingId] FOREIGN KEY ([BookingId]) REFERENCES [Bookings] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Contracts_Clients_ClientId] FOREIGN KEY ([ClientId]) REFERENCES [Clients] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260410211308_InitialCreate'
)
BEGIN
    CREATE TABLE [Invoices] (
        [Id] int NOT NULL IDENTITY,
        [InvoiceNumber] nvarchar(450) NOT NULL,
        [BookingId] int NOT NULL,
        [ContractId] int NULL,
        [ClientId] int NOT NULL,
        [InvoiceDate] datetime2 NOT NULL,
        [DueDate] datetime2 NOT NULL,
        [Amount] decimal(18,2) NOT NULL,
        [AmountPaid] decimal(18,2) NOT NULL,
        [Balance] decimal(18,2) NOT NULL,
        [PeriodStart] datetime2 NOT NULL,
        [PeriodEnd] datetime2 NOT NULL,
        [BillingMonth] int NOT NULL,
        [BillingYear] int NOT NULL,
        [PaymentReference] nvarchar(max) NULL,
        [PaymentDate] datetime2 NULL,
        [PaymentMethod] nvarchar(max) NULL,
        [Status] int NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [PaidAt] datetime2 NULL,
        [Notes] nvarchar(max) NULL,
        CONSTRAINT [PK_Invoices] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Invoices_Bookings_BookingId] FOREIGN KEY ([BookingId]) REFERENCES [Bookings] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Invoices_Clients_ClientId] FOREIGN KEY ([ClientId]) REFERENCES [Clients] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260410211308_InitialCreate'
)
BEGIN
    CREATE TABLE [Payments] (
        [Id] int NOT NULL IDENTITY,
        [PaymentReference] nvarchar(max) NULL,
        [BookingId] int NOT NULL,
        [Amount] decimal(18,2) NOT NULL,
        [PaymentDate] datetime2 NOT NULL,
        [Method] int NOT NULL,
        [Status] int NOT NULL,
        [ProofOfPaymentPath] nvarchar(max) NULL,
        [TransactionId] nvarchar(max) NULL,
        [ClientId] int NULL,
        CONSTRAINT [PK_Payments] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Payments_Bookings_BookingId] FOREIGN KEY ([BookingId]) REFERENCES [Bookings] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Payments_Clients_ClientId] FOREIGN KEY ([ClientId]) REFERENCES [Clients] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260410211308_InitialCreate'
)
BEGIN
    CREATE TABLE [Cartages] (
        [Id] int NOT NULL IDENTITY,
        [CartageNumber] nvarchar(max) NULL,
        [BookingId] int NOT NULL,
        [DriverId] int NULL,
        [PickupAddress] nvarchar(max) NULL,
        [DeliveryAddress] nvarchar(max) NULL,
        [GoodsDescription] nvarchar(max) NULL,
        [GoodsWeight] decimal(18,2) NOT NULL,
        [ItemCount] int NOT NULL,
        [ScheduledDate] datetime2 NOT NULL,
        [PickupDate] datetime2 NULL,
        [DeliveryDate] datetime2 NULL,
        [Status] int NOT NULL,
        [Cost] decimal(18,2) NOT NULL,
        CONSTRAINT [PK_Cartages] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Cartages_Bookings_BookingId] FOREIGN KEY ([BookingId]) REFERENCES [Bookings] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Cartages_Drivers_DriverId] FOREIGN KEY ([DriverId]) REFERENCES [Drivers] ([Id]) ON DELETE SET NULL
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260410211308_InitialCreate'
)
BEGIN
    CREATE TABLE [DeliverySchedules] (
        [Id] int NOT NULL IDENTITY,
        [ScheduleNumber] nvarchar(450) NOT NULL,
        [BookingId] int NOT NULL,
        [ClientId] int NOT NULL,
        [DeliveryType] int NOT NULL,
        [ScheduledDate] datetime2 NOT NULL,
        [ScheduledTime] time NOT NULL,
        [TimeSlot] nvarchar(max) NOT NULL,
        [PickupAddress] nvarchar(max) NOT NULL,
        [DeliveryAddress] nvarchar(max) NOT NULL,
        [GoodsDescription] nvarchar(max) NOT NULL,
        [ItemCount] int NOT NULL,
        [EstimatedWeight] decimal(18,2) NOT NULL,
        [Status] int NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [ConfirmedAt] datetime2 NULL,
        [CompletedAt] datetime2 NULL,
        [AssignedDriverId] int NULL,
        [SpecialInstructions] nvarchar(max) NULL,
        [ContactPerson] nvarchar(max) NULL,
        [ContactPhone] nvarchar(max) NULL,
        CONSTRAINT [PK_DeliverySchedules] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_DeliverySchedules_Bookings_BookingId] FOREIGN KEY ([BookingId]) REFERENCES [Bookings] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_DeliverySchedules_Clients_ClientId] FOREIGN KEY ([ClientId]) REFERENCES [Clients] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_DeliverySchedules_Drivers_AssignedDriverId] FOREIGN KEY ([AssignedDriverId]) REFERENCES [Drivers] ([Id]) ON DELETE SET NULL
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260410211308_InitialCreate'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'ClimateControl', N'IsActive', N'Location', N'MonthlyRate', N'Size', N'UnitNumber') AND [object_id] = OBJECT_ID(N'[StorageUnits]'))
        SET IDENTITY_INSERT [StorageUnits] ON;
    EXEC(N'INSERT INTO [StorageUnits] ([Id], [ClimateControl], [IsActive], [Location], [MonthlyRate], [Size], [UnitNumber])
    VALUES (1, N''None'', CAST(1 AS bit), N''Building A'', 100.0, N''10x10'', N''A101''),
    (2, N''Basic'', CAST(1 AS bit), N''Building A'', 180.0, N''10x20'', N''A102''),
    (3, N''Premium'', CAST(1 AS bit), N''Building B'', 350.0, N''20x20'', N''B201'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'ClimateControl', N'IsActive', N'Location', N'MonthlyRate', N'Size', N'UnitNumber') AND [object_id] = OBJECT_ID(N'[StorageUnits]'))
        SET IDENTITY_INSERT [StorageUnits] OFF;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260410211308_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AspNetRoleClaims_RoleId] ON [AspNetRoleClaims] ([RoleId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260410211308_InitialCreate'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [RoleNameIndex] ON [AspNetRoles] ([NormalizedName]) WHERE [NormalizedName] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260410211308_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AspNetUserClaims_UserId] ON [AspNetUserClaims] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260410211308_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AspNetUserLogins_UserId] ON [AspNetUserLogins] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260410211308_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AspNetUserRoles_RoleId] ON [AspNetUserRoles] ([RoleId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260410211308_InitialCreate'
)
BEGIN
    CREATE INDEX [EmailIndex] ON [AspNetUsers] ([NormalizedEmail]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260410211308_InitialCreate'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [UserNameIndex] ON [AspNetUsers] ([NormalizedUserName]) WHERE [NormalizedUserName] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260410211308_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Assets_ClientId] ON [Assets] ([ClientId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260410211308_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Bids_AssetId] ON [Bids] ([AssetId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260410211308_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Bids_BuyerId] ON [Bids] ([BuyerId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260410211308_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Bookings_ClientId] ON [Bookings] ([ClientId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260410211308_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Bookings_StorageUnitId] ON [Bookings] ([StorageUnitId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260410211308_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Cartages_BookingId] ON [Cartages] ([BookingId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260410211308_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Cartages_DriverId] ON [Cartages] ([DriverId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260410211308_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Clients_Email] ON [Clients] ([Email]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260410211308_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Clients_IdNumber] ON [Clients] ([IdNumber]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260410211308_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Clients_Phone] ON [Clients] ([Phone]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260410211308_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Contracts_BookingId] ON [Contracts] ([BookingId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260410211308_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Contracts_ClientId] ON [Contracts] ([ClientId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260410211308_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Contracts_ContractNumber] ON [Contracts] ([ContractNumber]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260410211308_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_DeliverySchedules_AssignedDriverId] ON [DeliverySchedules] ([AssignedDriverId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260410211308_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_DeliverySchedules_BookingId] ON [DeliverySchedules] ([BookingId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260410211308_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_DeliverySchedules_ClientId] ON [DeliverySchedules] ([ClientId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260410211308_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_DeliverySchedules_ScheduleNumber] ON [DeliverySchedules] ([ScheduleNumber]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260410211308_InitialCreate'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_Drivers_AssignedVehicleId] ON [Drivers] ([AssignedVehicleId]) WHERE [AssignedVehicleId] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260410211308_InitialCreate'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_Drivers_LicenseNumber] ON [Drivers] ([LicenseNumber]) WHERE [LicenseNumber] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260410211308_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Invoices_BookingId] ON [Invoices] ([BookingId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260410211308_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Invoices_ClientId] ON [Invoices] ([ClientId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260410211308_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Invoices_InvoiceNumber] ON [Invoices] ([InvoiceNumber]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260410211308_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_MaintenanceRecords_VehicleId] ON [MaintenanceRecords] ([VehicleId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260410211308_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Payments_BookingId] ON [Payments] ([BookingId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260410211308_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Payments_ClientId] ON [Payments] ([ClientId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260410211308_InitialCreate'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_StorageUnits_UnitNumber] ON [StorageUnits] ([UnitNumber]) WHERE [UnitNumber] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260410211308_InitialCreate'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_Vehicles_RegistrationNumber] ON [Vehicles] ([RegistrationNumber]) WHERE [RegistrationNumber] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260410211308_InitialCreate'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260410211308_InitialCreate', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260410212628_FixDecimalPrecision'
)
BEGIN
                    -- StorageUnits
                    IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('StorageUnits') AND name = 'MonthlyRate' AND system_type_id = 106)
                        ALTER TABLE StorageUnits ALTER COLUMN MonthlyRate DECIMAL(18,2);
                    -- Assets
                    IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Assets') AND name = 'StartingPrice' AND system_type_id = 106)
                        ALTER TABLE Assets ALTER COLUMN StartingPrice DECIMAL(18,2);
                    IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Assets') AND name = 'CurrentBid' AND system_type_id = 106)
                        ALTER TABLE Assets ALTER COLUMN CurrentBid DECIMAL(18,2);
                    -- Bids
                    IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Bids') AND name = 'Amount' AND system_type_id = 106)
                        ALTER TABLE Bids ALTER COLUMN Amount DECIMAL(18,2);
                    -- Bookings
                    IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Bookings') AND name = 'TotalAmount' AND system_type_id = 106)
                        ALTER TABLE Bookings ALTER COLUMN TotalAmount DECIMAL(18,2);
                    -- Cartages
                    IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Cartages') AND name = 'Cost' AND system_type_id = 106)
                        ALTER TABLE Cartages ALTER COLUMN Cost DECIMAL(18,2);
                    IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Cartages') AND name = 'GoodsWeight' AND system_type_id = 106)
                        ALTER TABLE Cartages ALTER COLUMN GoodsWeight DECIMAL(18,2);
                    -- Contracts
                    IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Contracts') AND name = 'MonthlyRate' AND system_type_id = 106)
                        ALTER TABLE Contracts ALTER COLUMN MonthlyRate DECIMAL(18,2);
                    IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Contracts') AND name = 'SecurityDeposit' AND system_type_id = 106)
                        ALTER TABLE Contracts ALTER COLUMN SecurityDeposit DECIMAL(18,2);
                    IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Contracts') AND name = 'TotalContractValue' AND system_type_id = 106)
                        ALTER TABLE Contracts ALTER COLUMN TotalContractValue DECIMAL(18,2);
                    -- DeliverySchedules
                    IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('DeliverySchedules') AND name = 'EstimatedWeight' AND system_type_id = 106)
                        ALTER TABLE DeliverySchedules ALTER COLUMN EstimatedWeight DECIMAL(18,2);
                    -- Invoices
                    IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Invoices') AND name = 'Amount' AND system_type_id = 106)
                        ALTER TABLE Invoices ALTER COLUMN Amount DECIMAL(18,2);
                    IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Invoices') AND name = 'AmountPaid' AND system_type_id = 106)
                        ALTER TABLE Invoices ALTER COLUMN AmountPaid DECIMAL(18,2);
                    IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Invoices') AND name = 'Balance' AND system_type_id = 106)
                        ALTER TABLE Invoices ALTER COLUMN Balance DECIMAL(18,2);
                    -- MaintenanceRecords
                    IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('MaintenanceRecords') AND name = 'Cost' AND system_type_id = 106)
                        ALTER TABLE MaintenanceRecords ALTER COLUMN Cost DECIMAL(18,2);
                    -- Payments
                    IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Payments') AND name = 'Amount' AND system_type_id = 106)
                        ALTER TABLE Payments ALTER COLUMN Amount DECIMAL(18,2);
                    -- Vehicles
                    IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Vehicles') AND name = 'Capacity' AND system_type_id = 106)
                        ALTER TABLE Vehicles ALTER COLUMN Capacity DECIMAL(18,2);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260410212628_FixDecimalPrecision'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260410212628_FixDecimalPrecision', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260411011549_AddStorageUnitColumns'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260411011549_AddStorageUnitColumns', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260411012912_AddLocationAndClimateControl'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260411012912_AddLocationAndClimateControl', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260423201622_AddStaffEntities'
)
BEGIN
    DROP INDEX [IX_Invoices_InvoiceNumber] ON [Invoices];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260423201622_AddStaffEntities'
)
BEGIN
    DROP INDEX [IX_DeliverySchedules_ScheduleNumber] ON [DeliverySchedules];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260423201622_AddStaffEntities'
)
BEGIN
    DROP INDEX [IX_Contracts_ContractNumber] ON [Contracts];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260423201622_AddStaffEntities'
)
BEGIN
    DROP INDEX [IX_Clients_Email] ON [Clients];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260423201622_AddStaffEntities'
)
BEGIN
    DROP INDEX [IX_Clients_IdNumber] ON [Clients];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260423201622_AddStaffEntities'
)
BEGIN
    DROP INDEX [IX_Clients_Phone] ON [Clients];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260423201622_AddStaffEntities'
)
BEGIN
    ALTER TABLE [StorageUnits] ADD [OccupancyStatus] int NOT NULL DEFAULT 0;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260423201622_AddStaffEntities'
)
BEGIN
    DECLARE @var0 sysname;
    SELECT @var0 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Invoices]') AND [c].[name] = N'InvoiceNumber');
    IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [Invoices] DROP CONSTRAINT [' + @var0 + '];');
    ALTER TABLE [Invoices] ALTER COLUMN [InvoiceNumber] nvarchar(450) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260423201622_AddStaffEntities'
)
BEGIN
    ALTER TABLE [Drivers] ADD [DriverStatus] int NOT NULL DEFAULT 0;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260423201622_AddStaffEntities'
)
BEGIN
    ALTER TABLE [Drivers] ADD [Email] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260423201622_AddStaffEntities'
)
BEGIN
    ALTER TABLE [Drivers] ADD [EmployeeNumber] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260423201622_AddStaffEntities'
)
BEGIN
    ALTER TABLE [Drivers] ADD [HireDate] datetime2 NOT NULL DEFAULT '0001-01-01T00:00:00.0000000';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260423201622_AddStaffEntities'
)
BEGIN
    ALTER TABLE [Drivers] ADD [Role] int NOT NULL DEFAULT 0;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260423201622_AddStaffEntities'
)
BEGIN
    ALTER TABLE [Drivers] ADD [Status] int NOT NULL DEFAULT 0;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260423201622_AddStaffEntities'
)
BEGIN
    ALTER TABLE [Drivers] ADD [TerminationDate] datetime2 NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260423201622_AddStaffEntities'
)
BEGIN
    ALTER TABLE [Drivers] ADD [VehicleAssigned] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260423201622_AddStaffEntities'
)
BEGIN
    DECLARE @var1 sysname;
    SELECT @var1 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[DeliverySchedules]') AND [c].[name] = N'TimeSlot');
    IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [DeliverySchedules] DROP CONSTRAINT [' + @var1 + '];');
    ALTER TABLE [DeliverySchedules] ALTER COLUMN [TimeSlot] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260423201622_AddStaffEntities'
)
BEGIN
    DECLARE @var2 sysname;
    SELECT @var2 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[DeliverySchedules]') AND [c].[name] = N'ScheduleNumber');
    IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [DeliverySchedules] DROP CONSTRAINT [' + @var2 + '];');
    ALTER TABLE [DeliverySchedules] ALTER COLUMN [ScheduleNumber] nvarchar(450) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260423201622_AddStaffEntities'
)
BEGIN
    DECLARE @var3 sysname;
    SELECT @var3 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[DeliverySchedules]') AND [c].[name] = N'PickupAddress');
    IF @var3 IS NOT NULL EXEC(N'ALTER TABLE [DeliverySchedules] DROP CONSTRAINT [' + @var3 + '];');
    ALTER TABLE [DeliverySchedules] ALTER COLUMN [PickupAddress] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260423201622_AddStaffEntities'
)
BEGIN
    DECLARE @var4 sysname;
    SELECT @var4 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[DeliverySchedules]') AND [c].[name] = N'GoodsDescription');
    IF @var4 IS NOT NULL EXEC(N'ALTER TABLE [DeliverySchedules] DROP CONSTRAINT [' + @var4 + '];');
    ALTER TABLE [DeliverySchedules] ALTER COLUMN [GoodsDescription] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260423201622_AddStaffEntities'
)
BEGIN
    DECLARE @var5 sysname;
    SELECT @var5 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[DeliverySchedules]') AND [c].[name] = N'DeliveryAddress');
    IF @var5 IS NOT NULL EXEC(N'ALTER TABLE [DeliverySchedules] DROP CONSTRAINT [' + @var5 + '];');
    ALTER TABLE [DeliverySchedules] ALTER COLUMN [DeliveryAddress] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260423201622_AddStaffEntities'
)
BEGIN
    ALTER TABLE [DeliverySchedules] ADD [DriverId] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260423201622_AddStaffEntities'
)
BEGIN
    DECLARE @var6 sysname;
    SELECT @var6 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Contracts]') AND [c].[name] = N'TermsAndConditions');
    IF @var6 IS NOT NULL EXEC(N'ALTER TABLE [Contracts] DROP CONSTRAINT [' + @var6 + '];');
    ALTER TABLE [Contracts] ALTER COLUMN [TermsAndConditions] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260423201622_AddStaffEntities'
)
BEGIN
    DECLARE @var7 sysname;
    SELECT @var7 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Contracts]') AND [c].[name] = N'SpecialConditions');
    IF @var7 IS NOT NULL EXEC(N'ALTER TABLE [Contracts] DROP CONSTRAINT [' + @var7 + '];');
    ALTER TABLE [Contracts] ALTER COLUMN [SpecialConditions] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260423201622_AddStaffEntities'
)
BEGIN
    DECLARE @var8 sysname;
    SELECT @var8 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Contracts]') AND [c].[name] = N'ContractNumber');
    IF @var8 IS NOT NULL EXEC(N'ALTER TABLE [Contracts] DROP CONSTRAINT [' + @var8 + '];');
    ALTER TABLE [Contracts] ALTER COLUMN [ContractNumber] nvarchar(450) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260423201622_AddStaffEntities'
)
BEGIN
    DECLARE @var9 sysname;
    SELECT @var9 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Clients]') AND [c].[name] = N'UserId');
    IF @var9 IS NOT NULL EXEC(N'ALTER TABLE [Clients] DROP CONSTRAINT [' + @var9 + '];');
    ALTER TABLE [Clients] ALTER COLUMN [UserId] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260423201622_AddStaffEntities'
)
BEGIN
    DECLARE @var10 sysname;
    SELECT @var10 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Clients]') AND [c].[name] = N'PreferredName');
    IF @var10 IS NOT NULL EXEC(N'ALTER TABLE [Clients] DROP CONSTRAINT [' + @var10 + '];');
    ALTER TABLE [Clients] ALTER COLUMN [PreferredName] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260423201622_AddStaffEntities'
)
BEGIN
    DECLARE @var11 sysname;
    SELECT @var11 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Clients]') AND [c].[name] = N'Phone');
    IF @var11 IS NOT NULL EXEC(N'ALTER TABLE [Clients] DROP CONSTRAINT [' + @var11 + '];');
    ALTER TABLE [Clients] ALTER COLUMN [Phone] nvarchar(450) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260423201622_AddStaffEntities'
)
BEGIN
    DECLARE @var12 sysname;
    SELECT @var12 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Clients]') AND [c].[name] = N'IdNumber');
    IF @var12 IS NOT NULL EXEC(N'ALTER TABLE [Clients] DROP CONSTRAINT [' + @var12 + '];');
    ALTER TABLE [Clients] ALTER COLUMN [IdNumber] nvarchar(450) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260423201622_AddStaffEntities'
)
BEGIN
    DECLARE @var13 sysname;
    SELECT @var13 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Clients]') AND [c].[name] = N'FullName');
    IF @var13 IS NOT NULL EXEC(N'ALTER TABLE [Clients] DROP CONSTRAINT [' + @var13 + '];');
    ALTER TABLE [Clients] ALTER COLUMN [FullName] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260423201622_AddStaffEntities'
)
BEGIN
    DECLARE @var14 sysname;
    SELECT @var14 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Clients]') AND [c].[name] = N'Email');
    IF @var14 IS NOT NULL EXEC(N'ALTER TABLE [Clients] DROP CONSTRAINT [' + @var14 + '];');
    ALTER TABLE [Clients] ALTER COLUMN [Email] nvarchar(450) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260423201622_AddStaffEntities'
)
BEGIN
    DECLARE @var15 sysname;
    SELECT @var15 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Clients]') AND [c].[name] = N'Address');
    IF @var15 IS NOT NULL EXEC(N'ALTER TABLE [Clients] DROP CONSTRAINT [' + @var15 + '];');
    ALTER TABLE [Clients] ALTER COLUMN [Address] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260423201622_AddStaffEntities'
)
BEGIN
    CREATE TABLE [Staff] (
        [Id] int NOT NULL IDENTITY,
        [UserId] nvarchar(max) NULL,
        [FullName] nvarchar(max) NULL,
        [EmployeeNumber] nvarchar(max) NULL,
        [Phone] nvarchar(max) NULL,
        [Email] nvarchar(max) NULL,
        [Role] int NOT NULL,
        [Status] int NOT NULL,
        [HireDate] datetime2 NOT NULL,
        [TerminationDate] datetime2 NULL,
        CONSTRAINT [PK_Staff] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260423201622_AddStaffEntities'
)
BEGIN
    CREATE TABLE [WarehouseStaff] (
        [Id] int NOT NULL IDENTITY,
        [UserId] nvarchar(max) NULL,
        [FullName] nvarchar(max) NULL,
        [EmployeeNumber] nvarchar(max) NULL,
        [Phone] nvarchar(max) NULL,
        [Email] nvarchar(max) NULL,
        [Role] int NOT NULL,
        [Status] int NOT NULL,
        [HireDate] datetime2 NOT NULL,
        [TerminationDate] datetime2 NULL,
        [AssignedZone] nvarchar(max) NULL,
        [WarehouseRole] int NOT NULL,
        CONSTRAINT [PK_WarehouseStaff] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260423201622_AddStaffEntities'
)
BEGIN
    CREATE TABLE [GoodsIntakes] (
        [Id] int NOT NULL IDENTITY,
        [DeliveryScheduleId] int NOT NULL,
        [BookingId] int NOT NULL,
        [WarehouseStaffId] int NOT NULL,
        [IntakeDate] datetime2 NOT NULL,
        [StorageLocation] nvarchar(max) NULL,
        [ConditionNotes] nvarchar(max) NULL,
        [Status] int NOT NULL,
        CONSTRAINT [PK_GoodsIntakes] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_GoodsIntakes_Bookings_BookingId] FOREIGN KEY ([BookingId]) REFERENCES [Bookings] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_GoodsIntakes_DeliverySchedules_DeliveryScheduleId] FOREIGN KEY ([DeliveryScheduleId]) REFERENCES [DeliverySchedules] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_GoodsIntakes_WarehouseStaff_WarehouseStaffId] FOREIGN KEY ([WarehouseStaffId]) REFERENCES [WarehouseStaff] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260423201622_AddStaffEntities'
)
BEGIN
    EXEC(N'UPDATE [StorageUnits] SET [OccupancyStatus] = 0
    WHERE [Id] = 1;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260423201622_AddStaffEntities'
)
BEGIN
    EXEC(N'UPDATE [StorageUnits] SET [OccupancyStatus] = 0
    WHERE [Id] = 2;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260423201622_AddStaffEntities'
)
BEGIN
    EXEC(N'UPDATE [StorageUnits] SET [OccupancyStatus] = 0
    WHERE [Id] = 3;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260423201622_AddStaffEntities'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_Invoices_InvoiceNumber] ON [Invoices] ([InvoiceNumber]) WHERE [InvoiceNumber] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260423201622_AddStaffEntities'
)
BEGIN
    CREATE INDEX [IX_DeliverySchedules_DriverId] ON [DeliverySchedules] ([DriverId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260423201622_AddStaffEntities'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_DeliverySchedules_ScheduleNumber] ON [DeliverySchedules] ([ScheduleNumber]) WHERE [ScheduleNumber] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260423201622_AddStaffEntities'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_Contracts_ContractNumber] ON [Contracts] ([ContractNumber]) WHERE [ContractNumber] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260423201622_AddStaffEntities'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_Clients_Email] ON [Clients] ([Email]) WHERE [Email] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260423201622_AddStaffEntities'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_Clients_IdNumber] ON [Clients] ([IdNumber]) WHERE [IdNumber] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260423201622_AddStaffEntities'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_Clients_Phone] ON [Clients] ([Phone]) WHERE [Phone] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260423201622_AddStaffEntities'
)
BEGIN
    CREATE INDEX [IX_GoodsIntakes_BookingId] ON [GoodsIntakes] ([BookingId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260423201622_AddStaffEntities'
)
BEGIN
    CREATE INDEX [IX_GoodsIntakes_DeliveryScheduleId] ON [GoodsIntakes] ([DeliveryScheduleId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260423201622_AddStaffEntities'
)
BEGIN
    CREATE INDEX [IX_GoodsIntakes_WarehouseStaffId] ON [GoodsIntakes] ([WarehouseStaffId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260423201622_AddStaffEntities'
)
BEGIN
    ALTER TABLE [DeliverySchedules] ADD CONSTRAINT [FK_DeliverySchedules_Drivers_DriverId] FOREIGN KEY ([DriverId]) REFERENCES [Drivers] ([Id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260423201622_AddStaffEntities'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260423201622_AddStaffEntities', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260425095857_AddStaffTables'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260425095857_AddStaffTables', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260505212055_AddDescriptionToStorageUnit'
)
BEGIN
    DROP INDEX [IX_StorageUnits_UnitNumber] ON [StorageUnits];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260505212055_AddDescriptionToStorageUnit'
)
BEGIN
    DROP INDEX [IX_Clients_IdNumber] ON [Clients];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260505212055_AddDescriptionToStorageUnit'
)
BEGIN
    DECLARE @var16 sysname;
    SELECT @var16 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[StorageUnits]') AND [c].[name] = N'OccupancyStatus');
    IF @var16 IS NOT NULL EXEC(N'ALTER TABLE [StorageUnits] DROP CONSTRAINT [' + @var16 + '];');
    ALTER TABLE [StorageUnits] DROP COLUMN [OccupancyStatus];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260505212055_AddDescriptionToStorageUnit'
)
BEGIN
    DECLARE @var17 sysname;
    SELECT @var17 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Clients]') AND [c].[name] = N'IdNumber');
    IF @var17 IS NOT NULL EXEC(N'ALTER TABLE [Clients] DROP CONSTRAINT [' + @var17 + '];');
    ALTER TABLE [Clients] DROP COLUMN [IdNumber];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260505212055_AddDescriptionToStorageUnit'
)
BEGIN
    DECLARE @var18 sysname;
    SELECT @var18 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[StorageUnits]') AND [c].[name] = N'UnitNumber');
    IF @var18 IS NOT NULL EXEC(N'ALTER TABLE [StorageUnits] DROP CONSTRAINT [' + @var18 + '];');
    EXEC(N'UPDATE [StorageUnits] SET [UnitNumber] = N'''' WHERE [UnitNumber] IS NULL');
    ALTER TABLE [StorageUnits] ALTER COLUMN [UnitNumber] nvarchar(50) NOT NULL;
    ALTER TABLE [StorageUnits] ADD DEFAULT N'' FOR [UnitNumber];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260505212055_AddDescriptionToStorageUnit'
)
BEGIN
    DECLARE @var19 sysname;
    SELECT @var19 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[StorageUnits]') AND [c].[name] = N'Size');
    IF @var19 IS NOT NULL EXEC(N'ALTER TABLE [StorageUnits] DROP CONSTRAINT [' + @var19 + '];');
    EXEC(N'UPDATE [StorageUnits] SET [Size] = N'''' WHERE [Size] IS NULL');
    ALTER TABLE [StorageUnits] ALTER COLUMN [Size] nvarchar(50) NOT NULL;
    ALTER TABLE [StorageUnits] ADD DEFAULT N'' FOR [Size];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260505212055_AddDescriptionToStorageUnit'
)
BEGIN
    DECLARE @var20 sysname;
    SELECT @var20 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[StorageUnits]') AND [c].[name] = N'Location');
    IF @var20 IS NOT NULL EXEC(N'ALTER TABLE [StorageUnits] DROP CONSTRAINT [' + @var20 + '];');
    ALTER TABLE [StorageUnits] ALTER COLUMN [Location] nvarchar(100) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260505212055_AddDescriptionToStorageUnit'
)
BEGIN
    DECLARE @var21 sysname;
    SELECT @var21 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[StorageUnits]') AND [c].[name] = N'ClimateControl');
    IF @var21 IS NOT NULL EXEC(N'ALTER TABLE [StorageUnits] DROP CONSTRAINT [' + @var21 + '];');
    ALTER TABLE [StorageUnits] ALTER COLUMN [ClimateControl] nvarchar(50) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260505212055_AddDescriptionToStorageUnit'
)
BEGIN
    ALTER TABLE [StorageUnits] ADD [Description] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260505212055_AddDescriptionToStorageUnit'
)
BEGIN
    EXEC(N'UPDATE [StorageUnits] SET [Description] = N''''
    WHERE [Id] = 1;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260505212055_AddDescriptionToStorageUnit'
)
BEGIN
    EXEC(N'UPDATE [StorageUnits] SET [Description] = N''''
    WHERE [Id] = 2;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260505212055_AddDescriptionToStorageUnit'
)
BEGIN
    EXEC(N'UPDATE [StorageUnits] SET [Description] = N''''
    WHERE [Id] = 3;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260505212055_AddDescriptionToStorageUnit'
)
BEGIN
    CREATE UNIQUE INDEX [IX_StorageUnits_UnitNumber] ON [StorageUnits] ([UnitNumber]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260505212055_AddDescriptionToStorageUnit'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260505212055_AddDescriptionToStorageUnit', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260506215359_AddContractExtensions'
)
BEGIN
    CREATE TABLE [ContractExtensions] (
        [Id] int NOT NULL IDENTITY,
        [ContractId] int NOT NULL,
        [CustomerId] nvarchar(max) NULL,
        [RequestedDays] int NOT NULL,
        [RequestedDate] datetime2 NOT NULL,
        [CurrentEndDate] datetime2 NOT NULL,
        [ProposedNewEndDate] datetime2 NOT NULL,
        [Reason] nvarchar(max) NULL,
        [Status] nvarchar(max) NULL,
        [AdminNotes] nvarchar(max) NULL,
        [ApprovedDate] datetime2 NULL,
        [ApprovedBy] nvarchar(max) NULL,
        CONSTRAINT [PK_ContractExtensions] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ContractExtensions_Contracts_ContractId] FOREIGN KEY ([ContractId]) REFERENCES [Contracts] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260506215359_AddContractExtensions'
)
BEGIN
    CREATE INDEX [IX_ContractExtensions_ContractId] ON [ContractExtensions] ([ContractId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260506215359_AddContractExtensions'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260506215359_AddContractExtensions', N'8.0.0');
END;
GO

COMMIT;
GO

