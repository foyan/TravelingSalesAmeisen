using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace TravelingSalesPerson {
    public class Program {

        // ALPHA BETA RHO  ANT_COUNT => BEST
        // 1.0   1.0  0.05 100          1891
        // 1.2   1.0  0.05 200          1895

        /*
        Olten=>Basel=>Solothurn=>Biel=>Neuchatel=>Fribourg=>Bern=>Thun=>Interlaken=>Luze
        rn=>Zug=>Schwyz=>Andermatt=>Brig=>Sion=>Martigny=>Montreux=>Lausanne=>Genf=>Zerm
        att=>Locarno=>Bellinzona=>Lugano=>Chiasso=>St. Moritz=>Davos=>Chur=>Vaduz=>Glaru
        s=>St. Gallen=>Kreuzlingen=>Schaffhausen=>Winterthur=>Z?rich=>Aarau=>Olten: 1895
         */

        private const double ALPHA = 1.2;
        private const double BETA = 1.0;
        private const double RHO = 0.05;
        private const int ANT_COUNT = 200;
        private const int ITERATIONS = 100;

        private static readonly Random _rand = new Random();

        static void Main(string[] args) {

            var form = new Form1();

            ThreadPool.QueueUserWorkItem(state => {
                var cities = InitializeCities(@"..\..\Cities.txt");

                var nn = CreateInitialSolution(cities);

                var tau0 = ANT_COUNT / (double)nn.Distance;
                cities.ForEach(c => c.Neighbours.ForEach(n => n.Tau = tau0));

                form.Invoke(new Action(() => form.Cities.AddRange(cities)));

                Solution best = null;

                var it = 0;

                while (it < ITERATIONS) {
                    var ants = Enumerable.Range(0, ANT_COUNT).Select(i => new CyberAmeise(cities[_rand.Next(0, cities.Count)], cities)).ToList();

                    foreach (var a in ants) {
                        for (var i = 0; i < cities.Count; i++) {
                            a.Crawl();
                        }
                    }

                    cities.ForEach(c => c.Neighbours.ForEach(r => r.Tau *= (1 - RHO)));

                    ants.SelectMany(a => (a.Solution.Itinerary.Union(a.Solution.Itinerary.Select(r => r.Reverse))).Select(r => new { d = a.Solution.Distance, r })).ToList().ForEach(r => r.r.Tau += 1 / (double)r.d);

                    var s = ants.OrderBy(a => a.Solution.Distance).First();

                    if (best == null || s.Solution.Distance < best.Distance) {
                        best = s.Solution;
                        Console.WriteLine(best + "\n------");
                        it = 0;
                        form.BeginInvoke(new Action(() => form.Redraw(s.Solution)));
                    } else {
                        it++;
                        //if (it % 10 == 0) {
                            form.BeginInvoke(new Action(() => form.Redraw(null)));
                        //}
                    }

                }

                Console.WriteLine("Done. Ciao.");

            });
            Application.Run(form);
        }

        private static List<City> InitializeCities(string filename) {
            var lines = File.ReadAllLines(filename);

            var cities = lines.Take(35).Select(l => new City { Name = l, Neighbours = new List<Road>() }).ToList();

            lines.Skip(37).Select((l, i) => new { line = l, index = i + 1 }).ToList()
                .ForEach(i => i.line.Split(' ').Select(l => l.Trim()).Where(l => l != "").Select((l, j) => new { line = l, index = j }).ToList()
                               .ForEach(j => {
                                   var there = new Road { Start = cities[i.index], Destination = cities[j.index], Distance = int.Parse(j.line) };
                                   var fro = new Road {Start = cities[j.index], Destination = cities[i.index], Distance = int.Parse(j.line), IsSecond = true};
                                   there.Reverse = fro;
                                   fro.Reverse = there;
                                   cities[i.index].Neighbours.Add(there);
                                   cities[j.index].Neighbours.Add(fro);
                               }));

            cities.Single(c => c.Name == "Aarau").Coords = new Point(465, 182);
            cities.Single(c => c.Name == "Andermatt").Coords = new Point(585, 401);
            cities.Single(c => c.Name == "Basel").Coords = new Point(370, 129);
            cities.Single(c => c.Name == "Bellinzona").Coords = new Point(671, 536);
            cities.Single(c => c.Name == "Bern").Coords = new Point(340, 315);
            cities.Single(c => c.Name == "Biel").Coords = new Point(297, 260);
            cities.Single(c => c.Name == "Brig").Coords = new Point(364, 513);
            cities.Single(c => c.Name == "Chiasso").Coords = new Point(671, 650);
            cities.Single(c => c.Name == "Chur").Coords = new Point(768, 334);
            cities.Single(c => c.Name == "Davos").Coords = new Point(832, 354);
            cities.Single(c => c.Name == "Fribourg").Coords = new Point(279, 355);
            cities.Single(c => c.Name == "Genf").Coords = new Point(71, 535);
            cities.Single(c => c.Name == "Glarus").Coords = new Point(677, 282);
            cities.Single(c => c.Name == "Interlaken").Coords = new Point(430, 392);
            cities.Single(c => c.Name == "Kreuzlingen").Coords = new Point(690, 95);
            cities.Single(c => c.Name == "Lausanne").Coords = new Point(170, 443);
            cities.Single(c => c.Name == "Locarno").Coords = new Point(625, 550);
            cities.Single(c => c.Name == "Lugano").Coords = new Point(658, 597);
            cities.Single(c => c.Name == "Luzern").Coords = new Point(518, 281);
            cities.Single(c => c.Name == "Martigny").Coords = new Point(235, 503);
            cities.Single(c => c.Name == "Montreux").Coords = new Point(211, 457);
            cities.Single(c => c.Name == "Neuchatel").Coords = new Point(235, 299);
            cities.Single(c => c.Name == "Olten").Coords = new Point(415, 202);
            cities.Single(c => c.Name == "St. Gallen").Coords = new Point(735, 167);
            cities.Single(c => c.Name == "St. Moritz").Coords = new Point(840, 437);
            cities.Single(c => c.Name == "Schaffhausen").Coords = new Point(584, 85);
            cities.Single(c => c.Name == "Schwyz").Coords = new Point(585, 292);
            cities.Single(c => c.Name == "Sion").Coords = new Point(326, 527);
            cities.Single(c => c.Name == "Solothurn").Coords = new Point(360, 233);
            cities.Single(c => c.Name == "Thun").Coords = new Point(383, 372);
            cities.Single(c => c.Name == "Vaduz").Coords = new Point(791, 238);
            cities.Single(c => c.Name == "Winterthur").Coords = new Point(613, 141);
            cities.Single(c => c.Name == "Zermatt").Coords = new Point(280, 589);
            cities.Single(c => c.Name.EndsWith("rich")).Coords = new Point(566, 186);
            cities.Single(c => c.Name == "Zug").Coords = new Point(562, 242);

            return cities;
        } 

        private static Solution CreateInitialSolution(List<City> cities) {
            Solution best = null;
            for (var i = 0; i < cities.Count; i++) {
                var s = CreateInitialSolution(cities, i);
                if (best == null || s.Distance < best.Distance) {
                    best = s;
                }
            }
            return best;
        }

        private static Solution CreateInitialSolution(List<City> cities, int startIndex) {

            var start = cities[startIndex];
            var cs = cities.ToList();

            var solution = new Solution {Start = start};
            cs.Remove(start);

            var current = start;
            while (cs.Count > 0) {
                var next = current.Neighbours.Where(r => cs.Contains(r.Destination)).OrderBy(r => r.Distance).First();
                cs.Remove(next.Destination);
                solution.Itinerary.Add(next);
                current = next.Destination;
            }

            solution.Itinerary.Add(current.Neighbours.Single(r => r.Destination == start));

            return solution;
        }

        public class City {
            public string Name { get; set; }
            public List<Road> Neighbours { get; set; }

            public Point Coords { get; set; }

            public override string ToString() {
                return Name;
            }
        }

        public class Road {
            public City Start { get; set; }
            public City Destination { get; set; }
            public int Distance { get; set; }

            public bool IsSecond { get; set; }

            public Road Reverse { get; set; }

            public double Tau { get; set; }

            public double Ny { get { return 1/(double) Distance; } }

            public override string ToString() {
                return Start + "=>" + Destination + " (" + Distance + ")";
            }
        }

        public class Solution {

            public Solution() {
                Itinerary = new List<Road>();
            }

            public List<Road> Itinerary { get; private set; }

            public City Start { get; set; }

            public int Distance {
                get {
                    return Itinerary.Sum(i => i.Distance);
                }
            }

            public override string ToString() {
                return Start.Name + "=>" + string.Join("=>", Itinerary.Select(c => c.Destination.Name)) + ": " + Distance;
            }

        }

        private class CyberAmeise {

            private readonly List<City> _nk;
            private City _curr;

            public CyberAmeise(City start, IEnumerable<City> cities) {
                Solution = new Solution {Start = start};
                _nk = cities.Except(new[] {start}).ToList();
                _curr = start;
            }
 
            public Solution Solution { get; private set; }

            public void Crawl() {
                if (_nk.Count > 0) {
                    var next = SelectFromRouletteWheel(_curr.Neighbours.Where(r => _nk.Contains(r.Destination)), ij => Math.Pow(ij.Tau, ALPHA)*Math.Pow(ij.Ny, BETA));
                    Solution.Itinerary.Add(next);
                    _nk.Remove(next.Destination);
                    _curr = next.Destination;
                } else {
                    Solution.Itinerary.Add(_curr.Neighbours.Single(r => r.Destination == Solution.Start));
                }
            }

        }

        private static T SelectFromRouletteWheel<T>(IEnumerable<T> candidates, Func<T, double> prop) {
            var cands = candidates.Select(c => new {cand = c, prob = prop(c)}).ToList();

            var running = 0d;
            var index = 0;

            for (var i = 1; i < cands.Count; i++) {
                var cand = cands[i];
                running += cand.prob;
                if (_rand.NextDouble() <= cand.prob/running) {
                    index = i;
                }
            }
            return cands[index].cand;

        }

    }

}
