namespace SmartStorage.Core.Constants
{
    public static class FeeConstants
    {
        public const decimal ADMIN_FEE = 25m;
        public const decimal SECURITY_FEE = 50m;
        public const decimal TOTAL_FEES = ADMIN_FEE + SECURITY_FEE;
        public const decimal LATE_FEE = 10m;
        public const decimal HOLDOVER_FEE_PER_DAY = 20m;

        public static decimal CalculateTotalWithFees(decimal baseAmount)
        {
            return baseAmount + ADMIN_FEE + SECURITY_FEE;
        }

        public static string GetFeesDescription()
        {
            return $"Admin Fee: R{ADMIN_FEE:N2}, Security Fee: R{SECURITY_FEE:N2}";
        }
    }
}