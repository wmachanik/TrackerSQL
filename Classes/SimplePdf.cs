using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace TrackerSQL.Classes
{
    /// <summary>
    /// A small PDF writer for short documents such as the proof of delivery: Helvetica text, lines, boxes and JPEG images.
    /// Coordinates are points from the top-left corner of an A4 page; a text y is its baseline.
    /// Text is written in WinAnsi (Windows-1252), so characters outside it become their closest match or "?".
    /// </summary>
    public sealed class SimplePdf
    {
        public const float PageWidth = 595.28f;
        public const float PageHeight = 841.89f;

        private static readonly Encoding WinAnsi = Encoding.GetEncoding(1252);

        private static readonly short[] HelveticaWidths =
        {
            278, 278, 355, 556, 556, 889, 667, 191, 333, 333, 389, 584, 278, 333, 278, 278,
            556, 556, 556, 556, 556, 556, 556, 556, 556, 556,
            278, 278, 584, 584, 584, 556, 1015,
            667, 667, 722, 722, 667, 611, 778, 722, 278, 500, 667, 556, 833, 722, 778, 667, 778, 722, 667, 611, 722, 667, 944, 667, 667, 611,
            278, 278, 278, 469, 556, 333,
            556, 556, 500, 556, 556, 278, 556, 556, 222, 222, 500, 222, 833, 556, 556, 556, 556, 333, 500, 278, 556, 500, 722, 500, 500, 500,
            334, 260, 334, 584
        };

        private static readonly short[] HelveticaBoldWidths =
        {
            278, 333, 474, 556, 556, 889, 722, 238, 333, 333, 389, 584, 278, 333, 278, 278,
            556, 556, 556, 556, 556, 556, 556, 556, 556, 556,
            333, 333, 584, 584, 584, 611, 975,
            722, 722, 722, 722, 667, 611, 778, 722, 278, 556, 722, 611, 833, 722, 778, 667, 778, 722, 667, 611, 722, 667, 944, 667, 667, 611,
            333, 278, 333, 584, 556, 333,
            556, 611, 556, 611, 556, 333, 611, 611, 278, 278, 556, 278, 889, 611, 611, 611, 611, 389, 556, 333, 611, 556, 778, 556, 556, 500,
            389, 280, 389, 584
        };

        private sealed class PdfImage
        {
            public string Name;
            public byte[] Jpeg;
            public int Width;
            public int Height;
        }

        private readonly List<StringBuilder> _pages = new List<StringBuilder>();
        private readonly List<PdfImage> _images = new List<PdfImage>();
        private StringBuilder _page;

        public SimplePdf()
        {
            NewPage();
        }

        public int PageCount => _pages.Count;

        public void NewPage()
        {
            _page = new StringBuilder();
            _pages.Add(_page);
        }

        /// <summary>Draw on an earlier page (0-based), e.g. footers once the page count is known.</summary>
        public void SelectPage(int index)
        {
            _page = _pages[index];
        }

        /// <summary>Draws one line of text; <paramref name="rgb"/> is 0xRRGGBB.</summary>
        public void Text(float x, float y, string text, float size, bool bold = false, int rgb = 0x000000)
        {
            if (string.IsNullOrEmpty(text))
                return;
            _page.Append("BT /").Append(bold ? "F2 " : "F1 ").Append(N(size)).Append(" Tf ")
                 .Append(Colour(rgb)).Append(" rg ")
                 .Append(N(x)).Append(' ').Append(N(PageHeight - y)).Append(" Td (")
                 .Append(Escape(text)).Append(") Tj ET\n");
        }

        /// <summary>Text ending at <paramref name="right"/>.</summary>
        public void TextRight(float right, float y, string text, float size, bool bold = false, int rgb = 0x000000)
        {
            Text(right - Width(text, size, bold), y, text, size, bold, rgb);
        }

        public void Line(float x1, float y1, float x2, float y2, float width = 0.5f, int rgb = 0x000000)
        {
            _page.Append(Colour(rgb)).Append(" RG ").Append(N(width)).Append(" w ")
                 .Append(N(x1)).Append(' ').Append(N(PageHeight - y1)).Append(" m ")
                 .Append(N(x2)).Append(' ').Append(N(PageHeight - y2)).Append(" l S\n");
        }

        public void FillRect(float x, float y, float width, float height, int rgb)
        {
            _page.Append(Colour(rgb)).Append(" rg ")
                 .Append(N(x)).Append(' ').Append(N(PageHeight - y - height)).Append(' ')
                 .Append(N(width)).Append(' ').Append(N(height)).Append(" re f\n");
        }

        public void StrokeRect(float x, float y, float width, float height, float lineWidth = 0.5f, int rgb = 0x000000)
        {
            _page.Append(Colour(rgb)).Append(" RG ").Append(N(lineWidth)).Append(" w ")
                 .Append(N(x)).Append(' ').Append(N(PageHeight - y - height)).Append(' ')
                 .Append(N(width)).Append(' ').Append(N(height)).Append(" re S\n");
        }

        /// <summary>Adds a JPEG once (see <see cref="ToJpeg"/>) and returns its name for <see cref="Image"/>.</summary>
        public string AddJpeg(byte[] jpeg, int pixelWidth, int pixelHeight)
        {
            var image = new PdfImage { Name = "Im" + (_images.Count + 1), Jpeg = jpeg, Width = pixelWidth, Height = pixelHeight };
            _images.Add(image);
            return image.Name;
        }

        public void Image(string name, float x, float y, float width, float height)
        {
            _page.Append("q ").Append(N(width)).Append(" 0 0 ").Append(N(height)).Append(' ')
                 .Append(N(x)).Append(' ').Append(N(PageHeight - y - height)).Append(" cm /").Append(name).Append(" Do Q\n");
        }

        public static float Width(string text, float size, bool bold = false)
        {
            if (string.IsNullOrEmpty(text))
                return 0f;
            short[] table = bold ? HelveticaBoldWidths : HelveticaWidths;
            int total = 0;
            foreach (byte b in WinAnsi.GetBytes(text))
            {
                if (b >= 32 && b <= 126)
                    total += table[b - 32];
                else
                    total += ExtendedWidth(b, bold);
            }
            return total * size / 1000f;
        }

        /// <summary>Splits text into lines no wider than <paramref name="maxWidth"/> (line breaks in the text are kept).</summary>
        public static List<string> Wrap(string text, float size, bool bold, float maxWidth)
        {
            var lines = new List<string>();
            foreach (string paragraph in (text ?? string.Empty).Replace("\r\n", "\n").Split('\n'))
            {
                string line = string.Empty;
                foreach (string word in paragraph.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    string candidate = line.Length == 0 ? word : line + " " + word;
                    if (Width(candidate, size, bold) <= maxWidth)
                    {
                        line = candidate;
                        continue;
                    }
                    if (line.Length > 0)
                        lines.Add(line);
                    line = word;
                    while (Width(line, size, bold) > maxWidth && line.Length > 1)
                    {
                        int fit = 1;
                        while (fit < line.Length && Width(line.Substring(0, fit + 1), size, bold) <= maxWidth)
                            fit++;
                        lines.Add(line.Substring(0, fit));
                        line = line.Substring(fit);
                    }
                }
                lines.Add(line);
            }
            return lines;
        }

        /// <summary>
        /// Any image System.Drawing can read (PNG, JPEG, GIF, BMP) as a JPEG on a white background (transparent signatures stay readable),
        /// at most <paramref name="maxPixelWidth"/> wide. Returns null when the image cannot be read (e.g. WebP).
        /// </summary>
        public static byte[] ToJpeg(byte[] image, int maxPixelWidth, out int pixelWidth, out int pixelHeight)
        {
            pixelWidth = pixelHeight = 0;
            if (image == null || image.Length == 0)
                return null;
            try
            {
                using (var input = new MemoryStream(image))
                using (var source = System.Drawing.Image.FromStream(input))
                {
                    double scale = Math.Min(1.0, maxPixelWidth / (double)Math.Max(1, source.Width));
                    pixelWidth = Math.Max(1, (int)Math.Round(source.Width * scale));
                    pixelHeight = Math.Max(1, (int)Math.Round(source.Height * scale));
                    using (var bitmap = new Bitmap(pixelWidth, pixelHeight, PixelFormat.Format24bppRgb))
                    {
                        using (var g = Graphics.FromImage(bitmap))
                        {
                            g.Clear(Color.White);
                            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                            g.SmoothingMode = SmoothingMode.HighQuality;
                            g.DrawImage(source, 0, 0, pixelWidth, pixelHeight);
                        }
                        ImageCodecInfo codec = ImageCodecInfo.GetImageEncoders().First(c => c.FormatID == ImageFormat.Jpeg.Guid);
                        using (var parameters = new EncoderParameters(1))
                        using (var output = new MemoryStream())
                        {
                            parameters.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, 90L);
                            bitmap.Save(output, codec, parameters);
                            return output.ToArray();
                        }
                    }
                }
            }
            catch (Exception)
            {
                pixelWidth = pixelHeight = 0;
                return null;
            }
        }

        public byte[] Save(string title)
        {
            using (var ms = new MemoryStream())
            {
                var offsets = new List<long>();
                Action<string> write = s =>
                {
                    byte[] bytes = Encoding.ASCII.GetBytes(s);
                    ms.Write(bytes, 0, bytes.Length);
                };
                Action<int> begin = number =>
                {
                    while (offsets.Count < number)
                        offsets.Add(0);
                    offsets[number - 1] = ms.Position;
                    write(number + " 0 obj\n");
                };
                Action<byte[]> stream = data =>
                {
                    ms.Write(data, 0, data.Length);
                    write("\nendstream\nendobj\n");
                };

                write("%PDF-1.4\n");
                ms.Write(new byte[] { 0x25, 0xE2, 0xE3, 0xCF, 0xD3, 0x0A }, 0, 6);

                int firstImage = 6;
                int firstPage = firstImage + _images.Count;
                var pageNumbers = Enumerable.Range(0, _pages.Count).Select(i => firstPage + i * 2).ToList();

                begin(1);
                write("<< /Type /Catalog /Pages 2 0 R >>\nendobj\n");
                begin(2);
                write("<< /Type /Pages /Kids [" + string.Join(" ", pageNumbers.Select(n => n + " 0 R")) + "] /Count " + _pages.Count + " >>\nendobj\n");
                begin(3);
                write("<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica /Encoding /WinAnsiEncoding >>\nendobj\n");
                begin(4);
                write("<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica-Bold /Encoding /WinAnsiEncoding >>\nendobj\n");
                begin(5);
                DateTime now = TimeZoneUtils.Now();
                write("<< /Title (" + AsciiEscape(title) + ") /Producer (Tracker) /CreationDate (D:" + now.ToString("yyyyMMddHHmmss", CultureInfo.InvariantCulture) + ") >>\nendobj\n");

                for (int i = 0; i < _images.Count; i++)
                {
                    PdfImage image = _images[i];
                    begin(firstImage + i);
                    write("<< /Type /XObject /Subtype /Image /Width " + image.Width + " /Height " + image.Height
                          + " /ColorSpace /DeviceRGB /BitsPerComponent 8 /Filter /DCTDecode /Length " + image.Jpeg.Length + " >>\nstream\n");
                    stream(image.Jpeg);
                }

                string xobjects = _images.Count == 0 ? string.Empty
                    : " /XObject << " + string.Join(" ", _images.Select((img, i) => "/" + img.Name + " " + (firstImage + i) + " 0 R")) + " >>";
                for (int i = 0; i < _pages.Count; i++)
                {
                    int pageNumber = pageNumbers[i];
                    begin(pageNumber);
                    write("<< /Type /Page /Parent 2 0 R /MediaBox [0 0 " + N(PageWidth) + " " + N(PageHeight) + "]"
                          + " /Resources << /Font << /F1 3 0 R /F2 4 0 R >>" + xobjects + " >> /Contents " + (pageNumber + 1) + " 0 R >>\nendobj\n");
                    byte[] content = Encoding.ASCII.GetBytes(_pages[i].ToString());
                    begin(pageNumber + 1);
                    write("<< /Length " + content.Length + " >>\nstream\n");
                    stream(content);
                }

                long xref = ms.Position;
                var table = new StringBuilder();
                table.Append("xref\n0 ").Append(offsets.Count + 1).Append("\n0000000000 65535 f \n");
                foreach (long offset in offsets)
                    table.Append(offset.ToString("D10", CultureInfo.InvariantCulture)).Append(" 00000 n \n");
                table.Append("trailer\n<< /Size ").Append(offsets.Count + 1).Append(" /Root 1 0 R /Info 5 0 R >>\nstartxref\n")
                     .Append(xref.ToString(CultureInfo.InvariantCulture)).Append("\n%%EOF\n");
                write(table.ToString());
                return ms.ToArray();
            }
        }

        private static int ExtendedWidth(byte b, bool bold)
        {
            switch (b)
            {
                case 0xA0: return 278;
                case 0xB7: return 278;
                case 0xD7: return 584;
                case 0x95: return 350;
                case 0x96: return 556;
                case 0x97: return 1000;
                case 0x85: return 1000;
                case 0x91:
                case 0x92: return bold ? 278 : 222;
                case 0x93:
                case 0x94: return bold ? 500 : 333;
                default:
                    if (b >= 0xC0 && b <= 0xDE)
                        return 722;
                    return bold ? 611 : 556;
            }
        }

        /// <summary>A PDF string literal body: ( ) \ escaped, bytes outside ASCII as octal so the content stream stays ASCII.</summary>
        private static string Escape(string text)
        {
            var sb = new StringBuilder(text.Length + 8);
            foreach (byte b in WinAnsi.GetBytes(text))
            {
                if (b == '(' || b == ')' || b == '\\')
                    sb.Append('\\').Append((char)b);
                else if (b < 32 || b > 126)
                    sb.Append('\\').Append(Convert.ToString(b, 8).PadLeft(3, '0'));
                else
                    sb.Append((char)b);
            }
            return sb.ToString();
        }

        private static string AsciiEscape(string text)
        {
            var sb = new StringBuilder();
            foreach (char c in text ?? string.Empty)
            {
                if (c == '(' || c == ')' || c == '\\')
                    sb.Append('\\').Append(c);
                else if (c >= 32 && c <= 126)
                    sb.Append(c);
                else
                    sb.Append('?');
            }
            return sb.ToString();
        }

        private static string Colour(int rgb)
        {
            return N(((rgb >> 16) & 0xFF) / 255f) + " " + N(((rgb >> 8) & 0xFF) / 255f) + " " + N((rgb & 0xFF) / 255f);
        }

        private static string N(float value)
        {
            return value.ToString("0.###", CultureInfo.InvariantCulture);
        }
    }
}
