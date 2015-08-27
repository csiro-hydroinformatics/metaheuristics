#pragma once

#include <algorithm>
#include "core.hpp"
#include "evaluations.hpp"


namespace mhcpp
{
	namespace logging
	{
		/// <summary>
		/// A facade for logging information from optimisation processes, to avoid coupling to specific frameworks.
		/// </summary>
		class ILoggerMh
		{
			//virtual void Write(std::vector<IBaseObjectiveScores> scores, std::map<string, string> tags) = 0;
			//virtual void Write(FitnessAssignedScores<double> worstPoint, std::map<string, string> tags) = 0;
			virtual void Write(IHyperCube<double>* newPoint, std::map<string, string> tags) = 0;
			virtual void Write(string message, std::map<string, string> tags) = 0;
		};
	}
}

namespace mhcpp
{
	namespace optimization
	{
		using namespace mhcpp::logging;
		using namespace mhcpp::objectives;

		template<typename T>
		class IOptimizationResults // : public std::vector < IObjectiveScores<T> >
			//where T : ISystemConfiguration
		{
			std::vector<IObjectiveScores<T>> scores;

		public:
			typedef typename std::vector<IObjectiveScores<T>>::const_iterator const_iterator;
			IOptimizationResults() {}

			IOptimizationResults(const std::vector<IObjectiveScores<T>>& scores)
			{
				this->scores = scores;
			}

			IOptimizationResults(const IOptimizationResults& src)
			{
				this->scores = src.scores;
			}

			IOptimizationResults(const IOptimizationResults&& src)
			{
				this->scores = std::move(src.scores);
			}

			IOptimizationResults& operator=(const IOptimizationResults& src)
			{
				if (&src == this) {
					return *this;
				}
				this->scores = src.scores;
				return *this;
			}

			IOptimizationResults& operator=(const IOptimizationResults&& src)
			{
				if (&src == this) {
					return *this;
				}
				this->scores = std::move(src.scores);
				return *this;
			}

			size_t size() const
			{
				return scores.size();
			}

			const_iterator begin() const
			{
				return scores.begin();
			}

			const_iterator end() const
			{
				return scores.end();
			}

			const IObjectiveScores<T>& operator[](size_t index) const
			{
				return scores[index];
			}

			void PrintTo(std::ostream& stream)
			{
				int n = size();
				for (size_t i = 0; i < n; ++i)
					stream << i << ": " << scores[i].ToString() << std::endl;
			}

		};

		//template<typename T>
		//class BasicOptimizationResults : public IOptimizationResults<T>
		//{
		//public:
		//	BasicOptimizationResults() {}
		//	BasicOptimizationResults(const BasicOptimizationResults& src) {}
		//	BasicOptimizationResults(const std::vector<IObjectiveScores<T>>& scores) {}
		//};

		class SceParameters
		{
		public:
			SceParameters()
			{
				this->TrapezoidalDensityParameter = 1.9;
				this->ReflectionRatio = -1.0;
				this->ContractionRatio = 0.5;
				// FIXME: the following is due to short timelines asked in the AWRA and related projects.
				//this->ReflectionRatio = -0.8;
				//this->ContractionRatio = 0.45;
			}
			/// <summary>
			/// Number of geometrical transformation for each subcomplex
			/// </summary>
			int Alpha;
			/// <summary>
			/// Number of evolution steps taken by each sub-complex before shuffling occurs
			/// </summary>
			int Beta;
			/// <summary>
			/// Number of complexes
			/// </summary>
			int P = 5;
			/// <summary>
			/// Minimum number of complexes (populations of points)
			/// </summary>
			int Pmin = 3;
			/// <summary>
			/// Number of points per complex
			/// </summary>
			int M;
			/// <summary>
			/// Number of points per SUB-complex
			/// </summary>
			int Q;
			int NumShuffle;
			double TrapezoidalDensityParameter;

			/// <summary>
			/// The homothetic ratio used in the reflection phase of the complex evolution: default -1.0
			/// </summary>
			double ReflectionRatio;

			/// <summary>
			/// The homothetic ratio used in the contraction phase of the complex evolution: default 0.5
			/// </summary>
			double ContractionRatio;

			static SceParameters CreateForProblemOfDimension(int n, int nshuffle)
			{
				if (n <= 0)
					throw new std::logic_error("There must be at least one free parameter to calibrate");
				SceParameters result;
				result.M = 2 * n + 1;
				result.Q = std::max(result.M - 2, 2);
				result.Alpha = 1;
				result.Beta = result.M;
				result.NumShuffle = nshuffle;
				return result;
			}
		};

		enum SceOptions
		{
			None = 0x00,
			ReflectionRandomization = 0x01,
			RndInSubComplex = 0x02,
			FutureOption_1 = 0x04,
			FutureOption_2 = 0x08
		};

		template<typename T>
		class Complex;

		template<typename T>
		class SubComplex
		{
		public:

			SubComplex(const std::vector<IObjectiveScores<T>>& complexPopulation, IObjectiveEvaluator<T>* evaluator, int q, int alpha,
				IRandomNumberGeneratorFactory rng, ICandidateFactory<T> * candidateFactory, RngInt& discreteRng, 
				IFitnessAssignment<double, T> fitnessAssignment, SceOptions options = SceOptions::RndInSubComplex, double contractionRatio = 0.5, double reflectionRatio = -1.0)
			{
				Init(complexPopulation, evaluator, q, alpha, rng, candidateFactory, fitnessAssignment, options, contractionRatio, reflectionRatio, discreteRng);
			}

			SubComplex(Complex<T>& complex)
			{
				this->complex = &complex;
				Init(complex.scores, complex.evaluator, complex.q, complex.alpha, 
					complex.rng, complex.candidateFactory, complex.fitnessAssignment, 
					complex.options, complex.ContractionRatio, complex.ReflectionRatio, complex.discreteGenerator);
			}

