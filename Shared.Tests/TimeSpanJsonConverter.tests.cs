using BlazorApp.Shared;
using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BlazorApp.Shared.Tests
{
    [TestClass]
    public class TimeSpanJsonConverterTests
    {
        [TestMethod]
        [DataRow("1.00:00:00:", 24 * 60 * 60 * 1000.0)]
        [DataRow("0.04:00:00:", 4 * 60 * 60 * 1000.0)]
        [DataRow("0.00:19:59:999", (19 * 60 + 59) * 1000.0 + 999)]
        [DataRow("1.00:00:00", 24 * 60 * 60 * 1000.0)]
        [DataRow("04:00:00", 4 * 60 * 60 * 1000.0)]
        public void Deserialize_ShouldAcceptLegacyAndStandardFormats(string frequency, double expectedMs)
        {
            var json = $"{{\"Qty\":1,\"Frequency\":\"{frequency}\"}}";

            var target = JsonSerializer.Deserialize(json, SerializationContext.Default.Target);

            Assert.IsNotNull(target);
            Assert.AreEqual(TimeSpan.FromMilliseconds(expectedMs), target.Frequency);
        }

        [TestMethod]
        public void RoundTrip_ShouldPreserveValue()
        {
            var original = new Target { Qty = 2, Frequency = TimeSpan.FromHours(4.5) };

            var json = JsonSerializer.Serialize(original, SerializationContext.Default.Target);
            var result = JsonSerializer.Deserialize(json, SerializationContext.Default.Target);

            StringAssert.Contains(json, "\"04:30:00\"");
            Assert.AreEqual(original.Frequency, result!.Frequency);
        }
    }
}
