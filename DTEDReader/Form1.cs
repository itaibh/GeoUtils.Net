using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Threading;

namespace DTEDReader
{
    public partial class Form1 : Form
    {
        static Point[] s_dirs = new Point[8];

        // 5 6 7
        // 4 - 0
        // 3 2 1
        public Form1()
        {
            InitializeComponent();
            BuildTerrainPalette();

            s_dirs[0] = new Point(1, 0);
            s_dirs[1] = new Point(1, 1);
            s_dirs[2] = new Point(0, 1);
            s_dirs[3] = new Point(-1, 1);
            s_dirs[4] = new Point(-1, 0);
            s_dirs[5] = new Point(-1, -1);
            s_dirs[6] = new Point(0, -1);
            s_dirs[7] = new Point(1, -1);
        }

        private void BuildTerrainPalette()
        {
            CreateGradient(Color.Black, Color.DarkGreen, -500, -100);
            CreateGradient(Color.DarkGreen, Color.Green, -100, 200);
            CreateGradient(Color.Green, Color.Brown, 200, 700);
            CreateGradient(Color.Brown, Color.Yellow, 700, 1200);
            CreateGradient(Color.Yellow, Color.White, 1200, 9000);
        }

        private void CreateGradient(Color c1, Color c2, int startIdx, int stopIdx)
        {
            for (int i = startIdx; i < stopIdx; ++i)
            {
                double value = (i - startIdx) / (double)(stopIdx - startIdx);
                Color c = Blend(c1, c2, value);
                s_terrainPalette[i + 500] = c;
            }
        }

        private Color Blend(Color c1, Color c2, double p)
        {
            double rp = 1 - p;
            byte r = (byte)(c1.R * rp + c2.R * p);
            byte g = (byte)(c1.G * rp + c2.G * p);
            byte b = (byte)(c1.B * rp + c2.B * p);
            return Color.FromArgb(r, g, b);
        }

        const int tileWidth = 120;
        const int tileHeight = 120;
        private static Color[] s_terrainPalette = new Color[10000];

        private void Form1_Load(object sender, EventArgs e)
        {
            for (int lon = 33; lon <= 36; ++lon)
            {
                for (int lat = 29; lat <= 33; ++lat)
                {
                    try
                    {
                        Bitmap bmp = ReadTile(lon, lat);

                        PictureBox pb = new PictureBox();
                        pb.Image = bmp;
                        pb.Size = bmp.Size;
                        pb.Top = 50 + (33 - lat) * tileWidth;
                        pb.Left = (lon - 33) * tileHeight;

                        this.Controls.Add(pb);
                    }
                    catch (FileNotFoundException) { }
                }
            }

        }

        private static Bitmap ReadTile(int lon, int lat)
        {
            DTEDFile dted = new DTEDFile();
            using (FileStream fs = File.OpenRead(string.Format(@"Data\dted\e0{0}\n{1}.dt0", lon, lat)))
                dted.Read(fs);

            UserHeaderLabel uhl = dted.UHL;

            Bitmap bmp = new Bitmap(tileWidth, tileHeight);
            BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, tileWidth, tileHeight), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
            byte[] data = new byte[bmpData.Stride * bmpData.Height];

            for (int y = 0; y < tileHeight; ++y)
            {
                for (int x = 0; x < tileWidth; ++x)
                {
                    int value = dted.GetElevation(x, y);
                    Color col = s_terrainPalette[value + 500];
                    int pixelOffset = (tileHeight - y - 1) * bmpData.Stride + x * 4;
                    data[pixelOffset + 0] = col.B;
                    data[pixelOffset + 1] = col.G;
                    data[pixelOffset + 2] = col.R;
                    data[pixelOffset + 3] = 255;
                }
            }
            Marshal.Copy(data, 0, bmpData.Scan0, data.Length);
            bmp.UnlockBits(bmpData);
            return bmp;
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            this.Opacity = trackBar1.Value / 100d;
        }

        const int PixelSize = 5;