			bool IsCancelledOrFinished()
			{
				if (complex != nullptr)
					return this->complex->IsCancelledOrFinished();
				return false;
			}

			void Evolve()
			{
				int a = 0;
				while (a < alpha && !IsCancelledOrFinished())
				{
					std::vector<FitnessAssignedScores<double, T>> withoutWorstPoint;
					FitnessAssignedScores<double, T> worstPoint = FindWorstPoint(evolved, withoutWorstPoint);
					//loggerWrite(worstPoint, createTagConcat(
					//	LoggerMhHelper.MkTuple("Message", "Worst point in subcomplex"),
					//	createTagCatComplexNo()));
					//loggerWrite(withoutWorstPoint, createTagConcat(
					//	LoggerMhHelper.MkTuple("Message", "Subcomplex without worst point"),
					//	createTagCatComplexNo()
					//	));
					T centroid = getCentroid(withoutWorstPoint);

					bool success;
					T reflectedPoint = reflect(worstPoint, centroid, success);
					if (success)
					{
						std::vector<FitnessAssignedScores<double, T>> candidateSubcomplex;
						// TODO: if we are using multi-objective Pareto optimisation, the following two lines should be re-scrutinized. 
						// While the behavior in the C# implementation was sensical, there may be some smarter ways to test the 
						// improvement in the fitness. Also a valid remark in single-objective case.
						FitnessAssignedScores<double, T> fitReflectedPoint = evaluateNewSet(reflectedPoint, withoutWorstPoint, candidateSubcomplex);
						if (fitReflectedPoint.CompareTo(worstPoint) <= 0)
						{
							replaceEvolved(candidateSubcomplex);
							//deleteElements(evolved);
							//evolved = candidateSubcomplex;

							//loggerWrite(fitReflectedPoint, createTagConcat(
							//	LoggerMhHelper.MkTuple("Message", "Reflected point in subcomplex"),
							//	createTagCatComplexNo()));
						}
						else
						{
							clear(candidateSubcomplex);
							//loggerWrite(fitReflectedPoint,
							//	createTagConcat(LoggerMhHelper.MkTuple("Message", "Reflected point in subcomplex - Failed"), createTagCatComplexNo()));
							candidateSubcomplex = contractionOrRandom(withoutWorstPoint, worstPoint, centroid);
							//if (candidateSubcomplex == nullptr) // this can happen if the feasible region of the parameter space is not convex.
							//	candidateSubcomplex = fitnessAssignment.AssignFitness(bufferComplex);
							replaceEvolved(candidateSubcomplex);
						}
					}
					else
					{
						// 2012-02-02 A change to fit the specs of the Duan 1993 paper, to validate the use for AWRA-L.
						// This change is in line after discussions with Neil Viney
						// TODO: After discussion with Neil Viney (2012-02-03): Allow for a strategy where the generation 
						// of the random point can be based on another hypercube than the complex. Duan documents that, but this may
						// prevent a faster convergence.
						//evolved = contractionOrRandom(withoutWorstPoint, worstPoint, centroid, bufferComplex);

						replaceEvolved(replaceWithRandom(withoutWorstPoint, worstPoint));
#ifdef _DEBUG
						//CheckParameterFeasible(evolved);
#endif

					}
					a++;
				}
			}

			std::vector<IObjectiveScores<T>> WholePopulation()
			{
				auto scores = aggregatePoints(evolved, leftOutFromSubcomplex);
#ifdef _DEBUG
				//CheckParameterFeasible(scores);
#endif
				return scores;
			}

			static FitnessAssignedScores<double, T> FindWorstPoint(std::vector<FitnessAssignedScores<double, T>>& subComplex, std::vector<FitnessAssignedScores<double, T>>& pointRemoved)
			{
				auto tmp = AsPointers<FitnessAssignedScores<double, T>>(subComplex);
				FitnessAssignedScores<double, T>::Sort(tmp);
				auto worst = tmp[tmp.size() - 1];
				pointRemoved.clear();
				for (auto& point : subComplex)
				{
					if (&point != worst)
						pointRemoved.push_back(point);
				}
				return *worst;
			}

		private:
			std::vector<IObjectiveScores<T>> leftOutFromSubcomplex;
			std::vector<FitnessAssignedScores<double, T>> evolved;
			SceOptions options;
			IObjectiveEvaluator<T>* evaluator;
			IFitnessAssignment<double, T> fitnessAssignment;
			double ContractionRatio;
			double ReflectionRatio;
			Complex<T>* complex = nullptr;
			int alpha, q;
			IRandomNumberGeneratorFactory rng;
			ICandidateFactory<T> * cf = nullptr;
			RngInt * discreteRng = nullptr;

			void Init(const std::vector<IObjectiveScores<T>>& complexPopulation, IObjectiveEvaluator<T>* evaluator, int q, int alpha,
				IRandomNumberGeneratorFactory rng, ICandidateFactory<T> * candidateFactory,
				IFitnessAssignment<double, T> fitnessAssignment, SceOptions options, 
				double contractionRatio, double reflectionRatio, RngInt& discreteRng)
			{
				this->options = options;
				this->evaluator = evaluator;
				this->fitnessAssignment = fitnessAssignment;
				this->ContractionRatio = contractionRatio;
				this->ReflectionRatio = reflectionRatio;
				this->alpha = alpha;
				this->q = q;
				this->rng = rng;
				this->cf = candidateFactory;
				if(cf==nullptr)
					cf = new UniformRandomSamplingFactory<T>(rng, complexPopulation[0].SystemConfiguration());
				this->discreteRng = &discreteRng;
#ifdef _DEBUG
				//CheckParameterFeasible(complexPopulation);
#endif
				evolved = getSubComplex(complexPopulation, leftOutFromSubcomplex);
#ifdef _DEBUG
				//CheckParameterFeasible(leftOutFromSubcomplex);
#endif

			}

