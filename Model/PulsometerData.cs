using System;

namespace Monitor.Model
{
    public class PulsometerData
    {
        public PulsometerData(
            DateTime timestamp,
            double? spo2,
            double? pulse)
        {
            this.Timestamp = timestamp;
            this.SPO2 = spo2;
            this.Pulse = pulse;
        }

        public DateTime Timestamp { get; }

        public double? SPO2 { get; set; }
        public double? Pulse { get; set; }

        public override string ToString()
        {
            return
                $"{nameof(Timestamp)}: {Timestamp}, {nameof(SPO2)}: {SPO2}, {nameof(Pulse)}: {Pulse}";
        }
    }
}