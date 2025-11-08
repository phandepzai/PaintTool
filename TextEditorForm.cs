using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;
using System.Linq;
using System.Windows.Forms;

namespace ImageMerger
{
    public partial class TextEditorForm : Form
    {
        public string ResultText { get; private set; }
        public Font ResultFont { get; private set; }
        public Color ResultColor { get; private set; }

        private readonly string[] commonSizes = { "8", "9", "10", "11", "12", "14", "16", "18", "20", "22", "24", "26", "28", "36", "48", "72" };

        private class ColorItem
        {
            public string Name { get; set; }
            public Color Color { get; set; }
            public override string ToString() => Name;
        }

        private readonly List<ColorItem> presetColors = new List<ColorItem>
        {
            new ColorItem { Name = "Đen",       Color = Color.FromArgb(0, 0, 0) },
            new ColorItem { Name = "Trắng",     Color = Color.FromArgb(255, 255, 255) },
            new ColorItem { Name = "Đỏ",        Color = Color.FromArgb(255, 0, 0) },
            new ColorItem { Name = "Cam",       Color = Color.FromArgb(255, 165, 0) },
            new ColorItem { Name = "Vàng",      Color = Color.FromArgb(255, 255, 0) },
            new ColorItem { Name = "Xanh lá",   Color = Color.FromArgb(0, 128, 0) },
            new ColorItem { Name = "Xanh nhạt", Color = Color.FromArgb(0, 255, 0) },
            new ColorItem { Name = "Xanh dương",Color = Color.FromArgb(0, 0, 255) },
            new ColorItem { Name = "Cyan",      Color = Color.FromArgb(0, 255, 255) },
            new ColorItem { Name = "Hồng",      Color = Color.FromArgb(255, 0, 255) },
            new ColorItem { Name = "Tím",       Color = Color.FromArgb(128, 0, 128) },
            new ColorItem { Name = "Hồng nhạt", Color = Color.FromArgb(255, 192, 203) },
            new ColorItem { Name = "Nâu",       Color = Color.FromArgb(165, 42, 42) },
            new ColorItem { Name = "Xám",       Color = Color.FromArgb(128, 128, 128) },
            new ColorItem { Name = "Bạc",       Color = Color.FromArgb(192, 192, 192) },
            new ColorItem { Name = "Đỏ tươi",   Color = Color.FromArgb(255, 99, 71) },
            new ColorItem { Name = "Đào",       Color = Color.FromArgb(255, 218, 185) },
            new ColorItem { Name = "Xanh nhạt", Color = Color.FromArgb(173, 216, 230) },
            new ColorItem { Name = "Xanh lá nhạt", Color = Color.FromArgb(144, 238, 144) },
            new ColorItem { Name = "Tím nhạt",  Color = Color.FromArgb(221, 160, 221) },
        };

        public TextEditorForm(string initialText = "Text Here", Font initialFont = null, Color initialColor = default)
        {
            InitializeComponent();

            txtInput.Text = initialText;
            ResultFont = initialFont ?? new Font("Arial", 24f, FontStyle.Regular);
            ResultColor = initialColor != default(Color) ? initialColor : Color.White;

            LoadFonts();
            LoadSizes();
            LoadPresetColors();
            UpdateUIFromFont();
            UpdatePreview();
        }

        private void LoadFonts()
        {
            using (var fonts = new InstalledFontCollection())
                cmbFont.Items.AddRange(fonts.Families.OrderBy(f => f.Name).Select(f => f.Name).ToArray());
        }

        private void LoadSizes() => cmbSize.Items.AddRange(commonSizes);

        private void LoadPresetColors()
        {
            cmbColor.DisplayMember = "Name";
            cmbColor.Items.AddRange(presetColors.ToArray());
        }

        private void UpdateUIFromFont()
        {
            cmbFont.SelectedItem = ResultFont.FontFamily.Name;
            string sizeStr = ResultFont.Size.ToString("0");
            cmbSize.Text = commonSizes.Contains(sizeStr) ? sizeStr : sizeStr;

            pnlColorSwatch.BackColor = ResultColor;

            int closestIndex = 0, minDistance = int.MaxValue;
            for (int i = 0; i < presetColors.Count; i++)
            {
                int d = ColorDistance(ResultColor, presetColors[i].Color);
                if (d < minDistance) { minDistance = d; closestIndex = i; }
            }
            cmbColor.SelectedIndex = minDistance < 100 ? closestIndex : -1;
        }

        private void UpdatePreview()
        {
            lblPreview.Text = string.IsNullOrWhiteSpace(txtInput.Text) ? "Xem trước..." : txtInput.Text;
            lblPreview.Font = ResultFont;
            lblPreview.ForeColor = ResultColor;
        }

        private int ColorDistance(Color a, Color b)
        {
            int dr = a.R - b.R, dg = a.G - b.G, db = a.B - b.B;
            return dr * dr + dg * dg + db * db;
        }

        // === EVENT HANDLERS ===
        private void txtInput_TextChanged(object sender, EventArgs e) => UpdatePreview();

        private void cmbFont_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbFont.SelectedItem != null)
            {
                ResultFont = new Font(cmbFont.SelectedItem.ToString(), ResultFont.Size, ResultFont.Style);
                UpdatePreview();
            }
        }

        private void cmbSize_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (float.TryParse(cmbSize.Text, out float size) && size > 0)
            {
                ResultFont = new Font(ResultFont.FontFamily, size, ResultFont.Style);
                UpdatePreview();
            }
        }

        private void cmbColor_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbColor.SelectedIndex >= 0 && cmbColor.SelectedItem is ColorItem item)
            {
                ResultColor = item.Color;
                pnlColorSwatch.BackColor = ResultColor;
                UpdatePreview();
            }
        }

        private void cmbColor_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0 || e.Index >= presetColors.Count) return;
            var item = presetColors[e.Index];
            e.DrawBackground();
            using (var b = new SolidBrush(item.Color))
                e.Graphics.FillRectangle(b, e.Bounds.X + 2, e.Bounds.Y + 2, 16, 16);
            e.Graphics.DrawRectangle(Pens.Black, e.Bounds.X + 2, e.Bounds.Y + 2, 16, 16);
            using (var tb = new SolidBrush(e.ForeColor))
                e.Graphics.DrawString(item.Name, e.Font, tb, e.Bounds.X + 22, e.Bounds.Y + 2);
            e.DrawFocusRectangle();
        }

        private void btnMoreColors_Click(object sender, EventArgs e)
        {
            using (var picker = new ColorPickerForm(ResultColor))
            {
                if (picker.ShowDialog() == DialogResult.OK)
                {
                    ResultColor = picker.SelectedColor;
                    pnlColorSwatch.BackColor = ResultColor;
                    UpdatePreview();

                    int closestIndex = 0, minDistance = int.MaxValue;
                    for (int i = 0; i < presetColors.Count; i++)
                    {
                        int d = ColorDistance(ResultColor, presetColors[i].Color);
                        if (d < minDistance) { minDistance = d; closestIndex = i; }
                    }
                    cmbColor.SelectedIndex = minDistance < 100 ? closestIndex : -1;
                }
            }
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            ResultText = txtInput.Text;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}