			void clear(std::vector<FitnessAssignedScores<double, T>>& vec)
			{
				//for (auto ptr : vec)
				//	delete ptr;
				vec.clear();
			}

			void replaceEvolved(const std::vector<FitnessAssignedScores<double, T>>& candidateSubcomplex)
			{
				clear(evolved);
				evolved = candidateSubcomplex;
			}

			std::vector<FitnessAssignedScores<double, T>> replaceWithRandom(const std::vector<FitnessAssignedScores<double, T>>& withoutWorstPoint, const FitnessAssignedScores<double, T>& worstPoint)
			{
				std::vector<FitnessAssignedScores<double, T>> result;
				if ((options & SceOptions::RndInSubComplex) == SceOptions::RndInSubComplex)
				{
					// TODO: what was SceOptions::ReflectionRandomization about? Needs re-clarification to assess whether worth porting.
					//						if ((options & SceOptions::ReflectionRandomization) == SceOptions::ReflectionRandomization)
					result = generateRandomWithinSubcomplex(withoutWorstPoint, worstPoint);
					//						else
					//							result = generateRandomWithinShuffleBounds(worstPoint, withoutWorstPoint);
				}
				else
				{
					result = addRandomInHypercube(withoutWorstPoint, this->evolved);
				}
				return result;
			}

			T getCentroid(const std::vector<FitnessAssignedScores<double, T>>& population)
			{
				auto tmp = convertAllToHyperCube(population);
				return T::GetCentroid(tmp);
			}

			void loggerWrite(string infoMsg, std::map<string, string> tags)
			{
				//LoggerMhHelper.Write(infoMsg, tags, logger);
			}

			void loggerWrite(std::vector<IObjectiveScores<T>> scores, std::map<string, string> tags)
			{
				//tags = LoggerMhHelper.MergeDictionaries(logTags, tags);
				//LoggerMhHelper.Write(scores, tags, logger);
			}

			void loggerWrite(FitnessAssignedScores<double, T> scores, std::map<string, string> tags)
			{
				//tags = LoggerMhHelper.MergeDictionaries(logTags, tags);
				//LoggerMhHelper.Write(scores, tags, logger);
			}

			std::tuple<string, string> createTagCatComplexNo()
			{
				//return LoggerMhHelper.MkTuple("Category", "Complex No " + complexId);
				return std::tuple<string, string>();
			}

			//void loggerWrite(T point, std::map<string, string> tags)
			//{
			//    if (logger != nullptr)
			//        logger.Write(point, tags);
			//}

			void loggerWrite(IObjectiveScores<T> point, std::map<string, string> tags)
			{
				this->loggerWrite(new std::vector < IObjectiveScores<T> >{ point }, tags);
			}

			std::vector<IObjectiveScores<T>> aggregatePoints(const std::vector<FitnessAssignedScores<double, T>>& subComplex, const std::vector<IObjectiveScores<T>>& leftOutFromSubcomplex)
			{
				std::vector<IObjectiveScores<T>> result;
				for (size_t i = 0; i < subComplex.size(); i++)
				{
					result.push_back(subComplex.at(i).Scores());
				}
				for (size_t i = 0; i < leftOutFromSubcomplex.size(); i++)
				{
					result.push_back(leftOutFromSubcomplex[i]);
				}
				return result;
			}

			FitnessAssignedScores<double, T> evaluateNewSet(T newPoint, const std::vector<FitnessAssignedScores<double, T>>& withoutWorstPoint, std::vector<FitnessAssignedScores<double, T>>& candidateSubcomplex)
			{
				IObjectiveScores<T> scoreNewPoint = evaluator->EvaluateScore(newPoint);

				std::vector<IObjectiveScores<T>> scores;
				for (auto& s : withoutWorstPoint)
					scores.push_back(s.Scores());
				scores.push_back(scoreNewPoint);
				auto fitness = fitnessAssignment.AssignFitness(scores);
				candidateSubcomplex.clear();
				for (size_t i = 0; i < fitness.size(); i++)
				{
					candidateSubcomplex.push_back(FitnessAssignedScores<double, T>(fitness[i]));
				}
				//return Array.Find<FitnessAssignedScores<double, T>>(candidateSubcomplex, (x = > (x.Scores() == scoreNewPoint)));
				return (candidateSubcomplex[candidateSubcomplex.size() - 1]);
			}

			T reflect(const FitnessAssignedScores<double, T>& worstPoint, T centroid, bool& success)
			{
				//double ratio = -1.0;
				double ratio = this->ReflectionRatio;
				return performHomothecy(worstPoint, centroid, ratio, success);
			}

			T contract(FitnessAssignedScores<double, T> worstPoint, T centroid, bool& success)
			{
				//double ratio = 0.5;
				double ratio = this->ContractionRatio;
				return performHomothecy(worstPoint, centroid, ratio, success);
			}

			static T performHomothecy(const FitnessAssignedScores<double, T>& worstPoint, T centroid, double ratio, bool& success)
			{
				T candidate = centroid.HomotheticTransform(worstPoint.Scores().SystemConfiguration(), ratio);
				success = candidate.IsFeasible();
				return candidate;
			}

			static std::vector<T> convertAllToHyperCube(const std::vector<FitnessAssignedScores<double, T>>& points)
			{
				std::vector<T> result;
				for (size_t i = 0; i < points.size(); i++)
				{
					result.push_back(points[i].Scores().SystemConfiguration());
				}
				return result;
			}

			static std::vector<T> convertAllToHyperCube(const std::vector<IObjectiveScores<T>>& points)
			{
				std::vector<T> result;
				for (size_t i = 0; i < points.size(); i++)
				{
					result.push_back(points[i].SystemConfiguration());
				}
				return result;
			}

