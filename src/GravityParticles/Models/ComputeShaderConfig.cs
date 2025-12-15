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
        [FieldOffset(12)] public int steps;

        [FieldOffset(16)] public float dt;
        [FieldOffset(20)] public float constantG;
        [FieldOffset(24)] public float sigma2;
        [FieldOffset(28)] public float clampVel;
        [FieldOffset(32)] public float clampAcc;

        // padding to align vec2 to 8 bytes
        [FieldOffset(40)] public Vector2 initPos;
        [FieldOffset(48)] public Vector2 initVel;

        [FieldOffset(56)] public float initR;
        [FieldOffset(60)] public float initVR;

        // arrays start at 64
        [FieldOffset(64)] public fixed float mass[16];
        [FieldOffset(128)] public fixed float position_x[16];
        [FieldOffset(192)] public fixed float position_y[16];
    }
}
