using System;

namespace VerletSim
{
    /// <summary>
    /// Represents a point mass in the Verlet integration simulation.
    /// Uses position-based Verlet integration for stable physics.
    /// </summary>
    public class Point
    {
        /// <summary>
        /// Current X position.
        /// </summary>
        public float X;

        /// <summary>
        /// Current Y position.
        /// </summary>
        public float Y;

        /// <summary>
        /// Previous X position (used for velocity calculation).
        /// </summary>
        public float OldX;

        /// <summary>
        /// Previous Y position (used for velocity calculation).
        /// </summary>
        public float OldY;

        /// <summary>
        /// X-axis acceleration.
        /// </summary>
        public float AccelX;

        /// <summary>
        /// Y-axis acceleration (gravity).
        /// </summary>
        public float AccelY;

        /// <summary>
        /// Whether this point is fixed in space.
        /// </summary>
        public bool IsPinned;

        /// <summary>
        /// Initializes a new point at the specified position.
        /// </summary>
        /// <param name="x">Initial X position.</param>
        /// <param name="y">Initial Y position.</param>
        /// <param name="isPinned">Whether the point is fixed in space.</param>
        public Point(float x, float y, bool isPinned = false)
        {
            X = OldX = x;
            Y = OldY = y;
            IsPinned = isPinned;
            AccelX = 0;
            AccelY = 20f; // Gravity!
        }

        /// <summary>
        /// Updates the point's position using Verlet integration.
        /// </summary>
        /// <param name="deltaTime">Time step for the simulation.</param>
        public void Update(float deltaTime)
        {
            if (IsPinned) return;

            // The Magic Formula:
            // NewPos = CurrentPos + (CurrentPos - OldPos) + Acceleration * (dt * dt)
            float vx = (X - OldX) * 0.99f; // 1% energy loss per frame
            float vy = (Y - OldY) * 0.99f; // Bug fix: parentheses required for correct damping

            OldX = X;
            OldY = Y;

            X += vx + AccelX * (deltaTime * deltaTime);
            Y += vy + AccelY * (deltaTime * deltaTime);
        }

        /// <summary>
        /// Applies boundary constraints to keep the point within the simulation area.
        /// </summary>
        /// <param name="width">Maximum X coordinate.</param>
        /// <param name="height">Maximum Y coordinate.</param>
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
}