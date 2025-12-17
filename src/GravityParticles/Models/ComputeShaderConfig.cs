using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Mathematics;

namespace GravityParticles.Models
{
    [StructLayout(LayoutKind.Explicit, Size = 288)]
    public unsafe struct ComputeShaderConfig
    {
        [FieldOffset(0)] public int massCount;
        [FieldOffset(4)] public int particleCount;
        [FieldOffset(8)] public int mode;
        [FieldOffset(12)] public int colors;
        [FieldOffset(16)] public int markersVisible;
        [FieldOffset(20)] public int steps;

        // ---- floats ----
        [FieldOffset(24)] public float dt;
        [FieldOffset(28)] public float constantG;
        [FieldOffset(32)] public float sigma2;
        [FieldOffset(36)] public float clampVel;
        [FieldOffset(40)] public float clampAcc;

        // ---- vec2 (8-byte aligned) ----
        // next multiple of 8 after 44 → 48
        [FieldOffset(48)] public Vector2 initPos;
        [FieldOffset(56)] public Vector2 initVel;

        // ---- floats ----
        [FieldOffset(64)] public float initR;
        [FieldOffset(68)] public float initVR;

        // ---- arrays (std430 = tightly packed) ----
        [FieldOffset(72)] public fixed float mass[16];        // 64 bytes
        [FieldOffset(136)] public fixed float position_x[16];  // 64 bytes
        [FieldOffset(200)] public fixed float position_y[16];  // 64 bytes
    }
}
