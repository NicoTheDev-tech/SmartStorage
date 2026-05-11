using System;

namespace SmartStorage.Core.Entities
{
    public class Auction
    {
        public int Id { get; set; }
        public string AuctionNumber { get; set; } = string.Empty;
        public int BreachCaseId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal StartingBid { get; set; }
        public string Status { get; set; } = "Draft"; // Draft, Published, Closed
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public int? WinningBidId { get; set; }
        public string? WinnerId { get; set; }
        public virtual ICollection<AuctionItem> AuctionItems { get; set; } = new List<AuctionItem>();
    }

    public class AuctionItem
    {
        public int Id { get; set; }
        public int AuctionId { get; set; }
        public int AssetId { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public decimal ReservePrice { get; set; }
        public string Status { get; set; } = "Active";
    }

    public class AuctionBid
    {
        public int Id { get; set; }
        public int AuctionId { get; set; }
        public int? AuctionItemId { get; set; }
        public string BidderId { get; set; } = string.Empty;
        public string BidderName { get; set; } = string.Empty;
        public decimal BidAmount { get; set; }
        public DateTime BidTime { get; set; } = DateTime.Now;
        public string Status { get; set; } = "Active";
    }
}