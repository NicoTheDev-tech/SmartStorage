using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartStorage.Core.Entities
{
    public class Bid
    {
        public int Id { get; set; }
        public int AssetId { get; set; }
        public Asset? Asset { get; set; }
        public int BuyerId { get; set; } // Client Id
        public Client? Buyer { get; set; }
        public decimal Amount { get; set; }
        public DateTime BidTime { get; set; }
        public bool IsWinning { get; set; }
    }
}
