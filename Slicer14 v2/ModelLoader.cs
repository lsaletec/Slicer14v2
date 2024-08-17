using System.Collections.ObjectModel;
using System.Windows.Media.Media3D;
using Assimp;
using HelixToolkit.Wpf.SharpDX;
using SharpDX;
using MeshGeometry3D = HelixToolkit.Wpf.SharpDX.MeshGeometry3D;

namespace Slicer14_v2;

public class ModelLoader
{
    public ObservableCollection<Element3D> Models { get; set; }

    public ModelLoader( ObservableCollection<Element3D> models)
    {
        Models = models;
    }
    public void Load()
    {
        var openFileDialog = new Microsoft.Win32.OpenFileDialog
        {
            Filter = "3D Models|*.obj;*.fbx;*.dae,*.stl|All Files|*.*"
        };
        IList<MeshGeometryModel3D> model = null;
        
        if (openFileDialog.ShowDialog() == true)
        {
            model = Load3DModel(openFileDialog.FileName);
        }

        foreach (var m in model)
        {
            Models.Add(m);
        }
    }

    public IList<MeshGeometryModel3D> Load3DModel(string filePath)
    {
        var importer = new AssimpContext();
        var scene = importer.ImportFile(filePath,
            PostProcessSteps.Triangulate | PostProcessSteps.GenerateNormals | PostProcessSteps.FlipUVs);

        if (scene == null || scene.RootNode == null)
        {
            throw new System.Exception("Failed to load model.");
        }

        var fileName = System.IO.Path.GetFileNameWithoutExtension(filePath);

        IList<MeshGeometryModel3D> result = new List<MeshGeometryModel3D>();
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

            // Calculate the model's center
            var modelCenter = CalculateModelCenter(positions);

            // Adjust each vertex position relative to the model's center
            for (int i = 0; i < positions.Count; i++)
            {
                positions[i] -= modelCenter;
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
            fileName = GenerateUniqueName(fileName);
            var model = new MeshGeometryModel3D
            {
                Geometry = helixMesh,
                Material = material,
                Transform = new TranslateTransform3D(0, 0, 0), // No additional transform needed
                Name = fileName, // Assign the file name as the model name,
                Tag = fileName // Use the Tag to store the identifier
            };
            result.Add(model);
        }

        return result;
    }
    
    // Helper method to generate a unique name
    private string GenerateUniqueName(string baseName)
    {
        string uniqueName = baseName;
        int counter = 1;

        while (Models.Any(m => m.Tag?.ToString() == uniqueName))
        {
            uniqueName = $"{baseName}{counter++}";
        }

        return uniqueName;
    }

    private Vector3 CalculateModelCenter(List<Vector3> positions)
    {
        if (positions.Count == 0)
        {
            return new Vector3(0, 0, 0);
        }

        Vector3 sum = Vector3.Zero;
        foreach (var pos in positions)
        {
            sum += pos;
        }
    
        return sum / positions.Count;
    }
}