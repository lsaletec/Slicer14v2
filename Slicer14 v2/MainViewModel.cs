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
    public ICommand ResetTransformsCommand { get; }

    private Element3D _selectedModel;
    public Element3D SelectedModel
    {
        get => _selectedModel;
        set
        {
            if (_selectedModel != value)
            {
                _selectedModel = value;
                if (_selectedModel is MeshGeometryModel3D meshModel && meshModel.Transform == null)
                {
                    meshModel.Transform = new TranslateTransform3D(0, 0, 0);
                }
                OnPropertyChanged(nameof(SelectedModel));
            }
        }
    }

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
        ResetTransformsCommand = new RelayCommand(ResetTransforms);
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
            var hits = viewport.FindHits(e.GetPosition(viewport));
            if (hits.Count > 0)
            {
                Console.WriteLine($"Models size: {Models.Count}");
                foreach (var hit in hits)
                {
                    // Check if the hit object is a model you want to select, and not the manipulator
                    
                    if (hit.ModelHit is MeshGeometryModel3D m )
                    {
                        Console.WriteLine($"Tag: {m.Tag}");
                        Console.WriteLine($"GUID: {m.GUID}");
                        MeshGeometryModel3D m2 = (MeshGeometryModel3D)hit.ModelHit;
                        Console.WriteLine($"hitTag: {m2.Tag}");
                        Console.WriteLine("Is meshgeometry");
                    }
                    if (Models.Contains(hit.ModelHit))
                    {
                        Console.WriteLine("Contains");
                    }
                }
                SelectedModel = hits[0].ModelHit as Element3D;
            }
        }
    }

    private void ResetTransforms()
    {
        foreach (var model in Models)
        {
            if (model is MeshGeometryModel3D meshModel)
            {
                meshModel.Transform = new TranslateTransform3D(0, 0, 0);
            }
        }
    }

    private void LoadModel()
    {
        var openFileDialog = new Microsoft.Win32.OpenFileDialog
        {
            Filter = "3D Models|*.obj;*.fbx;*.dae,*.stl|All Files|*.*"
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

        var fileName = System.IO.Path.GetFileNameWithoutExtension(filePath);
        
        Models.Clear();

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
                Material = material,
                Transform = new TranslateTransform3D(0, 0, 0),
                Name = fileName, // Assign the file name as the model name,
                Tag = fileName // Use the Tag to store the identifier
            };
            Console.WriteLine(model.Tag);
            Models.Add(model);
        }
    }
}
