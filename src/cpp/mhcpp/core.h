#pragma once

#include <string>
#include <vector>
#include <map>
#include <random>
#include <boost/random.hpp>
#include "utils.h"

using namespace std;

namespace mhcpp
{

	class IRandomNumberGeneratorFactory
	{

	private:
		/** \brief	A random number generator factory. */

		template<class RNG = std::mt19937>
		class RandomNumberGeneratorFactory
		{
		private:
			RNG seedEngine;
		public:
			typedef RNG engine_type; // No typename needed here. See http://stackoverflow.com/questions/6489351/nested-name-specifier

									 //	http://stackoverflow.com/questions/495021/why-can-templates-only-be-implemented-in-the-header-file

			RandomNumberGeneratorFactory(int seed) : seedEngine(seed)
			{
			}

			virtual ~RandomNumberGeneratorFactory()
			{
			}

			RNG * CreateNewEngine()
			{
				return new RNG(seedEngine());
			}

			RandomNumberGeneratorFactory<RNG> * CreateNewFactory()
			{
				return new RandomNumberGeneratorFactory<RNG>(seedEngine());
			}

			unsigned int operator()() {
				return seedEngine();
			}

			template<class DistributionType = std::uniform_real_distribution<double>>
			static boost::variate_generator<RNG*, DistributionType> * CreateVariateGenerator(RandomNumberGeneratorFactory<RNG>& rngf, DistributionType& dist)
			{
				return new boost::variate_generator<RNG*, DistributionType>(rngf.CreateNewEngine(), dist);
			}

		};

		RandomNumberGeneratorFactory<> rng;

	public:
		IRandomNumberGeneratorFactory() : rng(0)
		{
		}
		IRandomNumberGeneratorFactory(unsigned int seed) : rng(seed)
		{
		}
		unsigned int Next()
		{ 
			return rng(); 
		}

		IRandomNumberGeneratorFactory CreateNew()
		{
			return IRandomNumberGeneratorFactory(Next());
		}

		std::mt19937 CreateNewStd()
		{
			return std::mt19937(Next());
		}

		// http://stackoverflow.com/questions/7166799/specialize-template-function-for-template-class
		template<class DistributionType = std::uniform_real_distribution<double>>
		boost::variate_generator<std::mt19937, DistributionType> * CreateVariateGenerator(DistributionType& dist)
		{
			return new boost::variate_generator<std::mt19937, DistributionType>(CreateNewStd(), dist);
		}
	};

	class ISystemConfiguration
	{
	public:
		virtual ~ISystemConfiguration() {}
		/// <summary>
		/// Gets an alphanumeric description for this system configuration
		/// </summary>
		virtual string GetConfigurationDescription() = 0;

		/// <summary>
		/// Apply this system configuration to a compatible system, usually a 'model' in the broad sense of the term.
		/// </summary>
		/// <param name="system">A compatible system, usually a 'model' in the broad sense of the term</param>
		/// <exception cref="ArgumentException">thrown if this system configuration cannot be meaningfully applied to the specified system</exception>
		virtual void ApplyConfiguration(void* system) = 0;
	};

	/// <summary>
	/// Interface for system configurations that support cloning. Helps to support parallelism in solvers.
	/// </summary>
	class ICloneableSystemConfiguration : public ISystemConfiguration //, ICloningSupport<ICloneableSystemConfiguration>
	{
	public:
		virtual ~ICloneableSystemConfiguration() {}
	};

	/// <summary>
	/// Interface for system configurations that are a set of numeric parameters, each with min and max feasible values.
	/// </summary>
	/// <typeparam name="T">A comparable type; typically a float or double, but possibly integer or more esoteric type</typeparam>
	template<typename T>
	class IHyperCube : public ICloneableSystemConfiguration //where T : IComparable
	{
	public:
		virtual ~IHyperCube() {}
		/// <summary>
		/// Gets the names of the variables defined for this hypercube.
		/// </summary>
		/// <returns></returns>
		virtual vector<string> GetVariableNames() = 0;

		/// <summary>
		/// Gets the number of dimensions in this hypercube
		/// </summary>
		virtual size_t Dimensions() = 0;

		/// <summary>
		/// Gets the value for a variable
		/// </summary>
		/// <param name="variableName"></param>
		/// <returns></returns>
		virtual T GetValue(string variableName) = 0;

		/// <summary>
		/// Gets the maximum feasible value for a variable
		/// </summary>
		/// <param name="variableName"></param>
		/// <returns></returns>
		virtual T GetMaxValue(string variableName) = 0;

