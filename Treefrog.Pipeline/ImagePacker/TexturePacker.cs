/*******************************************************************************
 * Copyright 2011 See AUTHORS file.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 *   http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 ******************************************************************************/
/* Copyright 2013 See AUTHORS file.
 * This work is a port from libgdx.
 ******************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;

namespace Treefrog.Pipeline.ImagePacker
{
    public class TexturePacker
    {
        private readonly Settings _settings;
        private readonly MaxRectsPacker _maxRectsPacker;
        private readonly ImageProcessor _imageProcessor;

        public TexturePacker (string rootDir, Settings settings)
        {
            _settings = settings;

            if (_settings.PowerOfTwo) {
                if (_settings.MaxWidth != MathUtils.NextPowerOfTwo(_settings.MaxWidth))
                    throw new Exception("If PowerOfTwo is true, MaxWidth must be a power of two: " + _settings.MaxWidth);
                if (_settings.MaxHeight != MathUtils.NextPowerOfTwo(_settings.MaxHeight))
                    throw new Exception("If PowerOfTwo is true, MaxHeight must be a power of two: " + _settings.MaxHeight);
            }

            _maxRectsPacker = new MaxRectsPacker(settings);
            _imageProcessor = new ImageProcessor(rootDir, settings);
        }

        public TexturePacker (Settings settings)
            : this(null, settings)
        { }

        public void AddImage (string file)
        {
            _imageProcessor.AddImage(file);
        }

        public void AddImage (Bitmap image, string name)
        {
            _imageProcessor.AddImage(image, name);
        }

        public List<Page> Pack (string outputDir, string packFileName)
        {
            if (!Directory.Exists(outputDir))
                Directory.CreateDirectory(outputDir);

            if (string.IsNullOrEmpty(Path.GetExtension(packFileName)))
                packFileName += ".atlas";

            List<Page> pages = _maxRectsPacker.Pack(_imageProcessor.Images);
            WriteImages(outputDir, pages, packFileName);

            try {
                WritePackFile(outputDir, pages, packFileName);
            }
            catch (Exception e) {
                throw new Exception("Error writing pack file.", e);
            }

            return pages;
        }

        private void WriteImages (string outputDir, List<Page> pages, string packFileName)
        {
            string imageName = Path.GetFileNameWithoutExtension(packFileName);

            int fileIndex = 0;
            foreach (Page page in pages) {
                int width = page.Width;
                int height = page.Height;
                int paddingX = _settings.PaddingX;
                int paddingY = _settings.PaddingY;

                if (_settings.DuplicatePadding) {
                    paddingX /= 2;
                    paddingY /= 2;
                }

                width -= _settings.PaddingX;
                height -= _settings.PaddingY;

                if (_settings.EdgePadding) {
                    page.X = paddingX;
                    page.Y = paddingY;
                    width += paddingX * 2;
                    height += paddingY * 2;
                }

                if (_settings.PowerOfTwo) {
                    width = MathUtils.NextPowerOfTwo(width);
                    height = MathUtils.NextPowerOfTwo(height);
                }

                width = Math.Max(_settings.MinWidth, width);
                height = Math.Max(_settings.MinHeight, height);

                if (_settings.ForceSquareOutput) {
                    if (width > height)
                        height = width;
                    else
                        width = height;
                }

                string outputFile;
                while (true) {
                    outputFile = Path.Combine(outputDir, imageName + (fileIndex++ == 0 ? "" : fileIndex.ToString()) + "." + _settings.OutputFormat);
                    if (!File.Exists(outputFile))
                        break;
                }

                page.ImageName = Path.GetFileName(outputFile);

                using (Bitmap canvas = new Bitmap(width, height, _settings.Format)) {
                    foreach (Rect rect in page.OutputRects) {
                        Bitmap image = rect.Image;
                        int iw = image.Width;
                        int ih = image.Height;
                        int rectX = page.X + rect.X;
                        int rectY = page.Y + page.Height - rect.Y - rect.Height;

                        if (_settings.DuplicatePadding) {
                            int amountX = _settings.PaddingX / 2;
                            int amountY = _settings.PaddingY / 2;

                            if (rect.Rotated) {
                                // Copy corner pixels into corners of padding
                                for (int i = 1; i <= amountX; i++) {
                                    for (int j = 1; j <= amountY; j++) {
                                        canvas.SetPixel(rectX - j, rectY + iw - 1 + i, image.GetPixel(0, 0));
                                        canvas.SetPixel(rectX + ih - 1 + j, rectY + iw - 1 + i, image.GetPixel(0, ih - 1));
                                        canvas.SetPixel(rectX - j, rectY - i, image.GetPixel(iw - 1, 0));
                                        canvas.SetPixel(rectX + ih - 1 + j, rectY - i, image.GetPixel(iw - 1, ih - 1));
                                    }
                                }

                                // Copy edge pixels into padding
                                for (int i = 1; i <= amountY; i++) {
                                    for (int j = 0; j < iw; j++) {
                                        canvas.SetPixel(rectX - i, rectY + iw - 1 - j, image.GetPixel(j, 0));
                                        canvas.SetPixel(rectX + ih - 1 + i, rectY + iw - 1 - j, image.GetPixel(j, ih - 1));
                                    }
                                }

                                for (int i = 1; i <= amountX; i++) {
                                    for (int j = 0; j < ih; j++) {
                                        canvas.SetPixel(rectX + j, rectY - i, image.GetPixel(iw - 1, j));
                                        canvas.SetPixel(rectX + j, rectY + iw - 1 + i, image.GetPixel(0, j));
                                    }
                                }
                            }
                            else {
                                // Copy corner pixels into corners of padding
                                for (int i = 1; i <= amountX; i++) {
                                    for (int j = 1; j <= amountY; j++) {
                                        canvas.SetPixel(rectX - i, rectY - j, image.GetPixel(0, 0));
                                        canvas.SetPixel(rectX - i, rectY + ih - 1 + j, image.GetPixel(0, ih - 1));
                                        canvas.SetPixel(rectX + iw - 1 + i, rectY - j, image.GetPixel(iw - 1, 0));
                                        canvas.SetPixel(rectX + iw - 1 + i, rectY + ih - 1 + j, image.GetPixel(iw - 1, ih - 1));
                                    }
                                }

                                // Copy edge pixels into padding
                                for (int i = 1; i <= amountY; i++) {
                                    for (int j = 0; j < iw; j++) {
                                        canvas.SetPixel(rectX + j, rectY - i, image.GetPixel(j, 0));
                                        canvas.SetPixel(rectX + j, rectY + ih - 1 + i, image.GetPixel(j, ih - 1));
                                    }
                                }

                                for (int i = 1; i <= amountX; i++) {
                                    for (int j = 0; j < ih; j++) {
                                        canvas.SetPixel(rectX - i, rectY + j, image.GetPixel(0, j));
                                        canvas.SetPixel(rectX + iw - 1 + i, rectY + j, image.GetPixel(iw - 1, j));
                                    }
                                }
                            }
                        }

                        // Copy image
                        if (rect.Rotated) {
                            for (int i = 0; i < iw; i++) {
                                for (int j = 0; j < ih; j++) {
                                    canvas.SetPixel(rectX + j, rectY + iw - i - 1, image.GetPixel(i, j));
                                }
                            }
                        }
                        else {
                            for (int i = 0; i < iw; i++) {
                                for (int j = 0; j < ih; j++) {
                                    canvas.SetPixel(rectX + i, rectY + j, image.GetPixel(i, j));
                                }
                            }
                        }
                    }

                    using (FileStream fstr = File.OpenWrite(outputFile)) {
                        try {
                            if (string.Compare(_settings.OutputFormat, "jpg", true) == 0) {
                                ImageCodecInfo codec = GetEncoderInfo("image/jpeg");
                                EncoderParameter[] codecParams = new EncoderParameter[] {
                                new EncoderParameter(Encoder.Quality, _settings.JpegQuality),
                            };

                                canvas.Save(fstr, codec, new EncoderParameters() { Param = codecParams });
                            }
                            else {
                                if (_settings.PremultiplyAlpha) {
                                    using (Bitmap pmaCanvas = PremultiplyAlpha(canvas)) {
                                        pmaCanvas.Save(fstr, ImageFormat.Png);
                                    }
                                }
                                else
                                    canvas.Save(fstr, ImageFormat.Png);
                            }
                        }
                        catch (Exception e) {
                            throw new Exception("Error writing file: " + outputFile, e);
                        }
                    }
                }
            }
        }

        private void WritePackFile (string outputDir, List<Page> pages, string packFileName)
        {
            string packFile = Path.Combine(outputDir, packFileName);

            if (File.Exists(packFile)) {

            }

            using (FileStream fstr = File.OpenWrite(packFile)) {
                using (TextWriter writer = new StreamWriter(fstr)) {
                    foreach (Page page in pages) {
                        writer.Write("\n" + page.ImageName + "\n");
                        writer.Write("format: " + _settings.Format + "\n");
                        writer.Write("filter: " + "" + "," + "" + "\n");
                        writer.Write("repeat: " + "" + "\n");

                        foreach (Rect rect in page.OutputRects) {
                            WriteRect(writer, page, rect, rect.Name);

                            foreach (Alias alias in rect.Aliases) {
                                Rect aliasRect = new Rect();
                                aliasRect.Set(rect);
                                alias.Apply(aliasRect);
                                WriteRect(writer, page, aliasRect, alias.Name);
                            }
                        }
                    }
                }
            }
        }

        private void WriteRect (TextWriter writer, Page page, Rect rect, string name)
        {
            writer.Write(Rect.GetAliasName(name, _settings.FlattenPaths) + "\n");
            writer.Write("  rotate: " + rect.Rotated + "\n");
            writer.Write("  xy: " + (page.X + rect.X) + ", " + (page.Y + page.Height - rect.Height - rect.Y) + "\n");
            writer.Write("  size: " + rect.Image.Width + ", " + rect.Image.Height + "\n");

            if (rect.Splits != null)
                writer.Write(string.Format("  split: {0}, {1}, {2}, {3}\n", rect.Splits[0], rect.Splits[1], rect.Splits[2], rect.Splits[3]));
            if (rect.Pads != null) {
                if (rect.Splits == null)
                    writer.Write("  split: 0, 0, 0, 0\n");
                else
                    writer.Write(string.Format("  pad: {0}, {1}, {2}, {3}\n", rect.Pads[0], rect.Pads[1], rect.Pads[2], rect.Pads[3]));
            }

            writer.Write(string.Format("  orig: {0}, {1}\n", rect.OriginalWidth, rect.OriginalHeight));
            writer.Write(string.Format("  offset: {0}, {1}\n", rect.OffsetX, rect.OriginalHeight - rect.Image.Height - rect.OffsetY));
            writer.Write(string.Format("  index: {0}\n", rect.Index));
        }

        private static Bitmap PremultiplyAlpha (Bitmap source)
        {
            if (source.PixelFormat == PixelFormat.Format32bppPArgb)
                return new Bitmap(source);

            Bitmap image = new Bitmap(source.Width, source.Height, PixelFormat.Format32bppPArgb);

            for (int y = 0; y < source.Height; y++) {
                for (int x = 0; x < source.Width; x++) {
                    Color c = source.GetPixel(x, y);
                    image.SetPixel(x, y, Color.FromArgb(c.A, c.R, c.G, c.B));
                }
            }

            return image;
        }

        private static ImageCodecInfo GetEncoderInfo (String mimeType)
        {
            int j;
            ImageCodecInfo[] encoders;
            encoders = ImageCodecInfo.GetImageEncoders();
            for (j = 0; j < encoders.Length; ++j) {
                if (encoders[j].MimeType == mimeType)
                    return encoders[j];
            }
            return null;
        }

        

        

        

        
    }
}
