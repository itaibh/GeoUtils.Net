using System;
using GeoUtilsDotNet.DataTypes;

namespace GeoUtilsDotNet.Projections
{
    public class Mercator : IProjection
    {
        #region IProjection Members

        public Point Project(LatLongAlt worldPosition)
        {
            double x = worldPosition.Longitude.Degrees;

            double latRads = worldPosition.Latitude.Radians;
            double tan = Math.Tan(latRads);
            double sec = 1 / Math.Cos(latRads);
            double y = Math.Log(tan + sec);

            Point point = new Point(x, y);
            return point;
        }

        public LatLongAlt Unproject(Point canvasPosition)
        {
            double sinh = Math.Sinh(canvasPosition.Y);
            double tan = Math.Tan(sinh);
            double lat = Math.Pow(tan, -1);

            double lon = canvasPosition.X;

            return LatLongAlt.FromDegrees(lat, lon);
        }

        #endregion
    }
}
