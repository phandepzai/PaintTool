using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Linq;

#region GHÉP ẢNH
namespace ImageMerger
{
    public partial class MainForm : Form
    {
        private abstract class DraggableItem
        {
            public RectangleF Rect { get; set; }
            public bool IsPinnedTop { get; set; }
            public bool IsPinnedBottom { get; set; }
            public abstract void Draw(Graphics g);
        }

        private class DraggableImage : DraggableItem
        {
            public Image Image { get; set; }

            public override void Draw(Graphics g)
            {
                g.DrawImage(Image, Rect);
            }
        }

        private class DraggableText : DraggableItem
        {
            public string Text { get; set; }
            public Font Font { get; set; }
            public Color Color { get; set; }
            public StringFormat Format { get; set; } = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };

            public override void Draw(Graphics g)
            {
                using (SolidBrush brush = new SolidBrush(Color))
                {
                    g.DrawString(Text, Font, brush, Rect, Format);
                }
            }
        }

        private readonly List<DraggableItem> draggableItems = new List<DraggableItem>();
        private DraggableItem selectedItem = null;
        private PointF lastMousePos;
        private bool isDragging = false;
        private bool isResizing = false;
        private string resizeEdge = "";
        private const int EDGE_MARGIN = 8;
        private string lastSavedPath = null;
        private string lastOriginalFileName = "merged_image";
        private bool showVerticalGuide = false;
        private bool showHorizontalGuide = false;
        private int gridColumns = 10; // Số cột
        private int gridRows = 6;    // Số hàng
        private const float GRID_SNAP_THRESHOLD = 5f; // Độ nhạy “hít”
        private float showVerticalGuideX = 0f;
        private float showHorizontalGuideY = 0f;
        private ContextMenuStrip ctxMenu;
        private Timer neonTimer;
        private float neonOffset = 0f;
        private float neonHue = 0f; // góc màu H trong dải 0–360

        public MainForm()
        {
            InitializeComponent();
            InitLogic();
        }

        private void InitLogic()
        {
            cmbResolution.Items.AddRange(new object[]
            {
                "200x200",
                "500x500",
                "800x800",
                "1024x768 (Thấp)",
                "1280x720 (HD)",
                "1280x1024 (HD+)",
                "1920x1080 (Full HD)",
                "2560x1440 (2K-Cao)"
            });
            cmbResolution.SelectedIndex = 6;// Mặc định chọn 1920X1080

            btnSelect.Click += BtnSelect_Click;
            btnMerge.Click += BtnMerge_Click;
            btnAddText.Click += BtnAddText_Click; 

            picPreview.MouseDown += PicPreview_MouseDown;
            picPreview.MouseMove += PicPreview_MouseMove;
            picPreview.MouseUp += PicPreview_MouseUp;
            picPreview.Paint += PicPreview_Paint;
            picPreview.KeyDown += PicPreview_KeyDown;
            picPreview.Focus();

            // Context menu chuột phải
            ctxMenu = new ContextMenuStrip();
            ctxMenu.Items.Add("Đưa lên trên cùng", null, (s, e) => BringToFront());
            ctxMenu.Items.Add("Đưa xuống dưới cùng", null, (s, e) => SendToBack());
            ctxMenu.Items.Add(new ToolStripSeparator());
            ctxMenu.Items.Add("📌 Ghim lên trên cùng", null, (s, e) => PinItemToTop());
            ctxMenu.Items.Add("📍 Ghim xuống dưới cùng", null, (s, e) => PinItemToBottom());
            ctxMenu.Items.Add(new ToolStripSeparator());
            ctxMenu.Items.Add("✏️ Chỉnh sửa văn bản", null, (s, e) => EditTextIfApplicable()); // Thêm tùy chọn chỉnh sửa văn bản

            lblStatus.Cursor = Cursors.Hand;
            lblStatus.Click += LblStatus_Click;

            // --- Thêm mới: cho phép kéo-thả và dán ảnh ---
            picPreview.AllowDrop = true;
            picPreview.DragEnter += PicPreview_DragEnter;
            picPreview.DragDrop += PicPreview_DragDrop;

            this.KeyPreview = true;
            this.KeyDown += MainForm_KeyDown;

            // --- Hiệu ứng neon chạy quanh với màu RGB ---
            neonTimer = new Timer();
            neonTimer.Interval = 50; // tốc độ khung hình
            neonTimer.Tick += (s, e) =>
            {
                neonOffset += 2f;   // di chuyển nét sáng quanh khung
                if (neonOffset > 1000f) neonOffset = 0f;

                neonHue += 2f;      // đổi màu dần dần
                if (neonHue > 360f) neonHue = 0f;

                picPreview.Invalidate(); // cập nhật lại giao diện
            };
            neonTimer.Start();
        }

