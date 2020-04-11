using CoordinateSharp;
using System;

namespace SGP_Ephemerides.Support
{

    public class Astrometry
    {
        public static Celestial mLocal { get; private set; }
        public static DateTime AstronomicalDawn { get; private set; }
        public static DateTime AstronomicalDusk { get; private set; }
        public static double SunAltitude { get; private set; }
        public static DateTime LunarRise { get; private set; }
        public static DateTime LunarSet { get; private set; }
        public static double LunarAltitude { get; private set; }
        public static string LunarPhase { get; private set; }
        public static double LunarIlluminationFraction { get; private set; }

        private static EagerLoad mEgagerLoad = EagerLoad.Create(EagerLoadType.Celestial);
        private static DateTime? mDuskToday;
        private static DateTime? mDawnToday;
        private static DateTime? mDuskYesterday;
        private static DateTime? mDawnTomorrow;

        public Astrometry()
        {
        }

        // ################################################################################################################################
        // ################################################################################################################################
        public static void Location(Location.Location localLocation)
        {
            double LongitudeSign;
            TimeSpan utcOffset;

            LongitudeSign = localLocation.West ? -1.0 : 1.0;
            utcOffset = TimeZoneInfo.Local.GetUtcOffset(localLocation.DateTime);
            mLocal = Celestial.CalculateCelestialTimes(localLocation.Latitude, LongitudeSign * localLocation.Longitude, localLocation.DateTime, mEgagerLoad, utcOffset.Hours);

            SunAltitude = mLocal.SunAltitude;
            LunarAltitude = mLocal.MoonAltitude;
            LunarRise = mLocal.MoonRise.HasValue ? mLocal.MoonRise.Value : DateTime.MinValue;
            LunarSet = mLocal.MoonSet.HasValue ? mLocal.MoonSet.Value : DateTime.MinValue;
            LunarPhase = mLocal.MoonIllum.PhaseName;
            LunarIlluminationFraction = mLocal.MoonIllum.Fraction;

            mDawnToday = mLocal.AdditionalSolarTimes.AstronomicalDawn;
            mDuskToday = mLocal.AdditionalSolarTimes.AstronomicalDusk;

            if (localLocation.DateTime > mDawnToday)
            {
                mLocal = Celestial.CalculateCelestialTimes(localLocation.Latitude, LongitudeSign * localLocation.Longitude, localLocation.DateTime.AddDays(1), mEgagerLoad, utcOffset.Hours);
                mDawnTomorrow = mLocal.AdditionalSolarTimes.AstronomicalDawn;

                AstronomicalDawn = (DateTime)mDawnTomorrow;
                AstronomicalDusk = (DateTime)mDuskToday;
                return;
            }

            if (localLocation.DateTime < mDawnToday)
            {
                mLocal = Celestial.CalculateCelestialTimes(localLocation.Latitude, LongitudeSign * localLocation.Longitude, localLocation.DateTime.AddDays(-1), mEgagerLoad, utcOffset.Hours);
                mDuskYesterday = mLocal.AdditionalSolarTimes.AstronomicalDusk;

                AstronomicalDawn = (DateTime)mDawnToday;
                AstronomicalDusk = (DateTime)mDuskYesterday;
                return;
            }
        }

        // ####################################################################################################################################
        // ####################################################################################################################################

        private static double JulianDay(double yy, double mm, double dd)
        {
            double tulos;

            tulos = 367.0 * yy - Math.Floor(7.0 * (yy + Math.Floor((mm + 9.0) / 12.0)) / 4.0)
                    - Math.Floor(3.0 * (Math.Floor((yy + (mm - 9.0) / 7.0) / 100.0) + 1.0) / 4.0)
                    + Math.Floor(275.0 * mm / 9.0) + dd + 1721028.5;

            return tulos;
        }

