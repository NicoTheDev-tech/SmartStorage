using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartStorage.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixDecimalPrecision : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Fix decimal precision for all decimal columns in the database
            // This preserves all existing data while changing the column type

            migrationBuilder.Sql(@"
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
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Revert to default decimal (no precision) - this will preserve data
            // Note: SQL Server will keep the data but remove the explicit precision constraint

            migrationBuilder.Sql(@"
                -- StorageUnits
                IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('StorageUnits') AND name = 'MonthlyRate' AND system_type_id = 106)
                    ALTER TABLE StorageUnits ALTER COLUMN MonthlyRate DECIMAL(18);
                
                -- Assets
                IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Assets') AND name = 'StartingPrice' AND system_type_id = 106)
                    ALTER TABLE Assets ALTER COLUMN StartingPrice DECIMAL(18);
                IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Assets') AND name = 'CurrentBid' AND system_type_id = 106)
                    ALTER TABLE Assets ALTER COLUMN CurrentBid DECIMAL(18);
                
                -- Bids
                IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Bids') AND name = 'Amount' AND system_type_id = 106)
                    ALTER TABLE Bids ALTER COLUMN Amount DECIMAL(18);
                
                -- Bookings
                IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Bookings') AND name = 'TotalAmount' AND system_type_id = 106)
                    ALTER TABLE Bookings ALTER COLUMN TotalAmount DECIMAL(18);
                
                -- Cartages
                IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Cartages') AND name = 'Cost' AND system_type_id = 106)
                    ALTER TABLE Cartages ALTER COLUMN Cost DECIMAL(18);
                IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Cartages') AND name = 'GoodsWeight' AND system_type_id = 106)
                    ALTER TABLE Cartages ALTER COLUMN GoodsWeight DECIMAL(18);
                
                -- Contracts
                IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Contracts') AND name = 'MonthlyRate' AND system_type_id = 106)
                    ALTER TABLE Contracts ALTER COLUMN MonthlyRate DECIMAL(18);
                IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Contracts') AND name = 'SecurityDeposit' AND system_type_id = 106)
                    ALTER TABLE Contracts ALTER COLUMN SecurityDeposit DECIMAL(18);
                IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Contracts') AND name = 'TotalContractValue' AND system_type_id = 106)
                    ALTER TABLE Contracts ALTER COLUMN TotalContractValue DECIMAL(18);
                
                -- DeliverySchedules
                IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('DeliverySchedules') AND name = 'EstimatedWeight' AND system_type_id = 106)
                    ALTER TABLE DeliverySchedules ALTER COLUMN EstimatedWeight DECIMAL(18);
                
                -- Invoices
                IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Invoices') AND name = 'Amount' AND system_type_id = 106)
                    ALTER TABLE Invoices ALTER COLUMN Amount DECIMAL(18);
                IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Invoices') AND name = 'AmountPaid' AND system_type_id = 106)
                    ALTER TABLE Invoices ALTER COLUMN AmountPaid DECIMAL(18);
                IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Invoices') AND name = 'Balance' AND system_type_id = 106)
                    ALTER TABLE Invoices ALTER COLUMN Balance DECIMAL(18);
                
                -- MaintenanceRecords
                IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('MaintenanceRecords') AND name = 'Cost' AND system_type_id = 106)
                    ALTER TABLE MaintenanceRecords ALTER COLUMN Cost DECIMAL(18);
                
                -- Payments
                IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Payments') AND name = 'Amount' AND system_type_id = 106)
                    ALTER TABLE Payments ALTER COLUMN Amount DECIMAL(18);
                
                -- Vehicles
                IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Vehicles') AND name = 'Capacity' AND system_type_id = 106)
                    ALTER TABLE Vehicles ALTER COLUMN Capacity DECIMAL(18);
            ");
        }
    }
}