        private Color FromHsv(float hue, float saturation, float value)
        {
            if (saturation < 0f) saturation = 0f;
            if (saturation > 1f) saturation = 1f;
            if (value < 0f) value = 0f;
            if (value > 1f) value = 1f;

            float h = hue % 360f;
            if (h < 0f) h += 360f;

            float hf = h / 60f;
            int hi = (int)Math.Floor(hf) % 6;
            if (hi < 0) hi += 6;
            float f = hf - (float)Math.Floor(hf);

            int v = (int)(value * 255f);
            int p = (int)(value * (1f - saturation) * 255f);
            int q = (int)(value * (1f - f * saturation) * 255f);
            int t = (int)(value * (1f - (1f - f) * saturation) * 255f);

            v = Math.Min(255, Math.Max(0, v));
            p = Math.Min(255, Math.Max(0, p));
            q = Math.Min(255, Math.Max(0, q));
            t = Math.Min(255, Math.Max(0, t));
            switch (hi)
            {
                case 0: return Color.FromArgb(255, v, t, p);
                case 1: return Color.FromArgb(255, q, v, p);
                case 2: return Color.FromArgb(255, p, v, t);
                case 3: return Color.FromArgb(255, p, q, v);
                case 4: return Color.FromArgb(255, t, p, v);
                default: return Color.FromArgb(255, v, p, q);
            }
        }

        private void ShowStatus(string message, bool isError = false)
        {
            if (message.StartsWith("✅ Đã lưu"))
                lblStatus.ForeColor = Color.DarkGreen;
            else
                lblStatus.ForeColor = isError ? Color.Red : Color.Green;

            lblStatus.Text = message;
        }

        private void BtnSelect_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog
            {
                Multiselect = true,
                Filter = "Ảnh (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg|All Files (*.*)|*.*",
                //Chọn ảnh từ thư mục = @"D:\Non_Documents",
                //InitialDirectory = @"D:\Non_Documents"
            };

            // 💡 BƯỚC SỬA: Kiểm tra kết quả của ShowDialog()
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                // 💡 Lấy tên tệp gốc từ file đầu tiên được chọn
                if (ofd.FileNames.Length > 0)
                {
                    // Cập nhật biến lastOriginalFileName cho chức năng lưu tự động
                    lastOriginalFileName = Path.GetFileNameWithoutExtension(ofd.FileNames[0]);
                }

                foreach (string path in ofd.FileNames)
                {
                    try
                    {
                        Image img = Image.FromFile(path);
                        float newW = img.Width;
                        float newH = img.Height;

                        // Nếu ảnh lớn hơn khung thì co nhỏ lại một chút cho vừa
                        if (newW > picPreview.Width || newH > picPreview.Height)
                        {
                            float scale = Math.Min(
                                (float)picPreview.Width / img.Width,
                                (float)picPreview.Height / img.Height);

                            newW = img.Width * scale;
                            newH = img.Height * scale;
                        }

                        // Căn giữa
                        float x = (picPreview.Width - newW) / 2f;
                        float y = (picPreview.Height - newH) / 2f;

                        draggableItems.Add(new DraggableImage
                        {
                            Image = img,
                            Rect = new RectangleF(x, y, newW, newH)
                        });
                    }
                    catch (Exception ex)
                    {
                        ShowStatus($"❌ Lỗi khi tải ảnh {Path.GetFileName(path)}: {ex.Message}", true);
                    }
                }

