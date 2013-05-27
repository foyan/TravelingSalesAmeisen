using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Linq;

namespace TravelingSalesPerson {

    public partial class Form1 : Form {
    
        public Form1() {
            InitializeComponent();
            DoubleBuffered = true;
            Cities = new List<Program.City>();
        }

        public List<Program.City> Cities { get; private set; } 

        public void Redraw(Program.Solution s) {
            if (s != null) {
                _bestPath = new GraphicsPath();
                foreach (var r in s.Itinerary) {
                    _bestPath.AddLine(r.Start.Coords, r.Destination.Coords);
                }
            }

            _pheromones.Clear();
            _pheromones.AddRange(Cities.SelectMany(c => c.Neighbours).Where(r => !r.IsSecond).Select(r => new PheromoneTrail {End = r.Destination.Coords, Start = r.Start.Coords, Width = (float)(r.Tau * 10)}));

            Invalidate();
        }

        private GraphicsPath _bestPath = new GraphicsPath();
        private readonly List<PheromoneTrail> _pheromones = new List<PheromoneTrail>();
        
        private class PheromoneTrail {
            public Point Start { get; set; }
            public Point End { get; set; }
            public float Width { get; set; }
        }

        protected override void OnPaint(PaintEventArgs e) {
            base.OnPaint(e);
            e.Graphics.SmoothingMode = SmoothingMode.HighQuality;
            foreach (var ph in _pheromones.OrderBy(p => p.Width)) {
                var rgb = 255 - 128*ph.Width;
                if (rgb < 128) {
                    rgb = 128;
                }
                using (var pen = new Pen(Color.FromArgb((int)rgb, (int)rgb, (int)rgb), ph.Width)) {
                    e.Graphics.DrawLine(pen, ph.Start, ph.End);
                }
            }
            using (var pen = new Pen(Color.Red, 2) { LineJoin = LineJoin.Round }) {
                e.Graphics.DrawPath(pen, _bestPath);
            }
        }

    }

}
