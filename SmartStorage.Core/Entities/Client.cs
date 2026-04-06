using System;
using System.Collections.Generic;

namespace SmartStorage.Core.Entities
{
    public class Client
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string PreferredName { get; set; } = string.Empty;  // Add this
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string IdNumber { get; set; } = string.Empty;
        public DateTime RegistrationDate { get; set; }
        public ICollection<Booking>? Bookings { get; set; }
        public ICollection<Payment>? Payments { get; set; }
    }
}