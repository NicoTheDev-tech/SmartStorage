using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartStorage.Core.Entities
{
    public class Booking
    {
        public int Id { get; set; }
        public string? BookingNumber { get; set; }
        public int ClientId { get; set; }
        public Client? Client { get; set; }
        public int StorageUnitId { get; set; }
        public StorageUnit? StorageUnit { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal TotalAmount { get; set; }
        public BookingStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public ICollection<Payment>? Payments { get; set; }
        public ICollection<Cartage>? Cartages { get; set; }
    }

    public enum BookingStatus
    {
        Pending,
        Confirmed,
        Active,
        Completed,
        Cancelled,
        Overdue
    }

}
