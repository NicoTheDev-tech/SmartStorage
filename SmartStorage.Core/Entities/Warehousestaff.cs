using System.Collections.Generic;

namespace SmartStorage.Core.Entities
{
    public class WarehouseStaff
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
        public string? AssignedZone { get; set; }
        public WarehouseRole WarehouseRole { get; set; }
        public ICollection<GoodsIntake>? ProcessedIntakes { get; set; }
    }

    public enum WarehouseRole
    {
        Receiver = 0,
        Storer = 1,
        Picker = 2,
        Supervisor = 3
    }
}