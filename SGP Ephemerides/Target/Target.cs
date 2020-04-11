﻿using SGP_Ephemerides.Charts;
using System;
using System.Runtime.InteropServices;

namespace SGP_Ephemerides.Target
{
    public class Target
    {
        public string Name { get; set; }

        // Decimal Degrees
        private double _RightAscension;
        public double RightAscension 
        {
            get { return _RightAscension; }
            set 
            { 
                _RightAscension = value;
                RaDegrees = Math.Truncate(_RightAscension);
                RaHours   = TimeSpan.FromHours(_RightAscension / 15.0).Hours;
                RaMinutes = TimeSpan.FromHours(_RightAscension).Minutes;
                RaSeconds = TimeSpan.FromHours(_RightAscension).Seconds + TimeSpan.FromHours(_RightAscension).Milliseconds / 1000.0;
            }
        }
        public double RaDegrees { get; private set; }
        public double RaHours { get; private set; }
        public double RaMinutes { get; private set; }
        public double RaSeconds { get; private set; }
        
        // Decimal Degrees
        private double _Declination; 
        public double Declination 
        { 
            get { return _Declination; } 
            set 
            { 
                if (value < 0.0) 
                { 
                    _Declination = -value; 
                    North = false; 
                } 
                else 
                { 
                    _Declination = value; 
                    North = true; 
                }

                DecDegrees = Math.Truncate(_Declination);
                DecHours   = TimeSpan.FromHours(_Declination / 15.0).Hours;
                DecMinutes = TimeSpan.FromHours(_Declination).Minutes;
                DecSeconds = TimeSpan.FromHours(_Declination).Seconds + TimeSpan.FromHours(_Declination).Milliseconds / 1000.0;
            } 
        }
        public double DecDegrees { get; private set; }
        public double DecHours { get; private set; }
        public double DecMinutes { get; private set; }
        public double DecSeconds { get; private set; }
        public bool North { get; set; }  

        public string Directory { get; set; }
        public bool Enabled { get; set; }

        public AltitudeSeries mAltitudeSeries;

        //################################################################################################################
        //################################################################################################################

        public Target()
        {
            Name = "M31";
            RightAscension = 0.712306;
            Declination = 41.269167;
            Directory = string.Empty;
            North = true;
            Enabled = true;
            mAltitudeSeries = new AltitudeSeries();
        }

        //################################################################################################################
        //################################################################################################################
    }
}
