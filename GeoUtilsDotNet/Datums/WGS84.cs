using System;
using GeoUtilsDotNet.DataTypes;

namespace GeoUtilsDotNet.Datums
{
    public class WGS84 : IEllipsoid
    {
        public Length SemiMajorAxis
        {
            get { return Length.FromMeters(6378137); }
        }

        public Length SemiMinorAxis
        {
            get { return Length.FromMeters(6356752.314245179497563966599637); }
        }

        public double Flattening
        {
            get { return 0.003352810664747480719845528618; }
        }

        public double EccentricitySquared
        {
            get { return 0.00669437999014; }
        }

        public double Eccentricity
        {
            get { return 0.081819190842622; }
        }
    }
}
