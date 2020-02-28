using Chip8.Engine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace Chip8
{
    /// <summary>
    /// Main class that handles updating everything else.
    /// </summary>
    public class Controller : Game
    {
        GraphicsDeviceManager graphics;
        public static Controller Instance;

        private CPU cpu = new CPU();

        private const int scale = 10;

        public Controller()
        {
            Instance = this;
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = 64 * scale;
            graphics.PreferredBackBufferHeight = 32 * scale;
            Content.RootDirectory = "Content";
            IsMouseVisible = true;            
        }

        protected override void Initialize()
        {
            Render.Initialize(graphics.GraphicsDevice);
            MouseInput.Initialize();
            KeyboardInput.Initialize();
            base.Initialize();
            Console.WriteLine("engine initialized");
            cpu.Initialize();
        }        

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            base.Update(gameTime);
            cpu.Cycle();
            MouseInput.Update();
            KeyboardInput.Update();
                
        }

        protected override void Draw(GameTime gameTime)
        {
            if (!cpu.DrawFlag)
                return;

            GraphicsDevice.Clear(Color.Black);
            Render.Begin();

            for (int x = 0; x < 64; x++)
            {
                for (int y = 0; y < 32; y++)
                {
                    if (cpu.Get(x, y))
                    {
                        Render.Rect(new Vector2(x * scale, y * scale), scale, scale, Color.White);
                    }
                }
            }

            Render.End();
            base.Draw(gameTime);
        }        
    }
}