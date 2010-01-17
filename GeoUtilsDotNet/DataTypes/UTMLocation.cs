using System;
using System.Collections.Generic;
using System.Text;

namespace GeoUtilsDotNet.DataTypes
{
    public struct UTMLocation
    {
        private Length m_x;
        private Length m_y;
        private int m_zone;

        public UTMLocation(Length x, Length y, int zone)
        {
            m_x = x;
            m_y = y;
            m_zone = zone;
        }

        public Length X
        {
            get { return m_x; }
        }

        public Length Y
        {
            get { return m_y; }
        }

        public int Zone
        {
            get { return m_zone; }
        }
    }
}