			static std::vector<IObjectiveScores<T>> convertToScores(const std::vector<FitnessAssignedScores<double, T>>& points)
			{
				std::vector<IObjectiveScores<T>> result;
				for (size_t i = 0; i < points.size(); i++)
				{
					result.push_back(points[i].Scores());
				}
				return result;
			}

			std::vector<FitnessAssignedScores<double, T>> removePoint(const std::vector<FitnessAssignedScores<double, T>>& subComplex, FitnessAssignedScores<double, T> worstPoint)
			{
				std::vector<FitnessAssignedScores<double, T>> result;
				for (size_t i = 0; i < subComplex.size(); i++)
				{
					auto p = subComplex.at(i);
					if (p != worstPoint)
						result.push_back(p);
				}
				return result;
			}

			std::vector<FitnessAssignedScores<double, T>> contractionOrRandom(const std::vector<FitnessAssignedScores<double, T>>& withoutWorstPoint,
				const FitnessAssignedScores<double, T>& worstPoint, const T& centroid)
			{
				std::vector<FitnessAssignedScores<double, T>> result;
				std::vector<FitnessAssignedScores<double, T>> candidateSubcomplex;
				bool success;
				T contractionPoint = contract(worstPoint, centroid, success);

				if (success)
				{
					FitnessAssignedScores<double, T> trialPoint = evaluateNewSet(contractionPoint, withoutWorstPoint, candidateSubcomplex);
					if (trialPoint.CompareTo(worstPoint) <= 0)
					{
						result = candidateSubcomplex;
						//loggerWrite(trialPoint, createTagConcat(
						//	LoggerMhHelper.MkTuple("Message", "Contracted point in subcomplex"),
						//	createTagCatComplexNo()));
						return result;
					}
					else
					{
						clear(candidateSubcomplex);
						//loggerWrite(trialPoint, createTagConcat(
						//	LoggerMhHelper.MkTuple("Message", "Contracted point in subcomplex-Failed"),
						//	createTagCatComplexNo()));
					}
				}
				else
				{
					auto msg = "Contracted point unfeasible";
					//loggerWrite(msg, createTagConcat(LoggerMhHelper.MkTuple("Message", msg), createTagCatComplexNo()));
				}
				// 2012-02-14: The Duan et al 1993 paper specifies to use the complex to generate random points. However, comparison to a Matlab
				// implementation showed a slower rate of convergence. 
				// result = addRandomInHypercube(withoutWorstPoint, bufferComplex);
				result = replaceWithRandom(withoutWorstPoint, worstPoint);
				return result;
			}

/*
			std::vector<FitnessAssignedScores<double, T>> generateRandomWithinShuffleBounds(const FitnessAssignedScores<double, T>& worstPoint, const std::vector<FitnessAssignedScores<double, T>>  withoutWorstPoint)
			{
				auto sbcplx = convertAllToHyperCube(merge(withoutWorstPoint, worstPoint));
				auto wp = worstPoint.Scores().GetSystemConfiguration() as IHyperCube < double >;
				auto newPoint = wp.Clone() as IHyperCube < double >;
				auto varnames = newPoint.GetVariableNames();
				auto rand = hyperCubeOps.GenerateRandomWithinHypercube(sbcplx);
				for (int i = 0; i < varnames.Length; i++)
				{
					auto v = varnames[i];
					auto value = 2 * centroid.GetValue(v) - wp.GetValue(v);
					if (value < wp.GetMinValue(v) || value > wp.GetMaxValue(v))
						newPoint.SetValue(v, rand.GetValue(v));
					else
						newPoint.SetValue(v, value);
				}
				auto newScore = evaluator->EvaluateScore(newPoint);
				loggerWrite(newScore, createTagConcat(
					LoggerMhHelper.MkTuple("Message", "Adding a partially random point"),
					LoggerMhHelper.MkTuple("Category", "Complex No " + complexId)
					));
				return fitnessAssignment.AssignFitness(aggregate(newScore, withoutWorstPoint));
			}
*/
			std::vector<FitnessAssignedScores<double, T>> generateRandomWithinSubcomplex(const std::vector<FitnessAssignedScores<double, T>>& withoutWorstPoint, const FitnessAssignedScores<double, T>& worstPoint)
			{
				// 2012-02-14: The Duan et al 1993 paper specifies to use the complex to generate random points. However, comparison to a Matlab
				// implementation showed a slower rate of convergence. 
				std::vector<FitnessAssignedScores<double, T>> result;
				auto subCplx = merge(withoutWorstPoint, worstPoint);
				result = addRandomInHypercube(withoutWorstPoint, subCplx);
#ifdef _DEBUG
				//CheckParameterFeasible(result);
#endif
				return result;
			}

			std::vector<FitnessAssignedScores<double, T>> addRandomInHypercube(const std::vector<FitnessAssignedScores<double, T>>& withoutWorstPoint, const std::vector<FitnessAssignedScores<double, T>>& popForHypercubeDefn)
			{
				return 	addRandomInHypercube(withoutWorstPoint, convertToScores(popForHypercubeDefn));
			}

			std::vector<FitnessAssignedScores<double, T>> addRandomInHypercube(const std::vector<FitnessAssignedScores<double, T>>& withoutWorstPoint, const std::vector<IObjectiveScores<T>>& popForHypercubeDefn)
			{
				auto tmp = convertAllToHyperCube(popForHypercubeDefn);
				T newPoint = this->cf->CreateRandomCandidate(tmp);
							//if (newPoint == nullptr)
							//{
							//	auto msg = "Random point within hypercube bounds is unfeasible";
							//	//loggerWrite(msg, createTagConcat(LoggerMhHelper.MkTuple("Message", msg), createTagCatComplexNo()));
							//	return null;
							//}
				auto newScore = evaluator->EvaluateScore(newPoint);
				//loggerWrite(newScore, createTagConcat(
				//	LoggerMhHelper.MkTuple("Message", "Adding a random point in hypercube"),
				//	createTagCatComplexNo()
				//	));
				std::vector<IObjectiveScores<T>> newSubComplex = aggregate(newScore, withoutWorstPoint);
#ifdef _DEBUG
				//CheckParameterFeasible(newSubComplex);
#endif
				return fitnessAssignment.AssignFitness(newSubComplex);
			}

