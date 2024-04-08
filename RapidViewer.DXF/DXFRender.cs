using AnyCAD.Foundation;
using netDxf;
using System;
using System.Linq;

namespace RapidViewer.DXF
{
    internal class DXFRender
    {
        public DXFRender() { }

        public Document DxfToAnyCAD()
        {

            Document document = Application.Instance().CreateDocument("测试001");

            DxfDocument hardwareDxf = DxfDocument.Load($".\\test001.dxf");//读取工件dxf底图

            foreach (var line in hardwareDxf.Entities.Lines)
            {
                var shape = SketchBuilder.MakeLine(new GPnt((float)line.StartPoint.X, (float)line.StartPoint.Y, 0),
                             new GPnt((float)line.EndPoint.X, (float)line.EndPoint.Y, 0));

                var simpleCurveElement = SimpleCurveElement.Create(document);
                simpleCurveElement.SetShape(shape);
            }
            foreach (var arc in hardwareDxf.Entities.Arcs)
            {
                var shape = SketchBuilder.MakeArcOfCircle(new GCirc(new GAx2(ToPoint(arc.Center), GP.DZ()), arc.Radius),
                                                          Math.PI * arc.StartAngle / 180, Math.PI * arc.EndAngle / 180);
                var simpleCurveElement = SimpleCurveElement.Create(document);
                simpleCurveElement.SetShape(shape);
            }
            foreach (var circle in hardwareDxf.Entities.Circles)
            {
                var shape = SketchBuilder.MakeCircle(new GPnt(circle.Center.X, circle.Center.Y, 0), circle.Radius, new GDir() { z = 1, x = 0, y = 0 });
                var simpleCurveElement = SimpleCurveElement.Create(document);
                simpleCurveElement.SetShape(shape);
            }
            foreach (var polyline2D in hardwareDxf.Entities.Polylines2D)
            {
                var points = polyline2D.Vertexes.Select(x => x.Position);
                GPnt2dList gPnt2Ds = new GPnt2dList();
                foreach (var point in points)
                {
                    gPnt2Ds.Add(new GPnt2d(point.X, point.Y));
                }
                if (polyline2D.IsClosed)
                {
                    gPnt2Ds.Add(new GPnt2d(points.First().X, points.First().Y));
                }
                for (int i = 0; i < polyline2D.Vertexes.Count - 1; i++)
                {
                    var shape = Sketch2dBuilder.MakePolyline(gPnt2Ds);
                    var simpleCurveElement = SimpleCurveElement.Create(document);
                    simpleCurveElement.SetShape(shape);
                }
            }

            DocumentIO.Save(document, $".\\测试001.acad");
        }
    }
}
