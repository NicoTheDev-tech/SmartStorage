using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SmartStorage.Core.Entities
{
    public class Asset
    {
        public int Id { get; set; }
        public int? ClientId { get; set; } // null if company asset
        public Client? Client { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? Category { get; set; }
        public decimal StartingPrice { get; set; }
        public decimal? CurrentBid { get; set; }
        public DateTime AuctionStartDate { get; set; }
        public DateTime AuctionEndDate { get; set; }
        public AssetStatus Status { get; set; }
        public ICollection<Bid>? Bids { get; set; }
    }

    public enum AssetStatus
    {
        Available,
        UnderAuction,
        Sold,
        Withdrawn
    }
}
