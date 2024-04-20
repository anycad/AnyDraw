using CommunityToolkit.Mvvm.ComponentModel;
using System.IO;
using System.Text.Json;

namespace AnyDraw.IO.Nest
{
    public partial class NestConfig : ObservableObject
    {
        [ObservableProperty]
        private double _BinWidth  = 1600;
        [ObservableProperty]
        private double _BinLength = 10000;
        [ObservableProperty]
        private double _CurveTolerance = 0.02;
        [ObservableProperty]
        private double _Spacing= 0.0;
        [ObservableProperty]
        private int _PopulationSize = 10;
        [ObservableProperty]
        private int _MutationRate  = 10;
        [ObservableProperty]
        private bool _UseClipper = false;
        [ObservableProperty]
        private bool _UseHole = false;
        [ObservableProperty]
        private int _IterationNum  = 100;
        [ObservableProperty]
        private int _RotationFactor  = 4;
        [ObservableProperty]
        private string _EnginePath = "";
        public void Save(string fileName)
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
            };
            var data = JsonSerializer.Serialize(this, options);
            System.IO.File.WriteAllText(fileName, data);
        }

        static public NestConfig? Load(string fileName)
        {
            if(!System.IO.File.Exists(fileName))
            {
                return null;
            }
            using (StreamReader reader = new StreamReader(fileName))
            {
                var data = reader.ReadToEnd();
                return JsonSerializer.Deserialize<NestConfig>(data);
            }
        }
    }
}