			std::vector<FitnessAssignedScores<double, T>> getSubComplex(const std::vector<IObjectiveScores<T>>& bufferComplex, std::vector<IObjectiveScores<T>>& leftOutFromSubcomplex)
			{
				auto fitnessPoints = fitnessAssignment.AssignFitness(bufferComplex);
#ifdef _DEBUG
				//CheckParameterFeasible(fitnessPoints);
#endif
				FitnessAssignedScores<double, T>::Sort(fitnessPoints);
#ifdef _DEBUG
				//CheckParameterFeasible(fitnessPoints);
#endif
				std::vector<FitnessAssignedScores<double, T>> leftOut;
				std::vector<FitnessAssignedScores<double, T>> subset = SampleFrom(*discreteRng, fitnessPoints, q, leftOut, false);
#ifdef _DEBUG
				//CheckParameterFeasible(leftOut);
#endif
				leftOutFromSubcomplex.clear();
				for (size_t i = 0; i < leftOut.size(); i++)
				{
					leftOutFromSubcomplex.push_back(leftOut[i].Scores());
				}
#ifdef _DEBUG
				//CheckParameterFeasible(leftOutFromSubcomplex);
#endif
				return subset;
			}

			static std::vector<IObjectiveScores<T>> merge(const std::vector<FitnessAssignedScores<double, T>>& withoutWorstPoint, const FitnessAssignedScores<double, T>& worstPoint)
			{
				std::vector<IObjectiveScores<T>> tmp = convertToScores(withoutWorstPoint);
				tmp.push_back(worstPoint.Scores());
				return tmp;
			}

			std::vector<IObjectiveScores<T>> aggregatePoints(T newPoint, std::vector<IObjectiveScores<T>> withoutWorstPoint)
			{
				return aggregate(evaluator->EvaluateScore(newPoint), withoutWorstPoint);
			}

			std::vector<IObjectiveScores<T>> aggregate(const IObjectiveScores<T>& newPoint, std::vector<FitnessAssignedScores<double, T>> withoutWorstPoint)
			{
				std::vector<IObjectiveScores<T>> result = convertToScores(withoutWorstPoint);
				result.push_back(newPoint);
				return result;
			}
		};

		template<typename T>
		class Complex // : IComplex
		{
			std::map<string, string> createTagConcat(std::initializer_list<std::tuple<string, string>> tuples)
			{
				//return LoggerMhHelper.MergeDictionaries(LoggerMhHelper.CreateTag(tuples), this->tags);
			}

			friend SubComplex<T>::SubComplex(Complex&);

			std::vector<IObjectiveScores<T>> scores;
			int q = 10;
			int alpha = 2;
			int beta = 3;
			RngInt discreteGenerator;
			IRandomNumberGeneratorFactory rng;
			ICandidateFactory<T> * candidateFactory = nullptr;
			IFitnessAssignment<double, T> fitnessAssignment;
			// IHyperCubeOperations* hyperCubeOps;
			ILoggerMh* logger = nullptr; // new Log4netAdapter();
			double ContractionRatio = 0.5;
			double ReflectionRatio = -1;

			std::map<string, string> tags;
			double factorTrapezoidalPDF = -1;
			SceOptions options = SceOptions::None;

			bool IsCancelled = false;

			ITerminationCondition<T> * TerminationCondition = nullptr;

		protected:

			IObjectiveEvaluator<T>* evaluator = nullptr;

			void Init(const std::vector<IObjectiveScores<T>>& scores, IObjectiveEvaluator<T>* evaluator, int q, int alpha=2, int beta=3, double factorTrapezoidalPDF = -1,
				SceOptions options = SceOptions::None, double reflectionRatio = -1.0, double contractionRatio = 0.5)
			{
				// TODO checks on consistencies.
				this->scores = scores;
				this->q = q;
				this->alpha = alpha;
				this->beta = beta;
				this->factorTrapezoidalPDF = factorTrapezoidalPDF;
				this->options = options;
				this->ReflectionRatio = reflectionRatio;
				this->ContractionRatio = contractionRatio;
				this->evaluator = evaluator;
			}

			Complex(const std::vector<IObjectiveScores<T>>& scores, IObjectiveEvaluator<T>* evaluator, int q, int seed = 0, int alpha=2, int beta=3, double factorTrapezoidalPDF = -1,
				SceOptions options = SceOptions::None, double reflectionRatio = -1.0, double contractionRatio = 0.5) :
				rng(seed),
				discreteGenerator(CreateTrapezoidalRng(scores.size(), rng.CreateNewStd(), factorTrapezoidalPDF))
			{
				Init(scores, evaluator, q, alpha, beta, factorTrapezoidalPDF, options, reflectionRatio, contractionRatio);
			}

		public:
			string ComplexId;

			Complex(const std::vector<IObjectiveScores<T>>& scores, 
				IObjectiveEvaluator<T>* evaluator, IRandomNumberGeneratorFactory rng, ICandidateFactory<T>* candidateFactory,
				IFitnessAssignment<double, T> fitnessAssignment, ITerminationCondition<T> * terminationCondition,
				ILoggerMh* logger = nullptr, std::map<string, string> tags = std::map<string, string>(), int q=10, int alpha=2, int beta=3, double factorTrapezoidalPDF = -1,
				SceOptions options = SceOptions::None, double reflectionRatio = -1.0, double contractionRatio = 0.5) 
				:
				discreteGenerator(CreateTrapezoidalRng(scores.size(), rng.CreateNewStd(), factorTrapezoidalPDF))
			{
				Init(scores, evaluator, q, alpha, beta, factorTrapezoidalPDF, options, reflectionRatio, contractionRatio);
				this->fitnessAssignment = fitnessAssignment;
				this->candidateFactory = candidateFactory;
				this->logger = logger;
				this->tags = tags;
				this->rng = rng;
				this->TerminationCondition = terminationCondition;
			}

