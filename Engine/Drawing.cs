/*
	Drawing.cs
        By: Jakub P. Szarkowicz / JakubSzark
	Credit: One Lone Coder
*/

using System;
using System.Threading;

namespace PGE
{
    public class Graphics2D
    {
        private Sprite drawTarget;
        private Sprite fontSprite;

        public OpacityMode OpacityMode { get; set; }

        public Graphics2D(Sprite target) {
            this.drawTarget = target;
        }

        /// <summary>
        /// Returns the current Frame
        /// </summary>
        /// <returns>Draw Target</returns>
        public Sprite GetDrawTarget(bool copy = false) =>
            !copy ? drawTarget : new Sprite(drawTarget);

        /// <summary>
        /// Set the Draw Target.
        /// </summary>
        /// <param name="target">The New Target</param>
        public void SetDrawTarget(Sprite target) =>
            drawTarget = target;

        /// <summary>
        /// Draws a Pixel on the Screen
        /// </summary>
        /// <param name="x">X</param>
        /// <param name="y">Y</param>
        /// <param name="p">Color</param>
        public virtual void Draw(int x, int y, Pixel p)
        {
            if (OpacityMode == OpacityMode.ALPHA && p.a < 255)
            {
                var l = Pixel.Lerp(drawTarget.GetPixel(x, y), p, p.a / 255f);
                drawTarget.SetPixel(x, y, new Pixel(l.r, l.g, l.b));
                return;
            }

            if (OpacityMode == OpacityMode.MASK && p.a < 255)
                return;

            drawTarget.SetPixel(x, y, p);
        }

        /// <summary>
        /// Draws a Line
        /// </summary>
        /// <param name="x1">X1</param>
        /// <param name="y1">Y1</param>
        /// <param name="x2">X2</param>
        /// <param name="y2">Y2</param>
        /// <param name="p">Color</param>
        /// <param name="thickness">Thickness</param>
        public void DrawLine(int x1, int y1, int x2, int y2, Pixel p, int thickness = 1)
        {
            float x, y, step;
            float dx = x2 - x1;
            float dy = y2 - y1;

            float absDX = Math.Abs(dx);
            float absDY = Math.Abs(dy);

            step = absDX >= absDY ? absDX : absDY;

            dx /= step;
            dy /= step;

            x = x1;
            y = y1;

            for (int i = 1; i <= step; i++)
            {
                Draw((int)x, (int)y, p);

                if (thickness > 1)
                {
                    for(int j = 1; j < thickness; j++)
                    {
                        Draw((int)x + j, (int)y, p);
                        Draw((int)x, (int)y + j, p);
                    }
                }

                x += dx;
                y += dy;
            }
        }

        /// <summary>
        /// Draws a Rectangle Outline
        /// </summary>
        /// <param name="x">X</param>
        /// <param name="y">Y</param>
        /// <param name="w">Width</param>
        /// <param name="h">Height</param>
        /// <param name="p">Color</param>
        public void DrawRect(int x, int y, int w, int h, Pixel p)
        {
            if (w < 0)
            {
                w *= -1;
                x -= w;
            }

            DrawLine(x, y, x + w, y, p);
            DrawLine(x + w - 1, y, x + w - 1, y + h, p);
            DrawLine(x, y + h - 1, x + w, y + h - 1, p);
            DrawLine(x, y, x, y + h, p);
        }

        /// <summary>
        /// Draws a Filled In Rectangle
        /// </summary>
        /// <param name="x">X</param>
        /// <param name="y">Y</param>
        /// <param name="w">Width</param>
        /// <param name="h">Height</param>
        /// <param name="p">Color</param>
        public void FillRect(int x, int y, int w, int h, Pixel p)
        {
            for (int i = 0; i < w; i++)
                for (int j = 0; j < h; j++)
                    Draw(x + i, y + j, p);
        }

        /// <summary>
        /// Draws a Circle Outline
        /// </summary>
        /// <param name="x">X</param>
        /// <param name="y">Y</param>
        /// <param name="r">Radius</param>
        /// <param name="p">Color</param>
        public void DrawCircle(int x, int y, int r, Pixel p)
        {
            if (r == 0) return;

            if (r < 0)
            {
                r *= -1;
                x -= r;
            }

            int x0 = 0;
            int y0 = r / 2;
            int d = 3 - 2 * r;

            while (y0 >= x0)
            {
                Draw(x - x0, y - y0, p);
                Draw(x - y0, y - x0, p);
                Draw(x + y0, y - x0, p);
                Draw(x + x0, y - y0, p);
                Draw(x - x0, y + y0, p);
                Draw(x - y0, y + x0, p);
                Draw(x + y0, y + x0, p);
                Draw(x + x0, y + y0, p);
                if (d < 0) d += 4 * x0++ + 6;
                else d += 4 * (x0++ - y0--) + 10;
            }
        }

