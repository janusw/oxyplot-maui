using System.Diagnostics;
using ExampleLibrary;
using OxyPlot;

namespace OxyplotMauiSample
{
    public partial class CustomTrackerPage
    {
        public CustomTrackerPage()
        {
            InitializeComponent();
            this.Loaded += CustomTrackerPage_Loaded;
        }

        private void CustomTrackerPage_Loaded(object sender, EventArgs e)
        {
            PlotView.Model = ShowCases.CreateNormalDistributionModel();
            PlotView.Model.TrackerChanged += OnTrackerChanged;
        }

        private void OnTrackerChanged(object sender, TrackerEventArgs e)
        {
            Debug.WriteLine($"tracker changed: {e.HitResult}");
        }
    }
}
