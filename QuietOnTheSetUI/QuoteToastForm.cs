using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;

namespace QuietOnTheSetUI
{
    internal sealed class QuoteToastForm : Form
    {
        static readonly string[] Quotes = new string[]
        {
            "千山鸟飞绝，万径人踪灭。\n孤舟蓑笠翁，独钓寒江雪",
            "蝉噪林逾静，鸟鸣山更幽",
            "非淡泊无以明志，非宁静无以致远",
            "此时无声胜有声",
            "结庐在人境，而无车马喧。\n问君何能尔，心远地自偏",
            "致虚极，守静笃",
            "万籁此俱寂，惟闻钟磬音",
            "把世界调成静音，聆听名刀破碎的声音",
            "他们的质疑声好大啊",
            "嘘……",
            "我嫉妒沉默，因为我始终喧哗"
        };

        static readonly Random Rng = new Random();
        static readonly object Sync = new object();
        static readonly StringFormat Typographic;

        static QuoteToastForm current;
        static int lastIndex = -1;

        const int VisibleMs = 12000;
        const string LawBody = "违反噪声防治规定产生社会生活噪声，经劝阻、调解和处理未能制止，继续干扰他人正常生活的，处五日以下拘留或者一千元以下罚款；情节严重的，处五日以上十日以下拘留，可以并处一千元以下罚款。";
        const string LawCite = "——《中华人民共和国治安管理处罚法（第八十八条）》";

        readonly Timer closeTimer;
        readonly string quote;
        readonly Font quoteFont;
        readonly Font kickerFont;
        readonly Font lawFont;
        readonly Font citationFont;
        readonly string[] lines;
        readonly StringFormat wrapFormat;

        static QuoteToastForm()
        {
            Typographic = (StringFormat)StringFormat.GenericTypographic.Clone();
            Typographic.Alignment = StringAlignment.Center;
            Typographic.LineAlignment = StringAlignment.Near;
            Typographic.FormatFlags |= StringFormatFlags.MeasureTrailingSpaces;
        }

        QuoteToastForm(string quote)
        {
            this.quote = quote;
            lines = LayoutLines(quote);
            wrapFormat = (StringFormat)StringFormat.GenericTypographic.Clone();
            wrapFormat.Alignment = StringAlignment.Center;
            wrapFormat.LineAlignment = StringAlignment.Near;
            wrapFormat.Trimming = StringTrimming.None;

            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.Manual;
            ShowInTaskbar = false;
            TopMost = true;
            KeyPreview = true;
            AutoScaleMode = AutoScaleMode.None;
            BackColor = Color.FromArgb(16, 15, 14);
            ForeColor = Color.FromArgb(244, 240, 230);
            DoubleBuffered = true;
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);
            UpdateStyles();

            var screen = Screen.FromPoint(Cursor.Position);
            Bounds = screen.Bounds;

            float dpi;
            using (var g = CreateGraphics())
            {
                dpi = g.DpiY <= 0 ? 96f : g.DpiY;
            }

            int shortest = ShortestLineLength(lines);
            int longest = LongestLineLength(lines);
            float quotePx;
            if (longest <= 4)
            {
                quotePx = Math.Min(screen.Bounds.Height * 0.16f, 168f);
            }
            else if (longest <= 10 && lines.Length == 1)
            {
                quotePx = Math.Min(screen.Bounds.Height * 0.078f, 86f);
            }
            else if (lines.Length >= 2 || longest >= 14)
            {
                quotePx = Math.Min(screen.Bounds.Height * 0.052f, 58f);
            }
            else
            {
                quotePx = Math.Min(screen.Bounds.Height * 0.062f, 70f);
            }
            if (shortest <= 2 && lines.Length == 1)
            {
                quotePx = Math.Min(screen.Bounds.Height * 0.16f, 168f);
            }

            float quotePt = quotePx * 72f / dpi;
            float kickerPt = Math.Max(11f, quotePt * 0.28f);
            float lawPx = Math.Min(Math.Max(screen.Bounds.Height * 0.024f, 22f), 30f);
            float lawPt = lawPx * 72f / dpi;
            float citePt = Math.Max(12.5f, lawPt * 0.86f);

            quoteFont = CreateFont(QuoteFamilies, quotePt, FontStyle.Regular);
            kickerFont = CreateFont(SansFamilies, kickerPt, FontStyle.Regular);
            lawFont = CreateFont(SansFamilies, lawPt, FontStyle.Bold);
            citationFont = CreateFont(SansFamilies, citePt, FontStyle.Bold);

