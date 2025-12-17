using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using OpenTK.Mathematics;
using Button = System.Windows.Controls.Button;
using HorizontalAlignment = System.Windows.HorizontalAlignment;
using Orientation = System.Windows.Controls.Orientation;
using Panel = System.Windows.Controls.Panel;
using TextBox = System.Windows.Controls.TextBox;

namespace GravityParticles.Gui
{
    public static class GuiUtil
    {
        private static DpiScale? dpi;

        public static DpiScale Dpi
        {
            get
            {
                if (!dpi.HasValue)
                    dpi = VisualTreeHelper.GetDpi(System.Windows.Application.Current.MainWindow);

                return dpi.Value;
            }
        }
        public static void HsvToRgb(int h, out int r, out int g, out int b)
        {
            if (h < 0)
                h = 0;

            if (h > 255)
                h = 255;

            // Convert 0–255 hue to 0–1535 (6 * 256)
            int x = h * 6;            // range 0–1530
            int sector = x >> 8;      // fast divide by 256 → 0..5
            int frac = x & 255;     // fractional part in [0..255]

            // For V=1 and S=1, intermediate values:
            int q = 255 - frac;
            int t = frac;

            switch (sector)
            {
                case 0: r = 255; g = t; b = 0; return;
                case 1: r = q; g = 255; b = 0; return;
                case 2: r = 0; g = 255; b = t; return;
                case 3: r = 0; g = q; b = 255; return;
                case 4: r = t; g = 0; b = 255; return;
                default:
                case 5: r = 255; g = 0; b = q; return;
            }
        }

        public static bool IsInSquare(Vector2 point, Vector2 center, double r)
        {
            return (point.X >= center.X - r) && (point.X <= center.X + r) && (point.Y >= center.Y - r) && (point.Y <= center.Y + r);
        }

        public static Vector2 ProjectToScreen(Vector2 position, Matrix4 projectionMatrix, int viewportWidth, int viewportHeight)
        {
            Vector4 p = new Vector4(position.X, position.Y, 0.0f, 1.0f);
            //Vector4 clip = Vector4.Transform(p, projectionMatrix);
            Vector4 clip = new Vector4(
                p.X * projectionMatrix.M11 + p.Y * projectionMatrix.M21 + p.Z * projectionMatrix.M31 + p.W * projectionMatrix.M41,
                p.X * projectionMatrix.M12 + p.Y * projectionMatrix.M22 + p.Z * projectionMatrix.M32 + p.W * projectionMatrix.M42,
                p.X * projectionMatrix.M13 + p.Y * projectionMatrix.M23 + p.Z * projectionMatrix.M33 + p.W * projectionMatrix.M43,
                p.X * projectionMatrix.M14 + p.Y * projectionMatrix.M24 + p.Z * projectionMatrix.M34 + p.W * projectionMatrix.M44
            );

            if (clip.W == 0.0f)
                return Vector2.Zero;

            Vector3 ndc = clip.Xyz / clip.W;

            float x = (ndc.X * 0.5f + 0.5f) * viewportWidth;
            float y = viewportHeight - (ndc.Y * 0.5f + 0.5f) * viewportHeight;  // vertical flip

            return new Vector2(x, y);
        }

        public static Vector2 ScreenToWorld(Vector2 pixel, Matrix4 projection, int viewportWidth, int viewportHeight)
        {
            float x = (pixel.X / viewportWidth) * 2.0f - 1.0f;
            float y = 1.0f - (pixel.Y / viewportHeight) * 2.0f;  // vertical flip

            Vector4 clip = new Vector4(x, y, 0.0f, 1.0f);

            Matrix4 invProj = projection.Inverted();
            //Vector4 worldH = Vector4.Transform(clip, invProj);
            Vector4 worldH = Multiply(invProj, clip);

            if (worldH.W == 0.0f)
                return Vector2.Zero;

            return worldH.Xy / worldH.W;
        }

        public static Vector4 Multiply(Matrix4 m, Vector4 v)
        {
            return new Vector4(
                m.M11 * v.X + m.M21 * v.Y + m.M31 * v.Z + m.M41 * v.W,
                m.M12 * v.X + m.M22 * v.Y + m.M32 * v.Z + m.M42 * v.W,
                m.M13 * v.X + m.M23 * v.Y + m.M33 * v.Z + m.M43 * v.W,
                m.M14 * v.X + m.M24 * v.Y + m.M34 * v.Z + m.M44 * v.W
            );
        }

        public static void ShowMessage(string text, string title)
        {
            // Window
            Window dialog = new Window()
            {
                Width = 400,
                Height = 360,
                Title = title,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                ResizeMode = ResizeMode.NoResize,
                WindowStyle = WindowStyle.ToolWindow,
                Owner = System.Windows.Application.Current.MainWindow
            };

            // Layout
            StackPanel panel = new StackPanel() { Margin = new Thickness(10) };

            TextBlock txt = new TextBlock() { Text = text, Margin = new Thickness(0, 0, 0, 10) };

            // Buttons
            StackPanel buttonPanel = new StackPanel()
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, 10, 0, 0)
            };

            Button ok = new Button() { Content = "OK", Width = 70, Margin = new Thickness(5, 0, 0, 0) };

            ok.Click += (s, e) => { dialog.DialogResult = true; dialog.Close(); };

            buttonPanel.Children.Add(ok);

            // Compose UI
            panel.Children.Add(txt);
            panel.Children.Add(buttonPanel);

            dialog.Content = panel;

            // Show input dialog
            dialog.ShowDialog();
        }

        public static string FormatBigNumber(int value)
        {
            var culture = (CultureInfo)CultureInfo.InvariantCulture.Clone();
            culture.NumberFormat.NumberGroupSeparator = " ";
            return value.ToString("N0", culture);
        }

    }
}
