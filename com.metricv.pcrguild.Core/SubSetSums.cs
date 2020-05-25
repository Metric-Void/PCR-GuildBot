using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace com.metricv.pcrguild.Code {
    class SubSetSums {
        Dictionary<String, Double> choices = new Dictionary<string, double>();

        public void addChoice(String K, double V) {
            choices.Add(K, V);
        }

        public void addChoices(Dictionary<String, Double> KVs) {
            choices = choices.Keys.Union(KVs.Keys).ToDictionary(
                k => k,
                k => KVs.ContainsKey(k)? KVs[k] : choices[k] 
            );
        }
        private class SubList {
            public double size;
            public List<KeyValuePair<String, Double>> subList;

            public SubList() {
                this.size = 0;
                this.subList = new List<KeyValuePair<String, Double>>();
            }

            public SubList(double size, List<KeyValuePair<String, Double>> subList) {
                this.size = size;
                this.subList = subList;
            }
        }

        public List<String> sumTo(double V) {
            double listSum = 0;
            foreach (double x in choices.Values) {
                listSum += x;
            }
            if (listSum < V)
                return new List<String>();
            else {
                SubList opt = new SubList();

                HashSet<SubList> sums = new HashSet<SubList>();
                sums.Add(opt);

                List<KeyValuePair<String, Double>> sortedChoices = choices.ToList();
                sortedChoices.Sort(
                    (a,b) => b.Value - a.Value > 0 ? 1 : -1
                );

                foreach (KeyValuePair<String, Double> input in sortedChoices) {
                    HashSet<SubList> newSums = new HashSet<SubList>();
                    listSum -= input.Value;
                    foreach (SubList sum in sums) {
                        List<KeyValuePair<String, Double>> newSubList = new List<KeyValuePair<String, Double>>(sum.subList);
                        newSubList.Add(input);
                        SubList newSum = new SubList(sum.size + input.Value, newSubList);

                        // Ignore too small sums.
                        if (newSum.size + listSum > V)
                            newSums.Add(newSum);
                        
                        if (newSum.size >= V) {
                            if (opt.size < V || newSum.size < opt.size) {
                                opt = newSum;
                            }
                        }
                    }
                    sums.UnionWith(newSums);
                }

                List<String> result = new List<string>();
                foreach (KeyValuePair<String, Double> e in opt.subList) {
                    result.Add(e.Key);
                }
                return result;
            }
        }
    }
}