		/// <summary>
		/// Gets the minimum feasible value for a variable
		/// </summary>
		/// <param name="variableName"></param>
		/// <returns></returns>
		virtual T GetMinValue(string variableName) = 0;

		/// <summary>
		/// Sets the value of one of the variables in the hypercube
		/// </summary>
		/// <param name="variableName"></param>
		/// <param name="value"></param>
		virtual void SetValue(string variableName, T value) = 0;

		/// <summary>
		/// Perform a homotetic transformation centered around this hypercube, of a second hypercube.
		/// </summary>
		/// <param name="point">The hypercube from which to derive. This object must not be modified by the method.</param>
		/// <param name="factor">The factor in the homotecie. a value 1 leaves the effectively point unchanged</param>
		/// <returns>A new instance of an hypercube, result of the transformation</returns>
		//virtual IHyperCube<T> HomotheticTransform(IHyperCube<T> point, double factor) = 0;
	};

	template<typename T>
	class IHyperCubeSetBounds : public IHyperCube < T > //where T : IComparable
	{
	public:
		virtual ~IHyperCubeSetBounds() {}
		virtual void SetMinValue(string variableName, T value) = 0;
		virtual void SetMaxValue(string variableName, T value) = 0;
		virtual void SetMinMaxValue(string variableName, T min, T max, T value) = 0;
	};

	//class IObjectiveScore
	//{
	//public:
	//	virtual ~IObjectiveScore() {}
	//	/// <summary>
	//	/// Gets whether this objective is a maximizable one (higher is better).
	//	/// </summary>
	//	virtual bool Maximise() = 0;

	//	/// <summary>
	//	/// Get a text represtattion of this score
	//	/// </summary>
	//	/// <returns></returns>
	//	virtual string GetText() = 0;

	//	/// <summary>
	//	/// Get name of the objective measure, typically a bivariate statistic.
	//	/// </summary>
	//	virtual string Name() = 0;

	//	// /// <summary>
	//	// /// Gets the value of the objective. Inheritors should return the real value, and not worry about negating or not. This is taken care elsewhere.
	//	// /// </summary>
	//	// T Value() = 0;
	//};

	//bool operator==(const IObjectiveScore &a, const double &b)
	//{
	//	return false; // a.GetObjective(0)->Value();
	//}

	//template<typename T = double>
	//class ObjectiveScore : public IObjectiveScore
	//{
	//public:
	//	ObjectiveScore(T value)
	//	{
	//		this->value = value;
	//	}
	//	bool Maximise() { return this->maximise; }
	//	string GetText() { return string(""); } // +value;	}
	//	string Name() { return string(""); }
	//	T Value() { return value; }
	//private:
	//	bool maximise;
	//	T value;
	//};


	/// <summary>
	/// An interface for one or more objective scores derived from the evaluation of a candidate system configuration.
	/// </summary>
	/// <remarks>This interface is defined without generics on purpose, to reduce complexity. Limits the unnecessary proliferation of generic classes</remarks>
	class IBaseObjectiveScores
	{
	public:
		virtual ~IBaseObjectiveScores() {}

		///// <summary>
		///// Gets the system configuration that led to these scores.
		///// </summary>
		///// <returns></returns>
		//virtual ISystemConfiguration * GetSystemConfiguration() = 0;
	};

	/// <summary>
	/// A generic interface for one or more objective scores derived from the evaluation of a candidate system configuration.
	/// </summary>
	/// <typeparam name="TSysConf">The type of the system configuration</typeparam>
	template<typename TSysConf>
	class IObjectiveScores //: public IBaseObjectiveScores //where TSysConf : ISystemConfiguration
	{
	public:
		IObjectiveScores(const TSysConf& sysConfig, const string& name, double value, bool maximizable = false)
		{
			this->sys = sysConfig;
			this->objectives.push_back(ObjectiveValue(name, value, maximizable));
		}

		IObjectiveScores(const IObjectiveScores<TSysConf>& src)
		{
			this->sys = src.sys;
			this->objectives = src.objectives;
		}

		IObjectiveScores<TSysConf>& operator=(const IObjectiveScores<TSysConf> &src)
		{
			if (&src == this){
				return *this;
			}
			this->sys = src.sys;
			this->objectives = src.objectives;
			return *this;
		}

