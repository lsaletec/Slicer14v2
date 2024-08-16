using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using HelixToolkit.Wpf.SharpDX;
using ProjectionCamera = HelixToolkit.Wpf.SharpDX.ProjectionCamera;

namespace Slicer14_v2
{
    public partial class ViewportControl : UserControl
    {
        public ViewportControl()
        {
            InitializeComponent();
            this.DataContextChanged += ViewportControl_DataContextChanged;

            if (manipulator != null && manipulator.Target != null)
            {
                SubscribeToTransformChanges(manipulator.Target);
            }

            // Optionally, handle target changes
            DependencyPropertyDescriptor dpd = DependencyPropertyDescriptor.FromProperty(TransformManipulator3D.TargetProperty, typeof(TransformManipulator3D));
            if (dpd != null)
            {
                dpd.AddValueChanged(manipulator, (s, e) =>
                {
                    var newTarget = manipulator.Target;
                    if (newTarget != null)
                    {
                        SubscribeToTransformChanges(newTarget);
                    }
                });
            }
        }

        private void SubscribeToTransformChanges(Element3D target)
        {
            // Subscribe to changes in the Transform property
            DependencyPropertyDescriptor dpd = DependencyPropertyDescriptor.FromProperty(Element3D.TransformProperty, typeof(Element3D));
            if (dpd != null)
            {
                dpd.AddValueChanged(target, OnTransformChanged);
            }
        }

        private void OnTransformChanged(object sender, EventArgs e)
        {
            // Log the transformation change
            var target = sender as Element3D;
            if (target != null)
            {
                UpdateBoundingBox();
            }
        }
        
        private LineGeometryModel3D boundingBoxLines;
        private void UpdateBoundingBox()
        {
            var viewModel = DataContext as MainViewModel;

            // Remove existing bounding box
            RemoveBoundingBox();

            // Check if ShowBoundingBox is true
            if (viewModel?.ShowBoundingBox == true && viewModel?.SelectedModel is MeshGeometryModel3D model)
            {
                // Apply the model's transform to the bounding box
                var boundingBox = BoundingBoxExtensions.FromPoints(model.Geometry.Positions);
                boundingBox = boundingBox.Transform(model.Transform.Value.ToMatrix());

                // Generate the bounding box lines using LineBuilder
                var lineGeometry = LineBuilder.GenerateBoundingBox(boundingBox);

                // Create the LineGeometryModel3D
                boundingBoxLines = new LineGeometryModel3D
                {
                    Geometry = lineGeometry,
                    Color = Color.FromArgb(255, 255, 255, 255),
                    Thickness = 1.0
                };

                // Add directly to the viewport's Items to avoid making it selectable
                viewport.Items.Add(boundingBoxLines);
            }
        }



        private void RemoveBoundingBox()
        {
            if (boundingBoxLines != null)
            {
                viewport.Items.Remove(boundingBoxLines);
                boundingBoxLines = null;
            }
        }
        
        private void ViewportControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is MainViewModel viewModel)
            {
                viewModel.PropertyChanged += ViewModel_PropertyChanged;
            }
        }

        private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(MainViewModel.SelectedModel) || e.PropertyName == nameof(MainViewModel.ShowBoundingBox))
            {
                UpdateBoundingBox();
            }
        }

        
        // Pass MouseDown event to ViewModel
        private void Viewport_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is MainViewModel vm)
            {
                vm.OnMouseDown(sender, e);
            }
        }
        private void FindSelected()
        {
            if (DataContext is MainViewModel vm)
            {

                if (vm.SelectedModel != null)
                {
                    int index = 0;
                    for (int i = 0; i < vm.Models.Count; i++)
                    {
                        if (vm.Models[i].Tag == vm.SelectedModel.Tag)
                        {
                            Console.WriteLine($"Found model {vm.Models[i].Tag}");
                            (vm.Models[i] as MeshGeometryModel3D).PostEffects = "border[color:#00FFDE]";
                        }
                    }
                }
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