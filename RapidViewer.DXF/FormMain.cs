using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AnyCAD.Forms;
using AnyCAD.Foundation;

namespace RapidViewer.DXF
{
    public partial class FormMain : Form
    {
        RenderControl mRenderView;
        public FormMain()
        {
            InitializeComponent();

            mRenderView = new RenderControl();
            this.panel1.Controls.Add(mRenderView);
            mRenderView.Dock = DockStyle.Fill;
        }

        private void FormMain_Load(object sender, EventArgs e)
        {

        }

        GPnt ToPoint(netDxf.Vector3 pt)
        {
            return new GPnt(pt.X, pt.Y, pt.Z);
        }
        Vector3 ToColor(netDxf.AciColor clr)
        {
            return new Vector3(clr.R / 255.0f, clr.G / 255.0f, clr.B / 255.0f);
        }

        double D2R(double degreee)
        {
            return degreee / 180.0 * Math.PI;
        }
        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "DXF File(*.dxf)|*.dxf";
            if (dlg.ShowDialog() != DialogResult.OK)
                return;

            bool bBinary = false;
            var dxfVersion = netDxf.DxfDocument.CheckDxfFileVersion(dlg.FileName, out bBinary);
            // netDxf is only compatible with AutoCad2000 and higher DXF versions
            if (dxfVersion < netDxf.Header.DxfVersion.AutoCad2000) 
                return;
         
            var dxfDoc = netDxf.DxfDocument.Load(dlg.FileName);

            foreach(var line in dxfDoc.Lines)
            {
                if (line.StartPoint.Equals(line.EndPoint))
                    continue;

               var shape =  SketchBuilder.MakeLine(ToPoint(line.StartPoint), ToPoint(line.EndPoint));
                mRenderView.ShowShape(shape, ToColor(line.Color));
            }

            foreach(var arc in dxfDoc.Arcs)
            {
                var shape = SketchBuilder.MakeArcOfCircle(new GCirc(new GAx2(ToPoint(arc.Center), GP.DZ()), arc.Radius),
                    D2R(arc.StartAngle), D2R(arc.EndAngle));
                mRenderView.ShowShape(shape, ToColor(arc.Color));
            }

            foreach(var circle in dxfDoc.Circles)
            {
                var shape = SketchBuilder.MakeCircle(ToPoint(circle.Center), circle.Radius, GP.DZ());
                mRenderView.ShowShape(shape, ToColor(circle.Color));
            }

            mRenderView.ZoomAll();
        }

        private void fitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            mRenderView.ZoomAll();
        }

        private void topViewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            mRenderView.SetStandardView(EnumStandardView.Top);
        }

        private void clearToolStripMenuItem_Click(object sender, EventArgs e)
        {
            mRenderView.ClearAll();
        }
    }
}
