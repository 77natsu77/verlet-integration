using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Verlet_Integration
{
    internal class Program
    {
        static void Main(string[] args)
        {
            bool showSticks = true;

            List<Point> points = new List<Point>();
            List<Stick> sticks = new List<Stick>();

            // Create a simple 3-point hanging chain
            //points.Add(new Point(10, 0, true)); // Pinned at top
            // points.Add(new Point(15, 0));
            // points.Add(new Point(20, 0));

            //sticks.Add(new Stick(points[0], points[1]));
            //sticks.Add(new Stick(points[1], points[2]));

            //Creating a square
            // 1. Create Points
            points.Add(new Point(10, 5));  // P0
            points.Add(new Point(20, 5));  // P1
            points.Add(new Point(20, 15)); // P2
            points.Add(new Point(10, 15)); // P3

            // 2. Create Outer Shell
            sticks.Add(new Stick(points[0], points[1]));
            sticks.Add(new Stick(points[1], points[2]));
            sticks.Add(new Stick(points[2], points[3]));
            sticks.Add(new Stick(points[3], points[0]));

            // 3. Create Diagonal Braces (if commented out, the square will collaspe)
            sticks.Add(new Stick(points[0], points[2]));
            sticks.Add(new Stick(points[1], points[3]));

            while (true)
            {
                //User interaction!
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

                // 1. Update Points
                foreach (var p in points) p.Update(0.16f); // ~60fps simulation step

                // 2. Update Sticks (Multiple iterations for stability!)
                for (int i = 0; i < 5; i++)
                {
                    foreach (var s in sticks) s.Update();
                    foreach (var p in points) p.Constraints(38,18); // Console dimensions
                }

                // 3. Render
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

        public static float Clamp(float value, float min, float max)//My version of nath.clamp since this version of NET doesn't have it
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
        class Point
        {
            public float X, Y;
            public float OldX, OldY;
            public float AccelX, AccelY;
            public bool IsPinned; // Useful for the top of a rope!

            public Point(float x, float y, bool isPinned = false)
            {
                X = OldX = x;
                Y = OldY = y;
                IsPinned = isPinned;
                AccelX = 0;
                AccelY = 20f; //Gravity!
            }

            public void Update(float deltaTime)
            {
                if (IsPinned) return;

                // The Magic Formula:
                // NewPos = CurrentPos + (CurrentPos - OldPos) + Acceleration * (dt * dt)
                float vx = (X - OldX) * 0.99f; // 1% energy loss per frame
                float vy = (Y - OldY) * 0.99f; ;

                OldX = X;
                OldY = Y;

                X += vx + AccelX * (deltaTime * deltaTime);
                Y += vy + AccelY * (deltaTime * deltaTime);
            }

            public void Constraints(float width, float height)
            {
                // Floor Collision
                if (Y > height)
                {
                    // Calculate velocity BEFORE snapping
                    float vy = Y - OldY;
                    Y = height;
                    // Increase the bounce multiplier to 1.25f for a more obvious jump
                    OldY = Y + (vy * 1.25f);
                }

                // Walls
                if (X > width)
                {
                    float vx = X - OldX;
                    X = width;
                    OldX = X + (vx * 0.5f);
                }
                else if (X < 0)
                {
                    float vx = X - OldX;
                    X = 0;
                    OldX = X + (vx * 0.5f);
                }
            }
        }

        class Stick
        {
            public Point P1, P2;
            public float Length;

            public Stick(Point p1, Point p2)
            {
                P1 = p1;
                P2 = p2;
                // Calculate the initial length as the "resting" length
                float dx = P2.X - P1.X;
                float dy = P2.Y - P1.Y;
                Length = (float)Math.Sqrt(dx * dx + dy * dy);
            }

            public void Update()
            {
                float dx = P2.X - P1.X;
                float dy = P2.Y - P1.Y;
                float distance = (float)Math.Sqrt(dx * dx + dy * dy);

                // Avoid division by zero
                if (distance == 0) return;

                float difference = (Length - distance) / distance;
                float offsetX = dx * difference * 0.5f;
                float offsetY = dy * difference * 0.5f;

                if (!P1.IsPinned)
                {
                    P1.X -= offsetX;
                    P1.Y -= offsetY;
                }
                if (!P2.IsPinned)
                {
                    P2.X += offsetX;
                    P2.Y += offsetY;
                }
            }
        }
    }
}