        /// <summary>
        /// Draws a Filled in Circle
        /// </summary>
        /// <param name="x">X</param>
        /// <param name="y">Y</param>
        /// <param name="r">Radius</param>
        /// <param name="p">Color</param>
        public void FillCircle(int x, int y, int r, Pixel p)
        {
            for (int i = x; i < x + r * 2; i++)
            {
                for (int j = y; j < y + r * 2; j++)
                {
                    var dist = Math.Sqrt((x + r - i) * (x + r - i) + (y + r - j) * (y + r - j));
                    if (dist < r) Draw(x - 1 + i, y - 1 + j, p);
                }
            }
        }

        /// <summary>
        /// Draw a Triangle Outline
        /// </summary>
        /// <param name="x1">X1</param>
        /// <param name="y1">Y1</param>
        /// <param name="x2">X2</param>
        /// <param name="y2">Y2</param>
        /// <param name="x3">X3</param>
        /// <param name="y3">Y3</param>
        /// <param name="p">Color</param>
        public void DrawTriangle(int x1, int y1, int x2, int y2, int x3, int y3, Pixel p)
        {
            DrawLine(x1, y1, x2, y2, p);
            DrawLine(x2, y2, x3, y3, p);
            DrawLine(x1, y1, x3, y3, p);
        }

        /// <summary>
        /// Draws a Filled In Triangle
        /// </summary>
        /// <param name="x1">X1</param>
        /// <param name="y1">Y1</param>
        /// <param name="x2">X2</param>
        /// <param name="y2">Y2</param>
        /// <param name="x3">X3</param>
        /// <param name="y3">Y3</param>
        /// <param name="p">Color</param>
        public void FillTriangle(int x1, int y1, int x2, int y2, int x3, int y3, Pixel p)
        {
            var minX = Math.Min(Math.Min(x1, x2), x3);
            var maxX = Math.Max(Math.Max(x1, x2), x3);

            var minY = Math.Min(Math.Min(y1, y2), y3);
            var maxY = Math.Max(Math.Max(y1, y2), y3);

            for (int x = minX; x < maxX; x++)
            {
                for (int y = minY; y < maxY; y++)
                {
                    float d1, d2, d3;
                    bool hasNeg, hasPos;

                    d1 = Sign(x, y, x1, y1, x2, y2);
                    d2 = Sign(x, y, x2, y2, x3, y3);
                    d3 = Sign(x, y, x3, y3, x1, y1);

                    hasNeg = (d1 < 0) || (d2 < 0) || (d3 < 0);
                    hasPos = (d1 > 0) || (d2 > 0) || (d3 > 0);

                    if (!(hasNeg && hasPos))
                    {
                        Draw(x, y, p);
                    }
                }
            }
        }

        private float Sign(int x1, int y1, int x2, int y2, int x3, int y3) =>
            (x1 - x3) * (y2 - y3) - (x2 - x3) * (y1 - y3);

        /// <summary>
        /// Gives you the Distance Between to Points
        /// </summary>
        /// <param name="x1">X1</param>
        /// <param name="y1">Y1</param>
        /// <param name="x2">X2</param>
        /// <param name="y2">Y2</param>
        /// <returns>The Distance</returns>
        public float Distance(int x1, int y1, int x2, int y2) =>
            (float)Math.Sqrt((x2 - x1) * (x2 - x1) + (y2 - y1) * (y2 - y1));

        /// <summary>
        /// Draws a Sprite on the Screen
        /// </summary>
        /// <param name="x">X</param>
        /// <param name="y">Y</param>
        /// <param name="scale">Scale</param>
        /// <param name="sprite">The Sprite</param>
        public void DrawSprite(int x, int y, Sprite sprite, int scale = 1)
        {
            if (sprite == null || scale <= 0) return;
            for (int i = 0; i < sprite.Width; i++)
                for (int j = 0; j < sprite.Height; j++)
                    FillRect((x + i) * scale, (y + j) * scale, 
                        scale, scale, sprite.GetPixel(i, j));
        }

        /// <summary>
        /// Draws a Sprite on the Screen with Rotation
        /// </summary>
        /// <param name="x">X Position</param>
        /// <param name="y">Y Position</param>
        /// <param name="sprite">The Sprite</param>
        /// <param name="angle">Angle</param>
        public void DrawSprite(int x, int y, Sprite sprite, double angle)
        {
            if (sprite == null) return;

            int hWidth = sprite.Width / 2;
            int hHeight = sprite.Height / 2;

            double sin = Math.Sin(-angle);
            double cos = Math.Cos(-angle);

            for (int i = 0; i < sprite.Width * 2f; i++)
            {
                for (int j = 0; j < sprite.Height * 2f; j++)
                {
                    int xt = i - sprite.Width;
                    int yt = j - sprite.Height;
                    
                    int xs = (int)Math.Round(cos * xt - sin * yt) + hWidth;
                    int ys = (int)Math.Round(sin * xt + cos * yt) + hHeight;

                    var pixel = sprite.GetPixel(xs, ys);

                    if (!pixel.Compare(new Pixel(0, 0, 0, 0)))
                        Draw((int)(x + i - sprite.Width), (int)(y + j - sprite.Height), pixel);
                }
            }
        }

