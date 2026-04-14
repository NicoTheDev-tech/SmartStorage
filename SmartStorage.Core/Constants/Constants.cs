namespace SmartStorage.Core.Constants
{
    public static class Constants
    {
        // ===== FEE CONSTANTS =====
        public const decimal ADMIN_FEE = 25m;
        public const decimal SECURITY_FEE = 50m;
        public const decimal LATE_FEE = 10m;
        public const decimal HOLDOVER_FEE_PER_DAY = 20m;

        // ===== PAYMENT CONSTANTS =====
        public const int DEFAULT_INVOICE_DUE_DAYS = 14;
        public const int LATE_PAYMENT_GRACE_DAYS = 5;
        public const int ABANDONMENT_DAYS = 60;
        public const int EARLY_TERMINATION_NOTICE_DAYS = 30;
        public const int EXTENSION_NOTICE_DAYS = 30;

        // ===== BOOKING STATUS CONSTANTS =====
        public const string STATUS_PENDING = "Pending";
        public const string STATUS_CONFIRMED = "Confirmed";
        public const string STATUS_ACTIVE = "Active";
        public const string STATUS_COMPLETED = "Completed";
        public const string STATUS_CANCELLED = "Cancelled";

        // ===== CONTRACT STATUS CONSTANTS =====
        public const string CONTRACT_PENDING = "PendingAcceptance";
        public const string CONTRACT_ACCEPTED = "Accepted";
        public const string CONTRACT_ACTIVE = "Active";
        public const string CONTRACT_COMPLETED = "Completed";
        public const string CONTRACT_TERMINATED = "Terminated";

        // ===== INVOICE STATUS CONSTANTS =====
        public const string INVOICE_PENDING = "Pending";
        public const string INVOICE_PAID = "Paid";
        public const string INVOICE_OVERDUE = "Overdue";
        public const string INVOICE_CANCELLED = "Cancelled";
        public const string INVOICE_PARTIALLY_PAID = "PartiallyPaid";

        // ===== DELIVERY SCHEDULE CONSTANTS =====
        public const string DELIVERY_PENDING = "Pending";
        public const string DELIVERY_IN_PROGRESS = "InProgress";
        public const string DELIVERY_COMPLETED = "Completed";
        public const string DELIVERY_CANCELLED = "Cancelled";

        // ===== STORAGE UNIT CONSTANTS =====
        public const decimal DEFAULT_MONTHLY_RATE = 500m;
        public const string DEFAULT_CLIMATE_CONTROL = "None";
        public const bool DEFAULT_UNIT_ACTIVE = true;

        // ===== PAYMENT METHOD CONSTANTS =====
        public const string PAYMENT_METHOD_CARD = "Card";
        public const string PAYMENT_METHOD_CASH = "Cash";
        public const string PAYMENT_METHOD_EFT = "EFT";
        public const string PAYMENT_METHOD_MOBILE = "MobilePayment";

        // ===== PAYMENT STATUS CONSTANTS =====
        public const string PAYMENT_PENDING = "Pending";
        public const string PAYMENT_COMPLETED = "Completed";
        public const string PAYMENT_FAILED = "Failed";
        public const string PAYMENT_REFUNDED = "Refunded";

        // ===== DISCOUNT CONSTANTS =====
        public const int DISCOUNT_3_MONTHS = 10;   // 10% for 3+ months
        public const int DISCOUNT_6_MONTHS = 12;   // 12% for 6+ months
        public const int DISCOUNT_12_MONTHS = 15;  // 15% for 12+ months

        // ===== HELPERS / CALCULATION METHODS =====

        /// <summary>
        /// Calculates the total amount including admin and security fees
        /// </summary>
        /// <param name="baseAmount">The base storage amount</param>
        /// <returns>Total amount including fees</returns>
        public static decimal CalculateTotalWithFees(decimal baseAmount)
        {
            return baseAmount + ADMIN_FEE + SECURITY_FEE;
        }

        /// <summary>
        /// Calculates discounted rate based on contract duration
        /// </summary>
        /// <param name="originalRate">Original monthly rate</param>
        /// <param name="months">Number of months</param>
        /// <returns>Discounted monthly rate</returns>
        public static decimal CalculateDiscountedRate(decimal originalRate, int months)
        {
            decimal discountPercent = 0;
            if (months >= 12)
                discountPercent = DISCOUNT_12_MONTHS;
            else if (months >= 6)
                discountPercent = DISCOUNT_6_MONTHS;
            else if (months >= 3)
                discountPercent = DISCOUNT_3_MONTHS;

            return originalRate * (1 - discountPercent / 100);
        }

        /// <summary>
        /// Calculates the number of months between two dates
        /// </summary>
        /// <param name="startDate">Start date</param>
        /// <param name="endDate">End date</param>
        /// <returns>Number of months</returns>
        public static int CalculateMonths(DateTime startDate, DateTime endDate)
        {
            int months = ((endDate.Year - startDate.Year) * 12) + (endDate.Month - startDate.Month);
            return months <= 0 ? 1 : months;
        }

        /// <summary>
        /// Calculates the base storage amount for a given period
        /// </summary>
        /// <param name="monthlyRate">Monthly storage rate</param>
        /// <param name="startDate">Start date</param>
        /// <param name="endDate">End date</param>
        /// <returns>Base storage amount</returns>
        public static decimal CalculateBaseAmount(decimal monthlyRate, DateTime startDate, DateTime endDate)
        {
            int months = CalculateMonths(startDate, endDate);
            return monthlyRate * months;
        }

        /// <summary>
        /// Calculates the total contract amount including all fees
        /// </summary>
        /// <param name="monthlyRate">Monthly storage rate</param>
        /// <param name="startDate">Start date</param>
        /// <param name="endDate">End date</param>
        /// <returns>Total amount including fees</returns>
        public static decimal CalculateTotalContractAmount(decimal monthlyRate, DateTime startDate, DateTime endDate)
        {
            decimal baseAmount = CalculateBaseAmount(monthlyRate, startDate, endDate);
            return CalculateTotalWithFees(baseAmount);
        }

        /// <summary>
        /// Gets a formatted description of all fees
        /// </summary>
        /// <returns>Formatted fee description string</returns>
        public static string GetFeesDescription()
        {
            return $"Admin Fee: R{ADMIN_FEE:N2}, Security Fee: R{SECURITY_FEE:N2}";
        }

        /// <summary>
        /// Gets a detailed breakdown of all fees
        /// </summary>
        /// <returns>Detailed fee breakdown string</returns>
        public static string GetFeesBreakdown()
        {
            return $"Base Storage: {{0}}\nAdmin Fee: R{ADMIN_FEE:N2}\nSecurity Fee: R{SECURITY_FEE:N2}\nTotal: {{1}}";
        }

        // ===== VALIDATION METHODS =====

        /// <summary>
        /// Validates if a date range is valid
        /// </summary>
        public static bool IsValidDateRange(DateTime startDate, DateTime endDate)
        {
            return startDate < endDate && startDate >= DateTime.Today;
        }

        /// <summary>
        /// Validates if a monthly rate is valid
        /// </summary>
        public static bool IsValidMonthlyRate(decimal monthlyRate)
        {
            return monthlyRate > 0;
        }
    }
}