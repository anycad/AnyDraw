using AnyCAD.Foundation;
using AnyCAD.NX.ViewModel;
using AnyDraw.Nest;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Fluent;
using System.Diagnostics;

namespace AnyDraw
{
    internal partial class MainViewModelImpl : MainRibbonViewModel
    {
        public MainViewModelImpl(IRenderView viewer)
            : base(viewer)
        {
            this.FileFilter = "AnyDraw Files (*.draw;*.acad)|*.draw;*.acad";
            this.DefaultExt = ".draw";
        }

        public override Ribbon GetRibbonBar()
        {
            return ((MainWindow)App.Current.MainWindow).mRibbon;
        }

        public void SetTitle(string title)
        {
            ((MainWindow)App.Current.MainWindow).Title = title;
        }

        protected override void DoInitialize()
        {
            base.DoInitialize();            
            Viewer.SetStandardView(EnumStandardView.Top, false);
            Viewer.SetCoordinateWidget(EnumViewCoordinateType.Axis);      
            Viewer.SetCoordinateWidgetText("x", "y", "");

            Viewer.SetViewMode2D(true);
            Viewer.SetRulerWidget(EnumRulerWidgetType.Default);

            ViewContext.SetOrbitButton(EnumMouseButton.Zero);
            Viewer.SetBackgroundColor(new Vector4(33 / 255.0f, 40 / 255.0f, 48 / 255.0f, 0));
            var material = ViewContext.GetDefaultMaterial(EnumShapeFilter.Edge);
            material.SetColor(ColorTable.White);
        }

        protected override void OnImportModel()
        {
            var dlg = new Microsoft.Win32.OpenFileDialog
            {
                DefaultExt = ".dxf",
                Filter = "DXF Files (*.dxf;*.json)|*.dxf;*.json"
            };
            var result = dlg.ShowDialog();
            if (result != true)
                return;
            var doc = Document;
            var undo = new UndoTransaction(doc);
            undo.Start("Import");
            if (dlg.FileName.EndsWith(".dxf"))
            {
                var shapes = DxfIO.Load(dlg.FileName);
                for(int ii=0; ii<shapes.Count; ++ii)
                {
                    var shape = shapes[ii];
                    if(shape == null) 
                        continue;
                    if(shape.GetShapeType() == EnumTopoShapeType.Topo_EDGE)
                    {
                        var sce = SimpleCurveElement.Create(doc);
                        sce.SetCurve(shape);
                    }
                    else
                    {
                        var sp = ShapeElement.Create(doc);
                        sp.SetShape(shape);
                    }
                }
            }
            else
            {
                var db = AnyCAD.IO.Drawing.DrawingDb.Load(dlg.FileName);
                db?.Show(doc);
            }
            undo.Commit();

            //ZoomFitCommand.Execute("");
        }

        [RelayCommand]
        void Nest()
        {
            var view = new NestConfigView(Document);
            view.ShowDialog();
            StatusMessage = view.Message;
        }
    }
}
