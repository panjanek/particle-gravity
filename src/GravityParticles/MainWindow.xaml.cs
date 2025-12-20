using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using GravityParticles.Gui;
using GravityParticles.Models;
using Microsoft.WindowsAPICodePack.Dialogs;

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
            renderer = new OpenGlRenderer(placeholder, scene);
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

            if (e.Key >= Key.D1 && e.Key <= Key.D9)
            {
                scene.shaderConfig.massCount = (int)e.Key - (int)Key.D1 + 1;
                PopupMessage.Show(this, $"planets : {scene.shaderConfig.massCount}");
                return;
            }

            switch (e.Key)
            {
                case Key.Q:
                    if (scene.shaderConfig.particleCount > 10000)
                    {
                        scene.ChangeParticlesCount(scene.shaderConfig.particleCount - particlesDiv20);
                        PopupMessage.Show(this, $"Particle count decreased to {GuiUtil.FormatBigNumber(scene.shaderConfig.particleCount)}");
                    }
                    break;
                case Key.W:
                    {
                        scene.ChangeParticlesCount(scene.shaderConfig.particleCount + particlesDiv20);
                        PopupMessage.Show(this, $"Particle count increased to {GuiUtil.FormatBigNumber(scene.shaderConfig.particleCount)}");
                    }
                    break;
                case Key.A:
                    if (scene.shaderConfig.steps > 100 && scene.shaderConfig.mode == 1)
                    {
                        scene.shaderConfig.steps -= stepsDiv10;
                        PopupMessage.Show(this, $"attractor steps : {scene.shaderConfig.steps}");
                    }
                    break;
                case Key.S:
                    if (scene.shaderConfig.mode == 1)
                    {
                        scene.shaderConfig.steps += stepsDiv10;
                        PopupMessage.Show(this, $"attractor steps : {scene.shaderConfig.steps}");
                    }
                    break;
                case Key.Z:
                    if (scene.shaderConfig.massCount > 1)
                    {
                        scene.shaderConfig.massCount--;
                        PopupMessage.Show(this, $"planets : {scene.shaderConfig.massCount}");
                    }
                    break;
                case Key.X:
                    if (scene.shaderConfig.massCount < 15)
                    { 
                        scene.shaderConfig.massCount++;
                        PopupMessage.Show(this, $"planets : {scene.shaderConfig.massCount}");
                    }
                    break;
                case Key.OemMinus:
                    scene.shaderConfig.dt *= 0.9f;
                    PopupMessage.Show(this, $"dt = {scene.shaderConfig.dt.ToString("0.000")}");
                    break;
                case Key.OemPlus:
                    scene.shaderConfig.dt *= 1.1f;
                    PopupMessage.Show(this, $"dt = {scene.shaderConfig.dt.ToString("0.000")}");
                    break;
                case Key.M:
                    scene.shaderConfig.mode = 1 - scene.shaderConfig.mode;
                    PopupMessage.Show(this, scene.shaderConfig.mode == 0 ? "Simulation mode" : "Attractor mode");
                    if (scene.shaderConfig.mode == 1) renderer.Paused = false;
                    scene.ResetAll();
                    renderer.SetupBuffers();
                    break;
                case Key.C:
                    scene.shaderConfig.colors = 1 - scene.shaderConfig.colors;
                    PopupMessage.Show(this, scene.shaderConfig.colors == 0 ? "White particles" : "Particle color depends on velocity direction");
                    break;
                case Key.H:
                    scene.shaderConfig.markersVisible = 1 - scene.shaderConfig.markersVisible;
                    PopupMessage.Show(this, scene.shaderConfig.markersVisible == 1 ? "Planets and init region visible" : "Planets and init region hidden");
                    break;
                case Key.R:
                    if (scene.shaderConfig.mode == 0)
                    {
                        PopupMessage.Show(this, $"restarted");
                        scene.ResetAll();
                        renderer.SetupBuffers();
                    }
                    break;
                case Key.F:
                    ToggleFullscreen();
                    break;
                case Key.Space:
                    if (scene.shaderConfig.mode == 0)
                    {
                        renderer.Paused = !renderer.Paused;
                        PopupMessage.Show(this, renderer.Paused ? "paused" : "resumed");
                    }
                    break;
                case Key.P:
                    var dialog = new CommonSaveFileDialog { Title = "Select filename to save capture PNG", DefaultExtension = "png" };
                    dialog.Filters.Add(new CommonFileDialogFilter("PNG files", "*.png"));
                    if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
                    {
                        renderer.SaveToFile(dialog.FileName);
                        PopupMessage.Show(this, $"Capture saved to {dialog.FileName}");
                    }
                    break;
            }

            e.Handled = true;
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
                        $"mode:{(scene.shaderConfig.mode == 0 ? "[simulation]" : $"[attractor] steps:{scene.shaderConfig.steps}")} " +
                        $"particles:{scene.shaderConfig.particleCount} " +
                        $"dt:{scene.shaderConfig.dt.ToString("0.0000")} ";

                lastCheckFrameCount = renderer.FrameCounter;
                lastCheckTime = now;
            }
        }

        private void Help_Click(object sender, RoutedEventArgs e)
        {
        }

        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

        }
    }
}