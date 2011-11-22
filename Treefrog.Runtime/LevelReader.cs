using System;
using Microsoft.Xna.Framework.Content;

namespace Treefrog.Runtime
{
    /// <summary>
    /// Reads in a <see cref="Level"/> from an XNB through a <see cref="ContentReader"/>.
    /// </summary>
    public sealed class LevelReader : ContentTypeReader<Level>
    {
        /// <summary>
        /// Reads in a <see cref="Level"/> from an XNB.
        /// </summary>
        /// <param name="input">The <see cref="ContentReader"/> for reading the file.</param>
        /// <param name="existingInstance">An existing <see cref="Level"/> instance.</param>
        /// <returns>A new <see cref="Level"/>.</returns>
        protected override Level Read (ContentReader input, Level existingInstance)
        {
            return new Level(input);
        }
    }
}
