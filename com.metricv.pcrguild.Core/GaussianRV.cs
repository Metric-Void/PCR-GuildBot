using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.DataVisualization.Charting;

namespace com.metricv.pcrguild.Code {
    public class GaussianRV {
        Double E {get; set; }
        Double V {get; set; }

        List<long> DiscrOberv = new List<long>();
        Dictionary<long, Double> cache = new Dictionary<long, double>();

        public void addObservation(long K) {
            DiscrOberv.Add(K);
            cache = new Dictionary<long, double>();
        }

        public void addAll(List<long> Ks) {
            DiscrOberv.AddRange(Ks);
        }
        public void addAll(List<int> Ks) {
            DiscrOberv.AddRange(Ks.Select(i=>(long)i).ToList());
        }

        public void calculate() {
            long sum = 0;
            foreach(long K in DiscrOberv) {
                sum += K;
            }
            E = 1.0 * sum / DiscrOberv.Count;
            V = 0;
            foreach(long K in DiscrOberv) {
                V += Math.Pow(K-E, 2.0) / DiscrOberv.Count;
            }
        }

        public Double topPercent(int percent) {
            if (percent == 50) {
                return E;
            } else if (cache.ContainsKey(percent)) {
                return cache[percent];
            } else {
                var chart = new Chart();
                cache[percent] = chart.DataManipulator.Statistics.InverseNormalDistribution(1-percent/100.0)*Math.Sqrt(V)+E;
                return cache[percent];
            }
        }

        public Double getProbability(double outcome) {
            if (V == 0)
                return outcome == E ? 1.0 : 0.0 ;
            double Qin = (outcome - E) / Math.Sqrt(V);
            var chart = new Chart();
            return chart.DataManipulator.Statistics.NormalDistribution(Qin);
        }

        public Double getDeviateProbability(double outcome_deviate) {
            if (V == 0)
                return outcome_deviate == 0 ? 1.0 : 0.0;
            double Qin = outcome_deviate / Math.Sqrt(V);
            var chart = new Chart();
            return chart.DataManipulator.Statistics.NormalDistribution(Qin);
        }

        public Double confidence(int percent) {
            double delta = Math.Abs(topPercent(percent)-E);
            if (delta <= 0.0001)
                return 1.0;
            double true_percent = getDeviateProbability(delta);
            //double a = (delta/E) / (Math.Sqrt((percent/100.0) * (1.0-percent/100.0) / DiscrOberv.Count));
            double a = (delta/Math.Sqrt(V)) / (Math.Sqrt(true_percent * (1.0 - true_percent) / DiscrOberv.Count));
            return 1.0 - 1.0 / Math.Pow(a, 2);
        }

