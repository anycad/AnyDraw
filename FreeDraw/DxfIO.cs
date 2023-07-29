using AnyCAD.Foundation;
using System;

namespace AnyCAD.IO.DXF
{
    /// <summary>
    /// DXF读取
    /// </summary>
    public class DxfIO
    {
        static GPnt ToPoint(netDxf.Vector3 pt)
        {
            return new GPnt(pt.X, pt.Y, pt.Z);
        }
        static Vector3 ToColor(netDxf.AciColor clr)
        {
            return new Vector3(clr.R / 255.0f, clr.G / 255.0f, clr.B / 255.0f);
        }

        static double D2R(double degreee)
        {
            return degreee / 180.0 * Math.PI;
        }

        /// <summary>
        /// 加载
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="splinePrecision"></param>
        /// <returns></returns>
        public static TopoShapeList Load(string fileName, int splinePrecision)
        {
            TopoShapeList mEdges = new TopoShapeList();

            bool bBinary = false;
            var dxfVersion = netDxf.DxfDocument.CheckDxfFileVersion(fileName, out bBinary);
            // netDxf is only compatible with AutoCad2000 and higher DXF versions
            if (dxfVersion < netDxf.Header.DxfVersion.AutoCad2000)
                return mEdges;

            var dxfDoc = netDxf.DxfDocument.Load(fileName);

            foreach (var line in dxfDoc.Entities.Lines)
            {
                if (line.StartPoint.Equals(line.EndPoint))
                    continue;

                var shape = SketchBuilder.MakeLine(ToPoint(line.StartPoint), ToPoint(line.EndPoint));
                mEdges.Add(shape);
            }

            foreach (var arc in dxfDoc.Entities.Arcs)
            {
                var shape = SketchBuilder.MakeArcOfCircle(new GCirc(new GAx2(ToPoint(arc.Center), GP.DZ()), arc.Radius),
                    D2R(arc.StartAngle), D2R(arc.EndAngle));
                mEdges.Add(shape);
            }

            foreach (var circle in dxfDoc.Entities.Circles)
            {
                var shape = SketchBuilder.MakeCircle(ToPoint(circle.Center), circle.Radius, GP.DZ());
                mEdges.Add(shape);
            }
            foreach (var Lwpolylines in dxfDoc.Entities.Polylines2D)
            {
                GPntList pts = new GPntList();
                for (int i = 0; i < Lwpolylines.Vertexes.Count; i++)
                {
                    var v = Lwpolylines.Vertexes[i];
                    GPnt gPnt = new GPnt(v.Position.X, v.Position.Y, v.Bulge);

                    pts.Add(gPnt);
                }

                for (int ii = 0; ii < pts.Count - 1; ++ii)
                {
                    var start = pts[ii];
                    var endPt = pts[ii + 1];

                    var s2 = new GPnt2d(start.X(), start.Y());
                    var e2 = new GPnt2d(endPt.X(), endPt.Y());
                    if (start.Z() != 0)
                    {
                        var shape = Sketch2dBuilder.MakeArc(s2, e2, start.Z());
                        mEdges.Add(shape);
                    }
                    else
                    {
                        var shape = Sketch2dBuilder.MakeLine(s2, e2);
                        mEdges.Add(shape);
                    }

                }
            }
            foreach (var sp in dxfDoc.Entities.Splines)
            {

                var pline = sp.ToPolyline2D(splinePrecision);
                for (int ii = 0; ii < pline.Vertexes.Count - 1; ++ii)
                {
                    var v1 = pline.Vertexes[ii];
                    var v2 = pline.Vertexes[ii + 1];

                    var p1 = new GPnt2d(v1.Position.X, v1.Position.Y);
                    var p2 = new GPnt2d(v2.Position.X, v2.Position.Y);

                    var edge = Sketch2dBuilder.MakeLine(p1, p2);

                    mEdges.Add(edge);
                }

                if (sp.IsClosed)
                {
                    var v1 = pline.Vertexes[pline.Vertexes.Count - 1];
                    var v2 = pline.Vertexes[0];

                    var p1 = new GPnt2d(v1.Position.X, v1.Position.Y);
                    var p2 = new GPnt2d(v2.Position.X, v2.Position.Y);

                    var edge = Sketch2dBuilder.MakeLine(p1, p2);

                    mEdges.Add(edge);
                }
            }

            return mEdges;
        }

    }
}
