using System;

namespace SmartStorage.Core.Entities
{
    public class Staff
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string EmployeeNumber { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public StaffRole Role { get; set; }
        public StaffStatus Status { get; set; }
        public DateTime HireDate { get; set; }
        public DateTime? TerminationDate { get; set; }
    }

    public enum StaffRole
    {
        Driver = 0,
        WarehouseStaff = 1,
        InventoryStaff = 2,
        OperationsManager = 3,
        CustomerSupport = 4
    }

    public enum StaffStatus
    {
        Active = 0,
        OnLeave = 1,
        Terminated = 2
    }
}