		IObjectiveScores<TSysConf>& operator=(const IObjectiveScores<TSysConf>&& src)
		{
			if (&src == this){
				return *this;
			}
			std::swap(sys, src.sys);
			std::swap(objectives, src.objectives);
			return *this;
		}

		IObjectiveScores() {}

		virtual ~IObjectiveScores() {}

		/// <summary>
		/// Gets the system configuration that led to these scores.
		/// </summary>
		virtual TSysConf SystemConfiguration() const { return sys; }

		/// <summary>
		/// Gets the number of objective scores in this instance.
		/// </summary>
		virtual size_t ObjectiveCount() const { return this->objectives.size(); }
		//virtual size_t ObjectiveCount() = 0;

		// /// <summary>
		// /// Gets one of the objective 
		// /// </summary>
		// /// <param name="i">zero-based inex of the objective</param>
		// /// <returns></returns>
		// virtual IObjectiveScore * GetObjective(int i) = 0;

		virtual double Value(int i) const { return objectives[i].Value; } //= 0;

	private:
		class ObjectiveValue
		{
		public:
			ObjectiveValue(){}
			ObjectiveValue(string name, double value, bool maximizable) :
				Name(name), Value(value), Maximizable(maximizable)
			{
			}

			ObjectiveValue(const ObjectiveValue& src) :
				Name(src.Name), Value(src.Value), Maximizable(src.Maximizable)
			{
			}
			string Name;
			double Value;
			bool Maximizable;
		};

		TSysConf sys;
		std::vector<ObjectiveValue> objectives;

	};

	//template<typename TSysConf>
	//bool operator==(const IObjectiveScores<TSysConf> &a, const double &b) 
	//{ 
	//	return a.GetObjective(0)->Value(); 
	//}
	//template<typename TSysConf>
	//bool operator==(const double &a, const IObjectiveScores<TSysConf> &b) 
	//{ 
	//	return false;
	//}

	template<typename T>
	class ICandidateFactory
	{
	public:
		virtual ~ICandidateFactory() {}
		virtual T CreateRandomCandidate() = 0;
	};

	template<typename TSysConfig>
	class UniformRandomSamplingFactory : public ICandidateFactory<TSysConfig> //, IHyperCubeOperationsFactory
	{
	public:
		UniformRandomSamplingFactory(IRandomNumberGeneratorFactory rng, const TSysConfig& t)
		{
			this->rng = rng;
			std::srand(0);
			//if (!t.SupportsThreadSafloning)
			//	throw new ArgumentException("This URS factory requires cloneable and thread-safe system configurations");
			this->t = new TSysConfig(t);
			//this->hcOps = CreateIHyperCubeOperations();
			std::uniform_real_distribution<double> dist(0, 1);
			sampler = rng.CreateVariateGenerator<std::uniform_real_distribution<double>>(dist);
		}
		~UniformRandomSamplingFactory()
		{
			if (t != nullptr) {
				delete t; t = nullptr;
			}
			if (sampler != nullptr) {
				delete sampler; sampler = nullptr;
			}
		}

		//IHyperCubeOperations CreateNew(const IRandomNumberGeneratorFactory& rng)
		//{
		//	return new HyperCubeOperations(rng.CreateFactory());
		//}

		TSysConfig CreateRandomCandidate()
		{
			TSysConfig rt(*t);
			for (auto& vname : rt.GetVariableNames())
			{
				double min = rt.GetMinValue(vname);
				double max = rt.GetMaxValue(vname);
				rt.SetValue(vname, min + Urand() * (max - min));
			}
			return rt;
			//return (TSysConfig)hcOps.GenerateRandom(template);
		}
	private:
		boost::variate_generator<std::mt19937, std::uniform_real_distribution<double>> * sampler = nullptr;
		double Urand()
		{
			return (*sampler)();
		}
		//IHyperCubeOperations CreateIHyperCubeOperations()
		//{
		//	return new HyperCubeOperations(rng.CreateFactory());
		//}

		IRandomNumberGeneratorFactory rng;
		TSysConfig* t = nullptr;
	};

	template<typename T>
	class IEvolutionEngine
	{
	};

	template<typename T>
	class ITerminationCondition
	{
	public:
		void SetEvolutionEngine(IEvolutionEngine<T>* engine){};
		virtual bool IsFinished() { return false; }
	private:
		IEvolutionEngine<T>* engine = nullptr;
	};