        // calculates Greenwich sidereal time in hours
        private static double Greenwich(DateTime dateTime, double JD)
        {
            double T, GW, GWT0, GMST, time;

            T = (JD - 2451545.0) / 36525.0;

            time = (dateTime.Hour + dateTime.Minute / 60.0 + dateTime.Second / 3600.0);

            // GWT0 is Greenwich Sidereal Time in hours
            GWT0 = (24110.54841 + 8640184.812866 * T + 0.093104 * T * T - 0.0000062 * T * T * T) / 3600.0;
            GW = GWT0;

            if (GWT0 >= 24.0)
                GW = GWT0 - 24.0 * Math.Floor(GWT0 / 24.0);

            if (GWT0 <= -24.0)
                GW = 24.0 - (Math.Abs(GWT0) - 24.0 * Math.Floor(Math.Abs(GWT0) / 24.0));

            GMST = GW + 1.00273790935 * time;

            if (GMST >= 24.0)
                GMST = GMST - 24.0;

            if (GMST < 0)
                GMST = GMST + 24.0;

            return GMST;
        }


        public static Tuple<double, double, double> GetAltitudeAzimuth(Target.Target target, Location.Location location)
        {
            double decRadian;
            double raDegrees;
            double raRadian;
            double targetSiderealHourAngle;
            double x, minX, secX;
            DateTime gmt;
            double latRadian;
            double longDegree;
            double julianDay;
            double gmst;
            double lst;
            double hourAngle;
            double GHA;
            double sinAlt, maxsinAlt, maxaltitude, asx, maxx, maxasx, maxminx, maxsekx;
            double cosAz;
            double altitude;
            double altitudeOutput;
            double azimuth;
            double azimuthOutput;
            double maximumAltitude;


            decRadian = target.Declination * Math.PI / 180.0;
            decRadian = (target.North) ? decRadian : -decRadian;

            raDegrees = target.RightAscension;
            raRadian = target.RightAscension * Math.PI / 180.0;

            // SHA Sidereal Hour Angle of star in Nautical Almanacs
            targetSiderealHourAngle = 360.0 - raDegrees * 15.0;

            x = Math.Floor(targetSiderealHourAngle);
            minX = Math.Floor(60.0 * (targetSiderealHourAngle - x));
            secX = (targetSiderealHourAngle - x - minX / 60.0) * 3600.0;

            gmt = location.DateTime.ToUniversalTime();
            latRadian = location.Latitude * Math.PI / 180.0;

            latRadian = (location.North) ? latRadian : -latRadian;
            longDegree = (location.West) ? -location.Longitude : location.Longitude;

            // Julian Day at UT0   
            //julianDay = JulianDay(location.DateTime.Year, location.DateTime.Month, location.DateTime.Day);
            julianDay = gmt.ToOADate() + 2415018.0;

            // GMST Greenwich Mean Sidereal Time
            //gmst = gmt.Year + 0.0657098244 * gmt.Day + (1.00273791 * gmt.Hour) + (gmt.Minute / 60.0) + (gmt.Second / 3600.0);
            gmst = Greenwich(gmt, julianDay);

            x = Math.Floor(gmst);
            minX = Math.Floor(60.0 * (gmst - x));
            secX = (gmst - x - minX / 60.0) * 3600.0;

            // LST is Local Sidereal Time in hours, differs from GMST by longitude
            lst = Greenwich(gmt, julianDay) + longDegree / 15.0;

            if (lst < 0)
                lst = lst + 24;

            if (lst >= 24)
                lst = lst - 24;

            x = Math.Floor(lst);
            minX = Math.Floor(60.0 * (lst - x));
            secX = (lst - x - minX / 60.0) * 3600.0;

            // hourAngle is local hourangle in hours
            hourAngle = lst - raDegrees;

            if (hourAngle < 0)
                hourAngle = 24 + hourAngle;

            x = Math.Floor(hourAngle);
            minX = Math.Floor(60.0 * (hourAngle - x));
            secX = (hourAngle - x - minX / 60.0) * 3600.0;

            // GHA is Greenwich Hour Angle
            GHA = hourAngle - longDegree / 15.0;

            if (GHA >= 24.0)
                GHA = GHA - 24.0;

            if (GHA < 0.0)
                GHA = GHA + 24.0;

            x = Math.Floor(GHA);
            minX = Math.Floor(60.0 * (GHA - x));
            secX = (GHA - x - minX / 60.0) * 3600.0;

            hourAngle = hourAngle * Math.PI / 12;

            // Altitude
            raDegrees = raDegrees * Math.PI / 12.0;
            sinAlt = Math.Cos(hourAngle) * Math.Cos(decRadian) * Math.Cos(latRadian) + Math.Sin(decRadian) * Math.Sin(latRadian);
            maxsinAlt = Math.Cos(decRadian) * Math.Cos(latRadian) + Math.Sin(decRadian) * Math.Sin(latRadian);
            altitude = Math.Asin(sinAlt);
            maxaltitude = Math.Asin(maxsinAlt);

            x = Math.Abs(altitude * 180.0 / Math.PI);
            asx = Math.Floor(x);
            minX = Math.Floor(60 * (x - asx));
            secX = (x - asx - minX / 60.0) * 3600.0;

            if (altitude < 0)
            {
                asx = -asx;
            }

            altitudeOutput = asx + minX / 60.0 + secX / 3600.0;


            maxx = Math.Abs(maxaltitude * 180.0 / Math.PI);
            maxasx = Math.Floor(maxx);
            maxminx = Math.Floor(60.0 * (maxx - maxasx));
            maxsekx = (maxx - maxasx - maxminx / 60.0) * 3600.0;

            if (maxaltitude < 0)
                maxasx = -maxasx;

            // Azimuth
            cosAz = (Math.Sin(decRadian) - Math.Sin(latRadian) * Math.Sin(altitude)) / (Math.Cos(latRadian) * Math.Cos(altitude));
            azimuth = Math.Acos(cosAz);
            azimuth = azimuth * 180.0 / Math.PI;


            if (hourAngle < Math.PI)
                azimuth = 360.0 - azimuth;


            // North to East - Azimuth          	 
            asx = Math.Floor(azimuth);
            minX = Math.Floor(60.0 * (azimuth - asx));
            secX = (azimuth - asx - minX / 60.0) * 3600.0;


            azimuthOutput = asx + minX / 60.0 + secX / 3600.0;


            // South to West - North Azimuth
            if (azimuth < 180.0)
                azimuth = azimuth + 180.0;
            else
                azimuth = azimuth - 180.0;

            asx = Math.Floor(azimuth);
            minX = Math.Floor(60.0 * (azimuth - asx));
            secX = (azimuth - asx - minX / 60.0) * 3600.0;

            maximumAltitude = asx + minX / 60.0 + secX / 3600.0;

            return Tuple.Create(altitudeOutput, azimuthOutput, maximumAltitude);
        }

        public static Tuple<double, double> AltAz2RaDec(double alt, double az, Location.Location location)
        {
            double ra, dec;

            ra = Math.Asin(Math.Sin(alt) * Math.Sin(location.LatDegrees) + Math.Cos(alt) * Math.Cos(location.LatDegrees) * Math.Cos(az));
            dec = Math.Acos((Math.Sin(alt) - Math.Sin(location.LatDegrees) * Math.Sin(az)) / (Math.Cos(location.LatDegrees * Math.Cos(az))));
            return Tuple.Create(ra, dec);
        }

        public static double AngularDistance(Tuple<double, double> obj1, Tuple<double, double> obj2)
        {
            double angle;
            angle = Math.Acos(Math.Sin(obj1.Item2 * Math.PI / 180.0) * Math.Sin(obj2.Item2 * Math.PI / 180.0) +
                Math.Cos(obj1.Item2 * Math.PI / 180.0) * Math.Cos(obj2.Item2 * Math.PI / 180.0) * Math.Cos(obj1.Item1 * Math.PI / 180.0 - obj2.Item1 * Math.PI / 180.0));
            return angle * 180.0 / Math.PI;
        }
    }
}