                picPreview.Invalidate();
                ShowStatus("🖼️ Đã thêm ảnh mới thành công!"); // ✅ Thông báo trạng thái
            }
        }

        private void BtnAddText_Click(object sender, EventArgs e)
        {
            using (var form = new TextEditorForm())
            {
                if (form.ShowDialog() == DialogResult.OK && !string.IsNullOrWhiteSpace(form.ResultText))
                {
                    using (Graphics g = picPreview.CreateGraphics())
                    {
                        SizeF size = g.MeasureString(form.ResultText, form.ResultFont);
                        float x = (picPreview.Width - size.Width) / 2f;
                        float y = (picPreview.Height - size.Height) / 2f;

                        draggableItems.Add(new DraggableText
                        {
                            Text = form.ResultText,
                            Font = form.ResultFont,
                            Color = form.ResultColor,
                            Rect = new RectangleF(x, y, size.Width, size.Height)
                        });
                    }

                    picPreview.Invalidate();
                    ShowStatus("Đã thêm văn bản mới!");
                }
            }
        }

        private void EditTextIfApplicable()
        {
            if (selectedItem is DraggableText textItem)
            {
                using (var form = new TextEditorForm(textItem.Text, textItem.Font, textItem.Color))
                {
                    if (form.ShowDialog() == DialogResult.OK && !string.IsNullOrWhiteSpace(form.ResultText))
                    {
                        textItem.Text = form.ResultText;
                        textItem.Font = form.ResultFont;
                        textItem.Color = form.ResultColor;

                        using (Graphics g = picPreview.CreateGraphics())
                        {
                            SizeF size = g.MeasureString(form.ResultText, form.ResultFont);
                            textItem.Rect = new RectangleF(textItem.Rect.X, textItem.Rect.Y, size.Width, size.Height);
                        }

                        picPreview.Invalidate();
                        ShowStatus("Đã chỉnh sửa văn bản!");
                    }
                }
            }
        }

        private void LblStatus_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(lastSavedPath) && File.Exists(lastSavedPath))
            {
                // Mở thư mục và chọn file đó trong Explorer
                System.Diagnostics.Process.Start("explorer.exe", "/select,\"" + lastSavedPath + "\"");
            }
        }
        private void PicPreview_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;

            // 1️. Vẽ nền caro (checkerboard)
            DrawCheckerboard(g, picPreview.Width, picPreview.Height);

            // 2️. Vẽ item ghim xuống dưới cùng (nếu có)
            var bottomPinned = draggableItems.FirstOrDefault(i => i.IsPinnedBottom);
            if (bottomPinned != null)
            {
                bottomPinned.Draw(g);
            }

            // 3️. Vẽ các item bình thường (không ghim)
            foreach (var di in draggableItems.Where(i => !i.IsPinnedTop && !i.IsPinnedBottom))
            {
                di.Draw(g);

                // Vẽ viền trắng quanh item
                g.DrawRectangle(Pens.White, di.Rect.X, di.Rect.Y, di.Rect.Width, di.Rect.Height);
            }

            // 4️. Vẽ item ghim trên cùng (nếu có)
            var topPinned = draggableItems.FirstOrDefault(i => i.IsPinnedTop);
            if (topPinned != null)
            {
                topPinned.Draw(g);
            }

            // 5️. Hiển thị khung chọn item đang chọn
            if (selectedItem != null)
            {
                RectangleF r = selectedItem.Rect;

                // Tạo màu neon theo hue hiện tại
                Color neonColor = FromHsv(neonHue, 1f, 1f); // 100% saturation, 100% brightness

                // Vẽ viền chạy vòng quanh
                using (Pen neonPen = new Pen(neonColor, 2))
                {
                    neonPen.DashStyle = System.Drawing.Drawing2D.DashStyle.Custom;
                    neonPen.DashPattern = new float[] { 3, 3 };
                    neonPen.DashOffset = neonOffset;

                    g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                    g.DrawRectangle(neonPen, r.X, r.Y, r.Width, r.Height);
                }
            }

            // --- Hiển thị lưới chia vùng (mờ) ---
            using (Pen gridPen = new Pen(Color.FromArgb(60, Color.DeepSkyBlue), 1))
            {
                float colWidth = picPreview.Width / (float)gridColumns;
                float rowHeight = picPreview.Height / (float)gridRows;

                for (int i = 1; i < gridColumns; i++)
                {
                    float x = i * colWidth;
                    e.Graphics.DrawLine(gridPen, x, 0, x, picPreview.Height);
                }

                for (int j = 1; j < gridRows; j++)
                {
                    float y = j * rowHeight;
                    e.Graphics.DrawLine(gridPen, 0, y, picPreview.Width, y);
                }
            }

            // Hiển thị Grid line dọc khi kéo item
            if (showVerticalGuide)
            {
                using (Pen pen = new Pen(Color.FromArgb(150, Color.DeepSkyBlue), 1))
                {
                    pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                    float midX = showVerticalGuideX;
                    e.Graphics.DrawLine(pen, midX, 0, midX, picPreview.Height);
                }
            }
            // Hiển thị Grid line ngang khi kéo item
            if (showHorizontalGuide)
            {
                using (Pen pen = new Pen(Color.FromArgb(150, Color.DeepSkyBlue), 1))
                {
                    pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                    float midY = showHorizontalGuideY;
                    e.Graphics.DrawLine(pen, 0, midY, picPreview.Width, midY);
                }
            }
        }
        private void DrawCheckerboard(Graphics g, int width, int height)
        {
            int size = 15;
            using (SolidBrush light = new SolidBrush(Color.LightGray))
            using (SolidBrush dark = new SolidBrush(Color.Gray))
            {
                for (int y = 0; y < height; y += size)
                {
                    for (int x = 0; x < width; x += size)
                    {
                        bool isDark = ((x / size) + (y / size)) % 2 == 0;
                        g.FillRectangle(isDark ? dark : light, x, y, size, size);
                    }
                }
            }
        }

        private void PicPreview_MouseDown(object sender, MouseEventArgs e)
        {
            selectedItem = GetItemAtPoint(e.Location);

            if (e.Button == MouseButtons.Right)
            {
                if (selectedItem != null)
                {
                    ctxMenu.Show(Cursor.Position);
                }
                return;
            }

            if (selectedItem != null)
            {
                resizeEdge = GetResizeEdge(selectedItem, e.Location);
                if (resizeEdge != "")
                {
                    isResizing = true;
                }
                else
                {
                    isDragging = true;
                }

                lastMousePos = e.Location;
                draggableItems.Remove(selectedItem);
                draggableItems.Add(selectedItem);
            }
            else
            {
                isDragging = false;
                isResizing = false;
            }

            picPreview.Invalidate();
            picPreview.Focus();
        }

        private void PicPreview_MouseMove(object sender, MouseEventArgs e)
        {
            if (isResizing && selectedItem != null)
            {
                ResizeSelectedItem(e.Location);
                picPreview.Invalidate();
                return;
            }

            if (isDragging && selectedItem != null)
            {
                float dx = e.X - lastMousePos.X;
                float dy = e.Y - lastMousePos.Y;

                RectangleF rect = selectedItem.Rect;
                rect.X += dx;
                rect.Y += dy;

                showVerticalGuide = false;
                showHorizontalGuide = false;
                showVerticalGuideX = 0;
                showHorizontalGuideY = 0;

                float centerX = rect.X + rect.Width / 2f;
                float centerY = rect.Y + rect.Height / 2f;

                // --- Xử lý căn theo lưới ---
                float colWidth = picPreview.Width / (float)gridColumns;
                float rowHeight = picPreview.Height / (float)gridRows;

                // Căn theo các đường dọc
                for (int i = 1; i < gridColumns; i++)
                {
                    float gridX = i * colWidth;
                    if (Math.Abs(centerX - gridX) <= GRID_SNAP_THRESHOLD)
                    {
                        rect.X = gridX - rect.Width / 2f;
                        showVerticalGuide = true;
                        showVerticalGuideX = gridX;
                        break;
                    }
                }

                // Căn theo các đường ngang
                for (int j = 1; j < gridRows; j++)
                {
                    float gridY = j * rowHeight;
                    if (Math.Abs(centerY - gridY) <= GRID_SNAP_THRESHOLD)
                    {
                        rect.Y = gridY - rect.Height / 2f;
                        showHorizontalGuide = true;
                        showHorizontalGuideY = gridY;
                        break;
                    }
                }

                selectedItem.Rect = rect;
                lastMousePos = e.Location;
                picPreview.Invalidate();
                return;
            }

            var item = GetItemAtPoint(e.Location);
            if (item != null)
            {
                string edge = GetResizeEdge(item, e.Location);
                if (edge == "L" || edge == "R")
                    picPreview.Cursor = Cursors.SizeWE;
                else if (edge == "T" || edge == "B")
                    picPreview.Cursor = Cursors.SizeNS;
                else if (edge == "TL" || edge == "BR" || edge == "TR" || edge == "BL")
                    picPreview.Cursor = Cursors.SizeNWSE;
                else
                    picPreview.Cursor = Cursors.SizeAll;
            }
            else
            {
                picPreview.Cursor = Cursors.Default;
            }
        }

        private void PicPreview_MouseUp(object sender, MouseEventArgs e)
        {
            showVerticalGuide = false;
            showHorizontalGuide = false;
            isDragging = false;
            isResizing = false;
            resizeEdge = "";
        }

        private DraggableItem GetItemAtPoint(PointF pt)
        {
            // Ưu tiên kiểm tra item ghim trên cùng
            var topPinned = draggableItems.FirstOrDefault(i => i.IsPinnedTop);
            if (topPinned != null && topPinned.Rect.Contains(pt))
                return topPinned;

            // Sau đó kiểm tra các item bình thường
            for (int i = draggableItems.Count - 1; i >= 0; i--)
            {
                var item = draggableItems[i];
                if (!item.IsPinnedTop && !item.IsPinnedBottom && item.Rect.Contains(pt))
                    return item;
            }

            // Cuối cùng kiểm tra item ghim dưới cùng
            var bottomPinned = draggableItems.FirstOrDefault(i => i.IsPinnedBottom);
            if (bottomPinned != null && bottomPinned.Rect.Contains(pt))
                return bottomPinned;

            return null;
        }


        private string GetResizeEdge(DraggableItem item, PointF pt)
        {
            RectangleF r = item.Rect;

            bool left = Math.Abs(pt.X - r.Left) <= EDGE_MARGIN;
            bool right = Math.Abs(pt.X - r.Right) <= EDGE_MARGIN;
            bool top = Math.Abs(pt.Y - r.Top) <= EDGE_MARGIN;
            bool bottom = Math.Abs(pt.Y - r.Bottom) <= EDGE_MARGIN;

            if (top && left) return "TL";
            if (top && right) return "TR";
            if (bottom && left) return "BL";
            if (bottom && right) return "BR";
            if (left) return "L";
            if (right) return "R";
            if (top) return "T";
            if (bottom) return "B";
            return "";
        }

        private void ResizeSelectedItem(PointF mousePos)
        {
            if (selectedItem == null) return;

            RectangleF r = selectedItem.Rect;
            float dx = mousePos.X - lastMousePos.X;
            float dy = mousePos.Y - lastMousePos.Y;

            switch (resizeEdge)
            {
                case "L": r.X += dx; r.Width -= dx; break;
                case "R": r.Width += dx; break;
                case "T": r.Y += dy; r.Height -= dy; break;
                case "B": r.Height += dy; break;
                case "TL": r.X += dx; r.Y += dy; r.Width -= dx; r.Height -= dy; break;
                case "TR": r.Y += dy; r.Width += dx; r.Height -= dy; break;
                case "BL": r.X += dx; r.Width -= dx; r.Height += dy; break;
                case "BR": r.Width += dx; r.Height += dy; break;
            }

            if ((Control.ModifierKeys & Keys.Shift) == Keys.Shift && selectedItem is DraggableImage)
            {
                float ratio = ((DraggableImage)selectedItem).Image.Width / (float)((DraggableImage)selectedItem).Image.Height;
                r.Height = r.Width / ratio;
            }
            else if (selectedItem is DraggableText textItem)
            {
                // Thay đổi kích thước font để fit vào rect mới
                float newFontSize = Math.Min(r.Width / textItem.Text.Length * 2f, r.Height); // Ước lượng đơn giản
                textItem.Font = new Font(textItem.Font.FontFamily, newFontSize, textItem.Font.Style);
            }

            if (r.Width < 20) r.Width = 20;
            if (r.Height < 20) r.Height = 20;

            selectedItem.Rect = r;
            lastMousePos = mousePos;
        }

        private new void BringToFront()
        {
            if (selectedItem == null) return;
            draggableItems.Remove(selectedItem);
            draggableItems.Add(selectedItem);
            picPreview.Invalidate();
            ShowStatus("⬆️ Đã đưa lên trên cùng (Layer)");
        }

        private new void SendToBack()
        {
            if (selectedItem == null) return;
            draggableItems.Remove(selectedItem);
            draggableItems.Insert(0, selectedItem);
            picPreview.Invalidate();
            ShowStatus("⬇️ Đã đưa xuống dưới cùng (Layer)");
        }

        private void PinItemToTop()
        {
            if (selectedItem == null) return;

            foreach (var item in draggableItems)
            {
                item.IsPinnedTop = false;
            }

            selectedItem.IsPinnedTop = true;
            selectedItem.IsPinnedBottom = false;
            ShowStatus("📌 Ghim cố định lên trên cùng");
            picPreview.Invalidate();
        }

        private void PinItemToBottom()
        {
            if (selectedItem == null) return;

            foreach (var item in draggableItems)
            {
                item.IsPinnedBottom = false;
            }

            selectedItem.IsPinnedBottom = true;
            selectedItem.IsPinnedTop = false;
            ShowStatus("📍 Ghim cố định xuống dưới cùng");
            picPreview.Invalidate();
        }

        private void PicPreview_KeyDown(object sender, KeyEventArgs e)
        {
            if (selectedItem == null) return;

            if (e.Control && e.KeyCode == Keys.Up)
            {
                BringToFront();
                e.Handled = true;
                ShowStatus("⬆️ Đưa lên trên cùng");
            }
            else if (e.Control && e.KeyCode == Keys.Down)
            {
                SendToBack();
                e.Handled = true;
            }
            if (e.Control && e.Shift && e.KeyCode == Keys.Up)
            {
                PinItemToTop();
                e.Handled = true;
            }
            else if (e.Control && e.Shift && e.KeyCode == Keys.Down)
            {
                PinItemToBottom();
                e.Handled = true;
            }
        }

        // --- Mới thêm: Xử lý kéo-thả file ảnh ---
        private void PicPreview_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.Copy;
            else
                e.Effect = DragDropEffects.None;
        }

        private void PicPreview_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            // Gán tên tệp gốc từ tệp đầu tiên được kéo thả
            if (files.Length > 0)
            {
                lastOriginalFileName = Path.GetFileNameWithoutExtension(files[0]);
            }
            foreach (string path in files)
            {
                if (!File.Exists(path)) continue;
                string ext = Path.GetExtension(path).ToLower();
                if (ext != ".jpg" && ext != ".jpeg" && ext != ".png") continue;

                Image img = Image.FromFile(path);

                float newW = img.Width;
                float newH = img.Height;

                if (newW > picPreview.Width || newH > picPreview.Height)
                {
                    float scale = Math.Min(
                        (float)picPreview.Width / img.Width,
                        (float)picPreview.Height / img.Height);

                    newW = img.Width * scale;
                    newH = img.Height * scale;
                }

                float x = (picPreview.Width - newW) / 2f;
                float y = (picPreview.Height - newH) / 2f;

                draggableItems.Add(new DraggableImage
                {
                    Image = img,
                    Rect = new RectangleF(x, y, newW, newH)
                });
            }

            picPreview.Invalidate();
        }

        // --- Mới thêm: Dán ảnh từ clipboard ---
        private void MainForm_KeyDown(object sender, KeyEventArgs e)
        {
            // --- Xóa item đang chọn ---
            if (e.KeyCode == Keys.Delete)
            {
                if (selectedItem != null)
                {
                    draggableItems.Remove(selectedItem);
                    selectedItem = null;
                    picPreview.Invalidate();
                    ShowStatus("🗑️ Đã xóa được chọn", true);
                }
                e.Handled = true;
                return;
            }
            // Dán ảnh từ clipboard
            if (e.Control && e.KeyCode == Keys.V)
            {
                if (Clipboard.ContainsImage())
                {
                    Image img = Clipboard.GetImage();

                    float scale = Math.Min(
                        (float)picPreview.Width / img.Width,
                        (float)picPreview.Height / img.Height);

                    float newW = img.Width * scale;
                    float newH = img.Height * scale;
                    float x = (picPreview.Width - newW) / 2f;
                    float y = (picPreview.Height - newH) / 2f;

                    draggableItems.Add(new DraggableImage
                    {
                        Image = img,
                        Rect = new RectangleF(x, y, newW, newH)
                    });

                    picPreview.Invalidate();
                    ShowStatus("✅ Đã dán ảnh từ clipboard!");
                    e.Handled = true;
                }
                return;
            }

            // --- Di chuyển item đang chọn bằng phím mũi tên ---
            if (selectedItem == null) return;
            float moveStep = 1f;
            if (e.Shift) moveStep = 10f;

            switch (e.KeyCode)
            {
                case Keys.Left:
                    selectedItem.Rect = new RectangleF(
                        selectedItem.Rect.X - moveStep,
                        selectedItem.Rect.Y,
                        selectedItem.Rect.Width,
                        selectedItem.Rect.Height);
                    ShowStatus($"← Di chuyển sang trái {moveStep}px");
                    e.Handled = true;
                    break;

                case Keys.Right:
                    selectedItem.Rect = new RectangleF(
                        selectedItem.Rect.X + moveStep,
                        selectedItem.Rect.Y,
                        selectedItem.Rect.Width,
                        selectedItem.Rect.Height);
                    ShowStatus($"→ Di chuyển sang phải {moveStep}px");
                    e.Handled = true;
                    break;

                case Keys.Up:
                    if (e.Control)
                    {
                        BringToFront();
                        ShowStatus("⬆️ Đưa lên trên cùng");
                    }
                    else
                    {
                        selectedItem.Rect = new RectangleF(
                            selectedItem.Rect.X,
                            selectedItem.Rect.Y - moveStep,
                            selectedItem.Rect.Width,
                            selectedItem.Rect.Height);
                        ShowStatus($"↑ Di chuyển lên trên {moveStep}px");
                    }
                    e.Handled = true;
                    break;

                case Keys.Down:
                    if (e.Control)
                    {
                        SendToBack();
                        ShowStatus("⬇️ Đưa xuống dưới cùng");
                    }
                    else
                    {
                        selectedItem.Rect = new RectangleF(
                            selectedItem.Rect.X,
                            selectedItem.Rect.Y + moveStep,
                            selectedItem.Rect.Width,
                            selectedItem.Rect.Height);
                        ShowStatus($"↓ Di chuyển xuống dưới {moveStep}px");
                    }
                    e.Handled = true;
                    break;
            }

            picPreview.Invalidate();
        }

        private void BtnMerge_Click(object sender, EventArgs e)
        {
            if (draggableItems.Count == 0)
            {
                ShowStatus("⚠️ Hãy chọn ảnh trước khi ghép.", true);
                return;
            }

            Bitmap result = new Bitmap(picPreview.Width, picPreview.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            using (Graphics g = Graphics.FromImage(result))
            {
                g.Clear(Color.Transparent);
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;

                // Xử lý vẽ item
                var bottomPinned = draggableItems.FirstOrDefault(i => i.IsPinnedBottom);
                var normalItems = draggableItems.Where(i => !i.IsPinnedTop && !i.IsPinnedBottom).ToList();
                var topPinned = draggableItems.FirstOrDefault(i => i.IsPinnedTop);

                if (bottomPinned != null)
                    bottomPinned.Draw(g);

                foreach (var di in normalItems)
                    di.Draw(g);

                if (topPinned != null)
                    topPinned.Draw(g);

            }

            // Xử lý thay đổi độ phân giải
            int w = 1920, h = 1080;
            string res = cmbResolution.SelectedItem.ToString();
            if (res.Contains("200x200")) { w = 200; h = 200; }
            else if (res.Contains("500X500")) { w = 500; h = 500; }
            else if (res.Contains("800X800")) { w = 800; h = 800; }
            else if (res.Contains("1024x768")) { w = 1024; h = 768; }
            else if (res.Contains("1280x720")) { w = 1280; h = 720; }
            else if (res.Contains("1280x1024")) { w = 1280; h = 1024; }
            else if (res.Contains("1920x1080")) { w = 1920; h = 1080; }
            else if (res.Contains("2560x1440")) { w = 2560; h = 1440; }

            result = new Bitmap(result, new Size(w, h));

            SaveFileDialog sfd = new SaveFileDialog
            {
                Filter = "PNG|*.png|JPEG|*.jpg|BMP|*.bmp|GIF|*.gif|TIFF|*.tiff|All Files(*.*) | *.* ",
                InitialDirectory = @"D:\Non_Documents"
            };

            // 💡 Đặt tên tệp mặc định: [Tên_File_Gốc]_new.png
            string baseName = lastOriginalFileName + "_new";
            string initialExtension = ".png";
            sfd.FileName = baseName + initialExtension;

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                if (string.IsNullOrWhiteSpace(sfd.FileName))
                {
                    ShowStatus("❌ Không thể lưu ảnh: đường dẫn trống!", true);
                    return;
                }

                string saveDir = Path.GetDirectoryName(sfd.FileName);
                if (!Directory.Exists(saveDir))
                {
                    Directory.CreateDirectory(saveDir);
                }

                // 💡 BẮT ĐẦU LOGIC TỰ ĐỘNG TĂNG SỐ ĐẾM (AUTO-INCREMENT)
                string directory = Path.GetDirectoryName(sfd.FileName);
                string extension = Path.GetExtension(sfd.FileName).ToLower();
                string userChosenName = Path.GetFileNameWithoutExtension(sfd.FileName);

                // Chỉ áp dụng logic tự động tăng số nếu người dùng giữ nguyên tiền tố tên mặc định
                if (userChosenName.StartsWith(lastOriginalFileName + "_new"))
                {
                    string finalBaseName = lastOriginalFileName + "_new";
                    int count = 0;
                    string tempFileName = userChosenName;
                    string filePath = Path.Combine(directory, tempFileName + extension);

                    // Vòng lặp kiểm tra và tìm tên tệp duy nhất
                    while (File.Exists(filePath))
                    {
                        count++;
                        // Tạo tên tệp mới: [TênGốc]_new(count).png
                        tempFileName = finalBaseName + $"({count})";
                        filePath = Path.Combine(directory, tempFileName + extension);
                    }

                    // Cập nhật tên tệp cuối cùng để lưu
                    sfd.FileName = filePath;
                }
                // KẾT THÚC LOGIC TỰ ĐỘNG TĂNG SỐ ĐẾM

                try
                {
                    string ext = Path.GetExtension(sfd.FileName).ToLower();
                    switch (ext)
                    {
                        case ".png":
                            result.Save(sfd.FileName, System.Drawing.Imaging.ImageFormat.Png);
                            break;
                        case ".jpg":
                        case ".jpeg":
                            result.Save(sfd.FileName, System.Drawing.Imaging.ImageFormat.Jpeg);
                            break;
                        case ".bmp":
                            result.Save(sfd.FileName, System.Drawing.Imaging.ImageFormat.Bmp);
                            break;
                        case ".gif":
                            result.Save(sfd.FileName, System.Drawing.Imaging.ImageFormat.Gif);
                            break;
                        case ".tiff":
                            result.Save(sfd.FileName, System.Drawing.Imaging.ImageFormat.Tiff);
                            break;
                        default:
                            result.Save(sfd.FileName, System.Drawing.Imaging.ImageFormat.Png);
                            break;
                    }

                    lastSavedPath = sfd.FileName;
                    ShowStatus($"✅ Đã lưu thành công: {lastSavedPath}");
                }
                catch (Exception ex)
                {
                    ShowStatus($"❌ Lỗi khi lưu ảnh: {ex.Message}", true);
                }
            }
        }
    }
}
#endregion