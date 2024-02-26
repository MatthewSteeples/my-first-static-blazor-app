using System;
using System.Collections.Generic;
using System.Text;

namespace BlazorApp.Shared
{
    public class Occurrence
    {
        public DateTime ActualTimestamp { get; set; }

        public DateTime SafetyTimestamp { get; set; }
    }
}
