using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using OpenTK.Graphics.OpenGL;
using OpenTK.Graphics.Wgl;
using OpenTK.Wpf.Interop;

namespace OpenTK.Wpf
{

    /// Renderer that uses DX_Interop for a fast-path.
    internal sealed class GLWpfControlRenderer {
        
        private readonly Stopwatch _stopwatch = Stopwatch.StartNew();
        private readonly DxGlContext _context;
        
        public event Action<TimeSpan> GLRender;
        public event Action GLAsyncRender;
        
        private DxGLFramebuffer _framebuffer;

        /// The OpenGL framebuffer handle.
        public int FrameBufferHandle => _framebuffer.GLFramebufferHandle;

        /// The OpenGL Framebuffer width
        public int Width => _framebuffer?.FramebufferWidth ?? 0;
        
        /// The OpenGL Framebuffer height
        public int Height => _framebuffer?.FramebufferWidth ?? 0;

        public D3DImage D3dImage { get; }

        private TimeSpan _lastFrameStamp;

        public GLWpfControlRenderer(GLWpfControlSettings settings, D3DImage image)
        {
            _context = new DxGlContext(settings);
            D3dImage = image;
            SetSize(1, 1, 1, 1);
        }


        public void SetSize(int width, int height, double dpiScaleX, double dpiScaleY) {
            if (_framebuffer == null || _framebuffer.Width != width && _framebuffer.Height != height) {
                _framebuffer?.Dispose();
                _framebuffer = null;
                var backbuffer = IntPtr.Zero;
                if (width > 0 && height > 0) {
                    _framebuffer = new DxGLFramebuffer(_context, width, height, dpiScaleX, dpiScaleY);
                    backbuffer = _framebuffer.DxRenderTargetHandle;
                    D3dImage.Lock();
                    D3dImage.SetBackBuffer(D3DResourceType.IDirect3DSurface9, _framebuffer.DxRenderTargetHandle);
                    D3dImage.Unlock();
                }
        
                D3dImage.Lock();
                D3dImage.SetBackBuffer(D3DResourceType.IDirect3DSurface9, backbuffer);  // if backbuffer is zero, it will do a proper cleanup of the rendertarget, since D3DImage still retains a reference to it
                D3dImage.Unlock();
            }
        }

        public void Render() {
            if (_framebuffer == null) {
                return;
            }
            var curFrameStamp = _stopwatch.Elapsed;
            var deltaT = curFrameStamp - _lastFrameStamp;
            _lastFrameStamp = curFrameStamp;
            PreRender();
            GLRender?.Invoke(deltaT);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            GL.Flush();
            GLAsyncRender?.Invoke();
            PostRender();
            
        }

        /// Sets up the framebuffer, directx stuff for rendering. 
        private void PreRender()
        {
            D3dImage.Lock();
            Wgl.DXLockObjectsNV(_context.GlDeviceHandle, 1, new [] {_framebuffer.DxInteropRegisteredHandle});
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, _framebuffer.GLFramebufferHandle);
            GL.Viewport(0, 0, _framebuffer.FramebufferWidth, _framebuffer.FramebufferHeight);
        }

        /// Sets up the framebuffer and prepares stuff for usage in directx.
        private void PostRender()
        {
            Wgl.DXUnlockObjectsNV(_context.GlDeviceHandle, 1, new [] {_framebuffer.DxInteropRegisteredHandle});
            D3dImage.SetBackBuffer(D3DResourceType.IDirect3DSurface9, _framebuffer.DxRenderTargetHandle);
            D3dImage.AddDirtyRect(new Int32Rect(0, 0, _framebuffer.FramebufferWidth, _framebuffer.FramebufferHeight));
            D3dImage.Unlock();
        }
    }
}
