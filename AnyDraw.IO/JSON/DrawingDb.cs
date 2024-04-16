using AnyCAD.Foundation;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace AnyCAD.IO.Drawing
{

    public class Shape
    {
        public string Type { get; set; } = string.Empty;
        public double[] Data { get; set; } = new double[4];
        public string Content { get; set; } = string.Empty;

        void AddSimpleCurve(TopoShape shape, ObjectId? layerId, Document doc)
        {
            if (shape != null)
            {
                var curve = SimpleCurveElement.Create(doc);
                curve.SetCurve(shape);
                if (layerId != null)
                    curve.SetLayerId(layerId);
            }
        }

        public void Show(Document doc, ObjectId? layerId)
        {
            if(Type == "Polyline")
            {
                if(Data.Length == 4)
                {
                    var p1 = new GPnt(Data[0], Data[1], 0);
                    var p2 = new GPnt(Data[2], Data[3], 0);
                    if (p1.Distance(p2) < 0.01)
                        return;

                    var shape = SketchBuilder.MakeLine(p1, p2);
                    AddSimpleCurve(shape, layerId, doc);
                }
                else if(Data.Length > 4)
                {
                    var pts = new GPntList();
                    for(int ii=0; ii<Data.Length/2; ++ii)
                    {
                        pts.Add(new GPnt(Data[ii*2], Data[ii*2+1], 0));
                    }
   
                    var element = PolylineElement.Create(doc);
                    element.SetPoints(pts);
                    element.SetClosed(true);
                    if (layerId != null)
                        element.SetLayerId(layerId);
                }

            }
            else if(Type == "Text")
            {
                if (Content.Length > 0)
                {
                    //var mesh = FontManager.Instance().CreateMesh(Content);
                    //var node = PrimitiveSceneNode.Create(mesh, material);
                    //var trf = Matrix4.makeTranslation((float)Data[0], (float)Data[1], 0);
                    //float height = 0.004f * (float)Data[4];
                    //float width = height / (float)Data[5];
                    //var scale = Matrix4.makeScale(height, width, 1);
                    //var rotate = Matrix4.makeRotation(Vector3.UNIT_X, new Vector3((float)Data[2], (float)Data[3], 0));
                    //node.SetTransform(trf * rotate * scale);
                    //render.ShowSceneNode(node);
                    


                    var text = TextElement.Create(doc);
                    text.SetTextW(Content);
                    text.SetLocation(new GAx2(new GPnt(Data[0], Data[1], 0), new GDir(0,0,1), new GDir(Data[2], Data[3], 0)));
                    if(layerId != null)
                        text.SetLayerId(layerId);
                }

            }
            else if(Type == "Circle")
            {
                var pt = new GPnt(Data[0], Data[1], 0);
                var shape = SketchBuilder.MakeCircle(new GCirc(new GAx2(pt, new GDir(1, 0, 0)), Data[2]));
                AddSimpleCurve(shape, layerId, doc);
            }
            else if(Type == "CircArc")
            {
                var pt = new GPnt(Data[0], Data[1], 0);
                var radius = Data[2];
                var start = Data[3];
                var end = Data[4];
                var refVec = new GDir(Data[5], Data[6], 0);
                var shape = SketchBuilder.MakeArcOfCircle(new GCirc(new GAx2(pt, refVec), radius), start, end);
                AddSimpleCurve(shape, layerId, doc);
            }
        }
    }

    public class Layer
    {
        public uint Id { get; set; } = 0;

        public string Name { get; set; } = string.Empty;

        public float[] Color { get; set; } = new float[4];
    }

    public class Entity
    {
        public ulong Id { get; set; } = 0;
        public uint LayerId { get; set; } = 0;

        public string Type { get; set; } = string.Empty;

        public double[] Extends { get; set; } = new double[4];

        public float[] Color { get; set; } = new float[3];

        public List<Entity> Children { get; set; } = new();

        public List<Shape> Geometry { get; set; } = new();


        public Vector3 ToColor()
        {
            return new Vector3(Color[0], Color[1], Color[2]);
        }

        public void Show(Document doc, ObjectId? layerId)
        {            
            foreach (var shape in Children)
            {
                shape.Show(doc, layerId);
            }

            foreach (var shape in Geometry)
            {
                shape.Show(doc, layerId);
            }
        }
    }


    public class DrawingDb
    { 
        public List<Entity> Entity { get; set; } = new();
        public List<Layer> Layer { get; set; } = new();   

        static public DrawingDb? Load(string fileName)
        {
            using (StreamReader reader = new StreamReader(fileName))
            {
                var data = reader.ReadToEnd();
                return JsonSerializer.Deserialize<DrawingDb>(data);
            }
        }

        public void Show(Document doc)
        {
            var materials = new Dictionary<uint, ObjectId>();            
            foreach(var layer in Layer)
            {
                var material = MaterialElement.Create(doc);
                material.SetName(layer.Name);
                var color = new Vector3(layer.Color[0], layer.Color[1], layer.Color[2]);
                material.SetEdgeColor(color);
                material.SetFaceColor(color);
                material.UpdateMaterial();

                var pLayer = LayerElement.Create(doc);
                pLayer.SetName(layer.Name);
                pLayer.SetMaterialId(material.GetId());
                pLayer.SetDbViewId(doc.GetActiveDbViewId());

                materials.Add(layer.Id, pLayer.GetId());
            }
 
            foreach (var shape in Entity)
            {
                materials.TryGetValue(shape.LayerId, out var layerId);
                shape.Show(doc, layerId);
            }
        }

        public void Save(Document doc, string fileName)
        {
            var table = doc.FindTable(CurveElement.GetStaticClassId());
            for(  var itr = table.CreateIterator(); itr.More(); itr.Next())
            {
                var pe = PolylineElement.Cast(itr.Current());
                if (pe != null)
                {
                    
                    var shape = new Shape() { Type = "Polyline" };
                    var points = pe.GetPoints();
                    shape.Data = new double[points.Count * 2] ;
                    for(int ii=0; ii<points.Count; ii++)
                    {
                        var pt = points[ii];
                        shape.Data[ii * 2] = pt.x;
                        shape.Data[(ii * 2) + 1] = pt.y;
                    }

                    var entity = new Entity() { Id = pe.GetId().Value, Type = "Entity" };
                    entity.Geometry.Add(shape);
                    Entity.Add(entity);
                }
            }

            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
            };
            var data = JsonSerializer.Serialize(this, options);
            File.WriteAllText(fileName, data);
        }
    }
}