			virtual ~Complex()
			{
			}

			bool IsFinished()
			{
				if(TerminationCondition == nullptr) return false;
				return TerminationCondition->IsFinished();
			}

			bool IsCancelledOrFinished()
			{
				return (IsCancelled || IsFinished());
			}

			void Evolve()
			{
				//if (Thread.CurrentThread.Name == nullptr)
				//{
				//	Thread.CurrentThread.Name = ComplexId;
				//}
				int a, b; // counters for alpha and beta parameters
				b = 0;
				while (b < beta && !IsCancelledOrFinished())
				{
					SubComplex<T> subComplex(*this);
					subComplex.Evolve();
					this->scores = subComplex.WholePopulation();
#ifdef _DEBUG
					//CheckParameterFeasible(scores);
#endif
					b++;
				}

			}

			const std::vector<IObjectiveScores<T>> GetObjectiveScores()
			{
				return this->scores;
			}

		};


		template<typename T>
		class ShuffledComplexEvolution
			: public IEvolutionEngine<T>
			// , IPopulation<double>
			//where T : ICloneableSystemConfiguration
		{
		public:
			ShuffledComplexEvolution(IObjectiveEvaluator<T>* evaluator,
				ICandidateFactory<T>* populationInitializer,
				ITerminationCondition<T>* terminationCondition,
				const SceParameters& sceParameters,
				IRandomNumberGeneratorFactory rng = IRandomNumberGeneratorFactory(),
				IFitnessAssignment<double, T> fitnessAssignment = IFitnessAssignment<double, T>(),
				const std::map<string, string>& logTags = std::map<string, string>())
			{
				Init(evaluator, populationInitializer, terminationCondition,
					rng,
					fitnessAssignment,
					sceParameters.P,
					sceParameters.Pmin,
					sceParameters.M,
					sceParameters.Q,
					sceParameters.Alpha,
					sceParameters.Beta,
					sceParameters.NumShuffle,
					sceParameters.TrapezoidalDensityParameter,
					logTags,
					SceOptions::None,
					sceParameters.ReflectionRatio,
					sceParameters.ContractionRatio);
			}

			void Init(IObjectiveEvaluator<T>* evaluator,
				ICandidateFactory<T>* populationInitializer,
				ITerminationCondition<T>* terminationCondition,
				IRandomNumberGeneratorFactory rng,
				IFitnessAssignment<double, T> fitnessAssignment,
				int p = 5,
				int pmin = 5,
				int m = 13,
				int q = 7,
				int alpha = 3,
				int beta = 13,
				int numShuffle = 15,
				double trapezoidalPdfParam = 1.8,
				const std::map<string, string>& logTags = std::map<string, string>(),
				SceOptions options = SceOptions::None, double reflectionRatio = -1.0, double contractionRatio = 0.5)
			{
				if (m < 2)
					throw std::logic_error("M is too small");

				if (q > m)
					throw std::logic_error("Q must be less than or equal to M");

				this->evaluator = evaluator;
				this->populationInitializer = populationInitializer;
				this->terminationCondition = terminationCondition;
				//if (this->terminationCondition == nullptr)
				//	this->terminationCondition = new MaxShuffleTerminationCondition();
				this->terminationCondition->SetEvolutionEngine(this);
				this->p = p;
				this->pmin = pmin;
				this->m = m;
				this->q = q;
				this->alpha = alpha;
				this->beta = beta;
				this->numShuffle = numShuffle;
				this->rng = rng;
				//if (this->rng == nullptr)
				//	this->rng = new BasicRngFactory(0);
				this->fitnessAssignment = fitnessAssignment;
				//if (this->fitnessAssignment == nullptr)
				//	this->fitnessAssignment = new DefaultFitnessAssignment();
				this->logTags = logTags;
				this->trapezoidalPdfParam = trapezoidalPdfParam;
				this->options = options;
				this->ReflectionRatio = reflectionRatio;
				this->ContractionRatio = contractionRatio;
			}

			IOptimizationResults<T> Evolve()
			{
				isCancelled = false;
				std::vector<IObjectiveScores<T>> scores = evaluateScores(evaluator, initialisePopulation());
				//loggerWrite(scores, createSimpleMsg("Initial Population", "Initial Population"));
				auto isFinished = terminationCondition->IsFinished();
				if (isFinished)
				{
					logTerminationConditionMet();
					return packageResults(scores);
				}
				this->complexes = partition(scores);

				//OnAdvanced( new ComplexEvolutionEvent( complexes ) );

				CurrentShuffle = 1;
				isFinished = terminationCondition->IsFinished();
				if (isFinished) logTerminationConditionMet();
				while (!isFinished && !isCancelled)
				{
					if (evaluator->IsCloneable())
						execParallel(complexes);
					else
					{
						for (int i = 0; i < complexes.size(); i++)
						{
							auto currentComplex = complexes.at(i);
							//currentComplex.IsCancelled = isCancelled;

							currentComplex->Evolve();
							//auto complexPoints = currentComplex.GetObjectiveScores().ToArray();
							//loggerWrite(sortByFitness(complexPoints).First(), createSimpleMsg("Best point in complex", "Complex No " + currentComplex.ComplexId));
						}
					}
					//OnAdvanced( new ComplexEvolutionEvent( complexes ) );
					string shuffleMsg = "Shuffling No " + std::to_string(CurrentShuffle);
					auto shufflePoints = aggregate(complexes);
					//loggerWrite(shufflePoints, createSimpleMsg(shuffleMsg, shuffleMsg));
					this->PopulationAtShuffling = sortByFitness(shufflePoints);
					//loggerWrite(PopulationAtShuffling[0], createSimpleMsg("Best point in shuffle", shuffleMsg));
					auto bufferCplx = complexes;
					complexes = shuffle(complexes);
					for (auto c : bufferCplx) delete c;

					CurrentShuffle++;
					isFinished = terminationCondition->IsFinished();
					if (isFinished) logTerminationConditionMet();
				}
				return packageResults(complexes);
			}


		private:
			std::map<string, string> logTags;
			IObjectiveEvaluator<T>* evaluator;
			ICandidateFactory<T>* populationInitializer;
			ITerminationCondition<T>* terminationCondition;
			IRandomNumberGeneratorFactory rng;
			IFitnessAssignment<double, T> fitnessAssignment;
			double trapezoidalPdfParam;

			ILoggerMh* Logger;

			int pmin = 5;
			int p = 5, m = 27, q = 14, alpha = 3, beta = 27;
			int numShuffle = -1;

			int seed = 0;

			//class IComplex
			//{
			//public:
			//	//virtual std::vector<IObjectiveScores<T>> GetObjectiveScores() = 0;
			//	//virtual void Evolve() = 0;
			//	virtual std::vector<IObjectiveScores<T>> GetObjectiveScores() const { return std::vector<IObjectiveScores<T>>(); }
			//	virtual void Evolve() { ; }
			//	string ComplexId;
			//	bool IsCancelled;
			//};

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

			IPopulation<double> algorithm;
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
			std::vector<FitnessAssignedScores<double>> currentPopulation = algorithm.Population;
			if (currentPopulation == nullptr)
			return false;
			auto currentBest = currentPopulation.First().FitnessValue;
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
			ShuffledComplexEvolution<T> algorithm;
			double threshold;
			double maxHours;
			Stopwatch stopWatch;
			// FIXME: consider something where the termination criteria is customizable to an extent.
			// Func<double[], double> statistic;

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
			if (algorithm.PopulationAtShuffling == nullptr)
			return false; // start of the algorithm.
			int n = (int)Math.Ceiling(algorithm.PopulationAtShuffling.Length / 2.0);
			auto tmp = algorithm.PopulationAtShuffling.Where((x, i) = > i < n).ToArray();
			auto popToTest = Array.ConvertAll<FitnessAssignedScores<double>, IObjectiveScores<T>>(tmp, (x = > x.Scores()));
			return IsBelowCvThreshold(popToTest);
			}

