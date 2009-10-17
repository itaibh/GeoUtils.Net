using System;
using System.IO;

namespace DTEDReader
{
    class DTEDFile
    {
        private int[,] m_elevationData;
        private UserHeaderLabel m_uhl;

        public UserHeaderLabel UHL
        {
            get { return m_uhl; }
        }

        public int GetElevation(int x, int y)
        {
            return m_elevationData[x, y];
        }

        public void Read(Stream input)
        {
            using (BinaryReader br = new BinaryReader(input))
            {
                m_uhl = ReadUHL(br);
                ReadDSI(br);
                ReadACC(br);
                ReadElevationData(br);
            }
        }

        #region Text Based Data
        private UserHeaderLabel ReadUHL(BinaryReader br)
        {
            char[] header = br.ReadChars(4);
            if (new string(header) != "UHL1")
                throw new Exception("Wrong UHL header");

            UserHeaderLabel uhl = new UserHeaderLabel();

            uhl.Longitude = ReadGeoAngle(br, 3, 2);
            uhl.Latitude = ReadGeoAngle(br, 3, 2);
            uhl.LonInterval = ReadInterval(br);
            uhl.LatInterval = ReadInterval(br);

            char[] accuracy = br.ReadChars(4);
            char[] classification = br.ReadChars(3);
            char[] uniqueRef = br.ReadChars(12);

            uhl.Accuracy = new string(accuracy);
            uhl.Classification = new string(classification);
            uhl.UniqueRef = new string(uniqueRef);

            uhl.LongitudeLines = ReadInt(br, 4);
            uhl.LatitudePoints = ReadInt(br, 4);
            uhl.MultipleAccuracy = br.ReadChar() == '1';
            char[] reserved = br.ReadChars(24);

            return uhl;
        }

        private void ReadDSI(BinaryReader br)
        {
            char[] header = br.ReadChars(3);
            if (new string(header) != "DSI")
                throw new Exception("Wrong DSI header");

            char classification = br.ReadChar();
            char[] securityControl = br.ReadChars(2);
            char[] securityHandlingDescription = br.ReadChars(27);
            char[] reserved1 = br.ReadChars(26);

            char[] format = br.ReadChars(5); //DTED0, DTED1, DTED2
            string formatStr = new string(format);
            if (formatStr != "DTED0" && formatStr != "DTED1" && formatStr != "DTED2")
                throw new Exception("Wrong NIMA series designator");

            char[] uniqueRef = br.ReadChars(15);
            char[] reserved2 = br.ReadChars(8);

            int dataEdition = ReadInt(br, 2);
            char matchVersion = br.ReadChar();

            int maintenanceYear = ReadInt(br, 2);
            int maintenanceMonth = ReadInt(br, 2);

            int matchYear = ReadInt(br, 2);
            int matchMonth = ReadInt(br, 2);

            char[] maintenanceDescriptionCode = br.ReadChars(4);
            char[] producerCode = br.ReadChars(8);

            char[] reserved3 = br.ReadChars(16);

            char[] productSpec = br.ReadChars(9);
            int productSpecAmendment = ReadInt(br, 1);
            int productSpecChangeNumber = ReadInt(br, 1);

            int productSpecYear = ReadInt(br, 2);
            int productSpecMonth = ReadInt(br, 2);

            char[] vDatum = br.ReadChars(3);
            char[] hDatum = br.ReadChars(5);

            char[] digitizingSystem = br.ReadChars(10);

            int compilationYear = ReadInt(br, 2);
            int compilationMonth = ReadInt(br, 2);

            char[] reserved4 = br.ReadChars(22);

            double latitudeOrigin = ReadGeoAngle(br, 2, 4);
            double longitudeOrigin = ReadGeoAngle(br, 3, 4);

            double latitudeSWCorner = ReadGeoAngle(br, 2, 2);
            double longitudeSWCorner = ReadGeoAngle(br, 3, 2);

            double latitudeNWCorner = ReadGeoAngle(br, 2, 2);
            double longitudeNWCorner = ReadGeoAngle(br, 3, 2);

            double latitudeNECorner = ReadGeoAngle(br, 2, 2);
            double longitudeNECorner = ReadGeoAngle(br, 3, 2);

            double latitudeSECorner = ReadGeoAngle(br, 2, 2);
            double longitudeSECorner = ReadGeoAngle(br, 3, 2);

            double orientationAngle = ReadGeoAngle(br, 3, 4, false);

            double latInterval = ReadInterval(br);
            double lonInterval = ReadInterval(br);

            int latitudeLines = ReadInt(br, 4);
            int longitudeLines = ReadInt(br, 4);

            int dataCoverage = ReadInt(br, 2);
            if (dataCoverage == 0)
                dataCoverage = 100;

            char[] reserved5 = br.ReadChars(101);
            char[] reserved6 = br.ReadChars(100);
            char[] comments = br.ReadChars(156);
        }

