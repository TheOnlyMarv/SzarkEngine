﻿/*
	PixelGameEngine.cs
	
	License (MIT):

    Copyright (c) 2018 Jakub Szarkowicz

    Permission is hereby granted, free of charge, to any person obtaining a copy
    of this software and associated documentation files (the "Software"), to deal
    in the Software without restriction, including without limitation the rights
    to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
    copies of the Software, and to permit persons to whom the Software is
    furnished to do so, subject to the following conditions:

    The above copyright notice and this permission notice shall be included in all
    copies or substantial portions of the Software.

    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
    IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
    FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
    AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
    LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
    OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
    SOFTWARE.
*/

using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;

namespace PGE
{
    /// <summary>
    /// The main engine with all Open GL and drawing
    /// methods. Derive from this class to access the engine.
    /// </summary>
    public abstract class PixelGameEngine
    {
        public string WindowTitle { get; protected set; }

        public int WindowWidth { get; private set; }
        public int WindowHeight { get; private set; }

        public int ScreenWidth { get; private set; }
        public int ScreenHeight { get; private set; }
        public int PixelSize { get; private set; }

        public int RenderOffsetX { get; private set; }
        public int RenderOffsetY { get; private set; }

        public int CurrentFPS { get; private set; }
        public int BaseShaderID { get; private set; }

        public bool ShowFPS { get; set; } = true;
        public bool IsFullscreen { get; private set; }

        public Action AdditionalUpdates { get; set; }

        private double lastFPSCheck;
        private GameWindow gameWindow;
        private SpriteRenderer pixelGraphics;
        private Graphics2D graphics;
        private Sprite background;

        private const string vertexShader = 
        @"
            #version 400 

            layout(location = 0) in vec2 pos;
            layout(location = 1) in vec2 tex;

            out vec2 texCoord;

            uniform mat4 projection;
            uniform mat4 model;
            uniform mat4 rotation;
            uniform mat4 scale;

            void main() 
            {
                texCoord = tex;
                gl_Position = projection * scale * model * rotation * vec4(pos.x, pos.y, 0, 1.0);
            }
        ";

        private const string fragmentShader = 
        @"
            #version 400

            out vec4 FragColor;
            in vec2 texCoord;
            uniform sampler2D tex;

            void main() {
                FragColor = texture(tex, texCoord);
            } 
        ";

        /// <summary>
        /// Creates a window and starts OpenGL.
        /// </summary>
        /// <param name="width">Width of the Window</param>
        /// <param name="height">Height of the Window</param>
        /// <param name="pixelSize">Size of Each Pixel</param>
        public void Construct(int width = 800, int height = 800, int pixelSize = 8)
        {
            WindowWidth = width;
            WindowHeight = height;
            PixelSize = pixelSize;

            gameWindow = new GameWindow(WindowWidth, WindowHeight)
            {
                VSync = VSyncMode.Off,
                Title = WindowTitle
            };

            gameWindow.Load += Loaded;
            gameWindow.RenderFrame += Render;
            gameWindow.UpdateFrame += Update;
            gameWindow.Disposed += Disposed;

            ScreenWidth = WindowWidth / PixelSize;
            ScreenHeight = WindowHeight / PixelSize;

            background = new Sprite(ScreenWidth, ScreenHeight);
            graphics = new Graphics2D(ScreenWidth, ScreenHeight);
            background.Clear(Pixel.BLACK);

            GL.Enable(EnableCap.Blend);
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.Texture2D);

            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, 
                BlendingFactorDest.OneMinusSrcAlpha);

            BaseShaderID = ShaderLoader.CreateProgram(vertexShader, fragmentShader);
            pixelGraphics = new SpriteRenderer(this, graphics.DrawTarget, BaseShaderID);

            Audio.Init();
            Input.SetContext(this, gameWindow);

            gameWindow.WindowBorder = WindowBorder.Fixed;
            gameWindow.Run();
        }

        #region Events

        // On Window Loaded
        private void Loaded(object sender, EventArgs e) => Start();

        // On Window Disposed
        private void Disposed(object sender, EventArgs e) => Destroyed();

        // On Window Update
        private void Update(object sender, FrameEventArgs e) =>
            Update((float)e.Time);

        // On Window Render Frame
        private void Render(object sender, FrameEventArgs e)
        {
            GL.Viewport(RenderOffsetX, RenderOffsetY, WindowWidth, WindowHeight);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.ClearColor(0, 0, 0, 1);

            background.CopyTo(graphics.DrawTarget);
            pixelGraphics.Render(0, 0, 0, 1, -99, true);

            Draw(graphics, (float)e.Time);
            pixelGraphics.Refresh();
            GPUDraw((float)e.Time);

            if ((lastFPSCheck += e.Time) > 1)
            {
                CurrentFPS = (int)(1 / e.Time);
                gameWindow.Title = $"{WindowTitle} " + (ShowFPS ? 
                    $"| FPS: {CurrentFPS}" : "");
                lastFPSCheck = 0;
            }

            AdditionalUpdates?.Invoke();
            gameWindow.SwapBuffers();
        }

        #endregion

        #region Extra

        /// <summary>
        /// Sets the mode for VSync
        /// </summary>
        /// <param name="isActive">Is On?</param>
        public void SetVSync(VSyncMode mode) => 
            gameWindow.VSync = mode;

        /// <summary>
        /// Sets the window to be fullscreen
        /// </summary>
        /// <param name="fullscreen">Is Fullscreen?</param>
        public void SetFullscreen(bool fullscreen)
        {
            IsFullscreen = fullscreen;
            gameWindow.WindowState = fullscreen ? WindowState.Fullscreen :
                WindowState.Normal;

            RenderOffsetX = fullscreen ? (gameWindow.Width - WindowWidth) / 2 : 0;
            RenderOffsetY = fullscreen ? (gameWindow.Height - WindowHeight) / 2 : 0;
        }

        /// <summary>
        /// Sets the background color
        /// </summary>
        /// <param name="color">The Color</param>
        public void SetBackgroundColor(Pixel color) =>
            background.Clear(color);

        #endregion

        #region Abstractions

        /// <summary>
        /// Called when window is opened, use for initialization
        /// </summary>
        protected virtual void Start() {}

        /// <summary>
        /// Called every tick, use for logic
        /// </summary>
        /// <param name="deltaTime">Delta Time</param>
        protected virtual void Update(float deltaTime) {}

        /// <summary>
        /// Called every frame, use for drawing
        /// </summary>
        /// <param name="graphics">The Graphics</param>
        /// <param name="deltaTime">Delta Time</param>
        protected virtual void Draw(Graphics2D graphics, float deltaTime) {}

        /// <summary>
        /// Called every frame, used for drawing GPU Sprites, Shapes, etc.
        /// </summary>
        /// <param name="deltaTime">Delta Time</param>
        protected virtual void GPUDraw(float deltaTime) {}

        /// <summary>
        /// Called when window is closing, use for cleanup
        /// </summary>
        protected virtual void Destroyed() {}

        #endregion
    }
}