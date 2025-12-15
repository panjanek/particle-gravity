using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Mathematics;

namespace GravityParticles.Models
{
    [StructLayout(LayoutKind.Explicit, Size = 48)]
    public struct Particle
    {
        [FieldOffset(0)]
        public Vector2 position;

        [FieldOffset(8)]
        public Vector2 velocity;

        [FieldOffset(16)]
        public float mass;

        [FieldOffset(20)]
        private float _pad0;

        [FieldOffset(32)]
        public Vector4 color;
    }
}
