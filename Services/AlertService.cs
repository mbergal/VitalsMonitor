using System;
using System.Collections.Generic;
using System.Linq;
using Monitor.Model;

namespace Monitor.Services
{
    public class Alert : IEquatable<Alert>
    {
        public Alert(PulsometerData[] badStuff)
        {
            this.BadStuff = badStuff;
        }

        public PulsometerData[] BadStuff { get; }

        public bool Equals(Alert other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(BadStuff, other.BadStuff);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Alert) obj);
        }

        public override int GetHashCode()
        {
            return (BadStuff != null ? BadStuff.GetHashCode() : 0);
        }

        public static bool operator ==(Alert left, Alert right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Alert left, Alert right)
        {
            return !Equals(left, right);
        }
    }

    public class AlertService
    {
        private List<PulsometerData> dataPoints = new List<PulsometerData>();

        public Alert SetData(PulsometerData pusomRecordingResult)
        {
            dataPoints.Add(pusomRecordingResult);
            var badStuff = dataPoints
                .Where(x => B(pusomRecordingResult, x))
                .Where(IsBad)
                .ToArray();

            return badStuff.Count() >= 3 ? new Alert(badStuff.ToArray()) : null;
        }

        private static bool B(PulsometerData pusomRecordingResult, PulsometerData x)
        {
            return (pusomRecordingResult.Timestamp - x.Timestamp) < TimeSpan.FromMinutes(1);
        }

        private bool IsBad(PulsometerData pulsometerData)
        {
            return pulsometerData.SPO2 < 90 || pulsometerData.Pulse > 140;
        }
    }
}