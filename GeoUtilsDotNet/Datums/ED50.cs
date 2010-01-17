using System;
using GeoUtilsDotNet.DataTypes;

namespace GeoUtilsDotNet.Datums
{
    public class ED50 : IEllipsoid
    {
        #region IEllipsoid Members

        public Length SemiMajorAxis
        {
            get { throw new NotImplementedException(); }
        }

        public Length SemiMinorAxis
        {
            get { throw new NotImplementedException(); }
        }

        public double Eccentricity
        {
            get { throw new NotImplementedException(); }
        }

        public double EccentricitySquared
        {
            get { throw new NotImplementedException(); }
        }

        public double Flattening
        {
            get { throw new NotImplementedException(); }
        }

        #endregion
    }
}
