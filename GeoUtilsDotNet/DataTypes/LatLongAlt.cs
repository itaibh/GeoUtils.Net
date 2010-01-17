using System;

namespace GeoUtilsDotNet.DataTypes
{
    public struct LatLongAlt
    {
        private Angle m_latitude;
        private Angle m_longitude;
        private Length m_altitude;

        public LatLongAlt(Angle latitude, Angle longitude)
        {
            m_latitude = latitude;
            m_longitude = longitude;
            m_altitude = Length.Zero;
        }

        public LatLongAlt(Angle latitude, Angle longitude, Length altitude)
        {
            m_latitude = latitude;
            m_longitude = longitude;
            m_altitude = altitude;
        }

        public static LatLongAlt FromDegrees(double latitude, double longitude)
        {
            return new LatLongAlt(Angle.FromDegrees(latitude), Angle.FromDegrees(longitude));
        }

        public Angle Latitude
        {
            get { return m_latitude; }
        }

        public Angle LatitudeNormalized
        {
            get { return m_longitude.NormalizeDegrees(-90, 90); }
        }

        public Angle Longitude
        {
            get { return m_longitude; }
        }

        public Angle LongitudeNormalized
        {
            get { return m_longitude.NormalizeDegrees(-180,180); }
        }

        public Length Altitude
        {
            get { return m_altitude; }
        }
    }
}
