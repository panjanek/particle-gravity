using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Mathematics;

namespace GravityParticles.Models
{
    [StructLayout(LayoutKind.Explicit, Size=80)]
    public struct Particle
    {
        [FieldOffset(0)]
        public Vector2 position;

        [FieldOffset(8)]
        public Vector2 velocity;

        [FieldOffset(16)] public Vector2 prev_pos;        // 16–23
        [FieldOffset(24)] public Vector2 prev_vel;        // 24–31
        [FieldOffset(32)] public Vector2 prevprev_pos;    // 32–39
        [FieldOffset(40)] public Vector2 prevprev_vel;    // 40–47

        [FieldOffset(48)]
        public float mass;

        [FieldOffset(52)]
        private float _pad0;

        [FieldOffset(64)]
        public Vector4 color;
    }
}
