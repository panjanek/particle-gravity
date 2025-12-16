using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using GravityParticles.Gui;
using GravityParticles.Models;

namespace GravityParticles
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private OpenGlRenderer renderer;

        private SceneConfig scene;

        private FullscreenWindow fullscreen;

        private bool uiPending;

        private DateTime lastCheckTime;

        private long lastCheckFrameCount;
        public MainWindow()
        {
            InitializeComponent();
            scene = new SceneConfig();
        }

        private void parent_Loaded(object sender, RoutedEventArgs e)
        {
            renderer = new OpenGlRenderer(placeholder);
            KeyDown += MainWindow_KeyDown;
            System.Timers.Timer systemTimer = new System.Timers.Timer() { Interval = 10 };
            systemTimer.Elapsed += SystemTimer_Elapsed;
            systemTimer.Start();
            DispatcherTimer infoTimer = new DispatcherTimer() { Interval = TimeSpan.FromSeconds(1.0) };
            infoTimer.Tick += InfoTimer_Tick;
            infoTimer.Start();
        }

        private void MainWindow_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            int particlesDiv20 = scene.shaderConfig.particleCount / 20;
            int stepsDiv10 = scene.shaderConfig.steps / 50;
            switch (e.Key)
            {
                case Key.Q:
                    if (scene.shaderConfig.particleCount > 10000)
                    {
                        scene.ChangeParticlesCount(scene.shaderConfig.particleCount - particlesDiv20);
                        PopupMessage.Show(this, $"Particle count decreased to {scene.shaderConfig.particleCount}");
                    }
                    break;
                case Key.W:
                    {
                        scene.ChangeParticlesCount(scene.shaderConfig.particleCount + particlesDiv20);
                        PopupMessage.Show(this, $"Particle count increased to {scene.shaderConfig.particleCount}");
                    }
                    break;
                case Key.A:
                    if (scene.shaderConfig.steps > 100 && scene.shaderConfig.mode == 1)
                    {
                        scene.shaderConfig.steps -= stepsDiv10;
                        PopupMessage.Show(this, $"steps : {scene.shaderConfig.steps}");
                    }
                    break;
                case Key.S:
                    if (scene.shaderConfig.mode == 1)
                    {
                        scene.shaderConfig.steps += stepsDiv10;
                        PopupMessage.Show(this, $"steps : {scene.shaderConfig.steps}");
                    }
                    break;
                case Key.Z:
                    if (scene.shaderConfig.massCount > 1)
                        scene.shaderConfig.massCount--;
                    break;
                case Key.X:
                    if (scene.shaderConfig.massCount < 15)
                        scene.shaderConfig.massCount++;
                    break;
                case Key.O:
                    scene.shaderConfig.dt *= 0.9f;
                    break;
                case Key.P:
                    scene.shaderConfig.dt *= 1.1f;
                    break;
                case Key.M:
                    scene.shaderConfig.mode = 1 - scene.shaderConfig.mode;
                    scene.ResetAll();
                    renderer.SetupBuffers();
                    break;
                case Key.F:
                    ToggleFullscreen();
                    break;
            }
        }

        private void ToggleFullscreen()
        {
            if (fullscreen == null)
            {
                parent.Children.Remove(placeholder);
                fullscreen = new FullscreenWindow();
                fullscreen.KeyDown += MainWindow_KeyDown;
                fullscreen.ContentHost.Content = placeholder;
                fullscreen.ShowDialog();
            }
            else
            {
                fullscreen.ContentHost.Content = null;
                parent.Children.Add(placeholder);
                fullscreen.Close();
                fullscreen = null;
            }
        }

        private void SystemTimer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {
            if (!uiPending)
            {
                uiPending = true;
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    try
                    {
                        renderer.Draw(scene);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                    }
                    finally
                    {
                        uiPending = false;
                    }

                    uiPending = false;
                }), DispatcherPriority.Render);
            }
        }

        private void InfoTimer_Tick(object? sender, EventArgs e)
        {
            var now = DateTime.Now;
            var timespan = now - lastCheckTime;
            double frames = renderer.FrameCounter - lastCheckFrameCount;
            if (timespan.TotalSeconds >= 0.0001)
            {
                double fps = frames / timespan.TotalSeconds;
                Title = $"GravityParticles. " +
                        $"fps:{fps.ToString("0.0")} " +
                        $"particles:{scene.shaderConfig.particleCount} " +
                        $"steps:{scene.shaderConfig.steps} " +
                        $"dt:{scene.shaderConfig.dt.ToString("0.0000")} ";

                lastCheckFrameCount = renderer.FrameCounter;
                lastCheckTime = now;
            }
        }
    }
}