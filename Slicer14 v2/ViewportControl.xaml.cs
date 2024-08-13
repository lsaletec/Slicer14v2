using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using HelixToolkit.Wpf.SharpDX;

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
    }
}