using System;

namespace GeoUtilsDotNet.DataTypes
{
    public struct Angle
    {
        private double m_degrees;

        private Angle(double degrees)
        {
            m_degrees = degrees;
        }

        public static Angle FromDegrees(double degrees)
        {
            return new Angle(degrees);
        }

        public static Angle FromRadians(double radians)
        {
            return new Angle(radians * (180 / Math.PI));
        }

        public double Degrees
        {
            get { return m_degrees; }
        }

        public double Radians
        {
            get { return m_degrees * (Math.PI / 180); }
        }

        public Angle Normalize(Angle from, Angle to)
        {
            return NormalizeDegrees(from.Degrees, to.Degrees);
        }

        public Angle NormalizeDegrees(double from, double to)
        {
            double d = to - from;
            double res = (m_degrees + to) - Math.Truncate((m_degrees + to) / d) * d + from;
            return new Angle(res);
        }
    }
}