        /// <summary>
        /// Partially Draws a Sprite
        /// </summary>
        /// <param name="x">X</param>
        /// <param name="y">Y</param>
        /// <param name="sprite">The Sprite</param>
        /// <param name="ox">Offset X</param>
        /// <param name="oy">Offset Y</param>
        /// <param name="w">Width</param>
        /// <param name="h">Height</param>
        public void DrawPartialSprite(int x, int y, Sprite sprite, int ox, int oy, int w, int h)
        {
            if (sprite == null) return;
            for (int i = 0; i < w; i++)
                for (int j = 0; j < h; j++)
                    Draw(x + i, y + j, sprite.GetPixel(i + ox, j + oy));
        }

        /// <summary>
        /// Draws a String using Embeded Character Data.
        /// </summary>
        /// <param name="x">X</param>
        /// <param name="y">Y</param>
        /// <param name="text">Text</param>
        /// <param name="p">Color</param>
        /// <param name="scale">Scale</param>
        public void DrawString(int x, int y, string text, Pixel p, int scale = 1)
        {
            int sx = 0;
            int sy = 0;
            foreach (var c in text)
            {
                if (c == '\n')
                {
                    sx = 0; sy += 8 * scale;
                }
                else
                {
                    int ox = (c - 32) % 16;
                    int oy = (c - 32) / 16;

                    if (scale > 1)
                    {
                        for (int i = 0; i < 8; i++)
                            for (int j = 0; j < 8; j++)
                                if (fontSprite.GetPixel(i + ox * 8, j + oy * 8).r > 0)
                                    for (int s = 0; s < scale; s++)
                                        for (int js = 0; js < scale; js++)
                                            Draw(x + sx + (i*scale) + s, y + sy + (j*scale) + js, p);
                    }
                    else
                    {
                        for (int i = 0; i < 8; i++)
                            for (int j = 0; j < 8; j++)
                                if (fontSprite.GetPixel(i + ox * 8, j + oy * 8).r > 0)								
                                    Draw(x + sx + i, y + sy + j, p);
                    }	

                    sx += 8 * scale;
                }
            }
        }

        private void ConstructFontSheet()
        {
            string data = "";
            data += "?Q`0001oOch0o01o@F40o0<AGD4090LAGD<090@A7ch0?00O7Q`0600>00000000";
            data += "O000000nOT0063Qo4d8>?7a14Gno94AA4gno94AaOT0>o3`oO400o7QN00000400";
            data += "Of80001oOg<7O7moBGT7O7lABET024@aBEd714AiOdl717a_=TH013Q>00000000";
            data += "720D000V?V5oB3Q_HdUoE7a9@DdDE4A9@DmoE4A;Hg]oM4Aj8S4D84@`00000000";
            data += "OaPT1000Oa`^13P1@AI[?g`1@A=[OdAoHgljA4Ao?WlBA7l1710007l100000000";
            data += "ObM6000oOfMV?3QoBDD`O7a0BDDH@5A0BDD<@5A0BGeVO5ao@CQR?5Po00000000";
            data += "Oc``000?Ogij70PO2D]??0Ph2DUM@7i`2DTg@7lh2GUj?0TO0C1870T?00000000";
            data += "70<4001o?P<7?1QoHg43O;`h@GT0@:@LB@d0>:@hN@L0@?aoN@<0O7ao0000?000";
            data += "OcH0001SOglLA7mg24TnK7ln24US>0PL24U140PnOgl0>7QgOcH0K71S0000A000";
            data += "00H00000@Dm1S007@DUSg00?OdTnH7YhOfTL<7Yh@Cl0700?@Ah0300700000000";
            data += "<008001QL00ZA41a@6HnI<1i@FHLM81M@@0LG81?O`0nC?Y7?`0ZA7Y300080000";
            data += "O`082000Oh0827mo6>Hn?Wmo?6HnMb11MP08@C11H`08@FP0@@0004@000000000";
            data += "00P00001Oab00003OcKP0006@6=PMgl<@440MglH@000000`@000001P00000000";
            data += "Ob@8@@00Ob@8@Ga13R@8Mga172@8?PAo3R@827QoOb@820@0O`0007`0000007P0";
            data += "O`000P08Od400g`<3V=P0G`673IP0`@3>1`00P@6O`P00g`<O`000GP800000000";
            data += "?P9PL020O`<`N3R0@E4HC7b0@ET<ATB0@@l6C4B0O`H3N7b0?P01L3R000000020";

            fontSprite = new Sprite(128, 48);
            int px = 0, py = 0;
            for (int b = 0; b < 1024; b += 4)
            {
                uint sym1 = (uint)data[b + 0] - 48;
                uint sym2 = (uint)data[b + 1] - 48;
                uint sym3 = (uint)data[b + 2] - 48;
                uint sym4 = (uint)data[b + 3] - 48;
                uint r = sym1 << 18 | sym2 << 12 | sym3 << 6 | sym4;

                for (int i = 0; i < 24; i++)
                {
                    byte k = (byte)((r & (1 << i)) > 0 ? 255 : 0);
                    fontSprite.SetPixel(px, py, new Pixel(k, k, k, k));
                    if (++py == 48) { px++; py = 0; }
                }
            }
        }
    }
}