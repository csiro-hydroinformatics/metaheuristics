using System;
using System.Linq;
using CSIRO.Metaheuristics;
using CSIRO.Metaheuristics.Objectives;
using System.Text;
using CSIRO.Metaheuristics.RandomNumberGenerators;
using System.Collections.Generic;
using System.Collections;
using CSIRO.Metaheuristics.Utils;

namespace HelloWorld
{
    /// <summary>
    /// The canonical Hello World sample, albeit more involved than usually is. 
    /// </summary>
    class MainClass
    {
        public static void Main(string[] args)
        {
            // If you have to remember *one* thing from this tutorial, it is the three essential players 
            // 1- the tunable parameters of the optimisation problem: 
            //    ISystemConfiguration, here implemented by StringSpecification for this 'Hello World' problem
            // 2- the object evaluating the score(s) for a given "system configuration" (IObjectiveEvaluator<T>)
            // 3- the optimisation algorithm, implementing IEvolutionEngine<T>
            // It is recommended that you declare variables typed as interfaces, not concrete classes, 
            // whenever possible including in an 'Hello World'...
            IObjectiveEvaluator<StringSpecification> evaluator;
            IEvolutionEngine<StringSpecification> finder;

            string strToFind = args.Length == 0 ? "Hello, World!" : args[0];
            evaluator = buildEvaluator(strToFind);
            finder = new StringFinder(evaluator, strToFind.Length, new BasicRngFactory(0));
            var result = finder.Evolve();
            Console.WriteLine(MetaheuristicsHelper.GetHumanReadable(result));
        }

        static StringDistance buildEvaluator(string targetString)
        {
            return new StringDistance() { GoalString = targetString };
        }

        // It may seem odd and unnecessary cruft to have to wrap a string. 
        // It is indeed 'cruft' in the simpler Hello World context. 
        /// However this comes in really handy later on... 
        private class StringSpecification : ISystemConfiguration
        {
            public StringSpecification(string s)
            {
                this.StrVal = s;
            }

            public string StrVal { get; set; }

            public string GetConfigurationDescription()
            {
                return StrVal;
            }

            public void ApplyConfiguration(object system)
            {
                // Nothing to do here...
                // In "real" problems (not to disparage 'Hello, World!'), 
                // the system configuration, needs to be applied to a bigger system, e.g. a hydraulic model.
            }
        }

        // All optimistation problems need one or more measure of performance, 
        // e.g. goodness of fit to observables. Let's use a simple L-1 metric (linear distance) 
        private class StringDistance : IObjectiveEvaluator<StringSpecification>
        {
            public string GoalString { get; set; }

            public IObjectiveScores<StringSpecification> EvaluateScore(StringSpecification systemConfiguration)
            {
                // Let's reuse an existing container class from the core framework: 
                return new SingleScore<StringSpecification>(calculateDistance(systemConfiguration), systemConfiguration);
            }

            IObjectiveScore calculateDistance(StringSpecification systemConfiguration)
            {
                return new DoubleObjectiveScore("L one", calculateDist(GoalString, systemConfiguration), false);
            }

            double calculateDist(string goalString, StringSpecification systemConfiguration)
            {
                if (goalString.Length != systemConfiguration.StrVal.Length)
                    throw new ArgumentException();
                return calculateDist(goalString, systemConfiguration.StrVal);
            }

            double calculateDist(string goal, string candidate)
            {
                var g = goal.ToCharArray();
                var c = candidate.ToCharArray();
                double distance = 0;
                for (int i = 0; i < g.Length; i++)
                {
                    distance += Math.Abs(Convert.ToInt32(g[i]) - Convert.ToInt32(c[i]));
                }
                return distance;
            }
        }

        private class StringFinder : IEvolutionEngine<StringSpecification>
        {
            public StringFinder(IObjectiveEvaluator<StringSpecification> objEval, int strLength, IRandomNumberGeneratorFactory rng)
            {
                this.objEval = objEval;
                this.strLength = strLength;
                vector = new int[strLength];
                initString(rng, strLength);
            }
            public int Max = 150;

            public IOptimizationResults<StringSpecification> Evolve()
            {
                // The search algorithm. A simple directed search in the letter space 
                // (ASCII character mapped to a numeric space: [integer modulo 128] for each letter of the string) 
                // The details of the search algorithm is not the key message of this sample code; see the Main method...

                // Note that in general the Evolve() 
                // method should be called once only. There is no provision 
                // as of writing to "reset" a search (just create a new search...), 
                // though there is one to cancel (to prevent CPU time waste)
                cancelled = false;
                while (CurrentGeneration < Max)
                {
                    if (cancelled == true)
                        break;
                    if (vector.All(x => x == 0)) // We have found the best possible fit
                        break;
                    else
                    {
                        for (int i = 0; i < this.strLength; i++)
                        {
                            if (vector[i] == 0)
                                continue;
                            else
                            {
                                var candidate = testMove(i);
                                if (candidate.Item1 < 0) // new point is better than old
                                    best = candidate.Item2;
                                else
                                {
                                    vector[i] *= -1;
                                    candidate = testMove(i);
                                    if (candidate.Item1 < 0) // new point is better than old
                                        best = candidate.Item2;
                                    else // we must have found the right letter?
                                        vector[i] = 0;
                                }
                            }
                        }
                        Console.WriteLine("Current string is '{0}', distance {1}", best, objEval.EvaluateScore(new StringSpecification(best)));
                        CurrentGeneration++;
                    }
                }
                var res = objEval.EvaluateScore(new StringSpecification(best));
                return new StringFinderResults(res);
            }

            private Tuple<int, string> testMove(int index)
            {
                string candidate = move(best, vector, index);
                var newScore = objEval.EvaluateScore(new StringSpecification(candidate)).GetObjective(0).ValueComparable;
                var prevScore = objEval.EvaluateScore(new StringSpecification(best)).GetObjective(0).ValueComparable;
                return new Tuple<int, string>(newScore.CompareTo(prevScore), candidate);
            }

            private static string move(string start, int[] vector, int index)
            {
                var s = start.ToCharArray();
                s[index] = Convert.ToChar((Convert.ToUInt32(s[index]) + vector[index]) % 128);
                return new string(s);
            }

            public string GetDescription()
            {
                return "A simple search scheme for ASCII strings of a fixed length.";
            }

            public void Cancel()
            {
                lock (this)
                {
                    this.cancelled = true;
                }
            }

            public int CurrentGeneration
            {
                get;
                private set;
            }

            private class StringFinderResults : IOptimizationResults<StringSpecification>
            {
                public StringFinderResults(IObjectiveScores<StringSpecification> res)
                {
                    this.Results = new List<IObjectiveScores<StringSpecification>>();
                    Results.Add(res);
                }
                public List<IObjectiveScores<StringSpecification>> Results;
                public IEnumerator<IObjectiveScores> GetEnumerator()
                {
                    return Results.GetEnumerator();
                }

                IEnumerator IEnumerable.GetEnumerator()
                {
                    return Results.GetEnumerator();
                }
            }

            IObjectiveEvaluator<StringSpecification> objEval;
            int strLength;
            String best;
            int[] vector;
            private bool cancelled = true;
            private void initString(IRandomNumberGeneratorFactory rng, int length)
            {
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < length; i++)
                {
                    sb.Append(Convert.ToChar(rng.Next() % 128));
                    vector[i] = (rng.Next() % 2) * 2 - 1; // -1 or 1
                }
                this.best = sb.ToString();
            }
        }
    }
}