using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using System.Threading;
using System.Windows.Forms;
using Treefrog.Framework;

public class MockGraphicsDeviceService : GraphicsDeviceService
{
    private static Form _invisibleRenderTarget;

    protected MockGraphicsDeviceService (int width, int height)
        : base()
    {
    }

    public static GraphicsDeviceService AddRef (int width, int height)
    {
        if (_invisibleRenderTarget == null) {
            _invisibleRenderTarget = new Form();
        }

        return GraphicsDeviceService.AddRef(_invisibleRenderTarget.Handle, width, height);
    }
}