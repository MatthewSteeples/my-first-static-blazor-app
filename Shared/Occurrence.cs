using System;

namespace BlazorApp.Shared
{
    public class Occurrence
    {
        public DateTime ActualTimestamp { get; set; }

        public DateTime SafetyTimestamp { get; set; }

        public decimal? StockUsed { get; set; }
    }
}
