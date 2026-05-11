using System;

namespace SmartStorage.Core.Entities
{
    public class AssetForSale
    {
        public int Id { get; set; }
        public int BreachCaseId { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public string Condition { get; set; } = string.Empty; // Excellent, Good, Fair, Poor
        public decimal EstimatedValue { get; set; }
        public decimal ReservePrice { get; set; }
        public string Status { get; set; } = "Pending"; // Pending, Catalogued, Listed, Sold
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}