using System;
using GeoUtilsDotNet.DataTypes;

namespace GeoUtilsDotNet
{
    public interface IProjection
    {
        Point Project(LatLongAlt worldPosition);
        LatLongAlt Unproject(Point canvasPosition);
    }
}
