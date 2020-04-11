using System;

namespace SGP_Ephemerides.Location
{
    public class Location
    {
        public string Name { get; set; }
        private double _Latitude;
        public double Latitude 
        {
            get { return _Latitude; }
            set
            {
                _Latitude  = value;
                LatDegrees = Math.Truncate(_Latitude);
                LatHours   = TimeSpan.FromHours(_Latitude / 15.0).Hours;
                LatMinutes = TimeSpan.FromHours(_Latitude).Minutes;
                LatSeconds = TimeSpan.FromHours(_Latitude).Seconds + TimeSpan.FromHours(_Latitude).Milliseconds / 1000.0;
            } 
        }
        public double LatDegrees { get; private set; }
        public double LatHours { get; private set; }
        public double LatMinutes { get; private set; }
        public double LatSeconds { get; private set; }
        public bool North { get; set; }


        private double _Longitude;
        public double Longitude
        {
            get { return _Longitude; }
            set
            {
                _Longitude = value;
                LonDegrees = Math.Truncate(_Longitude);
                LonHours   = TimeSpan.FromHours(_Longitude / 15.0).Hours;
                LonMinutes = TimeSpan.FromHours(_Longitude).Minutes;
                LonSeconds = TimeSpan.FromHours(_Longitude).Seconds + TimeSpan.FromHours(_Longitude).Milliseconds / 1000.0;
            }
        }
        public double LonDegrees { get; private set; }
        public double LonHours { get; private set; }
        public double LonMinutes { get; private set; }
        public double LonSeconds { get; private set; }
        public bool West { get; set; }

        public double Horizon { get; set; }
        public double MinutesAboveHorizon { get { return Duration.TotalMinutes; } set { Duration = TimeSpan.FromMinutes(value); } }
        public TimeSpan Duration { get; set; }
        public DateTime DateTime { get; set; }
        public TimeZone TimeZone { get; set; }
        public bool DayChart { get; set; }
        public bool YearChart { get; set; }
        public bool OptimalChart { get; set; }

        public Location()
        {
            Name = "Penns Park";
            Latitude = 40.282835;
            Longitude = 74.997369;
            North = true;
            West = true;
            Horizon = 30;
            Duration = TimeSpan.FromMinutes(240);
            DateTime = DateTime.Now;
            TimeZone = TimeZone.CurrentTimeZone;
        }
    }
}
