using System;
using Microsoft.Xna.Framework.Content;

namespace Treefrog.Runtime
{
    /// <summary>
    /// Reads in a <see cref="TileSet"/> from an XNB through a <see cref="ContentReader"/>.
    /// </summary>
    public class TilesetReader : ContentTypeReader<TileSet>
    {
        /// <summary>
        /// Reads in a <see cref="TileSet"/> from an XNB.
        /// </summary>
        /// <param name="input">The <see cref="ContentReader"/> for reading the file.</param>
        /// <param name="existingInstance">An existing <see cref="TileSet"/> instance.</param>
        /// <returns>A new <see cref="Level"/>.</returns>
        protected override TileSet Read (ContentReader input, TileSet existingInstance)
        {
            return new TileSet(input);
        }
    }
}
