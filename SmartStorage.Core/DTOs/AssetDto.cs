using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartStorage.Core.DTOs
{
    public class CreateAssetDto
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? Category { get; set; }
        public decimal StartingPrice { get; set; }
        public DateTime AuctionEndDate { get; set; }
    }

    public class PlaceBidDto
    {
        public int AssetId { get; set; }
        public int BuyerId { get; set; }
        public decimal Amount { get; set; }
    }
}
