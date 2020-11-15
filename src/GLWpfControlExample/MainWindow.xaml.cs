﻿using System;
using System.Diagnostics;
using System.Timers;
using OpenTK.Wpf;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace GLWpfControlExample {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public sealed partial class MainWindow {

        private readonly Stopwatch _stopwatch = Stopwatch.StartNew();
        private TimeSpan _elapsedTime;
        private Timer _slowTimer = new Timer(500);

        private float brightness = 1;
        private bool updateHue = true;

        public MainWindow()
        {
            InitializeComponent();
            _elapsedTime = new TimeSpan();
            var settings = new GLWpfControlSettings();
            settings.MajorVersion = 2;
            settings.MinorVersion = 1;
            OpenTkControl.Start(settings);
            settings.RenderContinuously = false;
            InsetControl.Start(settings);
            _slowTimer.Enabled = true;
            _slowTimer.Start();
            var localDispatcher = System.Windows.Threading.Dispatcher.CurrentDispatcher;

            _slowTimer.Elapsed += delegate
            {
                localDispatcher.Invoke(() => { InsetControl.InvalidateVisual(); });
            };
        }
        
        private void OpenTkControl_OnRender(TimeSpan delta)
        {
            if(updateHue)
                _elapsedTime += delta;
            DrawStuff();
        }

        private void DrawStuff()
        {
            var c = Color4.FromHsv(new Vector4((float)_elapsedTime.TotalSeconds * 0.1f % 1, 1f, brightness, 1));
            GL.ClearColor(c);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.Enable(EnableCap.ScissorTest);
            var xPos = 50 + (ActualWidth - 250) * (0.5 + 0.5 * Math.Sin(_stopwatch.Elapsed.TotalSeconds));
            GL.Scissor((int)xPos, 25, 50, 50);
            GL.ClearColor(Color4.Blue);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.Disable(EnableCap.ScissorTest);

            GL.LoadIdentity();

            // RGB Triangle
            GL.Begin(PrimitiveType.Triangles);

            GL.Color4(1f, 0, 0, 1f);
            GL.Vertex2(0.87f, 1f);

            GL.Color4(0, 1f, 0, 1f);
            GL.Vertex2(0.87f, -0.5f);

            GL.Color4(0, 0, 1f, 1f);
            GL.Vertex2(-0.87f, -0.5f);

            GL.End();

            // Transparency check quad
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            int stepCount = 8;
            float stepsize = 1f / 8;

            // Black quad
            GL.Begin(PrimitiveType.Quads);

            GL.Color4(0, 0, 0, 1f);
            GL.Vertex2(-0.8f, 1);
            GL.Vertex2(-0.6f, 1);

            GL.Color4(0, 0, 0, 0f);
            GL.Vertex2(-0.6f, 0);
            GL.Vertex2(-0.8f, 0);

            GL.End();

            for(int i = 0; i < stepCount; i++)
            {
                GL.Begin(PrimitiveType.Quads);
                GL.Color4(0, 0, 0, i * stepsize);

                float ycord = i * stepsize;

                GL.Vertex2(-1, ycord + stepsize);
                GL.Vertex2(-0.8f, ycord + stepsize);
                GL.Vertex2(-0.8f, ycord);
                GL.Vertex2(-1f, ycord);
                GL.End();
            }

            // white quad
            GL.Begin(PrimitiveType.Quads);

            GL.Color4(1, 1, 1, 1f);
            GL.Vertex2(-0.6f, 1);
            GL.Vertex2(-0.4f, 1);

            GL.Color4(1, 1, 1, 0f);
            GL.Vertex2(-0.4f, 0);
            GL.Vertex2(-0.6f, 0);

            GL.End();


            for(int i = 0; i < stepCount; i++)
            {
                GL.Begin(PrimitiveType.Quads);
                GL.Color4(1, 1, 1, i * stepsize);

                float ycord = i * stepsize;

                GL.Vertex2(-0.4f, ycord + stepsize);
                GL.Vertex2(-0.2f, ycord + stepsize);
                GL.Vertex2(-0.2f, ycord);
                GL.Vertex2(-0.4f, ycord);
                GL.End();
            }


            GL.Disable(EnableCap.Blend);

            GL.Finish();

        }

        private void InsetControl_OnRender(TimeSpan delta)
        {
            var c = Color4.FromHsv(new Vector4((float)_elapsedTime.TotalSeconds * 0.35f % 1, 0.75f, 0.75f, 1));
            GL.ClearColor(c);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.LoadIdentity();
            GL.Begin(PrimitiveType.Triangles);

            GL.Color4(1.0f, 0.0f, 0.0f, 1.0f);
            GL.Vertex2(0.0f, 1.0f);

            GL.Color4(0.0f, 1.0f, 0.0f, 1.0f);
            GL.Vertex2(0.87f, -0.5f);

            GL.Color4(0.0f, 0.0f, 1.0f, 1.0f);
            GL.Vertex2(-0.87f, -0.5f);

            GL.End();
            GL.Finish();
        }

        private void Slider_ValueChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<double> e)
        {
            brightness = (float)e.NewValue / 10f;
        }

        private void Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
           updateHue = !updateHue;
        }
    }
}
