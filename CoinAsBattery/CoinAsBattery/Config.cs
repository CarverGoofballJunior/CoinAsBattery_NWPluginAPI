using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace CoinAsBattery
{
    public class Config
    {
        [Description("Is plugin enabled?")]
        public bool IsEnabled { get; set; } = true;

        [Description("Should Debug be enabled?")]
        public bool Debug { get; set; } = false;

        [Description("Radio battery capacity. Set between 0-100.")]
        public byte RadioBatteryCapacity { get; set; } = 30;

        [Description("MicroHID battery capacity. Set between 0-1.")]
        public float MicroBatteryCapacity { get; set; } = 0.2f;

        [Description("Should MicroHID explode, if charged when full? You can set explosion after picking coin (\"pick\") or when firing overcharged micro (\"fire\"). Type anything else to disable.")]
        public string ShouldExplode { get; set; } = "nope";
    }
}