	/// <summary>
	/// Capture a fitness score derived from a candidate system configuration and its objective scores.
	/// </summary>
	/// <typeparam name="T">The type of fitness used to compare system configuration.</typeparam>
	template<typename T, typename TSys>
	class FitnessAssignedScores //: IComparable<FitnessAssignedScores<T>> where T : IComparable
	{
	public:
		/// <summary>
		/// Creates a FitnessAssignedScores, a union of a candidate system configuration and its objective scores, and an overall fitness score.
		/// </summary>
		/// <param name="scores">Objective scores</param>
		/// <param name="fitnessValue">Fitness value, derived from the scores and context information such as a candidate population.</param>
		FitnessAssignedScores(const IObjectiveScores<TSys>& scores, T fitnessValue)
		{
			this->scores = scores;
			this->fitnessValue = fitnessValue;
		}

		/// <summary>
		/// Gets the objective scores
		/// </summary>
		IObjectiveScores<TSys> Scores() const { return scores; }


		/// <summary>
		/// Gets the fitness value that has been assigned to the candidate system configuration and its objective scores
		/// </summary>
		T FitnessValue() const { return fitnessValue; }

		IObjectiveScores<TSys> scores;
		T fitnessValue;

		/// <summary>
		/// Compares two FitnessAssignedScores<T>.
		/// </summary>
		/// <param name="other">Object to compare with this object</param>
		/// <returns>an integer as necessary to implement IComparable</returns>
		int CompareTo(const FitnessAssignedScores<T, TSys>& other) const
		{
			int result;
			if (FitnessValue() < other.FitnessValue())
				result = -1;
			else if (FitnessValue() == other.FitnessValue())
				result = 0;
			else
				result = 1;
			return result;
		}

		int CompareTo(const FitnessAssignedScores<T, TSys>* other)
		{
			return CompareTo(*other);
		}

		static bool BetterThan(const FitnessAssignedScores<T, TSys>& elem1, const FitnessAssignedScores<T, TSys>& elem2)
		{
			if (elem1.CompareTo(elem2) < 0)
				return true;
			return false;
		}

		static bool BetterThanPtr(const FitnessAssignedScores<T, TSys>* elem1, const FitnessAssignedScores<T, TSys>* elem2)
		{
			return BetterThan(*elem1, *elem2);
		}


		string ToString()
		{
			return FitnessValue().ToString() + ", " + Scores.ToString();
		}
	};

	template<typename TVal, typename TSys>
	class IFitnessAssignment
	{
	public:
		std::vector<FitnessAssignedScores<TVal, TSys>> AssignFitness(const std::vector<IObjectiveScores<TSys>>& scores)
		{
			std::vector<FitnessAssignedScores<TVal, TSys>> result;
			for (IObjectiveScores<TSys> s : scores)
				result.push_back(FitnessAssignedScores<TVal, TSys>(s, s.Value(0)));
			return result;
		}
	};

	template<typename TSysConf>
	class IObjectiveEvaluator
	{
	public:
		virtual ~IObjectiveEvaluator() {}
		/// <summary>
		/// Evaluate the objective values for a candidate system configuration
		/// </summary>
		/// <param name="systemConfiguration">candidate system configuration</param>
		/// <returns>An object with one or more objective scores</returns>
		virtual IObjectiveScores<TSysConf> EvaluateScore(TSysConf systemConfiguration) = 0;
		virtual bool IsCloneable() { return false; }
	};

	/// <summary>
	/// A superset of the <see cref="IObjectiveEvaluator"/> interface that is clonable, most notably to spawn evaluators that are thread safe.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <remarks>
	/// This interface is used for instance by the parallel version of the shuffled complex evolution algorithm, 
	/// when spawning complexes that will be run in parallel and for which we want thread safe structures, 
	/// e.g. not sharing the same model runner.
	/// </remarks>
	//template<typename TSysConf>
	//class IClonableObjectiveEvaluator<TSysConf> :
	//	IObjectiveEvaluator<TSysConf> //, ICloningSupport<IClonableObjectiveEvaluator<TSysConf>>
	//	//		where TSysConf : ISystemConfiguration
	//{
	//};

	///// <summary>
	///// Interface for scores used to quantify the performance of a system, defining the type of the score.
	///// </summary>
	///// <typeparam name="T">The type of the objective (score) value, e.g. double, int, float</typeparam>
	//template<typename T>
	//class IObjectiveScore //<out T> where T : IComparable
	//{
	//public:
	//	virtual ~Class() {}
	//	/// <summary>
	//	/// Gets the value of the objective.
	//	/// </summary>
	//	virtual T Value() = 0;
	//};