			double GetMaxParameterCoeffVar(std::vector<IObjectiveScores<T>> population)
			{
			auto pSets = ConvertAllToHyperCube(population);
			auto varNames = pSets[0].GetVariableNames();
			double[] coeffVar = new double[varNames.Length];
			for (int i = 0; i < varNames.Length; i++)
			{
			coeffVar[i] = calcCoeffVar(MetaheuristicsHelper.GetValues(pSets, varNames[i]));
			}
			return MetaheuristicsHelper.GetMaximum(coeffVar);
			}

			double calcCoeffVar(double[] p)
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

			bool IsBelowCvThreshold(std::vector<IObjectiveScores<T>> population)
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
			double maxHours;
			Stopwatch stopWatch;

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


			static ITerminationCondition<T> CreateMaxShuffleTerminationCondition()
			{
			return new MaxShuffleTerminationCondition();
			}


			static T[] ConvertAllToHyperCube(std::vector<IObjectiveScores<T>> population)
			{
			auto tmp = Array.ConvertAll<IObjectiveScores<T>, T>(population, (x = > x.GetSystemConfiguration()));
			return tmp;
			}

			*/

			int CurrentShuffle;

			//	int MaxDegreeOfParallelism
			//{
			//	get{ return parallelOptions.MaxDegreeOfParallelism; }
			//	set{ parallelOptions.MaxDegreeOfParallelism = value; }
			//}
			//ParallelOptions parallelOptions = new ParallelOptions();

			bool isCancelled = false;
			//IComplex currentComplex;

			//CancellationTokenSource tokenSource = new CancellationTokenSource();
			SceOptions options = SceOptions::None;
			std::vector<Complex<T>*> complexes;

			double ContractionRatio;
			double ReflectionRatio;

			void Cancel()
			{
				isCancelled = true;
				//if (currentComplex != nullptr)
				//	currentComplex.IsCancelled = isCancelled;
				//tokenSource.Cancel();
			}

			static IOptimizationResults<T> packageResults(const std::vector<Complex<T>*>& complexes)
			{
				//saveLog( logPopulation, fullLogFileName );
				std::vector<IObjectiveScores<T>> population = aggregate(complexes);
				//saveLogParetoFront( population );
				return packageResults(population);
			}

			static IOptimizationResults<T> packageResults(const std::vector<IObjectiveScores<T>>& population)
			{
				// cater for cases where we have null references (e.g. if the termination condition was in the middle of the population creation)
				// return new BasicOptimizationResults<T>(population.Where(p = > (p != nullptr)).ToArray());
				return IOptimizationResults<T>(population);
			}

			void logTerminationConditionMet()
			{
				//auto tags = createSimpleMsg("Termination condition", "Termination condition");
				//loggerWrite(string.Format("Termination condition using {0} is met", terminationCondition->GetType().Name), tags);
			}

			std::map<string, string> createSimpleMsg(string message, string category)
			{
				//return LoggerMhHelper.CreateTag(LoggerMhHelper.MkTuple("Message", message), LoggerMhHelper.MkTuple("Category", category));
				return std::map<string, string>();
			}

			void execParallel(std::vector<Complex<T>*>& complexes)
			{
				for (int i = 0; i < complexes.size(); i++)
					complexes.at(i)->ComplexId = std::to_string(i);
				//Parallel.ForEach(complexes, parallelOptions, c = > c.Evolve());
			}

