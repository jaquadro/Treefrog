using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Reflection;
using Microsoft.Xna.Framework.Graphics;

namespace Treefrog.Controls.XnaD3D
{
    internal static class GameReflector
    {
        internal static IntPtr GetRenderTargetSurface (RenderTarget2D renderTarget)
        {
            IntPtr surfacePointer;
            IDirect3DTexture9 texture = GetIUnknownObject<IDirect3DTexture9>(renderTarget);
            Marshal.ThrowExceptionForHR(texture.GetSurfaceLevel(0, out surfacePointer));
            Marshal.ReleaseComObject(texture);
            return surfacePointer;
        }

        internal static IntPtr GetGraphicsDeviceSurface (GraphicsDevice graphicsDevice)
        {
            IntPtr surfacePointer;
            IDirect3DDevice9 device = GetIUnknownObject<IDirect3DDevice9>(graphicsDevice);
            Marshal.ThrowExceptionForHR(device.GetBackBuffer(0, 0, 0, out surfacePointer));
            Marshal.ReleaseComObject(device);
            return surfacePointer;
        }

        internal static void ReleaseGraphicsDeviceSurface (IntPtr surface)
        {
            Marshal.Release(surface);
        }

        internal static T GetIUnknownObject<T> (object container)
        {
            unsafe {
                FieldInfo deviceField = container.GetType().GetField("pComPtr", BindingFlags.NonPublic | BindingFlags.Instance);
                IntPtr devicePointer = new IntPtr(Pointer.Unbox(deviceField.GetValue(container)));
                return (T)Marshal.GetObjectForIUnknown(devicePointer);
            }
        }

        [ComImport, Guid("85C31227-3DE5-4f00-9B3A-F11AC38C18B5"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        internal interface IDirect3DTexture9
        {
            void GetDevice ();
            void SetPrivateData ();
            void GetPrivateData ();
            void FreePrivateData ();
            void SetPriority ();
            void GetPriority ();
            void PreLoad ();
            void GetType ();
            void SetLOD ();
            void GetLOD ();
            void GetLevelCount ();
            void SetAutoGenFilterType ();
            void GetAutoGenFilterType ();
            void GenerateMipSubLevels ();
            void GetLevelDesc ();

            int GetSurfaceLevel (uint level, out IntPtr surfacePointer);
        }

        [ComImport, Guid("D0223B96-BF7A-43fd-92BD-A43B0D82B9EB"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        public interface IDirect3DDevice9
        {
            void TestCooperativeLevel ();
            void GetAvailableTextureMem ();
            void EvictManagedResources ();
            void GetDirect3D ();
            void GetDeviceCaps ();
            void GetDisplayMode ();
            void GetCreationParameters ();
            void SetCursorProperties ();
            void SetCursorPosition ();
            void ShowCursor ();
            void CreateAdditionalSwapChain ();
            void GetSwapChain ();
            void GetNumberOfSwapChains ();
            void Reset ();
            void Present ();

            int GetBackBuffer (uint swapChain, uint backBuffer, int type, out IntPtr backBufferPointer);
        }
    }
}
