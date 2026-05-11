using System;

namespace SmartStorage.Core.Entities
{
    public class GoodsIntake
    {
        public int Id { get; set; }
        public int DeliveryScheduleId { get; set; }
        public int BookingId { get; set; }
        public int WarehouseStaffId { get; set; }
        public DateTime IntakeDate { get; set; }
        public string? StorageLocation { get; set; }
        public string? ConditionNotes { get; set; }
        public IntakeStatus Status { get; set; }

        // Navigation properties
        public DeliverySchedule? DeliverySchedule { get; set; }
        public Booking? Booking { get; set; }
        public WarehouseStaff? WarehouseStaff { get; set; }
    }

    public enum IntakeStatus
    {
        Pending = 0,
        Received = 1,
        Stored = 2,
        Damaged = 3,
        Returned = 4
    }
}