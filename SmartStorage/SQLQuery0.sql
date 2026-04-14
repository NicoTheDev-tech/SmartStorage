-- Add Location column
ALTER TABLE StorageUnits ADD Location NVARCHAR(200) NULL;

-- Add ClimateControl column
ALTER TABLE StorageUnits ADD ClimateControl NVARCHAR(50) NULL;

-- Now update all records with proper values
-- Original units
UPDATE StorageUnits SET Location = 'Building A', ClimateControl = 'None' WHERE Name = 'A101';
UPDATE StorageUnits SET Location = 'Building A', ClimateControl = 'Basic' WHERE Name = 'A102';
UPDATE StorageUnits SET Location = 'Building B', ClimateControl = 'Premium' WHERE Name = 'B201';

-- Westville units
UPDATE StorageUnits SET Location = 'Westville', ClimateControl = 'None' WHERE Name = 'W101';
UPDATE StorageUnits SET Location = 'Westville', ClimateControl = 'Basic' WHERE Name = 'W102';
UPDATE StorageUnits SET Location = 'Westville', ClimateControl = 'Premium' WHERE Name = 'W103';

-- Pinetown units
UPDATE StorageUnits SET Location = 'Pinetown', ClimateControl = 'None' WHERE Name = 'P101';
UPDATE StorageUnits SET Location = 'Pinetown', ClimateControl = 'Basic' WHERE Name = 'P102';
UPDATE StorageUnits SET Location = 'Pinetown', ClimateControl = 'Premium' WHERE Name = 'P103';

-- Umhlanga units
UPDATE StorageUnits SET Location = 'Umhlanga', ClimateControl = 'None' WHERE Name = 'U101';
UPDATE StorageUnits SET Location = 'Umhlanga', ClimateControl = 'Basic' WHERE Name = 'U102';
UPDATE StorageUnits SET Location = 'Umhlanga', ClimateControl = 'Premium' WHERE Name = 'U103';

-- Durban Central units
UPDATE StorageUnits SET Location = 'Durban Central', ClimateControl = 'None' WHERE Name = 'D101';
UPDATE StorageUnits SET Location = 'Durban Central', ClimateControl = 'Basic' WHERE Name = 'D102';
UPDATE StorageUnits SET Location = 'Durban Central', ClimateControl = 'Premium' WHERE Name = 'D103';

-- Large units
UPDATE StorageUnits SET Location = 'Westville', ClimateControl = 'None' WHERE Name = 'W201';
UPDATE StorageUnits SET Location = 'Pinetown', ClimateControl = 'None' WHERE Name = 'P201';

-- Verify everything
SELECT Id, Name as UnitNumber, Size, Price as MonthlyRate, Location, ClimateControl, 
       CASE WHEN IsAvailable = 1 THEN 'Active' ELSE 'Inactive' END as Status
FROM StorageUnits
ORDER BY Name;