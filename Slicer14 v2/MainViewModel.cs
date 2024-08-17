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
    
    protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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
        _currentSelection.PropertyChanged += OnSelectionPropertyChanged;
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
                   if (hit.ModelHit is MeshGeometryModel3D hitModel)
                   {
                       foreach (var model in Models)
                       {
                           if (model is MeshGeometryModel3D meshModel && Equals(meshModel.Tag, hitModel.Tag))
                           {
                               _currentSelection.HandleSelection(meshModel,isShiftPressed);
                               Console.WriteLine("Handled selection");
                               OnPropertyChanged(nameof(CurrentSelection));
                           }
                       }
                       break;
                   }
               } 
            }
            else
            {
                _currentSelection.Clear(); 
            }
        }
    }
    private void OnSelectionPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(Selection))
        {
            if (CurrentSelection.SelectedObjects.Count > 0)
            {
                OnPropertyChanged(nameof(CurrentSelection)); // Forward the notification
                Console.WriteLine($"currentSelection: {CurrentSelection.SelectedObjects[0].Transform.ToMatrix()}");
            }
            
        }
    }
    
    private void LoadModel()
    {
        _modelLoader.Load();
    }
    
}

