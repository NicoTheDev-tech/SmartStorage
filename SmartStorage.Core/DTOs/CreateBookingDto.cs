using System;

namespace SmartStorage.Core.DTOs
{
    public class CreateBookingDto
    {
        public int StorageUnitId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public ClientInfoDto? ClientInfo { get; set; }
    }

    public class ClientInfoDto
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string IdNumber { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
    }
}