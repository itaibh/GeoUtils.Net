using System;
using GeoUtilsDotNet.DataTypes;

namespace GeoUtilsDotNet.Projections
{
    public class UTM
    {
        public UTMLocation Project(LatLongAlt worldPosition, IEllipsoid ellipsoid)
        {
            double lonDeg = worldPosition.LongitudeNormalized.Degrees;
            double lonRad = lonDeg * Math.PI / 180;

            double latDeg = worldPosition.LatitudeNormalized.Degrees;
            double latRad = latDeg * Math.PI / 180;

            int zone = GetZone(lonDeg, latDeg);

            double lonOrigDeg = (zone - 1) * 6 - 180 + 3;  //+3 puts origin in middle of zone
            double lonOrigRad = lonOrigDeg * Math.PI / 180;
            double ecc2 = ellipsoid.EccentricitySquared;
            double ecc4 = ecc2 * ecc2;
            double ecc6 = ecc4 * ecc2;
            double eccPrimeSquared = ecc2 / (1 - ecc2);

            double a = ellipsoid.SemiMajorAxis.Meters;

            double sinLat = Math.Sin(latRad);
            double cosLat = Math.Cos(latRad);
            double tanLat = Math.Tan(latRad);

            double N = a / Math.Sqrt(1 - ellipsoid.EccentricitySquared * sinLat * sinLat);
            double T = tanLat * tanLat;
            double C = eccPrimeSquared * cosLat * cosLat;
            double A = cosLat * (lonRad - lonOrigRad);
            double A2 = A * A;
            double A3 = A * A * A;

            double M = a * ((1 - ecc2 / 4 - 3 * ecc4 / 64 - 5 * ecc6 / 256) * latRad
                        - (3 * ecc2 / 8 + 3 * ecc4 / 32 + 45 * ecc6 / 1024) * Math.Sin(2 * latRad)
                        + (15 * ecc4 / 256 + 45 * ecc6 / 1024) * Math.Sin(4 * latRad)
                        - (35 * ecc6 / 3072) * Math.Sin(6 * latRad));

            double k0 = 0.9996;

            double easting = (double)(k0 * N * (A + (1 - T + C) * A3 / 6
                            + (5 - 18 * T + T * T + 72 * C - 58 * eccPrimeSquared) * A3 * A2 / 120)
                            + 500000.0);

            double northing = (double)(k0 * (M + N * tanLat * (A2 / 2 + (5 - T + 9 * C + 4 * C * C) * A3 * A / 24
                         + (61 - 58 * T + T * T + 600 * C - 330 * eccPrimeSquared) * A3 * A3 / 720)));

            if (latDeg < 0)
                northing += 10000000.0; //10000000 meter offset for southern hemisphere

            return new UTMLocation(Length.FromMeters(easting), Length.FromMeters(northing), zone);
        }

        public LatLongAlt Unproject(UTMLocation canvasPosition)
        {
            throw new NotImplementedException();
        }

        public virtual int GetZone(double lonDeg, double latDeg)
        {
            if (latDeg >= 56.0 && latDeg < 64.0 && lonDeg >= 3.0 && lonDeg < 12.0)
                return 32;

            // Special zones for Svalbard
            if (latDeg >= 72.0 && latDeg < 84.0)
            {
                if (lonDeg >= 0.0 && lonDeg < 9.0) return 31;
                else if (lonDeg >= 9.0 && lonDeg < 21.0) return 33;
                else if (lonDeg >= 21.0 && lonDeg < 33.0) return 35;
                else if (lonDeg >= 33.0 && lonDeg < 42.0) return 37;
            }

            int zone = (int)Math.Truncate((lonDeg + 180) / 6) + 1;
            return zone;
        }

        public char GetUtmLetter(Angle latitude)
        {
            return '#';
        }
    }
}
