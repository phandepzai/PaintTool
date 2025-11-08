using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace ImageMerger
{
    public partial class ColorPickerForm : Form
    {
        public Color SelectedColor { get; private set; } = Color.White;

        private float hue = 0f;
        private float saturation = 1f;
        private float value = 1f;

        private Point? svPoint = null;
        private bool isDraggingSV = false;
        private bool isDraggingHue = false;

        public ColorPickerForm(Color initialColor)
        {
            InitializeComponent();

            // BẬT DOUBLE BUFFERING CHO 2 PANEL
            pnlSV.GetType().GetProperty("DoubleBuffered",
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.NonPublic)?
                .SetValue(pnlSV, true, null);

            pnlHue.GetType().GetProperty("DoubleBuffered",
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.NonPublic)?
                .SetValue(pnlHue, true, null);

            SelectedColor = initialColor;
            HsvFromColor(initialColor);
            UpdateAll();
        }

        private void PnlSV_Paint(object sender, PaintEventArgs e)
        {
            using (Bitmap bmp = new Bitmap(220, 220))
            using (Graphics g = Graphics.FromImage(bmp))
            {
                for (int x = 0; x < 220; x++)
                    for (int y = 0; y < 220; y++)
                    {
                        float s = x / 219f;
                        float v = 1f - (y / 219f);
                        bmp.SetPixel(x, y, FromHsv(hue, s, v));
                    }
                e.Graphics.DrawImage(bmp, 0, 0);
            }

            if (svPoint.HasValue)
            {
                int cx = (int)(saturation * 219);
                int cy = (int)((1f - value) * 219);
                using (Pen pen = new Pen(Color.FromArgb(200, Color.White), 2))
                using (Pen pen2 = new Pen(Color.FromArgb(200, Color.Black), 2))
                {
                    e.Graphics.DrawEllipse(pen2, cx - 8, cy - 8, 16, 16);
                    e.Graphics.DrawEllipse(pen, cx - 7, cy - 7, 14, 14);
                }
            }
        }

        private void PnlHue_Paint(object sender, PaintEventArgs e)
        {
            using (Bitmap bmp = new Bitmap(30, 220))
            using (Graphics g = Graphics.FromImage(bmp))
            {
                for (int y = 0; y < 220; y++)
                {
                    float h = (y / 219f) * 360f;
                    Color c = FromHsv(h, 1f, 1f);
                    using (Brush b = new SolidBrush(c))
                        g.FillRectangle(b, 0, y, 30, 1);
                }
                e.Graphics.DrawImage(bmp, 0, 0);
            }

            int hy = (int)(hue / 360f * 219);
            using (Pen pen = new Pen(Color.White, 3))
            using (Pen pen2 = new Pen(Color.Black, 1))
            {
                e.Graphics.DrawRectangle(pen2, 0, hy - 1, 29, 2);
                e.Graphics.DrawRectangle(pen, 1, hy, 27, 0);
            }
        }

        private void pnlSV_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left) { isDraggingSV = true; UpdateSV(e.Location); }
        }

        private void pnlSV_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDraggingSV) UpdateSV(e.Location);
        }

        private void pnlSV_MouseUp(object sender, MouseEventArgs e)
        {
            isDraggingSV = false;
        }

        private void pnlHue_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left) { isDraggingHue = true; UpdateHue(e.Location); }
        }

        private void pnlHue_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDraggingHue) UpdateHue(e.Location);
        }

        private void pnlHue_MouseUp(object sender, MouseEventArgs e)
        {
            isDraggingHue = false;
        }

        private void UpdateSV(Point p)
        {
            if (p.X < 0 || p.X > 219 || p.Y < 0 || p.Y > 219) return;
            saturation = p.X / 219f;
            value = 1f - (p.Y / 219f);
            svPoint = p;
            UpdateColor();
        }

        private void UpdateHue(Point p)
        {
            if (p.Y < 0 || p.Y > 219) return;
            hue = (p.Y / 219f) * 360f;
            pnlSV.Invalidate();
            UpdateColor();
        }

        private void UpdateColor()
        {
            SelectedColor = FromHsv(hue, saturation, value);
            txtHex.Text = "#" + SelectedColor.R.ToString("X2") + SelectedColor.G.ToString("X2") + SelectedColor.B.ToString("X2");
            pnlPreview.BackColor = SelectedColor;
            lblPrev.ForeColor = GetContrastingColor(SelectedColor);
        }

        private void UpdateAll()
        {
            svPoint = new Point((int)(saturation * 219), (int)((1f - value) * 219));
            pnlSV.Invalidate();
            pnlHue.Invalidate();
            UpdateColor();
        }

        private void txtHex_TextChanged(object sender, EventArgs e)
        {
            if (TryParseHex(txtHex.Text, out Color c))
            {
                SelectedColor = c;
                HsvFromColor(c);
                UpdateAll();
            }
        }

        private void HsvFromColor(Color c)
        {
            float r = c.R / 255f, g = c.G / 255f, b = c.B / 255f;
            float max = Math.Max(r, Math.Max(g, b));
            float min = Math.Min(r, Math.Min(g, b));
            float delta = max - min;

            hue = 0;
            if (delta > 0)
            {
                if (max == r) hue = ((g - b) / delta) % 6f;
                else if (max == g) hue = (b - r) / delta + 2f;
                else hue = (r - g) / delta + 4f;
                hue *= 60f;
                if (hue < 0) hue += 360f;
            }

            saturation = max == 0 ? 0 : delta / max;
            value = max;
        }

        private Color FromHsv(float h, float s, float v)
        {
            h = h % 360f;
            float c = v * s;
            float x = c * (1 - Math.Abs((h / 60f) % 2 - 1));
            float m = v - c;

            float r = 0, g = 0, b = 0;
            if (h >= 0 && h < 60) { r = c; g = x; }
            else if (h < 120) { r = x; g = c; }
            else if (h < 180) { g = c; b = x; }
            else if (h < 240) { g = x; b = c; }
            else if (h < 300) { r = x; b = c; }
            else { r = c; b = x; }

            return Color.FromArgb(
                (int)((r + m) * 255),
                (int)((g + m) * 255),
                (int)((b + m) * 255)
            );
        }

        private bool TryParseHex(string hex, out Color color)
        {
            color = Color.White;
            if (!hex.StartsWith("#")) hex = "#" + hex;
            if (hex.Length != 7) return false;
            var c = ColorTranslator.FromHtml(hex);
            if (c == Color.Empty) return false;
            color = c;
            return true;
        }

        private Color GetContrastingColor(Color c)
        {
            int brightness = (int)Math.Sqrt(c.R * c.R * 0.241 + c.G * c.G * 0.691 + c.B * c.B * 0.068);
            return brightness > 130 ? Color.Black : Color.White;
        }
    }
}