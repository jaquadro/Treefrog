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

using System.Drawing.Imaging;

namespace Treefrog.Pipeline.ImagePacker
{
    public class Settings
    {
        public bool PowerOfTwo { get; set; }
        public int PaddingX { get; set; }
        public int PaddingY { get; set; }
        public bool EdgePadding { get; set; }
        public bool DuplicatePadding { get; set; }
        public bool Rotation { get; set; }
        public int MinWidth { get; set; }
        public int MinHeight { get; set; }
        public int MaxWidth { get; set; }
        public int MaxHeight { get; set; }
        public bool ForceSquareOutput { get; set; }
        public bool StripWhitespaceX { get; set; }
        public bool StripWhitespaceY { get; set; }
        public int AlphaThreshold { get; set; }
        //public TextureFilter FilterMin { get; set; }
        //public TextureFilter FilterMag { get; set; }
        //public TextureWrap WrapX { get; set; }
        //public TextureWrap WrapY { get; set; }
        public PixelFormat Format { get; set; }
        public bool Alias { get; set; }
        public string OutputFormat { get; set; }
        public int JpegQuality { get; set; }
        public bool IgnoreBlankImages { get; set; }
        public bool Fast { get; set; }
        public bool Debug { get; set; }
        public bool CombineSubdirectories { get; set; }
        public bool FlattenPaths { get; set; }
        public bool PremultiplyAlpha { get; set; }
        public bool UseIndexes { get; set; }

        public Settings ()
        {
            PowerOfTwo = true;
            PaddingX = 2;
            PaddingY = 2;
            EdgePadding = true;
            DuplicatePadding = true;
            MinWidth = 16;
            MinHeight = 16;
            MaxWidth = 1024;
            MaxHeight = 1024;
            //FilterMin = TextureFilter.Nearest;
            //FilterMag = TextureFilter.Nearest;
            //WrapX = TextureWrap.ClampToEdge;
            //WrapY = TextureWrap.ClampToEdge;
            Format = PixelFormat.Format32bppArgb;
            Alias = true;
            OutputFormat = "png";
            JpegQuality = 90;
            IgnoreBlankImages = true;
            UseIndexes = true;
        }

        public Settings (Settings settings)
        {
            Fast = settings.Fast;
            Rotation = settings.Rotation;
            PowerOfTwo = settings.PowerOfTwo;
            MinWidth = settings.MinWidth;
            MinHeight = settings.MinHeight;
            MaxWidth = settings.MaxWidth;
            MaxHeight = settings.MaxHeight;
            PaddingX = settings.PaddingX;
            PaddingY = settings.PaddingY;
            EdgePadding = settings.EdgePadding;
            AlphaThreshold = settings.AlphaThreshold;
            IgnoreBlankImages = settings.IgnoreBlankImages;
            StripWhitespaceX = settings.StripWhitespaceX;
            StripWhitespaceY = settings.StripWhitespaceY;
            Alias = settings.Alias;
            Format = settings.Format;
            JpegQuality = settings.JpegQuality;
            OutputFormat = settings.OutputFormat;
            //FilterMin = settings.FilterMin;
            //FilterMag = settings.FilterMag;
            //WrapX = settings.WrapX;
            //WrapY = settings.WrapY;
            DuplicatePadding = settings.DuplicatePadding;
            Debug = settings.Debug;
            CombineSubdirectories = settings.CombineSubdirectories;
            FlattenPaths = settings.FlattenPaths;
            PremultiplyAlpha = settings.PremultiplyAlpha;
        }
    }
}
