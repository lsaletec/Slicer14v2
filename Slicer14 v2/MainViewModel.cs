using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows.Media.Media3D;
using Assimp;
using SharpDX;
using Vector3D = System.Windows.Media.Media3D.Vector3D;

namespace Slicer14_v2;

using HelixToolkit.Wpf.SharpDX;
using System.ComponentModel; // INotifyPropertyChanged

public class MainViewModel : INotifyPropertyChanged
{
    public EffectsManager EffectsManager { get; }
    public Camera Camera { get; }

    private bool _showBoundingBox;
    public bool ShowBoundingBox
    {
        get => _showBoundingBox;
        set
        {
            if (_showBoundingBox != value)
            {
                _showBoundingBox = value;
                OnPropertyChanged(nameof(ShowBoundingBox));
            }
        }
    }

    public ObservableCollection<Element3D> Models { get; } = new ObservableCollection<Element3D>();
    public ICommand LoadModelCommand { get; }
    public ICommand DeleteModelCommand { get; }
    public ICommand ResetTransformsCommand { get; }
    
    protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    
    private bool _isManipulatorVisible;
    public bool IsManipulatorVisible
    {
        get => _isManipulatorVisible;
        set
        {
            if (_isManipulatorVisible != value)
            {
                _isManipulatorVisible = value;
            }
        }
    }
    private Selection _currentSelection;
    public Selection CurrentSelection
    {
        get => _currentSelection;
        set
        {
                _currentSelection = value;
        }
    }

    private ModelLoader _modelLoader;
    
    public MainViewModel()
    {
        EffectsManager = new DefaultEffectsManager();
        Camera = new PerspectiveCamera
        {
            Position = new Point3D(10, 10, 10),
            LookDirection = new Vector3D(-10, -10, -10),
            UpDirection = new Vector3D(0, 1, 0)
        };
        
        LoadModelCommand = new RelayCommand(LoadModel);
        DeleteModelCommand = new RelayCommand(DeleteSelectedModel);
        _currentSelection = new Selection();
        _modelLoader = new ModelLoader(Models);
    }

    public event PropertyChangedEventHandler PropertyChanged;
    
    private Point _lastMousePosition;
    private bool _isPanning;
    private void DeleteSelectedModel()
    {
        if (_currentSelection != null)
        {
            foreach (var model in _currentSelection.SelectedObjects)
            {
                Models.Remove(model);
            }
        }
    }
    public void OnMouseDown(object sender, MouseButtonEventArgs e)
    {
        var viewport = sender as Viewport3DX;
        if (viewport != null)
        { 
            var hits = viewport.FindHits(e.GetPosition(viewport));
            if (hits.Count>0)
            { 
                var isShiftPressed = Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift);
               foreach (var hit in hits)
               {
                   // Check if the hit object is a MeshGeometryModel3D
                   if (hit.ModelHit is MeshGeometryModel3D hitModel)
                   {
                       // Check if the Models collection contains a model with the same Tag
                       foreach (var model in Models)
                       {
                           if (model is MeshGeometryModel3D meshModel && Equals(meshModel.Tag, hitModel.Tag))
                           {
                               _currentSelection.HandleSelection(meshModel,isShiftPressed);
                               OnPropertyChanged(nameof(CurrentSelection));
                           }
                       }
                       break; // Stop searching hits after finding a MeshGeometryModel3D
                   }
               } 
            }
            else
            {
                _currentSelection.Clear(); 
            }
            UpdateManipulator();
        }
    }

    private void UpdateManipulator()
    {
        // Check if the selected model is a MeshGeometryModel3D
        if (CurrentSelection.groupModel3D.Children.Count> 0)
        {
            // Calculate the center of the model's bounds
            var center = _currentSelection.selectionCenter.Bounds.Center();
            Console.WriteLine($"center {center}");
          
            // Set the manipulator's position to the center
            ManipulatorPosition = new TranslateTransform3D(center.X, center.Y, center.Z);
          
            // Make the manipulator visible
            IsManipulatorVisible = true;  
        }
        else
        {
            IsManipulatorVisible = false;   
        }
      
        OnPropertyChanged(nameof(ManipulatorPosition));
        OnPropertyChanged(nameof(IsManipulatorVisible));
    }
    
    private Transform3D _manipulatorPosition;
    public Transform3D ManipulatorPosition
    {
        get => _manipulatorPosition;
        set
        {
            if (_manipulatorPosition != value)
            {
                _manipulatorPosition = value;
            }
        }
    }
    
    private void LoadModel()
    {
        _modelLoader.Load();
    }
}

