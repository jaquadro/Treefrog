using System;
using Microsoft.Xna.Framework.Content;

namespace Treefrog.Runtime
{
    /// <summary>
    /// Reads in a <see cref="LevelIndex"/> from an XNB through a <see cref="ContentReader"/>.
    /// </summary>
    public sealed class LevelIndexReader : ContentTypeReader<LevelIndex>
    {
        /// <summary>
        /// Reads in a <see cref="LevelIndex"/> from an XNB.
        /// </summary>
        /// <param name="input">The <see cref="ContentReader"/> for reading the file.</param>
        /// <param name="existingInstance">An existing <see cref="LevelIndex"/> instance.</param>
        /// <returns>A new <see cref="LevelIndex"/>.</returns>
        protected override LevelIndex Read (ContentReader input, LevelIndex existingInstance)
        {
            return new LevelIndex(input);
        }
    }
}
