# 🧱 Verlet Integration Physics — Console Simulation

> A real-time, constraint-based rigid-body physics engine built from scratch in C# — rendered entirely in the terminal.

This project implements **Verlet Integration**, the same numerical method used by professional game physics engines (Havok, PhysX, Bullet), to simulate a rigid square under gravity, collision, and user-applied forces — all in a 40×20 ASCII console viewport.

---

## ⚙️ Technical Depth

### 1. Verlet Integration (The Core Formula)

Classical physics engines store `position` and `velocity` separately. Verlet Integration eliminates explicit velocity, deriving it implicitly from positional history:

```
NewPosition = CurrentPosition + (CurrentPosition - OldPosition) + Acceleration × Δt²
```

**Why this is powerful:**
- Velocity is *implicit* — it emerges from `(X - OldX)`, making the simulation naturally stable
- Energy dissipation is trivial: multiply the velocity term by `< 1.0` (e.g., `0.99f`) for damping
- Constraints can be applied *directly to positions* without destabilising the integrator

In code (`Point.Update`):
```csharp
float vx = (X - OldX) * 0.99f; // Implicit velocity with 1% damping
OldX = X;
X += vx + AccelX * (deltaTime * deltaTime);
```

---

### 2. Constraint Solving (Position-Based Dynamics)

Each `Stick` represents a **distance constraint** between two points. Every frame, sticks measure their current length vs. their rest length and *push the endpoints apart or together* to correct the error:

```
difference = (RestLength - CurrentLength) / CurrentLength
offsetX    = dx × difference × 0.5
```

Both endpoints are nudged by half the correction each — a **symmetric, mass-equal projection**.

**The stability trick:** This correction is run **5 times per frame** (the iteration loop). More iterations = stiffer, more accurate constraints. This is the foundation of **XPBD (Extended Position-Based Dynamics)**, the algorithm behind cloth and soft-body physics in modern AAA games.

---

### 3. Rigid Body from Particles

A rigid square is not a single object — it is **4 particles + 6 sticks**:

```
P0 ---S0--- P1
|  \      / |
S3   S4  S5  S1
|      \/   |
P3 ---S2--- P2
```

The 4 outer sticks form the shell. The **2 diagonal braces (S4, S5) are critical** — without them, the square collapses into a rhombus because the shell alone is geometrically underdetermined. This is the computational equivalent of structural triangulation in civil engineering.

---

### 4. Collision Response via Verlet

When a point breaches the floor boundary, the engine doesn't apply an impulse — it *repositions* `OldY` to create an outward velocity on the next frame:

```csharp
float vy = Y - OldY;          // Capture downward velocity
Y = height;                    // Snap to boundary
OldY = Y + (vy * 1.25f);      // Exaggerate OldY → net upward velocity next frame
```

The `1.25f` multiplier gives a slight bounce coefficient `> 1.0` for a satisfying visual pop. Wall collisions use `0.5f` for a more damped response.

---

### 5. Impulse via Position Manipulation

User keypresses deliver a "kick" by directly modifying `OldX` for all points:

```csharp
foreach (var p in points) p.OldX -= 5.0f; // Right arrow kick
```

Subtracting from `OldX` means the integrator sees a large `(X - OldX)` delta next frame — equivalent to instantaneously setting a horizontal velocity of `5.0` units/frame. No force accumulation needed.

---

## 🚀 Installation & Usage

### Prerequisites
- [.NET SDK](https://dotnet.microsoft.com/download) (tested on .NET 6+)
- A terminal with at least **40 columns × 20 rows**

### Run

```bash
git clone https://github.com/77natsu77/verlet-integration-console.git
cd verlet-integration-console
dotnet run
```

### Controls

| Key | Action |
|-----|--------|
| `→` Right Arrow | Kick the square to the right |
| `←` Left Arrow  | Kick the square to the left |
| `Ctrl+C`        | Exit simulation |

### Tweak the Physics

All key parameters are at the top of `Program.cs`:

| Parameter | Location | Effect |
|-----------|----------|--------|
| `0.99f` damping | `Point.Update()` | Energy loss per frame (1.0 = frictionless) |
| `1.25f` bounce | `Point.Constraints()` | Floor bounciness |
| `AccelY = 20f` | `Point` constructor | Gravitational acceleration |
| `5` iterations | Main loop | Constraint solver passes (more = stiffer) |
| `0.16f` deltaTime | `p.Update(0.16f)` | Simulation timestep (~60fps) |

---

## 🧠 Learning Outcomes

Building this project developed mastery of:

- **Numerical Integration** — Understanding why Euler integration diverges and how Verlet's position-history approach achieves stability without storing velocity state
- **Constraint-Based Physics** — Implementing iterative position projection (the core of PBD/XPBD) and understanding the trade-off between iteration count and performance
- **Rigid Body Simulation from First Principles** — Discovering that rigidity is an *emergent property* of correctly braced particle systems, not a primitive concept
- **Collision Response without Impulses** — Engineering bouncing and wall reflection purely through positional manipulation of the Verlet history
- **Console Rendering** — Using `Console.SetCursorPosition`, line drawing via parametric interpolation (`t = i / segments`), and frame timing for smooth animation
- **Structural Engineering Intuition** — The diagonal brace requirement mirrors real-world triangulation (why triangles are the only geometrically stable polygon)

---

## 📐 Architecture

```
Program.cs
├── Main()          — Simulation loop: input → update → render
├── Point           — Particle with Verlet state (X, Y, OldX, OldY, Accel)
│   ├── Update()    — Verlet integration step
│   └── Constraints() — Boundary collision response
├── Stick           — Distance constraint between two Points
│   └── Update()    — Symmetric position projection
└── DrawStick()     — Bresenham-style line rasterisation to console
```

---

## 🔭 Extensions & Ideas

- [ ] Add a **hanging rope** (chain of points, no diagonals needed)
- [ ] Implement **mouse interaction** to drag points
- [ ] Port to a graphics context (WinForms / MonoGame) for a 2D renderer
- [ ] Add **breakable constraints** (sticks that snap beyond a stretch threshold)
- [ ] Simulate **cloth** as a 2D grid of particles and sticks

---

## 📚 Further Reading

- [Jakobsen, T. (2001) — *Advanced Character Physics* (GDC Paper)](https://www.cs.cmu.edu/afs/cs/academic/class/15462-s13/www/lec_slides/Jakobsen.pdf) — The original paper this engine is based on
- [Matthias Müller — Position Based Dynamics](https://matthias-research.github.io/pages/publications/posBasedDyn.pdf)
- [The Coding Train — Verlet Physics (YouTube)](https://www.youtube.com/c/TheCodingTrain)

---

*Built as a self-directed exploration of numerical methods and game physics fundamentals.*
