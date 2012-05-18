using System;
using Microsoft.Xna.Framework.Content;

namespace Treefrog.Runtime
{
    /// <summary>
    /// Reads in a <see cref="Tileset"/> from an XNB through a <see cref="ContentReader"/>.
    /// </summary>
    public class TilesetReader : ContentTypeReader<Tileset>
    {
        /// <summary>
        /// Reads in a <see cref="Tileset"/> from an XNB.
        /// </summary>
        /// <param name="input">The <see cref="ContentReader"/> for reading the file.</param>
        /// <param name="existingInstance">An existing <see cref="Tileset"/> instance.</param>
        /// <returns>A new <see cref="Level"/>.</returns>
        protected override Tileset Read (ContentReader input, Tileset existingInstance)
        {
            return new Tileset(input);
        }
    }
}
