using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Forms.Integration;
using GravityParticles.Models;
using OpenTK.GLControl;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using Application = System.Windows.Application;
using Panel = System.Windows.Controls.Panel;

namespace GravityParticles.Gui
{
    public unsafe class OpenGlRenderer
    {
        public const double ZoomingSpeed = 0.0005;
        public int FrameCounter => frameCounter;

        private Panel placeholder;

        private WindowsFormsHost host;

        private GLControl glControl;

        private SceneConfig scene;

        private int? draggedMassIdx;

        private bool draggedInitRegion; 

        private readonly int ubo;

        private readonly int maxGroupsX;

        private int computeProgram;

        private int renderProgram;

        private int pointsCount;

        private int frameCounter;

        private int pointsBuffer;

        private int initBuffer;

        private int dummyVao;

        private int projLocation;

        private OpenTK.Mathematics.Matrix4 projectionMatrix;
     
        public OpenGlRenderer(Panel placeholder)
        {
            this.placeholder = placeholder;
            this.placeholder = placeholder;
            host = new WindowsFormsHost();
            host.Visibility = Visibility.Visible;
            host.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            host.VerticalAlignment = VerticalAlignment.Stretch;
            glControl = new GLControl(new GLControlSettings
            {
                API = OpenTK.Windowing.Common.ContextAPI.OpenGL,
                APIVersion = new Version(3, 3), // OpenGL 3.3
                Profile = ContextProfile.Compatability,
                Flags = ContextFlags.Default,
                IsEventDriven = false
            });
            glControl.Dock = DockStyle.Fill;
            host.Child = glControl;
            placeholder.Children.Add(host);
            glControl.Paint += GlControl_Paint;

            //setup required features
            GL.Enable(EnableCap.ProgramPointSize);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.One);
            GL.BlendEquation(OpenTK.Graphics.OpenGL.BlendEquationMode.FuncAdd);
            GL.Enable(EnableCap.PointSprite);

