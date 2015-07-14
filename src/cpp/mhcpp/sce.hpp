#include "core.h"

namespace mhcpp
{
	namespace optimization
	{
		template<typename T>
		class ShuffledComplexEvolution<T> 
			//: IEvolutionEngine<T>, IPopulation<double>
			//where T : ICloneableSystemConfiguration
	{
		ShuffledComplexEvolution(IObjectiveEvaluator<T> evaluator,
		ICandidateFactory<T> populationInitializer,
		ITerminationCondition<T> terminationCondition,
		SceParameters sceParameters,
		IRandomNumberGeneratorFactory rng = null,
		IFitnessAssignment<double> fitnessAssignment = null, IDictionary<string, string> logTags = null)
		{
			Init(evaluator, populationInitializer, terminationCondition,
				sceParameters.P,
				sceParameters.Pmin,
				sceParameters.M,
				sceParameters.Q,
				sceParameters.Alpha,
				sceParameters.Beta,
				sceParameters.NumShuffle,
				rng,
				fitnessAssignment, logTags,
				sceParameters.TrapezoidalDensityParameter,
				sceParameters.ReflectionRatio,
				sceParameters.ContractionRatio);
		}

		Init(IObjectiveEvaluator<T> evaluator,
			ICandidateFactory<T> populationInitializer,
			ITerminationCondition<T> terminationCondition = null,
			int p = 5,
			int m = 13,
			int q = 7,
			int alpha = 3,
			int beta = 13,
			int numShuffle = 15,
			IRandomNumberGeneratorFactory rng = null,
			IFitnessAssignment<double> fitnessAssignment = null,
			IDictionary<string, string> logTags = null,
			double trapezoidalPdfParam = 1.8,
			SceOptions options = SceOptions.None, int pmin = 5, double reflectionRatio = -1.0, double contractionRatio = 0.5)
		{
			if (m < 2)
				throw new ArgumentException("M is too small");

			if (q > m)
				throw new ArgumentException("Q must be less than or equal to M");

			this->evaluator = evaluator;
			this->populationInitializer = populationInitializer;
			this->terminationCondition = terminationCondition;
			if (this->terminationCondition == null)
				this->terminationCondition = new MaxShuffleTerminationCondition();
			this->terminationCondition.SetEvolutionEngine(this);
			this->p = p;
			this->pmin = pmin;
			this->m = m;
			this->q = q;
			this->alpha = alpha;
			this->beta = beta;
			this->numShuffle = numShuffle;
			this->rng = rng;
			if (this->rng == null)
				this->rng = new BasicRngFactory(0);
			this->fitnessAssignment = fitnessAssignment;
			if (this->fitnessAssignment == null)
				this->fitnessAssignment = new DefaultFitnessAssignment();
			this->logTags = (logTags == null ? new Dictionary<string, string>() : logTags);
			this->trapezoidalPdfParam = trapezoidalPdfParam;
			this->options = options;
			this->ReflectionRatio = reflectionRatio;
			this->ContractionRatio = contractionRatio;
		}

		IDictionary<string, string> logTags = null;
		IObjectiveEvaluator<T> evaluator;
		ICandidateFactory<T> populationInitializer;
		ITerminationCondition<T> terminationCondition;
		IRandomNumberGeneratorFactory rng;
		IFitnessAssignment<double> fitnessAssignment;
		private double trapezoidalPdfParam;

		private ILoggerMh logger = null; // new Log4netAdapter();
		ILoggerMh Logger
		{
			get{ return logger; }
			set{ logger = value; }
		}

		int pmin = 5;
		int p = 5, m = 27, q = 14, alpha = 3, beta = 27;
		int numShuffle = -1;

		int seed = 0;
		//IObjectiveEvaluator<ISystemConfiguration> evaluator;
		//string fullLogFileName = @"c:\tmp\logMoscem.csv";
		//string paretoLog = @"c:\tmp\logParetoMoscem.csv";

		/*
		class MaxShuffleTerminationCondition : ITerminationCondition<T>
		{
			MaxShuffleTerminationCondition()
			{
			}
			ShuffledComplexEvolution<T> algorithm;
			bool IsFinished()
			{
				return algorithm.CurrentShuffle >= algorithm.numShuffle;
			}

			#region ITerminationCondition Members

			void SetEvolutionEngine(IEvolutionEngine<T> engine)
			{
				this->algorithm = (ShuffledComplexEvolution<T>)engine;
			}

			#endregion
		}

		class MarginalImprovementTerminationCondition : MaxWalltimeCheck, ITerminationCondition<T>
		{
			MarginalImprovementTerminationCondition(double maxHours, double tolerance, int cutoffNoImprovement)
				: base(maxHours)
			{
				this->tolerance = tolerance;
				this->maxConverge = cutoffNoImprovement;
			}

			private IPopulation<double> algorithm;
			void SetEvolutionEngine(IEvolutionEngine<T> engine)
			{
				this->algorithm = (IPopulation<double>)engine;
			}


			double oldBest = double.NaN;
			double tolerance = 1e-6;
			int converge = 0;
			int maxConverge = 10;
			bool IsFinished()
			{
				// https://jira.csiro.au/browse/WIRADA-129
				//current SWIFT SCE implementation uses this algorithm to define convergence and it normally guarantees 
				// reproducible optimum is found. It needs two parameters, Tolerance (normally of the order of 10e-6) 
				// and maxConverge (normally of the order of 10)

				if (this->HasReachedMaxTime())
					return true;
				FitnessAssignedScores<double>[] currentPopulation = algorithm.Population;
				if (currentPopulation == null)
					return false;
				var currentBest = currentPopulation.First().FitnessValue;
				if (double.IsNaN(oldBest))
				{
					oldBest = currentBest;
					return false;
				}
				if (Math.Abs(currentBest - oldBest) <= Math.Abs(oldBest * tolerance))
				{
					converge++;
				}
				else
				{
					converge = 0;
				}
				oldBest = currentBest;
				if (converge > maxConverge) return true;
				return false;
			}
		}

		class CoefficientOfVariationTerminationCondition : ITerminationCondition<T>
		{
			private ShuffledComplexEvolution<T> algorithm;
			private double threshold;
			private double maxHours;
			private Stopwatch stopWatch;
			// FIXME: consider something where the termination criteria is customizable to an extent.
			// private Func<double[], double> statistic;

			CoefficientOfVariationTerminationCondition(double threshold = 2.5e-2, double maxHours = 1.0)
			{
				this->threshold = threshold;
				this->maxHours = maxHours;
				this->stopWatch = new Stopwatch();
				stopWatch.Start();
			}

			void SetEvolutionEngine(IEvolutionEngine<T> engine)
			{
				this->algorithm = (ShuffledComplexEvolution<T>) engine;
			}

			bool IsFinished()
			{
				if (this->HasReachedMaxTime())
					return true;
				if (algorithm.numShuffle >= 0 && algorithm.CurrentShuffle >= algorithm.numShuffle)
					return true;
				if (algorithm.PopulationAtShuffling == null)
					return false; // start of the algorithm.
				int n = (int)Math.Ceiling(algorithm.PopulationAtShuffling.Length / 2.0);
				var tmp = algorithm.PopulationAtShuffling.Where((x, i) = > i < n).ToArray();
				var popToTest = Array.ConvertAll<FitnessAssignedScores<double>, IObjectiveScores>(tmp, (x = > x.Scores));
				return IsBelowCvThreshold(popToTest);
			}

			double GetMaxParameterCoeffVar(IObjectiveScores[] population)
			{
				var pSets = ConvertAllToHyperCube(population);
				var varNames = pSets[0].GetVariableNames();
				double[] coeffVar = new double[varNames.Length];
				for (int i = 0; i < varNames.Length; i++)
				{
					coeffVar[i] = calcCoeffVar(MetaheuristicsHelper.GetValues(pSets, varNames[i]));
				}
				return MetaheuristicsHelper.GetMaximum(coeffVar);
			}

			private double calcCoeffVar(double[] p)
			{
				double sum = p.Sum();
				double mean = sum / p.Length;
				double[] diffsMean = Array.ConvertAll(p, x = > x - mean);
				double sumSqrDiffs = Array.ConvertAll(diffsMean, x = > x * x).Sum();
				double sdev = Math.Sqrt(sumSqrDiffs / (p.Length - 1));
				if (mean == 0)
					if (sdev == 0) return 0;
					else return double.PositiveInfinity;
				else
					return Math.Abs(sdev / mean);

			}

			bool IsBelowCvThreshold(IObjectiveScores[] population)
			{
				return GetMaxParameterCoeffVar(population) < threshold;
			}

			bool HasReachedMaxTime()
			{
				double hoursElapsed = this->stopWatch.Elapsed.TotalHours;
				if (this->maxHours <= 0)
					return true;
				else if (this->maxHours < hoursElapsed)
					return true;
				else
					return false;
			}

			double RemainingHours
			{
				get
				{
					return this->maxHours - this->stopWatch.Elapsed.TotalHours;
				}
			}
		}

		class FalseTerminationCondition : ITerminationCondition<T>
		{
			void SetEvolutionEngine(IEvolutionEngine<T> engine)
			{
				// Nothing
			}

			bool IsFinished()
			{
				return false;
			}
		}

		abstract class MaxWalltimeCheck
		{
			private double maxHours;
			private Stopwatch stopWatch;

			protected MaxWalltimeCheck(double maxHours)
			{
				this->maxHours = maxHours;
				this->stopWatch = new Stopwatch();
				stopWatch.Start();
			}

			bool HasReachedMaxTime()
			{
				if (this->maxHours <= 0)
					return false;
				double hoursElapsed = this->stopWatch.Elapsed.TotalHours;
				return (this->maxHours < hoursElapsed);
			}
		}

		class MaxWalltimeTerminationCondition : MaxWalltimeCheck, ITerminationCondition<T>
		{
			MaxWalltimeTerminationCondition(double maxHours) : base(maxHours)
			{
			}

			virtual void SetEvolutionEngine(IEvolutionEngine<T> engine)
			{
				// Nothing
			}
			bool IsFinished()
			{
				return this->HasReachedMaxTime();
			}
		}

		*/

		// From IronPython there are some difficulties calling the constructor of a nested class. Hence the following helper:
		static ITerminationCondition<T> CreateMaxShuffleTerminationCondition()
		{
			return new MaxShuffleTerminationCondition();
		}

		internal static IHyperCube<double>[] ConvertAllToHyperCube(IObjectiveScores[] population)
		{
			var tmp = Array.ConvertAll<IObjectiveScores, IHyperCube<double>>(population, (x = > (IHyperCube<double>)x.GetSystemConfiguration()));
			return tmp;
		}

		int CurrentShuffle{ get; private set; }

			int MaxDegreeOfParallelism
		{
			get{ return parallelOptions.MaxDegreeOfParallelism; }
			set{ parallelOptions.MaxDegreeOfParallelism = value; }
		}
		private ParallelOptions parallelOptions = new ParallelOptions();

		private bool isCancelled = false;
		private IComplex currentComplex;

		private CancellationTokenSource tokenSource = new CancellationTokenSource();
		private SceOptions options = SceOptions.None;
		private IComplex[] complexes = null;

		double ContractionRatio{ get; set; }
		double ReflectionRatio{ get; set; }

			void Cancel()
		{
			isCancelled = true;
			if (currentComplex != null)
				currentComplex.IsCancelled = isCancelled;
			tokenSource.Cancel();
		}

		IOptimizationResults<T> Evolve()
		{
			isCancelled = false;
			IObjectiveScores[] scores = evaluateScores(evaluator, initialisePopulation());
			loggerWrite(scores, createSimpleMsg("Initial Population", "Initial Population"));
			var isFinished = terminationCondition.IsFinished();
			if (isFinished)
			{
				logTerminationConditionMet();
				return packageResults(scores);
			}
			this->complexes = partition(scores);

			//OnAdvanced( new ComplexEvolutionEvent( complexes ) );

			CurrentShuffle = 1;
			isFinished = terminationCondition.IsFinished();
			if (isFinished) logTerminationConditionMet();
			while (!isFinished && !isCancelled)
			{
				if (evaluator.SupportsThreadSafeCloning)
					execParallel(complexes);
				else
				{
					for (int i = 0; i < complexes.Length; i++)
					{
						currentComplex = complexes[i];
						currentComplex.IsCancelled = isCancelled;

						currentComplex.Evolve();
						var complexPoints = currentComplex.GetObjectiveScores().ToArray();
						loggerWrite(sortByFitness(complexPoints).First(), createSimpleMsg("Best point in complex", "Complex No " + currentComplex.ComplexId));
					}
				}
				//OnAdvanced( new ComplexEvolutionEvent( complexes ) );
				var shuffleMsg = "Shuffling No " + CurrentShuffle.ToString("D3");
				var shufflePoints = aggregate(complexes);
				loggerWrite(shufflePoints, createSimpleMsg(shuffleMsg, shuffleMsg));
				this->PopulationAtShuffling = sortByFitness(shufflePoints);
				loggerWrite(PopulationAtShuffling.First(), createSimpleMsg("Best point in shuffle", shuffleMsg));
				complexes = shuffle(complexes);

				CurrentShuffle++;
				isFinished = terminationCondition.IsFinished();
				if (isFinished) logTerminationConditionMet();
			}
			return packageResults(complexes);
		}

		private static IOptimizationResults<T> packageResults(IComplex[] complexes)
		{
			//saveLog( logPopulation, fullLogFileName );
			IObjectiveScores[] population = aggregate(complexes);
			//saveLogParetoFront( population );
			return packageResults(population);
		}

		private static IOptimizationResults<T> packageResults(IObjectiveScores[] population)
		{
			// cater for cases where we have null references (e.g. if the termination condition was in the middle of the population creation)
			return new BasicOptimizationResults<T>(population.Where(p = > (p != null)).ToArray());
		}

		private void logTerminationConditionMet()
		{
			var tags = createSimpleMsg("Termination condition", "Termination condition");
			loggerWrite(string.Format("Termination condition using {0} is met", terminationCondition.GetType().Name), tags);
		}

		private IDictionary<string, string> createSimpleMsg(string message, string category)
		{
			return LoggerMhHelper.CreateTag(LoggerMhHelper.MkTuple("Message", message), LoggerMhHelper.MkTuple("Category", category));
		}

		private void loggerWrite(string infoMsg, IDictionary<string, string> tags)
		{
			LoggerMhHelper.Write(infoMsg, tags, logger);
		}

		private void loggerWrite(IObjectiveScores[] scores, IDictionary<string, string> tags)
		{
			tags = LoggerMhHelper.MergeDictionaries(logTags, tags);
			LoggerMhHelper.Write(scores, tags, logger);
		}

		private void loggerWrite(FitnessAssignedScores<double> scores, IDictionary<string, string> tags)
		{
			tags = LoggerMhHelper.MergeDictionaries(logTags, tags);
			LoggerMhHelper.Write(scores, tags, logger);
		}


		private void execParallel(IComplex[] complexes)
		{
			for (int i = 0; i < complexes.Length; i++)
				complexes[i].ComplexId = i.ToString();
			Parallel.ForEach(complexes, parallelOptions, c = > c.Evolve());
		}

		string GetDescription()
		{
			throw new NotImplementedException();
		}

		interface IComplex
		{
			IEnumerable<IObjectiveScores> GetObjectiveScores();
			void Evolve();
			string ComplexId{ get; set; }
			bool IsCancelled{ get; set; }
		}

		private IComplex[] shuffle(IComplex[] complexes)
		{
			IObjectiveScores[] population = aggregate(complexes);
			IComplex[] newComplexes = partition(population);
			return newComplexes;
		}

		private static IObjectiveScores[] aggregate(IComplex[] complexes)
		{
			List<IObjectiveScores> scores = new List<IObjectiveScores>();
			foreach(var item in complexes)
				scores.AddRange(item.GetObjectiveScores());
			IObjectiveScores[] population = scores.ToArray();
			return population;
		}

		private IObjectiveScores[] evaluateScores(IObjectiveEvaluator<T> evaluator, T[] population)
		{
			return Evaluations.EvaluateScores(evaluator, population, () = > (this->isCancelled || terminationCondition.IsFinished()), parallelOptions);
		}

		private T[] initialisePopulation()
		{
			T[] result = new T[p * m];
			for (int i = 0; i < result.Length; i++)
				result[i] = populationInitializer.CreateRandomCandidate();
			return result;
		}

		private IComplex[] partition(FitnessAssignedScores<double>[] sortedScores)
		{
			List<IComplex> result = new List<IComplex>();
			if (CurrentShuffle > 0)
				if (this->pmin < this->p)
					this->p = this->p - 1;
			for (int a = 0; a < p; a++)
			{
				List<FitnessAssignedScores<double>> sample = new List<FitnessAssignedScores<double>>();
				for (int k = 1; k <= m; k++)
					sample.Add(sortedScores[a + p * (k - 1)]);
				IObjectiveScores[] scores = getScores(sample.ToArray());
				seed++;
				IComplex complex = createComplex(scores);
				complex.ComplexId = CurrentShuffle.ToString("D3") + "_" + Convert.ToString(a + 1);
				result.Add(complex);
			}
			return result.ToArray();
		}

		private IComplex createComplex(IObjectiveScores[] scores)
		{
			IHyperCubeOperationsFactory hyperCubeOperationsFactory = populationInitializer as IHyperCubeOperationsFactory;
			if (hyperCubeOperationsFactory == null)
				throw new NotSupportedException("Currently SCE uses an implementation of a 'complex' that needs a population initializer that implements IHyperCubeOperationsFactory");

			var loggerTags = LoggerMhHelper.MergeDictionaries(logTags, LoggerMhHelper.CreateTag(LoggerMhHelper.MkTuple("CurrentShuffle", this->CurrentShuffle.ToString("D3"))));

			var complex = new DefaultComplex(scores, m, q, alpha, beta,
				(evaluator.SupportsThreadSafeCloning ? evaluator.Clone() : evaluator),
				rng.CreateFactory(),
				getFitnessAssignment(), hyperCubeOperationsFactory.CreateNew(this->rng), logger: this->logger,
			tags : loggerTags, factorTrapezoidalPDF : this->trapezoidalPdfParam,
			   options : this->options, reflectionRatio : this->ReflectionRatio, contractionRatio : this->ContractionRatio);

			complex.TerminationCondition = createMaxWalltimeCondition(this->terminationCondition);
			return complex;
		}

		// https://github.com/jmp75/metaheuristics/issues/3
		private ITerminationCondition<T> createMaxWalltimeCondition(ITerminationCondition<T> terminationCondition)
		{
			var t = terminationCondition as CoefficientOfVariationTerminationCondition;
			if (t == null)
				return new FalseTerminationCondition();
			else
				return new MaxWalltimeTerminationCondition(t.RemainingHours);
		}

		private IComplex[] partition(IObjectiveScores[] scores)
		{
			var sortedScores = sortByFitness(scores);
			//logPoints( CurrentShuffle, sortedScores );
			IComplex[] complexes = partition(sortedScores);
			return complexes;
		}

		private FitnessAssignedScores<double>[] sortByFitness(IObjectiveScores[] scores)
		{
			IFitnessAssignment<double> assignment = getFitnessAssignment();
			var fittedScores = assignment.AssignFitness(scores);
			Array.Sort(fittedScores);
			return fittedScores;
		}

		private IFitnessAssignment<double> getFitnessAssignment()
		{
			return fitnessAssignment;
		}

		private static IObjectiveScores[] getScores(FitnessAssignedScores<double>[] fitnessedScores)
		{
			IObjectiveScores[] result = new IObjectiveScores[fitnessedScores.Length];
			for (int i = 0; i < result.Length; i++)
				result[i] = fitnessedScores[i].Scores;
			return result;
		}

		private class DefaultComplex : IComplex
		{
			private IObjectiveScores[] scores;
			private int m;
			private int q;
			private int alpha;
			private int beta;
			private DiscreteRandomNumberGenerator discreteGenerator;
			IFitnessAssignment<double> fitnessAssignment;
			IObjectiveEvaluator<T> evaluator;
			IHyperCubeOperations hyperCubeOps;
			private ILoggerMh logger = null; // new Log4netAdapter();

			/// <summary>
			/// Discrete version of the inverse transform method
			/// </summary>
			/// <remarks>
			/// Based on the method described in 
			/// chapter 32. MONTE CARLO TECHNIQUES, K. Hagiwara et al., Physical Review D66, 010001-1 (2002)
			/// found at http://pdg.lbl.gov/
			/// </remarks>
			private class DiscreteRandomNumberGenerator
			{
				/// <summary>
				/// Default constraint on absolute value of [ sum of a discrete PDF - 1.0 ]
				/// </summary>
				private const double pdfSumConstraint = smallValue;
				private const double smallValue = 1e-8;
				private double[, ] _cumulativeProbability;
				private int _numberDiscreteRealisations;
				private Random random;

				/// <summary>
				/// Build a discrete probability density function where 
				/// the only realisation is the value zero.
				/// </summary>
				DiscreteRandomNumberGenerator()
				{
					probabilities = new double[, ] { { 0, 1 } };
				}

				/// <summary>
				/// Build a discrete probability density function where 
				/// the only realisation is the value zero, and the random generator 
				/// has a seed.
				/// </summary>
				DiscreteRandomNumberGenerator(int seed)
					: this()
				{
					this->random = new Random(seed);
				}

				/// <summary>
				/// Gets or sets the discrete probability density function where p [ i , 0 ] = Xi
				/// and p [i , 1 ] = Pr ( X = Xi )
				/// It is wise to have the sum of probabilities really close to 1.0,
				/// even "artificially" a long as this is not biasing the PDF. 
				/// Will throw an ArgumentException , on setting, if there is 
				/// a negative value in the array of probabilities
				/// </summary>
				private double[, ] probabilities
				{
					get
					{
						double[, ] result = new double[_numberDiscreteRealisations, 2];
						for (int i = 0; i < _numberDiscreteRealisations - 1; i++)
						{
							result[i, 0] = _cumulativeProbability[i, 0];
							result[i, 1] = _cumulativeProbability[i + 1, 1] - _cumulativeProbability[i, 1];
						}
						result[_numberDiscreteRealisations - 1, 0] =
							_cumulativeProbability[_numberDiscreteRealisations - 1, 0];
						result[_numberDiscreteRealisations - 1, 1] = (1.0 -
							_cumulativeProbability[
								_numberDiscreteRealisations - 1, 1]);

						return result;
					}
						set
					{
						double cumulativeProbability = 0.0;
						_numberDiscreteRealisations = value.Length / 2;
						// checking first;
						double sumProbabilities = 0.0;
						for (int i = 0; i < _numberDiscreteRealisations; i++)
						{
							if (value[i, 1] < 0.0)
								throw new ArgumentException("The Discrete PDF provided contained negative values!");
							sumProbabilities += value[i, 1];
						}
						if (Math.Abs(1.0 - sumProbabilities) > pdfSumConstraint)
							throw new ArgumentException("The Discrete PDF provided adds up to a value to far from 1.0 !");

						_cumulativeProbability = new double[_numberDiscreteRealisations, 2];
						for (int i = 0; i < _numberDiscreteRealisations; i++)
						{
							_cumulativeProbability[i, 0] = value[i, 0];
							_cumulativeProbability[i, 1] = cumulativeProbability; // P ( X < xi )
							cumulativeProbability += value[i, 1];
						}
					}
				}

				double NextDouble()
					{
						double u = random.NextDouble();
						int i = 1; // because the first cumulative probability is at index 1.
						while (u > _cumulativeProbability[i, 1])
						{
							i++;
							if (i == _numberDiscreteRealisations)
								// this is normal : all checks have been performed on the PDF already, 
								// and we do not store probability 1.0 in the cumulative so :
							{
								break;
							}
						}
						return _cumulativeProbability[i - 1, 0];
					}

					/// <summary>
					/// Initialises the discrete PDF so that this is a trapezoidal overall shape
					/// the shape of the trapeze is fully defined by the parameters c and itemNumbers,
					/// knowing that the distribution function indexes are 0, 1, ..., n - 1
					/// </summary>
					/// <param name="c">multiplicator to apply to the index 0, must be between 0 and 2, or an exception occurs. ignored if n = 1</param>
					/// <param name="n">number of items in the discrete PDF</param>
					void initialiseTrapezoidal(double c, int n)
					{
						if ((c > 2) || (c < 0))
							throw new ArgumentException("initialiseTrapezoidal : parameter c cannot be out of [0;2]");
						if (n < 1)
							throw new ArgumentException("initialiseTrapezoidal : parameter n must be strictly positive");

						double increment;
						double first = c / n;
						double[, ] dpdf = new double[n, 2];
						double val;
						if (n == 1) // ( not really meaningful but could happen... )
						{
							dpdf[0, 0] = 0.0;
							dpdf[0, 1] = 1.0;
						}
						else
						{
							increment = -(2 * (c - 1)) / ((n - 1) * n);
							// from constraint sum(i) [0;n-1] ( c/n + increment * i ) = 1

							for (int i = 0; i < n; i++)
							{
								dpdf[i, 0] = (i);
								val = first + increment * i;
								if (Math.Abs(val) < smallValue)
									val = 0.0; // there can be some rounding effects which will give a negative probability density
								dpdf[i, 1] = val;
							}
						}
						probabilities = dpdf;
					}
			}

			private IDictionary<string, string> createTagConcat(params Tuple<string, string>[] tuples)
			{
				return LoggerMhHelper.MergeDictionaries(LoggerMhHelper.CreateTag(tuples), this->tags);
			}

			private string complexId = null; //This id is currently used for only reporting systed to identify the complex from which the log message comes.

			string ComplexId
			{
				get{ return complexId; }
				set{ complexId = value; }
			}

			//Added new argument IHyperCuberations to avoid programming to implementation 
			//by Bill Wang on 19/9/2010
			DefaultComplex(IObjectiveScores[] scores, int m, int q, int alpha, int beta,
			IObjectiveEvaluator<T> evaluator, IRandomNumberGeneratorFactory rng,
			IFitnessAssignment<double> fitnessAssignment, IHyperCubeOperations hyperCubeOperations, ILoggerMh logger = null, IDictionary<string, string> tags = null, double factorTrapezoidalPDF = 1.8,
			SceOptions options = SceOptions.None, double reflectionRatio = -1.0, double contractionRatio = 0.5)
				{
					if (factorTrapezoidalPDF > 2.0 || factorTrapezoidalPDF < 0.0)
						throw new ArgumentOutOfRangeException("factorTrapezoidalPDF", "This must be between 0 and 2");
					this->scores = scores;
					this->m = m;
					this->q = q;
					this->alpha = alpha;
					this->beta = beta;
					this->fitnessAssignment = fitnessAssignment;
					this->hyperCubeOps = hyperCubeOperations;
					this->evaluator = evaluator;
					this->logger = logger;
					this->tags = tags;
					this->factorTrapezoidalPDF = factorTrapezoidalPDF;
					initialiseDiscreteGenerator(rng.Next());
					this->options = options;
					this->ReflectionRatio = reflectionRatio;
					this->ContractionRatio = contractionRatio;
				}

				IDictionary<string, string> tags;
				private double factorTrapezoidalPDF;
				private SceOptions options;

				bool IsCancelled{ get; set; }

				ITerminationCondition<T> TerminationCondition;

				bool IsFinished
				{
					get
					{
						if (TerminationCondition == null)
						return false;
						else
							return TerminationCondition.IsFinished();
					}
				}

				void Evolve()
					{
						if (Thread.CurrentThread.Name == null)
						{
							Thread.CurrentThread.Name = ComplexId;
						}
						int a, b; // counters for alpha and beta parameters
						b = 0;
						while (b < beta && !IsCancelled && !IsFinished)
						{
							IObjectiveScores[] bufferComplex = (IObjectiveScores[])this->scores.Clone();
							IObjectiveScores[] leftOutFromSubcomplex = null;
							FitnessAssignedScores<double>[] subComplex = getSubComplex(bufferComplex, out leftOutFromSubcomplex);
							a = 0;
							while (a < alpha && !IsCancelled && !IsFinished)
							{
								FitnessAssignedScores<double> worstPoint = findWorstPoint(subComplex);
								loggerWrite(worstPoint, createTagConcat(
									LoggerMhHelper.MkTuple("Message", "Worst point in subcomplex"),
									createTagCatComplexNo()));
								IObjectiveScores[] withoutWorstPoint = removePoint(subComplex, worstPoint);
								loggerWrite(withoutWorstPoint, createTagConcat(
									LoggerMhHelper.MkTuple("Message", "Subcomplex without worst point"),
									createTagCatComplexNo()
									));
								T centroid = getCentroid(withoutWorstPoint);

								T reflectedPoint = reflect(worstPoint, centroid);
								if (reflectedPoint != null)
								{
									FitnessAssignedScores<double>[] candidateSubcomplex = null;
									FitnessAssignedScores<double> fitReflectedPoint = evaluateNewSet(reflectedPoint, withoutWorstPoint, out candidateSubcomplex);
									if (fitReflectedPoint.CompareTo(worstPoint) <= 0)
									{
										subComplex = candidateSubcomplex;
										loggerWrite(fitReflectedPoint, createTagConcat(
											LoggerMhHelper.MkTuple("Message", "Reflected point in subcomplex"),
											createTagCatComplexNo()));
									}
									else
									{
										loggerWrite(fitReflectedPoint,
											createTagConcat(LoggerMhHelper.MkTuple("Message", "Reflected point in subcomplex - Failed"), createTagCatComplexNo()));
										subComplex = contractionOrRandom(withoutWorstPoint, worstPoint, centroid, bufferComplex);
										if (subComplex == null) // this can happen if the feasible region of the parameter space is not convex.
											subComplex = fitnessAssignment.AssignFitness(bufferComplex);
									}
								}
								else
								{
									// 2012-02-02 A change to fit the specs of the Duan 1993 paper, to validate the use for AWRA-L.
									// This change is in line after discussions with Neil Viney
									// TODO: After discussion with Neil Viney (2012-02-03): Allow for a strategy where the generation 
									// of the random point can be based on another hypercube than the complex. Duan documents that, but this may
									// prevent a faster convergence.
									//subComplex = contractionOrRandom(withoutWorstPoint, worstPoint, centroid, bufferComplex);
									if ((options & SceOptions.RndInSubComplex) == SceOptions.RndInSubComplex)
									{
										// subComplex = addRandomInHypercube(withoutWorstPoint, bufferComplex);
										if ((options & SceOptions.ReflectionRandomization) == SceOptions.ReflectionRandomization)
											subComplex = generateRandomWithinSubcomplex(withoutWorstPoint, worstPoint);
										else
											subComplex = generateRandomWithinShuffleBounds(worstPoint, centroid, withoutWorstPoint);
									}
									else
									{
										subComplex = addRandomInHypercube(withoutWorstPoint, bufferComplex);
									}
								}
								a++;
							}
							this->scores = aggregatePoints(subComplex, leftOutFromSubcomplex);
							b++;
						}

					}

					private Tuple<string, string> createTagCatComplexNo()
					{
						return LoggerMhHelper.MkTuple("Category", "Complex No " + complexId);
					}

					private void loggerWrite(IObjectiveScores[] points, IDictionary<string, string> tags)
					{
						if (logger != null)
							logger.Write(points, tags);
					}

					private void loggerWrite(FitnessAssignedScores<double> point, IDictionary<string, string> tags)
					{
						if (logger != null)
							logger.Write(point, tags);
					}

					private void loggerWrite(string message, IDictionary<string, string> tags)
					{
						if (logger != null)
							logger.Write(message, tags);
					}

					//private void loggerWrite(IHyperCube<double> point, IDictionary<string, string> tags)
					//{
					//    if (logger != null)
					//        logger.Write(point, tags);
					//}

					private void loggerWrite(IObjectiveScores<T> point, IDictionary<string, string> tags)
					{
						this->loggerWrite(new IObjectiveScores[] { point }, tags);
					}

					private IObjectiveScores[] aggregatePoints(FitnessAssignedScores<double>[] subComplex, IObjectiveScores[] leftOutFromSubcomplex)
					{
						List<IObjectiveScores> result = new List<IObjectiveScores>(convertArrayToScores(subComplex));
						result.AddRange(leftOutFromSubcomplex);
						return result.ToArray();
					}

					private FitnessAssignedScores<double> evaluateNewSet(T reflectedPoint, IObjectiveScores[] withoutWorstPoint, out FitnessAssignedScores<double>[] candidateSubcomplex)
					{
						List<IObjectiveScores> scores = new List<IObjectiveScores>();
						scores.AddRange(withoutWorstPoint);
						IObjectiveScores scoreNewPoint = evaluator.EvaluateScore((T)reflectedPoint);
						scores.Add(scoreNewPoint);
						candidateSubcomplex = fitnessAssignment.AssignFitness(scores.ToArray());
						return Array.Find<FitnessAssignedScores<double>>(candidateSubcomplex, (x = > (x.Scores == scoreNewPoint)));
					}

					private T reflect(FitnessAssignedScores<double> worstPoint, T centroid)
					{
						//double ratio = -1.0;
						double ratio = this->ReflectionRatio;
						return performHomothecy(worstPoint, centroid, ratio);
					}

					private static T performHomothecy(FitnessAssignedScores<double> worstPoint, T centroid, double ratio)
					{
						return (T)((IHyperCube<double>)centroid).HomotheticTransform((IHyperCube<double>)worstPoint.Scores.GetSystemConfiguration(), ratio);
					}

					private T getCentroid(IObjectiveScores[] withoutWorstPoint)
					{
						var tmp = convertAllToHyperCube(withoutWorstPoint);
						IHyperCube<double> result = hyperCubeOps.GetCentroid(tmp);
						return (T)result;
					}

					private static IHyperCube<double>[] convertAllToHyperCube(IObjectiveScores[] withoutWorstPoint)
					{
						return ConvertAllToHyperCube(withoutWorstPoint);
					}

					private IObjectiveScores[] removePoint(FitnessAssignedScores<double>[] subComplex, FitnessAssignedScores<double> worstPoint)
					{
						var tmp = Array.FindAll(subComplex, (x = > !object.ReferenceEquals(worstPoint, x)));
						return convertArrayToScores(tmp);
					}

					private static IObjectiveScores[] convertArrayToScores(FitnessAssignedScores<double>[] tmp)
					{
						return Array.ConvertAll<FitnessAssignedScores<double>, IObjectiveScores>(tmp, (x = > x.Scores));
					}

					private FitnessAssignedScores<double> findWorstPoint(FitnessAssignedScores<double>[] subComplex)
					{
						FitnessAssignedScores<double>[] tmp = (FitnessAssignedScores<double>[])subComplex.Clone();
						Array.Sort(tmp);
						return tmp[tmp.Length - 1];
					}

					protected void initialiseDiscreteGenerator(int seed)
					{
						if (discreteGenerator == null)
							discreteGenerator = new DiscreteRandomNumberGenerator(seed);
						discreteGenerator.initialiseTrapezoidal(factorTrapezoidalPDF, m);
					}

					private FitnessAssignedScores<double>[] getSubComplex(IObjectiveScores[] bufferComplex, out IObjectiveScores[] leftOutFromSubcomplex)
					{
						var fitnessPoints = this->fitnessAssignment.AssignFitness(bufferComplex);
						Array.Sort(fitnessPoints);

						IObjectiveScores[] result = new IObjectiveScores[q];
						int[] selectedIndices = new int[q];
						for (int j = 0; j < selectedIndices.Length; j++)
							selectedIndices[j] = -1; // this is not a random choice !

						int i = 0;
						int counter = 0;
						while (counter < selectedIndices.Length)
						{
							i = (int)discreteGenerator.NextDouble();
							if (Array.IndexOf(selectedIndices, i) < 0)
							{
								selectedIndices[counter] = i;
								counter++;
							}
						}
						for (int j = 0; j < result.Length; j++)
							result[j] = fitnessPoints[selectedIndices[j]].Scores;

						List<IObjectiveScores> leftOut = new List<IObjectiveScores>();
						for (int j = 0; j < fitnessPoints.Length; j++)
						{
							if (Array.IndexOf(selectedIndices, j) < 0)
								leftOut.Add(fitnessPoints[j].Scores);
						}
						leftOutFromSubcomplex = leftOut.ToArray();
						return fitnessAssignment.AssignFitness(result);
					}

					private FitnessAssignedScores<double>[] contractionOrRandom(IObjectiveScores[] withoutWorstPoint,
						FitnessAssignedScores<double> worstPoint, T centroid, IObjectiveScores[] bufferComplex)
					{
						FitnessAssignedScores<double>[] result;
						FitnessAssignedScores<double>[] candidateSubcomplex = null;
						T contractionPoint = contract(worstPoint, centroid);
						FitnessAssignedScores<double> fitReflectedPoint = null;
						if (contractionPoint != null)
						{
							fitReflectedPoint = evaluateNewSet(contractionPoint, withoutWorstPoint, out candidateSubcomplex);
							if (fitReflectedPoint.CompareTo(worstPoint) <= 0)
							{
								result = candidateSubcomplex;
								loggerWrite(fitReflectedPoint, createTagConcat(
									LoggerMhHelper.MkTuple("Message", "Contracted point in subcomplex"),
									createTagCatComplexNo()));
								return result;
							}
						}
						if (contractionPoint != null && fitReflectedPoint != null)
							loggerWrite(fitReflectedPoint, createTagConcat(
							LoggerMhHelper.MkTuple("Message", "Contracted point in subcomplex-Failed"),
							createTagCatComplexNo()));
						else
						{
							var msg = "Contracted point unfeasible";
							loggerWrite(msg, createTagConcat(LoggerMhHelper.MkTuple("Message", msg), createTagCatComplexNo()));
						}
						// 2012-02-14: The Duan et al 1993 paper specifies to use the complex to generate random points. However, comparison to a Matlab
						// implementation showed a slower rate of convergence. 
						// result = addRandomInHypercube(withoutWorstPoint, bufferComplex);
						result = generateRandomWithinSubcomplex(withoutWorstPoint, worstPoint);
						return result;
					}

					private FitnessAssignedScores<double>[] generateRandomWithinShuffleBounds(FitnessAssignedScores<double> worstPoint, T centroid, IObjectiveScores[] withoutWorstPoint)
					{
						var ctr = centroid as IHyperCube<double>;
						var sbcplx = convertAllToHyperCube(merge(withoutWorstPoint, worstPoint));
						var wp = worstPoint.Scores.GetSystemConfiguration() as IHyperCube<double>;
						var newPoint = wp.Clone() as IHyperCube<double>;
						var varnames = newPoint.GetVariableNames();
						var rand = hyperCubeOps.GenerateRandomWithinHypercube(sbcplx);
						for (int i = 0; i < varnames.Length; i++)
						{
							var v = varnames[i];
							var value = 2 * ctr.GetValue(v) - wp.GetValue(v);
							if (value < wp.GetMinValue(v) || value > wp.GetMaxValue(v))
								newPoint.SetValue(v, rand.GetValue(v));
							else
								newPoint.SetValue(v, value);
						}
						var newScore = evaluator.EvaluateScore((T)newPoint);
						loggerWrite(newScore, createTagConcat(
							LoggerMhHelper.MkTuple("Message", "Adding a partially random point"),
							LoggerMhHelper.MkTuple("Category", "Complex No " + complexId)
							));
						return fitnessAssignment.AssignFitness(aggregate(newScore, withoutWorstPoint));
					}

					private FitnessAssignedScores<double>[] generateRandomWithinSubcomplex(IObjectiveScores[] withoutWorstPoint, FitnessAssignedScores<double> worstPoint)
					{
						// 2012-02-14: The Duan et al 1993 paper specifies to use the complex to generate random points. However, comparison to a Matlab
						// implementation showed a slower rate of convergence. 
						FitnessAssignedScores<double>[] result;
						var subCplx = merge(withoutWorstPoint, worstPoint);
						result = addRandomInHypercube(withoutWorstPoint, subCplx);
						return result;
					}

					private FitnessAssignedScores<double>[] addRandomInHypercube(IObjectiveScores[] withoutWorstPoint, IObjectiveScores[] popForHypercubeDefn)
					{
						var tmp = convertAllToHyperCube(popForHypercubeDefn);
						IHyperCube<double> newPoint = hyperCubeOps.GenerateRandomWithinHypercube(tmp);
						if (newPoint == null)
						{
							var msg = "Random point within hypercube bounds is unfeasible";
							loggerWrite(msg, createTagConcat(LoggerMhHelper.MkTuple("Message", msg), createTagCatComplexNo()));
							return null;
						}
						var newScore = evaluator.EvaluateScore((T)newPoint);
						loggerWrite(newScore, createTagConcat(
							LoggerMhHelper.MkTuple("Message", "Adding a random point in hypercube"),
							createTagCatComplexNo()
							));

						IObjectiveScores[] newSubComplex = aggregate(newScore, withoutWorstPoint);
						return fitnessAssignment.AssignFitness(newSubComplex);
					}

					private static IObjectiveScores[] merge(IObjectiveScores[] withoutWorstPoint, FitnessAssignedScores<double> worstPoint)
					{
						var tmp = withoutWorstPoint.ToList();
						tmp.Add(worstPoint.Scores);
						return tmp.ToArray();
					}

					private IObjectiveScores[] aggregatePoints(IHyperCube<double> newPoint, IObjectiveScores[] withoutWorstPoint)
					{
						return aggregate(evaluator.EvaluateScore((T)newPoint), withoutWorstPoint);
					}

					private IObjectiveScores[] aggregate(IObjectiveScores newPoint, IObjectiveScores[] withoutWorstPoint)
					{
						List<IObjectiveScores> result = new List<IObjectiveScores>(withoutWorstPoint);
						result.Add(newPoint);
						return result.ToArray();
					}

					private T contract(FitnessAssignedScores<double> worstPoint, T centroid)
					{
						//double ratio = 0.5;
						double ratio = this->ContractionRatio;
						return performHomothecy(worstPoint, centroid, ratio);
					}

					IEnumerable<IObjectiveScores> GetObjectiveScores()
					{
						return (IEnumerable<IObjectiveScores>)this->scores.Clone();
					}

					double ContractionRatio{ get; set; }
					double ReflectionRatio{ get; set; }
		}

		/*
		private class ComplexEvolutionEvent : EventArgs, IMonitoringEvent
		{
		private IObjectiveScores[] scoresSet;

		ComplexEvolutionEvent( IComplex[] complexes )
		{
		List<IObjectiveScores> list = new List<IObjectiveScores>( );

		foreach( IComplex complex in complexes )
		{
		foreach( IObjectiveScores scores in complex.GetObjectiveScores( ) )
		{
		list.Add( scores );
		}
		}

		this->scoresSet = list.ToArray( );
		}

		IObjectiveScores[] ScoresSet
		{
		get { return scoresSet; }
		}
		}

		*/

		FitnessAssignedScores<double>[] PopulationAtShuffling{ get; set; }

			FitnessAssignedScores<double>[] Population
		{
			get
			{
				if (complexes == null) return null;
				return sortByFitness(aggregate(complexes));
			}
		}

	}
	}

	}
}
