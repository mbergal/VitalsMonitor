using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Accord.Imaging.Filters;
using LanguageExt;
using Monitor.Model;
using NLog;
using Tesseract;

namespace Monitor.Services
{
    public class OcrImageService : IOcrImageService
    {
        private static readonly Logger Logger = LogManager.GetLogger("DataCollectionService");
        private readonly TesseractEngine engine;

        public OcrImageService()
        {
            this.engine = new TesseractEngine(@"./tessdata", "eng", EngineMode.Default);
        }

        public PulsometerRecordingResult OcrPulsometerData(Bitmap image)
        {
            Logger.Info("OcrPulsometerData: image=(image)");
            var text = OcrScreenshot(image);
            var result = text.Match(
                Some: t =>
                {
                    string[] lines = t.Split(
                        new[] {Environment.NewLine},
                        StringSplitOptions.None
                    );
                    string dataLine = lines[1];

                    var match = Regex.Match(dataLine, @"(\d+)[^\d].*?\s+(\d+)");
                    double? SPO2 = TryParse(match.Groups[1].Value);
                    double? pulse = TryParse(match.Groups[2].Value);

                    return new PulsometerRecordingResult(
                        new PulsometerData(DateTime.Now, SPO2, pulse),
                        t,
                        image);
                },
                None: () =>
                    new PulsometerRecordingResult(new PulsometerData(DateTime.Now, null, null), "",
                        image));
            Logger.Info($"   result={result}");
            return result;
        }

        private static double? TryParse(string value)
        {
            if (double.TryParse(value, out var d)) return d;
            else return null;
        }

        public Option<string> OcrScreenshot(Bitmap image)
        {
            var cropRect = screenshotRect(image);
            Logger.Debug($"OcrScreenshot: image={image.Size}");
            Logger.Debug($"OcrScreenshot: cropRect={cropRect}");

            if (cropRect.Width <= 0 || cropRect.Height <= 0)
            {
                return null;
            }

            using (var croppedBitmap = new Crop(cropRect).Apply(image))
            using (var croppedBitmap2 =
                new Crop(new Rectangle(
                    0,
                    (int) (croppedBitmap.Height * 0.2),
                    croppedBitmap.Width,
                    (int) (croppedBitmap.Height * 0.1))).Apply(croppedBitmap))
            {
                var text = this.OcrImage(croppedBitmap2);
                return text;
            }
        }

        public string OcrImage(Image image)
        {
            StringBuilder sb = new StringBuilder();
            {
                using (var img = Pix.LoadTiffFromMemory(ConvertJpegToTiff(image)))
                {
                    using (var page = this.engine.Process(img))
                    {
                        var text = page.GetText();
                        Console.WriteLine("Mean confidence: {0}", page.GetMeanConfidence());

                        Console.WriteLine("Text (GetText): \r\n{0}", text);
                        Console.WriteLine("Text (iterator):");
                        using (var iter = page.GetIterator())
                        {
                            iter.Begin();

                            do
                            {
                                do
                                {
                                    do
                                    {
                                        do
                                        {
                                            if (iter.IsAtBeginningOf(PageIteratorLevel.Block))
                                            {
                                                sb.AppendLine("<BLOCK>");
                                            }

                                            sb.Append(iter.GetText(PageIteratorLevel.Word));
                                            sb.Append(" ");

                                            if (iter.IsAtFinalOf(PageIteratorLevel.TextLine,
                                                PageIteratorLevel.Word))
                                            {
                                                sb.AppendLine();
                                            }
                                        } while (iter.Next(PageIteratorLevel.TextLine,
                                            PageIteratorLevel.Word));

                                        if (iter.IsAtFinalOf(PageIteratorLevel.Para,
                                            PageIteratorLevel.TextLine))
                                        {
                                            sb.AppendLine();
                                        }
                                    } while (iter.Next(PageIteratorLevel.Para,
                                        PageIteratorLevel.TextLine));
                                } while (iter.Next(PageIteratorLevel.Block,
                                    PageIteratorLevel.Para));
                            } while (iter.Next(PageIteratorLevel.Block));
                        }
                    }
                }
            }

            return sb.ToString();
        }

        private static Image cropImage(Image img, Rectangle cropArea)
        {
            Bitmap bmpImage = new Bitmap(img);
            return bmpImage.Clone(cropArea, bmpImage.PixelFormat);
        }

        static byte[] ConvertJpegToTiff(Image image)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                image.Save((Stream) ms, System.Drawing.Imaging.ImageFormat.Tiff);
//                image.Save(@"c:\temp\a.png", System.Drawing.Imaging.ImageFormat.Png);
                return ms.ToArray();
            }
        }

        private static Rectangle screenshotRect(Bitmap image)
        {
            var pixelsV = Enumerable.Range(0, image.Height)
                .Select((y, i) => (index: i, image.GetPixel(image.Width / 2, y)))
                .ToArray();

            var fV = pixelsV
                         .SkipWhile(c => !isBlack(c))
                         .SkipWhile(c => isBlack(c))
                         .Select(p => p.index)
                         .Cast<int?>()
                         .FirstOrDefault() ?? 0;

            var lV = pixelsV
                         .Reverse()
                         .SkipWhile(c => !isBlack(c))
                         .SkipWhile(c => isBlack(c))
                         .Select(p => p.index)
                         .Cast<int?>()
                         .FirstOrDefault() ?? image.Height;

            var pixelsH = Enumerable.Range(0, image.Width)
                .Select((x, i) => (i, image.GetPixel(x, image.Height / 2)))
                .ToArray();

            var fH = pixelsH
                         .SkipWhile(c => !isBlack(c))
                         .SkipWhile(c => isBlack(c))
                         .Select(p => p.i)
                         .Cast<int?>()
                         .FirstOrDefault() ?? 0;

            var lH = pixelsH
                         .Reverse()
                         .SkipWhile(c => !isBlack(c))
                         .SkipWhile(c => isBlack(c))
                         .Select(p => p.i)
                         .Cast<int?>()
                         .FirstOrDefault() ?? 0;


            var cropRect = new Rectangle(fH, fV, lH - fH, lV - fV);
            return cropRect;
        }


        private static bool isBlack((int i, Color) c)
        {
            return (c.Item2.B == 0 && c.Item2.G == 0 && c.Item2.R == 0);
        }

        public void Dispose()
        {
            engine?.Dispose();
        }
    }
}