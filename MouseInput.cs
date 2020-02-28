using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Text;

namespace Chip8
{
    /// <summary>
    /// Basic Mouse input class that handles mouse clicks and presses.
    /// </summary>
    public static class MouseInput
    {

        public static MouseState State;
        public static MouseState Previous;

        public static int X => State.Position.X;
        public static int Y => State.Position.Y;
        public static Point Position => State.Position;
        public static void Initialize()
        {
            State = Mouse.GetState();
        }

        public static void Update()
        {
            Previous = State;
            State = Mouse.GetState();
        }

        public static bool LeftClick()
        {
            return State.LeftButton == ButtonState.Pressed;
        }

        public static bool LeftPressed()
        {
            return LeftClick() && Previous.LeftButton == ButtonState.Released;
        }

        public static bool RightClick()
        {
            return State.RightButton == ButtonState.Pressed;
        }

        public static bool RightPressed()
        {
            return RightClick() && Previous.RightButton == ButtonState.Released;
        }
    }
}