	/// <summary>
	/// An interface for population based search algorithms
	/// </summary>
	/// <typeparam name="V">The type of the fitness score used to evolve the algorithm</typeparam>
	//template<typename V>
	//class IPopulation
	//	//		where V : IComparable
	//{
	//public:
	//	virtual ~IPopulation() {}
	//	FitnessAssignedScores<V>[] Population() = 0;
	//};


	/// <summary>
	/// An interface for constructs where the optimization problem is given to a solver, and ready to execute.
	/// </summary>
	//template<typename TSysConf>
	//class IEvolutionEngine
	//{
	//public:
	//	virtual ~IEvolutionEngine() {}
	//	/// TODO: think of ways to monitor the execution.

	//	/// <summary>
	//	/// Solve the metaheuristic this object defines.
	//	/// </summary>
	//	/// <returns>The results of the optimization process</returns>
	//	virtual IOptimizationResults<TSysConf> Evolve() = 0;

	//	/// <summary>
	//	/// Gets a description of this solver
	//	/// </summary>
	//	/// <returns></returns>
	//	virtual string GetDescription() = 0;

	//	/// <summary>
	//	/// Request a cancellation of the process.
	//	/// </summary>
	//	virtual void Cancel() = 0;

	//};


	template<typename T>
	class HyperCube : public IHyperCube<T> //where T : IComparable
	{
	public:
		HyperCube() {}

		HyperCube(const HyperCube& src)
		{
			this->def = src.def;
		}
		vector<string> GetVariableNames() {
			return mhcpp::utils::GetKeys(def);
		}
		void Define(string name, double min, double max, double value) {
			def[name] = MMV(name, min, max, value);
		}
		size_t Dimensions() { return def.size(); }
		T GetValue(string variableName) { return def[variableName].Value; }
		T GetMaxValue(string variableName) { return def[variableName].Max; }
		T GetMinValue(string variableName) { return def[variableName].Min; }
		void SetValue(string variableName, T value)    { def[variableName].Value = value; }
		void SetMinValue(string variableName, T value) { def[variableName].Min = value; }
		void SetMaxValue(string variableName, T value) { def[variableName].Max = value; }
		//IHyperCube<T> HomotheticTransform(IHyperCube<T> point, double factor) {}
		string GetConfigurationDescription() { return ""; }
		void ApplyConfiguration(void* system) {}

		static HyperCube GetCentroid(std::vector<HyperCube> points)
		{
			return HyperCube();
		}

	private:
		class MMV
		{
		public:
			MMV(){}
			MMV(string name, double min, double max, double value) : 
				Name(name), Max(max), Min(min), Value(value)
			{
			}
			string Name;
			double Min, Max, Value;
		};
		std::map<string, MMV> def;
	};

	template<typename TSysConf>
	class TopologicalDistance : public IObjectiveEvaluator<TSysConf>
	{
	public:
		TopologicalDistance(const TSysConf& goal) { this->goal = goal; }
		~TopologicalDistance() {}

		/// <summary>
		/// Evaluate the objective values for a candidate system configuration
		/// </summary>
		/// <param name="systemConfiguration">candidate system configuration</param>
		/// <returns>An object with one or more objective scores</returns>
		IObjectiveScores<TSysConf> EvaluateScore(TSysConf systemConfiguration)
		{
			double sumsqr=0;
			vector<string> varNames = goal.GetVariableNames();
			for (auto& v : varNames)
			{
				auto d = goal.GetValue(v) - systemConfiguration.GetValue(v);
				sumsqr += d*d;
			}
			return IObjectiveScores<TSysConf>(systemConfiguration, "L2 distance", std::sqrt(sumsqr));
		}

	private:
		TSysConf goal;
	};


	class IHyperCubeOperations
	{
		//virtual IHyperCube<double> GetCentroid(std::vector<IHyperCube<double>> points) = 0;
		//virtual IHyperCube<double> GenerateRandomWithinHypercube(std::vector<IHyperCube<double>> points) = 0;
		//virtual IHyperCube<double> GenerateRandom(IHyperCube<double> point) = 0;
	};

	class IHyperCubeOperationsFactory
	{
	public:
		//IHyperCubeOperations CreateNew(IRandomNumberGeneratorFactory rng) {
		//	return IHyperCubeOperations();
		//}
		virtual IHyperCubeOperations* CreateNew(IRandomNumberGeneratorFactory rng) = 0;
	};

}