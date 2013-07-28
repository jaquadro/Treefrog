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

using System.Collections.Generic;

namespace Treefrog.Pipeline.ImagePacker
{
    public class Page
    {
        public string ImageName { get; set; }
        public List<Rect> OutputRects { get; set; }
        public List<Rect> RemainingRects { get; set; }
        public float Occupancy { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
    }
}
