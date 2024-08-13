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
    public ObservableCollection<Element3D> Models { get; } = new ObservableCollection<Element3D>();
    public ICommand LoadModelCommand { get; }
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
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName]string info = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
    }

    protected bool Set<T>(ref T backingField, T value, [CallerMemberName]string propertyName = "")
    {
        if (Equals(backingField, value)) return false;
        backingField = value;
        OnPropertyChanged(propertyName);
        return true;
    }
    
    // Method to handle MouseLeftButtonDown event
    public void OnMouseDown(object sender, MouseButtonEventArgs e)
    {
        var viewport = sender as Viewport3DX;
        if (viewport != null)
        {
            var hitResult = viewport.FindHits(e.GetPosition(viewport));
            if (hitResult != null)
            {
                var hitModel = hitResult[0].ModelHit;
                // Perform action with hitModel
            }
        }
    }
    
    private void LoadModel()
    {
        // Implement the logic to open a file dialog and load a model
        var openFileDialog = new Microsoft.Win32.OpenFileDialog
        {
            Filter = "3D Models|*.obj;*.fbx;*.dae|All Files|*.*"
        };

        if (openFileDialog.ShowDialog() == true)
        {
            Load3DModel(openFileDialog.FileName);
        }
    }
    
    public void Load3DModel(string filePath)
    {
        var importer = new AssimpContext();
        var scene = importer.ImportFile(filePath, PostProcessSteps.Triangulate | PostProcessSteps.GenerateNormals | PostProcessSteps.FlipUVs);

        if (scene == null || scene.RootNode == null)
        {
            throw new System.Exception("Failed to load model.");
        }

        // Clear any previous models
        Models.Clear();

        // Convert Assimp meshes to HelixToolkit meshes
        foreach (var mesh in scene.Meshes)
        {
            var positions = new List<Vector3>();
            var normals = new List<Vector3>();
            var indices = new List<int>();

            for (int i = 0; i < mesh.VertexCount; i++)
            {
                positions.Add(new Vector3(mesh.Vertices[i].X, mesh.Vertices[i].Y, mesh.Vertices[i].Z));
                normals.Add(new Vector3(mesh.Normals[i].X, mesh.Normals[i].Y, mesh.Normals[i].Z));
            }

            for (int i = 0; i < mesh.FaceCount; i++)
            {
                indices.AddRange(mesh.Faces[i].Indices);
            }

            var helixMesh = new MeshGeometry3D
            {
                Positions = new Vector3Collection(positions),
                Normals = new Vector3Collection(normals),
                Indices = new IntCollection(indices)
            };

            var material = new PhongMaterial
            {
                DiffuseColor = Color.White.ToColor4()
            };

            var model = new MeshGeometryModel3D
            {
                Geometry = helixMesh,
                Material = material
            };

            Models.Add(model);
        }
    }
    
}