            // allocate space for ComputeShaderConfig passed to each compute shader
            ubo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ShaderStorageBuffer, ubo);
            int configSizeInBytes = Marshal.SizeOf<ComputeShaderConfig>();
            GL.BufferData(BufferTarget.ShaderStorageBuffer, configSizeInBytes, IntPtr.Zero, BufferUsageHint.DynamicDraw);
            GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 0, ubo);
            GL.GetInteger((OpenTK.Graphics.OpenGL.GetIndexedPName)All.MaxComputeWorkGroupCount, 0, out maxGroupsX);

            computeProgram = ShaderUtil.CompileAndLinkComputeShader("solver.comp");
            renderProgram = ShaderUtil.CompileAndLinkRenderShader("shader.vert", "shader.frag");
            projLocation = GL.GetUniformLocation(renderProgram, "projection");
            if (projLocation == -1)
                throw new Exception("Uniform 'projection' not found. Shader optimized it out?");

            glControl.MouseWheel += GlControl_MouseWheel;
            var dragging = new DraggingHandler(glControl, mousePos =>
            {
                var initRegionPixelCoord = GuiUtil.ProjectToScreen(scene.shaderConfig.initPos, projectionMatrix, glControl.Width, glControl.Height);
                if (GuiUtil.IsInSquare(mousePos, initRegionPixelCoord, 10))
                {
                    draggedMassIdx = null;
                    draggedInitRegion = true;
                    return true;
                }

                for (int i=0; i<scene.shaderConfig.massCount; i++)
                {
                    var massPosition = new Vector2(scene.shaderConfig.position_x[i], scene.shaderConfig.position_y[i]);
                    var massPixelCoord = GuiUtil.ProjectToScreen(massPosition, projectionMatrix, glControl.Width, glControl.Height);
                    if (GuiUtil.IsInSquare(mousePos, massPixelCoord, 10))
                    {
                        draggedMassIdx = i;
                        draggedInitRegion = false;
                        return true;
                    }
                }

                return true;
            }, (prev, curr) =>
            {
                var delta = (curr - prev) / scene.zoom;
                delta.Y = -delta.Y;
                if (draggedInitRegion)
                    scene.shaderConfig.initPos = new Vector2(scene.shaderConfig.initPos.X + delta.X, scene.shaderConfig.initPos.Y + delta.Y);
                else if (draggedMassIdx.HasValue)
                {
                    scene.shaderConfig.position_x[draggedMassIdx.Value] = scene.shaderConfig.position_x[draggedMassIdx.Value] + delta.X;
                    scene.shaderConfig.position_y[draggedMassIdx.Value] = scene.shaderConfig.position_y[draggedMassIdx.Value] + delta.Y;
                }
                else
                    scene.center -= delta;
               
            }, () => { draggedMassIdx = null; draggedInitRegion = false; });
        }

        private void GlControl_MouseWheel(object? sender, MouseEventArgs e)
        {
            var pos = new Vector2(e.X, e.Y);
            float zoomRatio = (float)(1.0 + ZoomingSpeed * e.Delta);

            var topLeft1 = GuiUtil.ScreenToWorld(new Vector2(0,0), projectionMatrix, glControl.Width, glControl.Height);
            var bottomRight1 = GuiUtil.ScreenToWorld(new Vector2(glControl.Width, glControl.Height), projectionMatrix, glControl.Width, glControl.Height);
            var zoomCenter = GuiUtil.ScreenToWorld(pos, projectionMatrix, glControl.Width, glControl.Height);

            var currentSize = bottomRight1 - topLeft1;
            var newSize = currentSize / (float)zoomRatio;

            var c = zoomCenter - topLeft1;
            var b = c / (float)zoomRatio;

            var topLeft2 = zoomCenter - b;
            var bottomRight2 = topLeft2 + newSize;

            scene.center = (bottomRight2 + topLeft2) / 2;
            scene.zoom = scene.zoom * zoomRatio;
        }

        private void GlControl_Paint(object? sender, PaintEventArgs e)
        {
            if (scene != null)
            {
                GL.Clear(ClearBufferMask.ColorBufferBit);

                GL.UseProgram(renderProgram);
                GL.BindVertexArray(dummyVao);
                projectionMatrix = GetProjectionMatrix();
                GL.UniformMatrix4(projLocation, false, ref projectionMatrix);
                GL.DrawArrays(PrimitiveType.Points, 0, pointsCount);


                glControl.SwapBuffers();
                frameCounter++;
            }
        }

        public void Draw(SceneConfig scene)
        {
            if (Application.Current.MainWindow.WindowState == System.Windows.WindowState.Minimized)
                return;
 
            this.scene = scene;
            lock (scene)
            {
                if (pointsCount == 0 || pointsCount != scene.shaderConfig.particleCount + scene.shaderConfig.massCount + 1)
                    SetupBuffers();

                //upload config
                GL.BindBuffer(BufferTarget.ShaderStorageBuffer, ubo);
                GL.BufferSubData(
                    BufferTarget.ShaderStorageBuffer,
                    IntPtr.Zero,
                    Marshal.SizeOf<ComputeShaderConfig>(),
                    ref scene.shaderConfig
                );
            }

            //compute
            GL.UseProgram(computeProgram);
            int dispatchGroupsX = (pointsCount + ShaderUtil.LocalSizeX - 1) / ShaderUtil.LocalSizeX;
            if (dispatchGroupsX > maxGroupsX)
                dispatchGroupsX = maxGroupsX;
            GL.DispatchCompute(dispatchGroupsX, 1, 1);
            GL.MemoryBarrier(MemoryBarrierFlags.ShaderStorageBarrierBit);

            glControl.Invalidate();
        }

        private void SetupBuffers()
        {
            // create dummy vao
            GL.GenVertexArrays(1, out dummyVao);
            GL.BindVertexArray(dummyVao);

            // create buffer for points data
            if (pointsBuffer > 0)
            {
                GL.DeleteBuffer(pointsBuffer);
                pointsBuffer = 0;
            }
            GL.GenBuffers(1, out pointsBuffer);
            GL.BindBuffer(BufferTarget.ShaderStorageBuffer, pointsBuffer);
            pointsCount = scene.shaderConfig.particleCount + scene.shaderConfig.massCount + 1;
            int shaderPointStrideSize = Marshal.SizeOf<Particle>();
            GL.BufferData(BufferTarget.ShaderStorageBuffer, pointsCount * shaderPointStrideSize, IntPtr.Zero, BufferUsageHint.DynamicDraw);
            GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 1, pointsBuffer);
            //upload initial data
            GL.BindBuffer(BufferTarget.ShaderStorageBuffer, pointsBuffer);
            GL.BufferSubData(BufferTarget.ShaderStorageBuffer, 0, scene.shaderConfig.particleCount * shaderPointStrideSize, scene.particles);

            // create buffer for initial particle data
            if (initBuffer > 0)
            {
                GL.DeleteBuffer(initBuffer);
                initBuffer = 0;
            }
            GL.GenBuffers(1, out initBuffer);
            GL.BindBuffer(BufferTarget.ShaderStorageBuffer, initBuffer);
            GL.BufferData(BufferTarget.ShaderStorageBuffer, scene.shaderConfig.particleCount * Marshal.SizeOf<Particle>(), scene.particles, BufferUsageHint.StaticDraw);
            GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 2, initBuffer);

            GL.Viewport(0, 0, glControl.Width, glControl.Height);
            glControl.Invalidate();
        }

        private Matrix4 GetProjectionMatrix()
        {
            // rescale by windows display scale setting to match WPF coordinates
            var w = (float)((glControl.Width / GuiUtil.Dpi.DpiScaleX) / scene.zoom) / 2;
            var h = (float)((glControl.Height / GuiUtil.Dpi.DpiScaleY) / scene.zoom) / 2;
            var translate = Matrix4.CreateTranslation(-scene.center.X, -scene.center.Y, 0.0f);
            var ortho = Matrix4.CreateOrthographicOffCenter(-w, w, -h, h, -1f, 1f);
            var matrix = translate * ortho;
            return matrix;
        }
    }
}
