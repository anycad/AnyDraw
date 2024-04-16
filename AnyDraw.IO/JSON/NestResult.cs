using AnyCAD.Foundation;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace AnyDraw.IO
{
    public class Placement
    {
        public ulong Id { get; set; } = 0;
        public double[] Transform { get; set; } = new double[16];

        public GTrsf ToTransform()
        {
            var mat = new Matrix4d(Transform[0], Transform[1], Transform[2], Transform[3]
                , Transform[4], Transform[5], Transform[6], Transform[7]
                , Transform[8], Transform[9], Transform[10], Transform[11]
                , Transform[12], Transform[13], Transform[14], Transform[15]);

            return TransformTool.ToTransform(mat);
        }
    }
    public class NestResult
    {
        public List<Placement> Placement { get; set; } = new();
        static public NestResult? Load(string fileName)
        {
            using (StreamReader reader = new StreamReader(fileName))
            {
                var data = reader.ReadToEnd();
                return JsonSerializer.Deserialize<NestResult>(data);
            }
        }

        void OnApply(Document doc)
        {
            foreach(var item in Placement)
            {
                var ce = CurveElement.Cast(doc.FindElement(new ObjectId(item.Id)));
                if (ce != null)
                {
                    ce.SetTransform(item.ToTransform());
                }
            }
        }
        public void Apply(Document doc)
        {
            var undo = new UndoTransaction(doc);
            undo.Start("Nest");

            OnApply(doc);

            undo.Commit();
        }
    }
}
