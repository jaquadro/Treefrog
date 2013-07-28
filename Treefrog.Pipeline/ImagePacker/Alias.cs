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

namespace Treefrog.Pipeline.ImagePacker
{
    public class Alias
    {
        public string Name { get; set; }
        public int Index { get; set; }
        public int[] Splits { get; set; }
        public int[] Pads { get; set; }

        public Alias (Rect rect)
        {
            Name = rect.Name;
            Index = rect.Index;
            Splits = rect.Splits;
            Pads = rect.Pads;
        }

        public void Apply (Rect rect)
        {
            rect.Name = Name;
            rect.Index = Index;
            rect.Splits = Splits;
            rect.Pads = Pads;
        }
    }
}
