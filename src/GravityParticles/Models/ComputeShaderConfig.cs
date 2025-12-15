using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Mathematics;

namespace GravityParticles.Models
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct ComputeShaderConfig
    {
        public int massCount;

        public int particleCount;

        public int mode;

        public int steps;

        public float dt;

        public float constantG;

        public float sigma2;

        public float clampVel;

        public float clampAcc;

        public fixed float mass[16];

        public fixed float position_x[16];

        public fixed float position_y[16];
    }
}