        private void ScanDTED(int lon, int lat)
        {
            DTEDFile dted = new DTEDFile();
            using (FileStream fs = File.OpenRead(string.Format(@"Data\dted\e0{0}\n{1}.dt0", lon, lat)))
                dted.Read(fs);

            int[,] data = new int[tileHeight + 2, tileWidth + 2];
            bool[,] visitMask = new bool[tileHeight + 2, tileWidth + 2];

            for (int y = 0; y < tileHeight + 2; ++y)
            {
                data[y, 0] = int.MinValue;
                data[y, tileWidth + 1] = int.MinValue;
            }
            for (int x = 0; x < tileWidth + 2; ++x)
            {
                data[0, x] = int.MinValue;
                data[tileHeight + 1, x] = int.MinValue;
            }

            int min = int.MaxValue;
            int max = int.MinValue;
            for (int y = 0; y < tileHeight; ++y)
            {
                for (int x = 0; x < tileWidth; ++x)
                {
                    int value = dted.GetElevation(x, y);
                    data[y + 1, x + 1] = value;
                    min = Math.Min(value, min);
                    max = Math.Max(value, max);
                }
            }

            Bitmap bmp = new Bitmap((tileWidth + 2) * PixelSize, (tileHeight + 2) * PixelSize, PixelFormat.Format32bppArgb);

            PictureBox pb = new PictureBox();
            pb.Image = bmp;
            pb.Width = (tileWidth + 2) * PixelSize;
            pb.Height = (tileHeight + 2) * PixelSize;
            pb.Top = 0;
            pb.Left = 500;
            Controls.Add(pb);

            using (Graphics g = Graphics.FromImage(bmp))
            {
                int counter = 0;
                //for (int h = max; h >= min; --h)
                int h = 600;
                {
                    //if (h % 300 != 0) continue;

                    for (int y = 0; y < tileHeight + 2; ++y)
                        for (int x = 0; x < tileWidth + 2; ++x)
                            visitMask[y, x] = false;

                    for (int y = 1; y <= tileHeight; ++y)
                    {
                        for (int x = 1; x <= tileWidth; ++x)
                        {
                            Rectangle pixelRect = new Rectangle(x * PixelSize, (tileHeight - y) * PixelSize, PixelSize, PixelSize);
                            int value = data[y, x];
                            if (value < h)
                                g.FillRectangle(Brushes.Black, pixelRect);
                            else
                                g.FillRectangle(Brushes.Gray, pixelRect);
                        }
                    }

                    bool inside = false;
                    for (int y = 1; y <= tileHeight; ++y)
                    {
                        for (int x = 1; x <= tileWidth; ++x)
                        {
                            if (visitMask[y, x])
                            {
                                inside = true;
                                continue;
                            }

                            int value = data[y, x];
                            if (value < h)
                                inside = false;
                            else if (!inside)
                            {
                                counter++;
                                if (counter == 7)
                                    return;
                                List<Point> vector = TryTrace(data, visitMask, g, x, y, h);
                                if (vector.Count == 1)
                                {
                                    g.DrawEllipse(Pens.Blue, vector[0].X, vector[0].Y, PixelSize, PixelSize);
                                }
                                if (vector.Count > 2)
                                    g.DrawPolygon(Pens.Blue, vector.ToArray());

                                inside = true;
                            }
                        }
                    }
                }
            }
        }

        // 5 6 7
        // 4 - 0
        // 3 2 1
        private List<Point> TryTrace(int[,] data, bool[,] visitMask, Graphics g, int x, int y, int h)
        {
            List<Point> vector = new List<Point>();
            Dictionary<int, List<int>> strides = new Dictionary<int, List<int>>();
            int firstX = x;
            int firstY = y;
            int nextX = x;
            int nextY = y;
            int dir = 0;
            int nextDir = 0;
            int count = 0;
            do
            {
                visitMask[nextY, nextX] = true;
                UpdateStrides(strides, nextX, nextY);
                Rectangle pixelRect = new Rectangle(nextX * PixelSize, (tileHeight - nextY) * PixelSize, PixelSize, PixelSize);
                g.FillRectangle(new SolidBrush(s_terrainPalette[h + 500]), pixelRect);
                x = nextX;
                y = nextY;
                dir = nextDir;
                count++;
                vector.Add(new Point(x * PixelSize, (tileHeight - y) * PixelSize));
                FindNextPixel(data, visitMask, h, nextX, nextY, dir, out nextX, out nextY, out nextDir);
            } while ((nextX != x || nextY != y) && (nextX != firstX || nextY != firstY));

            //FillVisitMaskByStrides(strides, data, visitMask, h);

            return vector;
        }

