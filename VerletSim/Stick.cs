using System;

namespace VerletSim
{
    /// <summary>
    /// Represents a distance constraint between two points.
    /// Maintains a fixed length between points using iterative relaxation.
    /// </summary>
    public class Stick
    {
        /// <summary>
        /// First endpoint of the stick.
        /// </summary>
        public Point P1;

        /// <summary>
        /// Second endpoint of the stick.
        /// </summary>
        public Point P2;

        /// <summary>
        /// Rest length of the stick.
        /// </summary>
        public float Length;

        /// <summary>
        /// Initializes a stick between two points.
        /// The rest length is calculated from the initial distance.
        /// </summary>
        /// <param name="p1">First point.</param>
        /// <param name="p2">Second point.</param>
        public Stick(Point p1, Point p2)
        {
            P1 = p1;
            P2 = p2;
            // Calculate the initial length as the "resting" length
            float dx = P2.X - P1.X;
            float dy = P2.Y - P1.Y;
            Length = (float)Math.Sqrt(dx * dx + dy * dy);
        }

        /// <summary>
        /// Updates the stick constraint by adjusting both points to maintain the rest length.
        /// Uses iterative position correction.
        /// </summary>
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