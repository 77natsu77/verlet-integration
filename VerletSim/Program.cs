using System;
using System.Collections.Generic;
using System.Threading;

namespace VerletSim
{
    internal class Program
    {
        static void Main(string[] args)
        {
            bool showSticks = true;

            List<Point> points = new List<Point>();
            List<Stick> sticks = new List<Stick>();

            // Create a simple 3-point hanging chain
            /*points.Add(new Point(10, 0, true)); // Pinned at top
             points.Add(new Point(15, 0));
             points.Add(new Point(20, 0));

            sticks.Add(new Stick(points[0], points[1]));
            sticks.Add(new Stick(points[1], points[2]));*/

            //Creating a square
            //  Create Points
            points.Add(new Point(10, 5));  // P0
            points.Add(new Point(20, 5));  // P1
            points.Add(new Point(20, 15)); // P2
            points.Add(new Point(10, 15)); // P3

            //  Create Outer Shell
            sticks.Add(new Stick(points[0], points[1]));
            sticks.Add(new Stick(points[1], points[2]));
            sticks.Add(new Stick(points[2], points[3]));
            sticks.Add(new Stick(points[3], points[0]));

            //  Create Diagonal Braces (if commented out, the square will collaspe)
            sticks.Add(new Stick(points[0], points[2]));
            sticks.Add(new Stick(points[1], points[3]));

            while (true)
            {
                //User interaction
                if (Console.KeyAvailable)
                {
                    var key = Console.ReadKey(true).Key;
                    if (key == ConsoleKey.RightArrow)
                    {
                        // Give the square a "kick" to the right
                        foreach (var p in points) p.OldX -= 5.0f;
                    }
                    else if (key == ConsoleKey.LeftArrow)
                    {
                        // Give the square a "kick" to the left
                        foreach (var p in points) p.OldX += 5.0f;
                    }
                }

                //  Update Points
                foreach (var p in points) p.Update(0.16f); // ~60fps simulation step

                //  Update Sticks (Multiple iterations for stability)
                for (int i = 0; i < 5; i++)
                {
                    foreach (var s in sticks) s.Update();
                    foreach (var p in points) p.Constraints(38, 18); // Console dimensions
                }

                //  Render
                Console.Clear();

                if (showSticks)
                {
                    Console.ForegroundColor = ConsoleColor.DarkGray; // Make the sticks subtle
                    foreach (var s in sticks) DrawStick(s, '.');
                    Console.ResetColor();
                }

                foreach (var p in points)
                {
                    int drawX = (int)Clamp(p.X, 0, 39);
                    int drawY = (int)Clamp(p.Y, 0, 19);
                    Console.SetCursorPosition(drawX, drawY);
                    Console.Write("X");
                }

                Thread.Sleep(33); // Small delay to see the movement
            }
        }

        /// <summary>
        /// Clamps a value between a minimum and maximum.
        /// </summary>
        public static float Clamp(float value, float min, float max)
        {
            return (value < min) ? min : (value > max) ? max : value;
        }

        static void DrawStick(Stick s, char segmentChar = '-')
        {
            float dx = s.P2.X - s.P1.X;
            float dy = s.P2.Y - s.P1.Y;
            float distance = (float)Math.Sqrt(dx * dx + dy * dy);

            // How many points to draw? One per console unit of distance is usually enough.
            int segments = (int)distance;

            for (int i = 1; i < segments; i++)
            {
                // t represents the percentage along the line (0.0 to 1.0)
                float t = (float)i / segments;

                int x = (int)Math.Round(s.P1.X + dx * t);
                int y = (int)Math.Round(s.P1.Y + dy * t);

                if (x >= 0 && x < 40 && y >= 0 && y < 20)
                {
                    Console.SetCursorPosition(x, y);
                    Console.Write(segmentChar);
                }
            }
        }
    }
}