        private void FillVisitMaskByStrides(Dictionary<int, List<int>> strides, int[,] data, bool[,] visitMask, int h)
        {
            foreach (KeyValuePair<int, List<int>> kvp in strides)
            {
                int y = kvp.Key;
                List<int> xVals = kvp.Value;

                if (xVals.Count == 1)
                    return;
                RemoveSequences(xVals);
                xVals.Add(xVals[0]);
                int i;
                bool mustDrawNext = false;
                for (i = 0; i < xVals.Count - 1; ++i)
                {
                    int x1 = xVals[i];
                    int x2 = xVals[i + 1];

                    if (x1 > x2 && !mustDrawNext)
                    {
                        mustDrawNext = true;
                        continue;
                    }
                    mustDrawNext = false;
                    if (x1 > x2)
                    {
                        int bucket = x1;
                        x1 = x2;
                        x2 = bucket;
                    }

                    for (int x = x1 + 1; x < x2; ++x)
                    {
                        if (data[y, x] >= h)
                        {
                            visitMask[y, x] = true;
                            //bmp.SetPixel(x, tileHeight - y, Color.Cyan);
                        }
                    }
                }
            }
        }

        private void RemoveSequences(List<int> xVals)
        {
            List<int> newList = new List<int>();
            int count = 0;
            for (int i = 0; i < xVals.Count - 1; ++i)
            {
                if (count == 0)
                    newList.Add(xVals[i]);

                if (Math.Abs(xVals[i] - xVals[i + 1]) == 1)
                    count++;
                else
                {
                    if (count > 0)
                    {
                        count = 0;
                        newList.Add(xVals[i]);
                    }
                }
            }
            newList.Add(xVals[xVals.Count - 1]);
            xVals.Clear();
            xVals.AddRange(newList);
        }

        private void UpdateStrides(Dictionary<int, List<int>> strides, int nextX, int nextY)
        {
            List<int> line;
            if (!strides.TryGetValue(nextY, out line))
            {
                line = new List<int>();
                strides[nextY] = line;
            }
            line.Add(nextX);
        }

        private void FindNextPixel(int[,] data, bool[,] visitMask, int h, int x, int y, int dir, out int nextX, out int nextY, out int nextDir)
        {
            nextX = x;
            nextY = y;
            nextDir = dir;
            int allPossiblePlaces = 0;
            int unvisitedPossiblePlaces = 0;
            Point lastPossibleDir = new Point(0, 0);
            int lastPossibleDirIdx = -1;
            Point lastUnvisitedPossibleDir = new Point(0, 0);
            bool? lastDirEmpty = null;
            bool dirEmpty = false;
            bool hitUnvisitedDir = false;
            int lastResortX = 0;
            int lastResortY = 0;
            int lastResortDir = 0;

            for (int i = dir; i <= dir + 8; ++i)
            {
                Point d = s_dirs[i % 8];
                dirEmpty = (data[y + d.Y, x + d.X] < h);

                if (!dirEmpty)
                {
                    if (!visitMask[y + d.Y, x + d.X])
                    {
                        unvisitedPossiblePlaces++;
                        lastUnvisitedPossibleDir = d;
                    }
                    allPossiblePlaces++;
                    lastPossibleDir = d;
                    lastPossibleDirIdx = i % 8;
                }
                else
                {
                    if (lastDirEmpty.HasValue && !lastDirEmpty.Value && (i + 3) % 8 != dir % 8)
                    {
                        if (visitMask[y + lastPossibleDir.Y, x + lastPossibleDir.X])
                        {
                            if (!hitUnvisitedDir)
                            {
                                hitUnvisitedDir = true;
                                lastResortDir = (i + 7) % 8;
                                lastResortX = x + lastPossibleDir.X;
                                lastResortY = y + lastPossibleDir.Y;
                            }
                        }
                        else
                        {
                            nextDir = (i + 7) % 8;
                            nextX = x + lastPossibleDir.X;
                            nextY = y + lastPossibleDir.Y;
                            return;
                        }
                    }
                }

                lastDirEmpty = dirEmpty;
            }

            if (hitUnvisitedDir)
            {
                nextX = lastResortX;
                nextY = lastResortY;
                nextDir = lastResortDir;
                return;
            }

            if (allPossiblePlaces == 0)
                return;

            if (allPossiblePlaces == 1)
            {
                nextX = x + lastPossibleDir.X;
                nextY = y + lastPossibleDir.Y;
                nextDir = lastPossibleDirIdx;
                return;
            }

            if (unvisitedPossiblePlaces == 1)
            {
                nextX = x + lastUnvisitedPossibleDir.X;
                nextY = y + lastUnvisitedPossibleDir.Y;
                return;
            }


            return;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            ScanDTED(35, 31);

        }

    }
}
