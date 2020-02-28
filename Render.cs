using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chip8
{
    /// <summary>
    /// Renderer class that reveals various pixel-based methods globally. Based on the `Draw` class of the Monogle game engine.
    /// See `https://bitbucket.org/MattThorson/monocle-engine/src/default/Monocle/Util/Draw.cs`
    /// </summary>
    public class Render
    {
        public static Texture2D Pixel;

        public static SpriteBatch SpriteBatch { get; set; }

        private static Rectangle rect;

        public static void Initialize(GraphicsDevice graphicsDevice)
        {
            SpriteBatch = new SpriteBatch(graphicsDevice);
            Pixel = new Texture2D(graphicsDevice, 1, 1);
            Color[] colors = new Color[1];
            colors[0] = Color.White;
            Pixel.SetData(colors);
        }

        public static void Point(Vector2 at, Color color)
        {
            SpriteBatch.Draw(Pixel, at, null, color, 0, Vector2.Zero, 1f, SpriteEffects.None, 0);
        }

        #region Line

        public static void Line(Vector2 start, Vector2 end, Color color)
        {
            LineAngle(start, Calc.Angle(start, end), Vector2.Distance(start, end), color);
        }

        public static void Line(Vector2 start, Vector2 end, Color color, float thickness)
        {
            LineAngle(start, Calc.Angle(start, end), Vector2.Distance(start, end), color, thickness);
        }

        public static void Line(float x1, float y1, float x2, float y2, Color color)
        {
            Line(new Vector2(x1, y1), new Vector2(x2, y2), color);
        }

        public static void Line(float x1, float y1, float x2, float y2, Color color, float t)
        {
            Line(new Vector2(x1, y1), new Vector2(x2, y2), color, t);
        }

        #endregion

        #region Line Angle

        public static void LineAngle(Vector2 start, float angle, float length, Color color)
        {
            SpriteBatch.Draw(Pixel, start, null, color, angle, Vector2.Zero, new Vector2(length, 1), SpriteEffects.None, 0);
        }

        public static void LineAngle(Vector2 start, float angle, float length, Color color, float thickness)
        {
            SpriteBatch.Draw(Pixel, start, null, color, angle, new Vector2(0, .5f), new Vector2(length, thickness), SpriteEffects.None, 0);
        }

        public static void LineAngle(float startX, float startY, float angle, float length, Color color)
        {
            LineAngle(new Vector2(startX, startY), angle, length, color);
        }

        #endregion

        #region Circle

        public static void Circle(Vector2 position, float radius, Color color, int resolution)
        {
            Vector2 last = Vector2.UnitX * radius;
            Vector2 lastP = last.Perpendicular();
            for (int i = 1; i <= resolution; i++)
            {
                Vector2 at = Calc.AngleToVector(i * MathHelper.PiOver2 / resolution, radius);
                Vector2 atP = at.Perpendicular();

                Render.Line(position + last, position + at, color);
                Render.Line(position - last, position - at, color);
                Render.Line(position + lastP, position + atP, color);
                Render.Line(position - lastP, position - atP, color);

                last = at;
                lastP = atP;
            }
        }

        public static void Circle(float x, float y, float radius, Color color, int resolution)
        {
            Circle(new Vector2(x, y), radius, color, resolution);
        }

        public static void Circle(Vector2 position, float radius, Color color, float thickness, int resolution)
        {
            Vector2 last = Vector2.UnitX * radius;
            Vector2 lastP = last.Perpendicular();
            for (int i = 1; i <= resolution; i++)
            {
                Vector2 at = Calc.AngleToVector(i * MathHelper.PiOver2 / resolution, radius);
                Vector2 atP = at.Perpendicular();

                Render.Line(position + last, position + at, color, thickness);
                Render.Line(position - last, position - at, color, thickness);
                Render.Line(position + lastP, position + atP, color, thickness);
                Render.Line(position - lastP, position - atP, color, thickness);

                last = at;
                lastP = atP;
            }
        }

        public static void Circle(float x, float y, float radius, Color color, float thickness, int resolution)
        {
            Circle(new Vector2(x, y), radius, color, thickness, resolution);
        }

        #endregion

        #region Rect

        public static void Rect(float x, float y, float width, float height, Color color)
        {
            rect.X = (int)x;
            rect.Y = (int)y;
            rect.Width = (int)width;
            rect.Height = (int)height;
            SpriteBatch.Draw(Pixel, rect, null, color);
        }

        public static void Rect(Vector2 position, float width, float height, Color color)
        {
            Rect(position.X, position.Y, width, height, color);
        }

        public static void Rect(Rectangle rect, Color color)
        {
            Render.rect = rect;
            SpriteBatch.Draw(Pixel, rect, null, color);
        }

        #endregion

        #region Hollow Rect

        public static void HollowRect(float x, float y, float width, float height, Color color)
        {
            rect.X = (int)x;
            rect.Y = (int)y;
            rect.Width = (int)width;
            rect.Height = 1;

            SpriteBatch.Draw(Pixel, rect, null, color);

            rect.Y += (int)height - 1;

            SpriteBatch.Draw(Pixel, rect, null, color);

            rect.Y -= (int)height - 1;
            rect.Width = 1;
            rect.Height = (int)height;

            SpriteBatch.Draw(Pixel, rect, null, color);

            rect.X += (int)width - 1;

            SpriteBatch.Draw(Pixel, rect, null, color);
        }

        public static void HollowRect(Vector2 position, float width, float height, Color color)
        {
            HollowRect(position.X, position.Y, width, height, color);
        }

        public static void HollowRect(Rectangle rect, Color color)
        {
            HollowRect(rect.X, rect.Y, rect.Width, rect.Height, color);
        }

        #endregion

        #region Weird Stuff

        public static void Function(Func<float, float> func, Vector2 origin, int max, Color color)
        {
            Vector2 from;
            Vector2 to = new Vector2(0, func(0)) + origin;

            Line(origin, origin + Vector2.UnitX * max, Color.Black);

            for (int x = 5; x < max; x += 1)
            {
                from = to;
                to = new Vector2(x, func(x)) + origin;

                Line(from, to, color);
            }

            from = to;
            to = new Vector2(max, func(max)) + origin;

            Line(from, to, color);
        }

        public static void PointFunction(Func<float, Vector2> func, Vector2 origin, int max, Color color)
        {
            Vector2 from;
            Vector2 to = func(0) + origin;

            for (int x = 5; x < max; x += 4)
            {
                from = to;
                to = func(x) + origin;

                Line(from, to, color);
            }

            from = to;
            to = func(max) + origin;

            Line(from, to, color);
        }

        #endregion

        public static void Begin()
        {
            SpriteBatch.Begin();
        }

        public static void End()
        {
            SpriteBatch.End();
        }
    }
}
