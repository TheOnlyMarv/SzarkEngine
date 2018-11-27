﻿using System;
using PGE;

namespace Example
{
    class RandomExample : PixelGameEngine
    {
        RandomExample() {
            WindowTitle = "Random Pixels Example";
        }

        protected override void OnUserCreate() {}

        protected override void OnUserUpdate(float fElapsedTime) {}

        protected override void OnUserRender(float fElapsedTime)
        {
            Random random = new Random();
            for (int i = 0; i < ScreenWidth; i++)
                for (int j = 0; j < ScreenHeight; j++)
                    Draw(i, j, new Pixel((byte)random.Next(255), 
                        (byte)random.Next(255), (byte)random.Next(255)));
        }

        protected override void OnUserDestroy() {}

        [STAThread]
        static void Main()
        {
            var game = new RandomExample();
            if (game.Construct(128, 128, 8, 8))
                game.Start();
        }
    }
}
