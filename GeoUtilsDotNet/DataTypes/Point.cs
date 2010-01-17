using System;

namespace GeoUtilsDotNet.DataTypes
{
    public struct Point
    {
        private double m_x;
        private double m_y;

        public Point(double x, double y)
        {
            m_x = x;
            m_y = y;
        }

        public double X
        {
            get { return m_x; }
        }
        
        public double Y
        {
            get { return m_y; }
        }
    }
}