			string GetDescription()
			{
				throw std::logic_error("Not implemented");
			}


			std::vector<Complex<T>*> shuffle(const std::vector<Complex<T>*>& complexes)
			{
				std::vector<IObjectiveScores<T>> population = aggregate(complexes);
				auto newComplexes = partition(population);
				return newComplexes;
			}

			static std::vector<IObjectiveScores<T>> aggregate(const std::vector<Complex<T>*>& complexes)
			{
				std::vector<IObjectiveScores<T>> result;
				for (auto c : complexes)
				{
					auto scores = c->GetObjectiveScores();
#ifdef _DEBUG
					//CheckParameterFeasible(scores);
#endif
					for (auto& s : scores)
					{
						result.push_back(s);
					}
				}
				return result;
			}

			std::vector<IObjectiveScores<T>> evaluateScores(IObjectiveEvaluator<T>* evaluator, const std::vector<T>& population)
			{
				//return Evaluations::EvaluateScores(evaluator, population, () = > (this->isCancelled || terminationCondition->IsFinished()), parallelOptions);
				return Evaluations::EvaluateScores(evaluator, population);
			}

			std::vector<T> initialisePopulation()
			{
				std::vector<T> result(p * m);
				for (int i = 0; i < result.size(); i++)
					result[i] = populationInitializer->CreateRandomCandidate();
				return result;
			}

			std::vector<Complex<T>*> partition(const std::vector<FitnessAssignedScores<double, T>>& sortedScores)
			{
				std::vector<Complex<T>*> result;
				if (CurrentShuffle > 0)
					if (this->pmin < this->p)
						this->p = this->p - 1;
				for (int a = 0; a < p; a++)
				{
					std::vector<FitnessAssignedScores<double, T>> sample;
					for (int k = 1; k <= m; k++)
						sample.push_back(sortedScores[a + p * (k - 1)]);
					std::vector<IObjectiveScores<T>> scores = getScores(sample);
					seed++; // TODO: check why this was done.
					Complex<T>* complex = createComplex(scores);
					complex->ComplexId = std::to_string(CurrentShuffle) + "_" + std::to_string(a + 1);
					result.push_back(complex);
				}
				return result;
			}

			Complex<T>* createComplex(std::vector<IObjectiveScores<T>> scores)
			{
				//IHyperCubeOperationsFactory* hyperCubeOperationsFactory = dynamic_cast<IHyperCubeOperationsFactory*>(populationInitializer);
				//if (hyperCubeOperationsFactory == nullptr)
				//	throw std::logic_error("Currently SCE uses an implementation of a 'complex' that needs a population initializer that implements IHyperCubeOperationsFactory");

				//auto loggerTags = LoggerMhHelper.MergeDictionaries(logTags, LoggerMhHelper.CreateTag(LoggerMhHelper.MkTuple("CurrentShuffle", std::to_string(this->CurrentShuffle))));

				return new Complex<T>(scores, evaluator, rng, populationInitializer,
					fitnessAssignment, this->terminationCondition,
					nullptr, std::map<string, string>(), q, alpha, beta, trapezoidalPdfParam,
					options, this->ReflectionRatio, this->ContractionRatio);

				//auto complex = new Complex<T>(scores, m, q, alpha, beta,
				//	(evaluator->SupportsThreadSafeCloning ? evaluator->Clone() : evaluator),
				//	rng.CreateFactory(),
				//	getFitnessAssignment(), hyperCubeOperationsFactory.CreateNew(this->rng), logger: this->logger,
				//	tags : loggerTags, factorTrapezoidalPDF : this->trapezoidalPdfParam,
				//   options : this->options, reflectionRatio : this->ReflectionRatio, contractionRatio : this->ContractionRatio);

				//complex.TerminationCondition = createMaxWalltimeCondition(this->terminationCondition);
			}

			// https://github.com/jmp75/metaheuristics/issues/3
			// ITerminationCondition<T> createMaxWalltimeCondition(ITerminationCondition<T> terminationCondition)
			// {
			// 	auto t = terminationCondition as CoefficientOfVariationTerminationCondition;
			// 	if (t == nullptr)
			// 		return new FalseTerminationCondition();
			// 	else
			// 		return new MaxWalltimeTerminationCondition(t.RemainingHours);
			// }

			std::vector<Complex<T>*> partition(const std::vector<IObjectiveScores<T>>& scores)
			{
				auto sortedScores = sortByFitness(scores);
				//logPoints( CurrentShuffle, sortedScores );
				auto complexes = partition(sortedScores);
				return complexes;
			}

			std::vector<FitnessAssignedScores<double, T>> sortByFitness(const std::vector<IObjectiveScores<T>>& scores)
			{
				auto fittedScores = fitnessAssignment.AssignFitness(scores);
				FitnessAssignedScores<double, T>::Sort(fittedScores);
				//std::sort(fittedScores.begin(), fittedScores.end());
				return fittedScores;
			}

			static std::vector<IObjectiveScores<T>> getScores(const std::vector<FitnessAssignedScores<double, T>>& fitnessedScores)
			{
				std::vector<IObjectiveScores<T>> result;
				for (int i = 0; i < fitnessedScores.size(); i++)
					result.push_back(fitnessedScores[i].Scores());
				return result;
			}

			/*
			class ComplexEvolutionEvent : EventArgs, IMonitoringEvent
			{
			IObjectiveScores[] scoresSet;

			ComplexEvolutionEvent( std::vector<IComplex> complexes )
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

			std::vector<FitnessAssignedScores<double, T>> PopulationAtShuffling;

			std::vector<FitnessAssignedScores<double, T>> GetPopulation()
			{
				throw std::logic_error("Not implemented");
				// if (complexes == nullptr) return null;
				// return sortByFitness(aggregate(complexes));
			}

		};
	}
}