            Click += (s, e) => Close();
            KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Escape || e.KeyCode == Keys.Enter || e.KeyCode == Keys.Space)
                {
                    Close();
                }
            };

            closeTimer = new Timer();
            closeTimer.Interval = VisibleMs;
            closeTimer.Tick += (s, e) =>
            {
                closeTimer.Stop();
                Close();
            };
        }

        protected override bool ShowWithoutActivation
        {
            get { return true; }
        }

        protected override CreateParams CreateParams
        {
            get
            {
                var cp = base.CreateParams;
                cp.ExStyle |= 0x08000000; // WS_EX_NOACTIVATE
                cp.ExStyle |= 0x00000080; // WS_EX_TOOLWINDOW
                cp.ExStyle |= 0x00000008; // WS_EX_TOPMOST
                return cp;
            }
        }

        public static void ShowRandom()
        {
            lock (Sync)
            {
                if (current != null && !current.IsDisposed && current.Visible)
                {
                    return;
                }

                int index = Rng.Next(Quotes.Length);
                if (Quotes.Length > 1)
                {
                    int guard = 0;
                    while (index == lastIndex && guard < 8)
                    {
                        index = Rng.Next(Quotes.Length);
                        guard++;
                    }
                }
                lastIndex = index;

                var previous = current;
                var toast = new QuoteToastForm(Quotes[index]);
                current = toast;
                toast.FormClosed += (s, e) =>
                {
                    lock (Sync)
                    {
                        if (current == toast)
                        {
                            current = null;
                        }
                    }
                    toast.Dispose();
                };

                if (previous != null && !previous.IsDisposed)
                {
                    previous.Close();
                }

                toast.Show();
                toast.closeTimer.Start();
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;
            g.Clear(BackColor);
            g.TextRenderingHint = TextRenderingHint.AntiAlias;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;

            var ivory = Color.FromArgb(244, 240, 230);
            var muted = Color.FromArgb(168, 160, 148);
            var accent = Color.FromArgb(196, 84, 68);
            var warning = Color.FromArgb(255, 45, 40);

            float cx = ClientSize.Width / 2f;
            float quoteLineHeight = quoteFont.GetHeight(g) * (lines.Length == 1 && LongestLineLength(lines) <= 4 ? 1.05f : 1.72f);
            float blockHeight = quoteLineHeight * lines.Length;
            float kickerH = kickerFont.GetHeight(g);
            float ruleW = Math.Max(120f, Math.Min(ClientSize.Width * 0.18f, 280f));
            float gap = Math.Max(22f, quoteFont.Size * 0.55f);
            float lawWidth = Math.Min(ClientSize.Width * 0.78f, 1080f);
            SizeF lawSize = MeasureWrapped(g, LawBody, lawFont, lawWidth);
            SizeF citeSize = MeasureWrapped(g, LawCite, citationFont, lawWidth);
            float afterQuote = Math.Max(gap * 1.35f, 36f);
            float citeGap = Math.Max(14f, lawFont.GetHeight(g) * 0.55f);

            float total = kickerH + gap + 2f + gap + blockHeight + afterQuote + lawSize.Height + citeGap + citeSize.Height;
            float y = (ClientSize.Height - total) / 2f;
            if (y < 36f) y = 36f;

            using (var mutedBrush = new SolidBrush(muted))
            using (var ivoryBrush = new SolidBrush(ivory))
            using (var warningBrush = new SolidBrush(warning))
            using (var rulePen = new Pen(accent, 2f))
            {
                DrawTracked(g, "音量上限已锁定", kickerFont, mutedBrush, cx, y, 0.62f);
                y += kickerH + gap;

                float ruleY = y + 1f;
                g.DrawLine(rulePen, cx - ruleW / 2f, ruleY, cx + ruleW / 2f, ruleY);
                y += 2f + gap;

                float tracking = LongestLineLength(lines) <= 4 ? 0.12f : 0.045f;
                foreach (var line in lines)
                {
                    DrawTracked(g, line, quoteFont, ivoryBrush, cx, y, tracking);
                    y += quoteLineHeight;
                }

                y += afterQuote;
                var lawBox = new RectangleF(cx - lawWidth / 2f, y, lawWidth, lawSize.Height + 4f);
                g.DrawString(LawBody, lawFont, warningBrush, lawBox, wrapFormat);
                y += lawSize.Height + citeGap;
                var citeBox = new RectangleF(cx - lawWidth / 2f, y, lawWidth, citeSize.Height + 4f);
                g.DrawString(LawCite, citationFont, warningBrush, citeBox, wrapFormat);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (closeTimer != null)
                {
                    closeTimer.Stop();
                    closeTimer.Dispose();
                }
                if (quoteFont != null) quoteFont.Dispose();
                if (kickerFont != null) kickerFont.Dispose();
                if (lawFont != null) lawFont.Dispose();
                if (citationFont != null) citationFont.Dispose();
                if (wrapFormat != null) wrapFormat.Dispose();
            }
            base.Dispose(disposing);
        }

        static readonly string[] QuoteFamilies = new string[]
        {
            "KaiTi", "STKaiti", "华文楷体", "FangSong", "STFangsong", "华文仿宋",
            "Microsoft YaHei", "微软雅黑", "SimSun", "宋体"
        };

        static readonly string[] SansFamilies = new string[]
        {
            "Microsoft YaHei UI", "Microsoft YaHei", "微软雅黑", "Segoe UI", "SimHei"
        };

        static SizeF MeasureWrapped(Graphics g, string text, Font font, float width)
        {
            using (var format = (StringFormat)StringFormat.GenericTypographic.Clone())
            {
                format.Alignment = StringAlignment.Center;
                format.LineAlignment = StringAlignment.Near;
                format.Trimming = StringTrimming.None;
                return g.MeasureString(text, font, new SizeF(width, 4000f), format);
            }
        }

        static string[] LayoutLines(string quote)
        {
            if (string.IsNullOrEmpty(quote))
            {
                return new string[] { string.Empty };
            }
            if (quote.IndexOf('\n') >= 0)
            {
                return quote.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            }
            // Break once at a mid-sentence pause so a long line can sit as a couplet.
            int comma = quote.IndexOf('，');
            if (comma > 0 && comma < quote.Length - 1 && quote.Length >= 12)
            {
                return new string[]
                {
                    quote.Substring(0, comma + 1),
                    quote.Substring(comma + 1)
                };
            }
            int period = quote.IndexOf('。');
            if (period > 0 && period < quote.Length - 1)
            {
                return new string[]
                {
                    quote.Substring(0, period + 1),
                    quote.Substring(period + 1)
                };
            }
            return new string[] { quote };
        }

        static int VisibleLength(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return 0;
            }
            int n = 0;
            foreach (var ch in text)
            {
                if (ch != '\n' && ch != '\r')
                {
                    n++;
                }
            }
            return n;
        }

        static int LongestLineLength(string[] lines)
        {
            int max = 0;
            foreach (var line in lines)
            {
                int n = VisibleLength(line);
                if (n > max) max = n;
            }
            return max;
        }

        static int ShortestLineLength(string[] lines)
        {
            int min = int.MaxValue;
            foreach (var line in lines)
            {
                int n = VisibleLength(line);
                if (n < min) min = n;
            }
            return min == int.MaxValue ? 0 : min;
        }

        static void DrawTracked(Graphics g, string text, Font font, Brush brush, float centerX, float topY, float trackingEm)
        {
            if (string.IsNullOrEmpty(text))
            {
                return;
            }

            var sizes = new SizeF[text.Length];
            float content = 0;
            for (int i = 0; i < text.Length; i++)
            {
                sizes[i] = g.MeasureString(text[i].ToString(), font, int.MaxValue, Typographic);
                content += sizes[i].Width;
            }
            float em = font.GetHeight(g);
            float tracking = em * trackingEm;
            float extra = tracking * Math.Max(0, text.Length - 1);
            float x = centerX - (content + extra) / 2f;
            for (int i = 0; i < text.Length; i++)
            {
                g.DrawString(text[i].ToString(), font, brush, x, topY, Typographic);
                x += sizes[i].Width + tracking;
            }
        }

        static Font CreateFont(string[] families, float pointSize, FontStyle style)
        {
            foreach (var name in families)
            {
                try
                {
                    var font = new Font(name, pointSize, style, GraphicsUnit.Point);
                    if (FontMatches(font, name))
                    {
                        return font;
                    }
                    font.Dispose();
                }
                catch
                {
                }
            }
            return new Font(FontFamily.GenericSerif, pointSize, style, GraphicsUnit.Point);
        }

        static bool FontMatches(Font font, string requested)
        {
            if (string.Equals(font.Name, requested, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
            if (requested == "微软雅黑" && font.Name.IndexOf("YaHei", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return true;
            }
            if (requested.IndexOf("KaiTi", StringComparison.OrdinalIgnoreCase) >= 0
                && font.Name.IndexOf("Kai", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return true;
            }
            if (requested.IndexOf("FangSong", StringComparison.OrdinalIgnoreCase) >= 0
                && font.Name.IndexOf("Fang", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return true;
            }
            return false;
        }
    }
}
