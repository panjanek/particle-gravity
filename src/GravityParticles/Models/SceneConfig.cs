using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GravityParticles.Gui;
using OpenTK.Mathematics;

namespace GravityParticles.Models
{
    public unsafe class SceneConfig
    {
        public SceneConfig()
        {
            //particles
            shaderConfig.particleCount = 150000;
            
            shaderConfig.dt = 0.1f;
            shaderConfig.constantG = 0.1f;
            shaderConfig.sigma2 = 0.1f;
            shaderConfig.clampVel = 1.0f;
            shaderConfig.clampAcc = 1.0f;

            // masses
            for (int i=0; i<16; i++)
            {
                shaderConfig.mass[i] = 1.0f;
                shaderConfig.position_x[i] = (float)(1.5 * Math.Sin(i * 2 * Math.PI / 16));
                shaderConfig.position_y[i] = (float)(1.5 * Math.Cos(i * 2 * Math.PI / 16));
            }
            shaderConfig.massCount = 3;
            shaderConfig.position_x[0] = -1;
            shaderConfig.position_y[0] = 0;
            shaderConfig.position_x[1] = 1;
            shaderConfig.position_y[1] = 0;
            shaderConfig.position_x[2] = 0;
            shaderConfig.position_y[2] = 1;

            //mode
            shaderConfig.mode = 1;
            shaderConfig.colors = 1;
            shaderConfig.markersVisible = 1;
            shaderConfig.steps = 1000;
            zoom = 100;
            shaderConfig.initPos = new Vector2(3, 2);
            shaderConfig.initR = 0.03f;
            shaderConfig.initVel = new Vector2(0.15f, 0);
            shaderConfig.initVR = 0.06f;

            ResetAll();
        }

        private void SetupParticle(Particle[] buffer, int idx)
        {
            buffer[idx].position.X = (float)(random.NextDouble() - 0.5) * shaderConfig.initR + shaderConfig.initPos.X;
            buffer[idx].position.Y = (float)(random.NextDouble() - 0.5) * shaderConfig.initR + shaderConfig.initPos.Y;
            buffer[idx].velocity.X = (float)(random.NextDouble() - 0.5) * shaderConfig.initVR + shaderConfig.initVel.X;
            buffer[idx].velocity.Y = (float)(random.NextDouble() -0.5) * shaderConfig.initVR + shaderConfig.initVel.Y;
        }

        public void ChangeParticlesCount(int newParticleCount)
        {
            lock (this)
            {
                var newParticles = new Particle[newParticleCount];
                for(int i=0; i<newParticleCount; i++)
                {
                    if (i < particles.Length)
                        newParticles[i] = particles[i];
                    else
                        SetupParticle(newParticles, i);
                }

                particles = newParticles;
                shaderConfig.particleCount = newParticleCount;
            }
        }

        public void ResetAll()
        {
            lock (this)
            {
                particles = new Particle[shaderConfig.particleCount];
                for (int i = 0; i < particles.Length; i++)
                {
                    SetupParticle(particles, i);
                }
            }
        }

        private Random random = new Random(123);

        public float zoom;

        public Vector2 center;

        public ComputeShaderConfig shaderConfig;

        public Particle[] particles;
    }
}