        private void ReadACC(BinaryReader br)
        {
            char[] header = br.ReadChars(3);
            if (new string(header) != "ACC")
                throw new Exception("Wrong ACC header");

            int? absoluteHorizontalAccuracy = ReadOptionalInt(br, 4);
            int? absoluteVerticalAccuracy = ReadOptionalInt(br, 4);
            int? relativeHorizontalAccuracy = ReadOptionalInt(br, 4);
            int? relativeVerticalAccuracy = ReadOptionalInt(br, 4);

            char[] reserved1 = br.ReadChars(4);
            char[] reserved2 = br.ReadChars(1);
            char[] reserved3 = br.ReadChars(31);

            int multipleAccuracyOutlineFlag = ReadInt(br, 2);
            if (multipleAccuracyOutlineFlag >= 2 && multipleAccuracyOutlineFlag <= 9)
            {
                for (int i = 0; i < multipleAccuracyOutlineFlag; ++i)
                {
                    ReadAccuracySubRegionDescription(br);
                }
            }
            else
                multipleAccuracyOutlineFlag = 0;

            br.ReadChars(284 * (9 - multipleAccuracyOutlineFlag)); //skip blank subregions

            char[] reserved4 = br.ReadChars(18);
            char[] reserved5 = br.ReadChars(69);
        }

        private void ReadAccuracySubRegionDescription(BinaryReader br)
        {
            int? absoluteHorizontalAccuracy = ReadOptionalInt(br, 4);
            int? absoluteVerticalAccuracy = ReadOptionalInt(br, 4);
            int? relativeHorizontalAccuracy = ReadOptionalInt(br, 4);
            int? relativeVerticalAccuracy = ReadOptionalInt(br, 4);

            int numOfCoordinates = ReadInt(br, 2);

            if (numOfCoordinates < 3 || numOfCoordinates > 14)
                throw new Exception("Number of coordinates in subregion must be between 3 and 14");

            for (int i = 0; i < 14; ++i)
            {
                double lat = ReadGeoAngle(br, 2, 4);
                double lon = ReadGeoAngle(br, 3, 4);
            }
        }

        private int? ReadOptionalInt(BinaryReader br, int chars)
        {
            char[] val = br.ReadChars(chars);
            string str = new string(val);
            if (str.StartsWith("NA"))
                return null;
            int ret = int.Parse(str);
            return ret;
        }

        private int ReadInt(BinaryReader br, int chars)
        {
            char[] str = br.ReadChars(chars);
            int ret = int.Parse(new string(str));
            return ret;
        }

        private double ReadDouble(BinaryReader br, int chars)
        {
            char[] str = br.ReadChars(chars);
            double ret = double.Parse(new string(str));
            return ret;
        }

        private double ReadGeoAngle(BinaryReader br, int degDitigs, int secsDigits)
        {
            return ReadGeoAngle(br, degDitigs, secsDigits, true);
        }

        private double ReadGeoAngle(BinaryReader br, int degDitigs, int secsDigits, bool readHemisphere)
        {
            int degs = ReadInt(br, degDitigs);
            int mins = ReadInt(br, 2);
            double secs = ReadDouble(br, secsDigits);
            int sign = 1;
            if (readHemisphere)
            {
                char signChr = br.ReadChar();
                sign = (signChr == 'E' || signChr == 'N') ? 1 : -1;
            }
            return (degs + mins / 60d + secs / 3600d) * sign;
        }

        private double ReadInterval(BinaryReader br)
        {
            int value = ReadInt(br, 4);
            return value / 36000d;
        }
        #endregion

        private void ReadElevationData(BinaryReader br)
        {
            byte header = br.ReadByte();
            if (header != 0xAA)
                throw new Exception("Wrong recognition sentinel");

            int blockCount = ReadSpecialInt(br, 3);
            ushort lonCount = br.ReadUInt16();
            ushort latCount = br.ReadUInt16();

            int lons = m_uhl.LongitudeLines - 1;
            int lats = m_uhl.LatitudePoints + 6;

            m_elevationData = new int[lons, lats];
            byte[] data = br.ReadBytes(lons * lats * 2);

            int idx = 0;
            for (int x = 0; x < lons; ++x)
                for (int y = 0; y < lats; ++y)
                {
                    uint value = (uint)(data[idx++] * 256 + data[idx++]);
                    if ((value & 0x8000) != 0)
                        m_elevationData[x, y] = -(int)(value & 0x7FFF);
                    else
                        m_elevationData[x, y] = (int)value;
                }

            uint checksum = br.ReadUInt32();
            long len = br.BaseStream.Length;
            long pos = br.BaseStream.Position;
        }

        private int ReadSpecialInt(BinaryReader br, int byteCount)
        {
            byte[] bytes = br.ReadBytes(byteCount);
            int result = 0;
            for (int i = 0; i < byteCount; ++i)
            {
                result *= 256;
                result += bytes[i];
            }
            return result;
        }
    }

    public class UserHeaderLabel
    {
        public double Longitude;
        public double Latitude;
        public double LonInterval;
        public double LatInterval;

        public string Accuracy;
        public string Classification;
        public string UniqueRef;

        public int LongitudeLines;
        public int LatitudePoints;
        public bool MultipleAccuracy;
    }
}
