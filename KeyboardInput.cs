using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Text;

namespace Chip8
{
    /// <summary>
    /// Basic Keyboard input class that handles key presses.
    /// </summary>
    public static class KeyboardInput
    {
        private static KeyboardState State;
        private static KeyboardState Previous;

        public static void Initialize()
        {
            State = Keyboard.GetState();
        }

        public static void Update()
        {
            Previous = State;
            State = Keyboard.GetState();
        }

        public static bool Check(Keys key)
        {
            return State.IsKeyDown(key);
        }

        public static bool CheckPressed(Keys key)
        {
            return Check(key) && !Previous.IsKeyDown(key);
        }
    }
}
