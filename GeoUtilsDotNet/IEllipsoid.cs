using System;

namespace GeoUtilsDotNet
{
    public interface IEllipsoid
    {
        double Eccentricity { get; }
        double EccentricitySquared { get; }
        double Flattening { get; }
        GeoUtilsDotNet.DataTypes.Length SemiMajorAxis { get; }
        GeoUtilsDotNet.DataTypes.Length SemiMinorAxis { get; }
    }
}
