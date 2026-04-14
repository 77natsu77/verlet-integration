# Verlet Integration Physics Engine

A production-quality C# implementation of Verlet integration for real-time physics simulation, demonstrating numerical stability, constraint solving, and test-driven development practices.

[![CI](https://github.com/77natsu77/VerletSim/actions/workflows/ci.yml/badge.svg)](https://github.com/77natsu77/VerletSim/actions/workflows/ci.yml)

## The Problem

Traditional explicit Euler integration (`position += velocity * dt; velocity += acceleration * dt`) suffers from **energy drift**—simulated systems gain artificial energy over time, causing instability. A simple pendulum modeled with Euler integration will swing higher and higher until it breaks, even with no external energy input. This makes Euler unsuitable for long-running simulations or systems requiring energy conservation.

## Approach & Key Decisions

### Why Verlet Over Euler?

**Verlet integration is symplectic**: it conserves energy over time by design. The algorithm implicitly encodes velocity in position history (`velocity ≈ current_position - previous_position`), which makes it:

1. **Time-reversible**: Running the simulation backward recovers the original state
2. **Energy-conserving**: Bounded energy error prevents drift (unlike Euler's unbounded linear growth)
3. **Second-order accurate**: Error scales as O(dt²) vs Euler's O(dt)

**Mathematical foundation**:
```
NewPosition = CurrentPosition + (CurrentPosition - OldPosition) + Acceleration × dt²
```

This is derived from Taylor expansion and eliminates the need to store velocity explicitly—position history IS the velocity.

### Position-Based Constraints

Rather than force-based constraints (which require careful spring constant tuning), this implementation uses **iterative position correction**. Sticks don't "pull" on points—they directly adjust positions to satisfy the distance constraint. This is:
- More stable (no stiff differential equations)
- Easier to tune (no spring constants)
- Naturally convergent (geometric rather than force-based)

## Technical Architecture

```
VerletSim/
├── Point.cs          Core particle: position, velocity (implicit), acceleration
├── Stick.cs          Distance constraint solver (SHAKE-like)
├── Program.cs        Main simulation loop + console rendering
└── VerletSim.csproj

VerletSim.Tests/
├── PhysicsTests.cs         5 unit tests validating physical laws
└── VerletSim.Tests.csproj

.github/workflows/ci.yml   Automated CI/CD pipeline
```

### Class Design

**`Point`**: 
- Stores current position (X, Y) and previous position (OldX, OldY)
- Velocity is **implicit**: `v = (X - OldX) * damping`
- Supports pinning (infinite mass constraint)

**`Stick`**: 
- Constraint between two points
- Maintains rest length through iterative relaxation
- Applies equal and opposite corrections to both points

**Simulation Loop**:
1. Update all points (Verlet integration step)
2. Solve constraints 5× per frame (iterative convergence)
3. Apply boundary constraints (floor/wall collisions)

### Physics Constants

All magic numbers extracted:
- `DELTA_TIME = 0.16f` (~60 FPS)
- `GRAVITY = 20f` (in arbitrary units/s²)
- `DAMPING = 0.99f` (1% energy loss per frame)
- `CONSTRAINT_ITERATIONS = 5` (balance between accuracy and performance)

## How To Run Locally

### Prerequisites
- .NET 8.0 SDK ([download](https://dotnet.microsoft.com/download))
- Windows/macOS/Linux terminal

### Build & Run
```bash
# Clone repository
git clone https://github.com/YOUR_USERNAME/VerletSim.git
cd VerletSim

# Restore packages
dotnet restore

# Build
dotnet build

# Run simulation
dotnet run --project VerletSim

# Run tests
dotnet test
```

### Controls
- `→` Right Arrow: Apply rightward impulse
- `←` Left Arrow: Apply leftward impulse

## Testing

### Test Coverage

Five physics validation tests verify mathematical correctness:

1. **Zero Force Stability**: No artificial motion with zero input
2. **Kinematic Correctness**: Gravity displacement matches d = ½at²
3. **Constraint Satisfaction**: Stick length converges within 0.01 units
4. **Boundary Conditions**: Pinned points are immovable
5. **Collision Response**: Floor constraint prevents penetration

### Run Tests
```bash
dotnet test --verbosity normal
```

Expected output:
```
Passed! - Failed:     0, Passed:     5, Skipped:     0, Total:     5
```

## What I Would Do Next

### 1. Performance Optimization
- **Spatial hashing**: O(n²) → O(n) collision detection for large particle counts
- **SIMD vectorization**: Parallelize point updates using `System.Numerics.Vector`
- **Adaptive time stepping**: Dynamically adjust dt based on maximum velocity

### 2. Advanced Physics
- **Friction model**: Coulomb friction at constraint contacts
- **Soft constraints**: Spring-damper system for non-rigid connections
- **Collision shapes**: Convex polygons using GJK/EPA algorithms
- **Rotation**: Extend to rigid bodies with angular momentum

### 3. Visual Improvements
- **GPU rendering**: Port to Raylib/MonoGame for smooth 60 FPS graphics
- **Particle effects**: Debris, sparks on collision
- **Debug visualization**: Constraint forces, velocity vectors

### 4. Engineering
- **Benchmark suite**: Track performance regressions across commits
- **Property-based tests**: Use FsCheck to verify invariants hold for random inputs
- **Determinism**: Ensure bit-exact reproducibility across platforms

---

**Author**: Oluwajomiloju Alagbe 
**Purpose**: Degree Apprenticeship application portfolio (J.P. Morgan, Microsoft)  
**License**: MIT
