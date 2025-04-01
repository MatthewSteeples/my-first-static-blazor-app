using System;

namespace BlazorApp.Shared
{
    public class StockAcquisition
    {
        public DateTime DateAcquired { get; set; }
        public decimal Quantity { get; set; }
        public string Note { get; set; }
    }
}