        // Ah it's the Q function loved by everyone.
        public static double QNorm(double p, double mu, double sigma, bool lower_tail, bool log_p) {
            if (double.IsNaN(p) || double.IsNaN(mu) || double.IsNaN(sigma))
                return (p + mu + sigma);
            double ans;
            bool isBoundaryCase = R_Q_P01_boundaries(p, double.NegativeInfinity, double.PositiveInfinity, lower_tail, log_p, out ans);
            if (isBoundaryCase)
                return (ans);
            if (sigma < 0)
                return (double.NaN);
            if (sigma == 0)
                return (mu);

            double p_ = R_DT_qIv(p, lower_tail, log_p);
            double q = p_ - 0.5;
            double r, val;

            if (Math.Abs(q) <= 0.425)  // 0.075 <= p <= 0.925
            {
                r = .180625 - q * q;
                val = q * (((((((r * 2509.0809287301226727 +
                           33430.575583588128105) * r + 67265.770927008700853) * r +
                         45921.953931549871457) * r + 13731.693765509461125) * r +
                       1971.5909503065514427) * r + 133.14166789178437745) * r +
                     3.387132872796366608)
                / (((((((r * 5226.495278852854561 +
                         28729.085735721942674) * r + 39307.89580009271061) * r +
                       21213.794301586595867) * r + 5394.1960214247511077) * r +
                     687.1870074920579083) * r + 42.313330701600911252) * r + 1.0);
            } else {
                r = q > 0 ? R_DT_CIv(p, lower_tail, log_p) : p_;
                r = Math.Sqrt(-((log_p && ((lower_tail && q <= 0) || (!lower_tail && q > 0))) ? p : Math.Log(r)));

                if (r <= 5)              // <==> min(p,1-p) >= exp(-25) ~= 1.3888e-11
                {
                    r -= 1.6;
                    val = (((((((r * 7.7454501427834140764e-4 +
                            .0227238449892691845833) * r + .24178072517745061177) *
                          r + 1.27045825245236838258) * r +
                         3.64784832476320460504) * r + 5.7694972214606914055) *
                       r + 4.6303378461565452959) * r +
                      1.42343711074968357734)
                     / (((((((r *
                              1.05075007164441684324e-9 + 5.475938084995344946e-4) *
                             r + .0151986665636164571966) * r +
                            .14810397642748007459) * r + .68976733498510000455) *
                          r + 1.6763848301838038494) * r +
                         2.05319162663775882187) * r + 1.0);
                } else                     // very close to  0 or 1 
                  {
                    r -= 5.0;
                    val = (((((((r * 2.01033439929228813265e-7 +
                            2.71155556874348757815e-5) * r +
                           .0012426609473880784386) * r + .026532189526576123093) *
                         r + .29656057182850489123) * r +
                        1.7848265399172913358) * r + 5.4637849111641143699) *
                      r + 6.6579046435011037772)
                     / (((((((r *
                              2.04426310338993978564e-15 + 1.4215117583164458887e-7) *
                             r + 1.8463183175100546818e-5) * r +
                            7.868691311456132591e-4) * r + .0148753612908506148525)
                          * r + .13692988092273580531) * r +
                         .59983220655588793769) * r + 1.0);
                }
                if (q < 0.0)
                    val = -val;
            }

            return (mu + sigma * val);
        }

        private static bool R_Q_P01_boundaries(double p, double _LEFT_, double _RIGHT_, bool lower_tail, bool log_p, out double ans) {
            if (log_p) {
                if (p > 0.0) {
                    ans = double.NaN;
                    return (true);
                }
                if (p == 0.0) {
                    ans = lower_tail ? _RIGHT_ : _LEFT_;
                    return (true);
                }
                if (p == double.NegativeInfinity) {
                    ans = lower_tail ? _LEFT_ : _RIGHT_;
                    return (true);
                }
            } else {
                if (p < 0.0 || p > 1.0) {
                    ans = double.NaN;
                    return (true);
                }
                if (p == 0.0) {
                    ans = lower_tail ? _LEFT_ : _RIGHT_;
                    return (true);
                }
                if (p == 1.0) {
                    ans = lower_tail ? _RIGHT_ : _LEFT_;
                    return (true);
                }
            }
            ans = double.NaN;
            return (false);
        }

        private static double R_DT_qIv(double p, bool lower_tail, bool log_p) {
            return (log_p ? (lower_tail ? Math.Exp(p) : -ExpM1(p)) : R_D_Lval(p, lower_tail));
        }

        private static double R_DT_CIv(double p, bool lower_tail, bool log_p) {
            return (log_p ? (lower_tail ? -ExpM1(p) : Math.Exp(p)) : R_D_Cval(p, lower_tail));
        }

        private static double R_D_Lval(double p, bool lower_tail) {
            return lower_tail ? p : 0.5 - p + 0.5;
        }

        private static double R_D_Cval(double p, bool lower_tail) {
            return lower_tail ? 0.5 - p + 0.5 : p;
        }
        private static double ExpM1(double x) {
            if (Math.Abs(x) < 1e-5)
                return x + 0.5 * x * x;
            else
                return Math.Exp(x) - 1.0;
        }
    }
}
