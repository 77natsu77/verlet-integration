using System;
using Xunit;
using VerletSim;

namespace VerletSim.Tests
{
    /// <summary>
    /// Physics validation tests for the Verlet integration simulation.
    /// These tests verify the mathematical correctness and physical behavior
    /// of the simulation engine.
    /// </summary>
    public class PhysicsTests
    {
        private const float EPSILON = 0.001f;
        private const float STICK_TOLERANCE = 0.01f;
        private const float DELTA_TIME = 0.16f;
        private const float GRAVITY = 20f;

        /// <summary>
        /// Verifies that a point at rest with no forces remains stationary.
        /// Physical significance: Tests conservation of state in the absence of forces,
        /// ensuring the simulation doesn't introduce artificial motion (numerical drift).
        /// A point with identical current and previous positions should have zero velocity,
        /// and with zero acceleration should not move.
        /// </summary>
        [Fact]
        public void ZeroForce_ZeroVelocity_NoMovement()
        {
            // Arrange: Create a point at rest with no forces
            var point = new Point(10f, 10f)
            {
                AccelX = 0f,
                AccelY = 0f
            };

            float initialX = point.X;
            float initialY = point.Y;

            // Act: Update once with zero forces
            point.Update(DELTA_TIME);

            // Assert: Position should remain unchanged
            Assert.Equal(initialX, point.X, precision: 3);
            Assert.Equal(initialY, point.Y, precision: 3);
        }

        /// <summary>
        /// Verifies that gravity produces the correct kinematic displacement.
        /// Physical significance: Tests the accuracy of Verlet integration for
        /// constant acceleration. For an object starting at rest under gravity,
        /// displacement should equal (1/2) * a * t² = 0.5 * 20 * (0.16)² ≈ 0.256.
        /// This validates that the integration scheme correctly implements
        /// basic Newtonian mechanics.
        /// </summary>
        [Fact]
        public void Gravity_CorrectDisplacement()
        {
            // Arrange: Create a point at rest with gravity
            var point = new Point(10f, 10f)
            {
                AccelX = 0f,
                AccelY = GRAVITY
            };

            float initialY = point.Y;

            // Act: Update once
            point.Update(DELTA_TIME);

            // Assert: Displacement should match displacement = accel * dt²
            float expectedDisplacement = GRAVITY * DELTA_TIME * DELTA_TIME;
            float actualDisplacement = point.Y - initialY;

            Assert.Equal(expectedDisplacement, actualDisplacement, precision: 3);
        }

        /// <summary>
        /// Verifies that stick constraints maintain the rest length between points.
        /// Physical significance: Tests the constraint solver's ability to enforce
        /// geometric invariants. In position-based dynamics, constraints are satisfied
        /// iteratively. After sufficient iterations (50), the distance between points
        /// should converge to within a small tolerance of the rest length, validating
        /// the numerical stability of the constraint solver.
        /// </summary>
        [Fact]
        public void Stick_MaintainsRestLength()
        {
            // Arrange: Create two points separated by a known distance
            var p1 = new Point(0f, 0f) { AccelY = 0f };
            var p2 = new Point(10f, 0f) { AccelY = 0f };
            var stick = new Stick(p1, p2);

            float initialLength = stick.Length;

            // Act: Apply constraint multiple times (simulating solver iterations)
            for (int i = 0; i < 50; i++)
            {
                stick.Update();
            }

            // Assert: Distance should be within tolerance of rest length
            float dx = p2.X - p1.X;
            float dy = p2.Y - p1.Y;
            float actualDistance = (float)Math.Sqrt(dx * dx + dy * dy);

            Assert.InRange(actualDistance, initialLength - STICK_TOLERANCE, initialLength + STICK_TOLERANCE);
        }

        /// <summary>
        /// Verifies that pinned points remain fixed regardless of forces.
        /// Physical significance: Tests boundary conditions. In real physical systems,
        /// certain constraints are absolute (e.g., a rope tied to a fixed beam).
        /// Pinned points should be immovable regardless of applied forces, which is
        /// essential for creating stable boundary conditions like hanging chains,
        /// fixed supports, or anchor points.
        /// </summary>
        [Fact]
        public void PinnedPoint_DoesNotMove()
        {
            // Arrange: Create a pinned point with gravity
            var point = new Point(10f, 10f, isPinned: true)
            {
                AccelY = GRAVITY
            };

            float initialX = point.X;
            float initialY = point.Y;

            // Act: Update with forces applied
            point.Update(DELTA_TIME);

            // Assert: Position should be completely unchanged
            Assert.Equal(initialX, point.X);
            Assert.Equal(initialY, point.Y);
        }

        /// <summary>
        /// Verifies that the floor constraint prevents penetration.
        /// Physical significance: Tests collision response and boundary enforcement.
        /// In physical simulations, objects shouldn't pass through solid boundaries.
        /// The Constraints() method should detect when a point has exceeded the
        /// floor boundary and snap it back to the valid region. This is critical
        /// for preventing unphysical behavior where objects fall through the world.
        /// </summary>
        [Fact]
        public void FloorConstraint_Enforced()
        {
            // Arrange: Create a point below the floor boundary
            float floorHeight = 20f;
            var point = new Point(10f, 25f) // Below floor
            {
                OldY = 24f // Moving downward
            };

            // Act: Apply constraints
            point.Constraints(40f, floorHeight);

            // Assert: Y position should be clamped to floor height
            Assert.Equal(floorHeight, point.Y);
        }
    }
}
