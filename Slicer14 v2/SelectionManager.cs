using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using HelixToolkit.Wpf.SharpDX;
using SharpDX;

namespace Slicer14_v2;

public class Selection
{
    public ObservableCollection<Element3D> SelectedObjects { get; private set; }
    public MeshGeometryModel3D selectionCenter { get; private set; }
    public BoundingBox CustomBounds { get; private set; }
    public BoundingBox UnmodifiedBounds { get; private set; }

    private Dictionary<Element3D, Matrix3D> initialOffsets;

    public Selection()
    {
        SelectedObjects = new ObservableCollection<Element3D>();
        selectionCenter = CreateSelectionCenterCube();
        initialOffsets = new Dictionary<Element3D, Matrix3D>();
    }

    private MeshGeometryModel3D CreateSelectionCenterCube()
    {
        var meshBuilder = new MeshBuilder();
        meshBuilder.AddBox(new Vector3(0, 0, 0), 1, 1, 1);

        var geometry = meshBuilder.ToMeshGeometry3D();

        return new MeshGeometryModel3D
        {
            Geometry = geometry,
            Material = new PhongMaterial { DiffuseColor = Colors.Red.ToColor4() },
            Transform = new MatrixTransform3D(Matrix3D.Identity)
        };
    }

    public void AddObject(Element3D obj)
    {
        if (!SelectedObjects.Contains(obj))
        {
            SelectedObjects.Add(obj);
        }
        
        Console.WriteLine($"Position of first object:{SelectedObjects[0].Transform.ToMatrix()}");
        UpdateCustomBounds();
        UpdateSelectionCenterPosition(); 
        var initialTransform = obj.Transform.Value;
        var selectionCenterTransform = selectionCenter.Transform.Value;
        selectionCenterTransform.Invert(); // Invert the matrix
        var relativeOffset = initialTransform * selectionCenterTransform;
        initialOffsets[obj] = relativeOffset;
    }

    public void RemoveObject(Element3D obj)
    {
        if (SelectedObjects.Contains(obj))
        {
            if (obj is MeshGeometryModel3D meshModel)
            {
                meshModel.PostEffects = string.Empty;
            }
            initialOffsets.Remove(obj);
            SelectedObjects.Remove(obj);
        }
    }
    
    private void UpdateSelectionCenterPosition()
    {
        if (!SelectedObjects.Any())
        {
            return;
        }
        var center = CustomBounds.Center;
        selectionCenter.Transform = new TranslateTransform3D(center.X, center.Y, center.Z);
    }

    public void Clear()
    {
        foreach (var model in SelectedObjects)
        {
            if (model is MeshGeometryModel3D meshModel)
            {
                meshModel.PostEffects = string.Empty;
            }
        }

        initialOffsets.Clear();
        SelectedObjects.Clear();
        CustomBounds = new BoundingBox();
    }

    private void UpdateCustomBounds()
    {
        if (!SelectedObjects.Any())
        {
            CustomBounds = new BoundingBox();
            return;
        }

        // Start with the transformed bounds of the first selected object
        Console.WriteLine($"CustomBounds1: ({SelectedObjects[0].Bounds.Center.X}, {SelectedObjects[0].Bounds.Center.Y}, {SelectedObjects[0].Bounds.Center.Z})");
        var firstBounds2 = SelectedObjects[0].Bounds;
        var firstBounds = SelectedObjects[0].Bounds.Transform(SelectedObjects[0].Transform.ToMatrix());
        CustomBounds = new BoundingBox(firstBounds.Minimum, firstBounds.Maximum);
        UnmodifiedBounds = new BoundingBox(firstBounds2.Minimum, firstBounds2.Maximum);

        // If there are more objects, merge their transformed bounds into CustomBounds
        foreach (var child in SelectedObjects.Skip(1))  // Skip the first one since it's already used
        {
            var transformedBounds = child.Bounds.Transform(child.Transform.ToMatrix());
            CustomBounds = BoundingBox.Merge(CustomBounds, transformedBounds);
            UnmodifiedBounds = BoundingBox.Merge(UnmodifiedBounds, child.Bounds);
        }

        Console.WriteLine($"CustomBounds2: ({CustomBounds.Center.X}, {CustomBounds.Center.Y}, {CustomBounds.Center.Z})");
    }


    public void HandleSelection(MeshGeometryModel3D meshModel, bool isShiftPressed)
    {
        if (SelectedObjects.Contains(meshModel))
        {
            RemoveObject(meshModel);
        }
        else
        {
            meshModel.PostEffects = "border[color:#00FFDE]";
            if (isShiftPressed)
            {
                AddObject(meshModel);
                OnPropertyChanged(nameof(selectionCenter));
            }
            else
            {
                Clear();
                AddObject(meshModel);
                OnPropertyChanged(nameof(selectionCenter));
            }
        }
    }
    
    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public void ApplyTransformToSelectedGroup()
    {
        var selectionCenterMatrix = selectionCenter.Transform.Value;

        foreach (var obj in SelectedObjects)
        {
            if (obj is MeshGeometryModel3D meshModel)
            {
                var initialOffset = initialOffsets[obj];
                var finalTransform = initialOffset * selectionCenterMatrix;
                meshModel.Transform = new MatrixTransform3D(finalTransform);
            }
        }
        UpdateCustomBounds();
        OnPropertyChanged(nameof(Selection));
    }
}
