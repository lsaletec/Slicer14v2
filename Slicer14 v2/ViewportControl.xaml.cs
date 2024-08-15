using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media.Media3D;
using ProjectionCamera = HelixToolkit.Wpf.SharpDX.ProjectionCamera;

namespace Slicer14_v2
{
    public partial class ViewportControl : UserControl
    {
        public ViewportControl()
        {
            InitializeComponent();
        }

        // Pass MouseDown event to ViewModel
        private void Viewport_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is MainViewModel vm)
            {
                vm.OnMouseDown(sender, e);
            }
        }
        private Point _lastMousePosition;
        private bool _isPanning;
        private void Viewport_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.MiddleButton== MouseButtonState.Pressed)
            {
                Console.WriteLine("MiddleButton Down");
                _isPanning = true;
                _lastMousePosition = e.GetPosition(viewport);
                viewport.CaptureMouse();
            }

            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Viewport_MouseLeftButtonDown(sender, e);
            }
            
        }
        private void Viewport_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isPanning)
            {
                var currentPosition = e.GetPosition(viewport);
                var delta = currentPosition - _lastMousePosition;

                // Adjust the pan sensitivity if necessary
                double panSpeed = 0.01;

                if (viewport.Camera is ProjectionCamera camera)
                {
                    // Calculate the pan offset
                    var offsetX = -delta.X * panSpeed;
                    var offsetY = delta.Y * panSpeed;

                    // Pan the camera (move its position)
                    var right = Vector3D.CrossProduct(camera.LookDirection, camera.UpDirection);
                    right.Normalize();

                    var up = camera.UpDirection;
                    up.Normalize();

                    var panOffset = right * offsetX + up * offsetY;

                    camera.Position += panOffset;
                    camera.LookDirection += panOffset;
                }

                _lastMousePosition = currentPosition;
            }
        }

        private void Viewport_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (_isPanning && e.MiddleButton == MouseButtonState.Released)
            {
                _isPanning = false;
                viewport.ReleaseMouseCapture();
            }
        }
        
    }
    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return boolValue ? Visibility.Visible : Visibility.Collapsed;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}