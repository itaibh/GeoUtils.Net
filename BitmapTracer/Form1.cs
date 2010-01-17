using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace BitmapTracer
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            BuildTracingTable();
        }

        private void BuildTracingTable()
        {
            Color full = Color.Gold;
            Color empty = Color.LightGreen;
            Color current = Color.Red;
            Color origin1 = Color.Blue;
            Color origin2 = Color.Pink;
            Color next = Color.White;

            int[] pow2 = new int[] { 1, 2, 4, 8, 16, 32, 64, 128 };

            Point[] dirs = new Point[8];
            dirs[0] = new Point(1, 0);
            dirs[1] = new Point(1, 1);
            dirs[2] = new Point(0, 1);
            dirs[3] = new Point(-1, 1);
            dirs[4] = new Point(-1, 0);
            dirs[5] = new Point(-1, -1);
            dirs[6] = new Point(0, -1);
            dirs[7] = new Point(1, -1);

            Bitmap bmp = new Bitmap(4 * 16 * 2, 4 * 16 * 2, PixelFormat.Format32bppArgb);

            int x = 1;
            int y = 1;
            for (int d = 0; d < 8; ++d)
                for (int v = 1; v < 256; ++v)
                {
                    if ((v & pow2[d]) == 0)
                        continue;

                    for (int s = 0; s < 2; ++s)
                    {
                        //int x = 1 + (v % 16) * 4 + (s * 4 * 16);
                        //int y = 1 + (v / 16) * 4 + (d * 4 * 16);

                        if (s == 0 && ((pow2[(d + 7) % 8] & v) != 0))
                            continue;

                        if (s == 0 && d % 2 == 0 && ((pow2[(d + 6) % 8] & v) != 0))
                            continue;

                        if (s == 1 && ((pow2[(d + 1) % 8] & v) != 0))
                            continue;

                        if (s == 1 && d % 2 == 0 && ((pow2[(d + 2) % 8] & v) != 0))
                            continue;

                        bmp.SetPixel(x, y, current);

                        int count = 0;
                        int lastDir = 0;
                        int lastDirNotOrigin = 0;
                        for (int p = 0; p < 8; ++p)
                        {
                            bool value = (v & pow2[p]) != 0;
                            if (value)
                            {
                                count++;
                                lastDir = p;
                                if (p != d)
                                    lastDirNotOrigin = p;
                            }
                            bmp.SetPixel(x + dirs[p].X, y + dirs[p].Y, value ? full : empty);
                        }

                        bmp.SetPixel(x + dirs[d].X, y + dirs[d].Y, s == 0 ? origin1 : origin2);

                        if (count == 1)
                        {
                            bmp.SetPixel(x + dirs[lastDir].X, y + dirs[lastDir].Y, next);
                            s++;
                        }
                        else if (count == 2)
                        {
                            bmp.SetPixel(x + dirs[lastDirNotOrigin].X, y + dirs[lastDirNotOrigin].Y, next);
                            s++;
                        }

                        x += 4;
                        if (x > 4 * 16 * 2)
                        {
                            x = 1;
                            y += 4;
                        }
                    }
                }

            bmp.Save("TraceTable.png", ImageFormat.Png);
        }
    }
}
