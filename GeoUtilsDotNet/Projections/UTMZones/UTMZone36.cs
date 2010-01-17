using System;
using System.Collections.Generic;
using System.Text;
using GeoUtilsDotNet.DataTypes;

namespace GeoUtilsDotNet.Projections.UTMZones
{
    public class UTMZone36 : IProjection
    {
        #region IProjection Members

        public Point Project(LatLongAlt worldPosition)
        {
            throw new NotImplementedException();
        }

        public LatLongAlt Unproject(Point canvasPosition)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
