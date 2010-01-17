using System;

namespace GeoUtilsDotNet.DataTypes
{
    public struct Length
    {
        public static readonly Length Zero = new Length(0);

        private double m_meters;

        private Length(double meters)
        {
            m_meters = meters;
        }

        public static Length FromMeters(double meters)
        {
            return new Length(meters);
        }

        public static Length FromKilometers(double meters)
        {
            return new Length(meters * 1000);
        }

        public double Meters
        {
            get { return m_meters; }
        }

        public double Kilometers
        {
            get { return m_meters / 1000; }
        }
    }
}
