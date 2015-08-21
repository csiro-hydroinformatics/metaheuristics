#pragma once

#include <string>
#include <vector>
#include <map>
#include <random>
#include <functional>
#include "random.hpp"
#include "utils.hpp"

using namespace std;

namespace mhcpp
{

	class ISystemConfiguration
	{
	public:
		virtual ~ISystemConfiguration() {}
		/// <summary>
		/// Gets an alphanumeric description for this system configuration
		/// </summary>
		virtual string GetConfigurationDescription() const = 0;

		/// <summary>
		/// Apply this system configuration to a compatible system, usually a 'model' in the broad sense of the term.
		/// </summary>
		/// <param name="system">A compatible system, usually a 'model' in the broad sense of the term</param>
		/// <exception cref="ArgumentException">thrown if this system configuration cannot be meaningfully applied to the specified system</exception>
		virtual void ApplyConfiguration(void* system) = 0;
	};

	/// <summary>
	/// Interface for system configurations that are a set of numeric parameters, each with min and max feasible values.
	/// </summary>
	/// <typeparam name="T">A comparable type; typically a float or double, but possibly integer or more esoteric type</typeparam>
	template<typename T = double>
	class IHyperCube : public ISystemConfiguration //where T : IComparable
	{
	public:
		virtual ~IHyperCube() {}
		/// <summary>
		/// Gets the names of the variables defined for this hypercube.
		/// </summary>
		/// <returns></returns>
		virtual vector<string> GetVariableNames() const = 0;

		/// <summary>
		/// Gets the number of dimensions in this hypercube
		/// </summary>
		virtual size_t Dimensions() const = 0;

		/// <summary>
		/// Gets the value for a variable
		/// </summary>
		/// <param name="variableName"></param>
		/// <returns></returns>
		virtual T GetValue(string variableName) const = 0;

		/// <summary>
		/// Gets the maximum feasible value for a variable
		/// </summary>
		/// <param name="variableName"></param>
		/// <returns></returns>
		virtual T GetMaxValue(string variableName) const = 0;

		/// <summary>
		/// Gets the minimum feasible value for a variable
		/// </summary>
		/// <param name="variableName"></param>
		/// <returns></returns>
		virtual T GetMinValue(string variableName) const = 0;

		/// <summary>
		/// Sets the value of one of the variables in the hypercube
		/// </summary>
		/// <param name="variableName"></param>
		/// <param name="value"></param>
		virtual void SetValue(string variableName, T value) = 0;

		virtual string ToString() const
		{
			string s;
			auto vnames = GetVariableNames();
			for (auto& v : vnames)
				s += v + ":" + std::to_string(GetValue(v)) + ", ";
			return s;
		}
	};

	template<typename T=double>
	class IHyperCubeSetBounds : public IHyperCube < T > //where T : IComparable
	{
	public:
		virtual ~IHyperCubeSetBounds() {}
		virtual void SetMinValue(string variableName, T value) = 0;
		virtual void SetMaxValue(string variableName, T value) = 0;
		virtual void SetMinMaxValue(string variableName, T min, T max, T value) = 0;
	};

	template<typename TSysConfig>
	class ICandidateFactory
	{
	public:
		virtual ~ICandidateFactory() {}
		virtual TSysConfig CreateRandomCandidate() = 0;
		virtual TSysConfig CreateRandomCandidate(const TSysConfig& point) = 0;
		virtual TSysConfig CreateRandomCandidate(const std::vector<TSysConfig>& points) = 0;
		virtual ICandidateFactory<TSysConfig>* CreateNew() = 0;
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

		IObjectiveScores() {}

		virtual ~IObjectiveScores() {}

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
			sys = std::move(src.sys);
			objectives = std::move(src.objectives);
			return *this;
		}

		/// <summary>
		/// Gets the system configuration that led to these scores.
		/// </summary>
		virtual TSysConf SystemConfiguration() const { return sys; }

		/// <summary>
		/// Gets the number of objective scores in this instance.
		/// </summary>
		virtual size_t ObjectiveCount() const { return this->objectives.size(); }

		virtual double Value(int i) const { return objectives[i].Value; } //= 0;

		string ToString() const
		{
			string s;
			for (size_t i = 0; i < this->ObjectiveCount(); i++)
			{
				s += this->objectives[i].ToString();
				s += ", ";
			}
			s += SystemConfiguration().ToString();
			return s;
		}

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
			string ToString() const
			{
				return Name + ":" + std::to_string(Value);
			}
		};

		TSysConf sys;
		std::vector<ObjectiveValue> objectives;

	};

	template<typename TSysConfig>
	class UniformRandomSamplingFactory : public ICandidateFactory<TSysConfig> //, IHyperCubeOperationsFactory
	{

		static unsigned int CreateSamplerSeed(const IRandomNumberGeneratorFactory& rng)
		{
			auto tmp_rng = rng;
			unsigned int s = tmp_rng.Next();
			unsigned int samplerSeed = s ^ 0xFFFFFFFF;
			return samplerSeed;
		}
		public:
		UniformRandomSamplingFactory(const IRandomNumberGeneratorFactory& rng, const TSysConfig& t)
		{
			this->rng = rng;
			unsigned int samplerSeed = CreateSamplerSeed(rng);
			//if (!t.SupportsThreadSafloning)
			//	throw new ArgumentException("This URS factory requires cloneable and thread-safe system configurations");
			this->t = t;
			//this->hcOps = CreateIHyperCubeOperations();
			SetSampler(samplerSeed);
		}

		UniformRandomSamplingFactory(const UniformRandomSamplingFactory& src)
		{
			this->rng = src.rng;
			this->t = src.t;
			unsigned int samplerSeed = CreateSamplerSeed(rng);
			SetSampler(samplerSeed);
		}

		UniformRandomSamplingFactory(const UniformRandomSamplingFactory&& src)
		{
			this->rng = std::move(src.rng);
			this->t = std::move(src.t);
			unsigned int samplerSeed = CreateSamplerSeed(rng);
			SetSampler(samplerSeed);
		}

		UniformRandomSamplingFactory& operator=(const UniformRandomSamplingFactory &src)
		{
			if (&src == this) {
				return *this;
			}
			this->rng = src.rng;
			this->t = src.t;
			return *this;
		}

		UniformRandomSamplingFactory& operator=(const UniformRandomSamplingFactory&& src)
		{
			if (&src == this) {
				return *this;
			}
			this->rng = std::move(src.rng);
			this->t = std::move(src.t);
			return *this;
		}

		~UniformRandomSamplingFactory() { }

		bool Equals(const UniformRandomSamplingFactory& other) const
		{
			if (&other == this) {
				return true;
			}
			bool samplerRngEquals = sampler.RngEngineEquals(other.sampler);
			bool rngEquals = this->rng.Equals(other.rng);
			return rngEquals && samplerRngEquals;
		}

		//IHyperCubeOperations CreateNew(const IRandomNumberGeneratorFactory& rng)
		//{
		//	return new HyperCubeOperations(rng.CreateFactory());
		//}

		TSysConfig CreateRandomCandidate()
		{
			return CreateRandomCandidate(t);
		}

		TSysConfig CreateRandomCandidate(const TSysConfig& bounds)
		{
			TSysConfig rt(t);
			for (auto& vname : rt.GetVariableNames())
			{
				double min = bounds.GetMinValue(vname);
				double max = bounds.GetMaxValue(vname);
				double d = Urand();
				if (d > 1 || d < 0)
					throw std::logic_error("[0-1] uniform distribution, but got a sample out of this interval");
				rt.SetValue(vname, min + d * (max - min));
			}
			return rt;
		}

		TSysConfig CreateRandomCandidate(const vector<TSysConfig>& population)
		{
			if (population.size() == 0)
				throw std::logic_error("There must be at least one point in the population to define a feasible space for the random point");
			TSysConfig bounds(t);
			for (auto& vname : bounds.GetVariableNames())
			{
				double min = std::numeric_limits<double>::min();
				double max = std::numeric_limits<double>::max();
				for (size_t i = 0; i < population.size(); i++)
				{
					double x = population[i].GetValue(vname);
					min = std::max(min, x);
					max = std::min(max, x);
				}
				bounds.SetMaxValue(vname, max);
				bounds.SetMinValue(vname, min);
			}
			auto result = CreateRandomCandidate(bounds);
#ifdef _DEBUG
			//CheckParameterFeasible(result);
#endif
			return result;
		}

		ICandidateFactory<TSysConfig>* CreateNew()
		{
			return new UniformRandomSamplingFactory<TSysConfig>(this->rng.CreateNew(), this->t);
		}

	private:

		void SetSampler(unsigned int seed)
		{
			std::uniform_real_distribution<double> dist(0, 1);
			sampler = rng.CreateVariateGenerator<std::uniform_real_distribution<double>>(dist, seed);
		}

		double Urand()
		{
			return sampler();
		}
		//IHyperCubeOperations CreateIHyperCubeOperations()
		//{
		//	return new HyperCubeOperations(rng.CreateFactory());
		//}

		VariateGenerator<std::default_random_engine, std::uniform_real_distribution<double>> sampler;
		IRandomNumberGeneratorFactory rng;
		TSysConfig t;
	};

	template<typename T>
	class IEvolutionEngine
	{
	};

	template<typename T>
	class ITerminationCondition
	{
	public:
		ITerminationCondition()
		{
			this->Check = [&](IEvolutionEngine<T>*) {return false; };
		}
		ITerminationCondition(std::function<bool(IEvolutionEngine<T>*)>& isFinishedFunc)
		{
			this->Check = isFinishedFunc;
		}
		void SetEvolutionEngine(IEvolutionEngine<T>* engine) { this->engine = engine; };
		bool IsFinished() 
		{ 
			return Check(engine);
		}
	private:
		std::function<bool(IEvolutionEngine<T>*)> Check;
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

		FitnessAssignedScores()
		{
			this->fitnessValue = T();
		}

		/// <summary>
		/// Gets the objective scores
		/// </summary>
		IObjectiveScores<TSys> Scores() const { return scores; }


		/// <summary>
		/// Gets the fitness value that has been assigned to the candidate system configuration and its objective scores
		/// </summary>
		T FitnessValue() const { return fitnessValue; }

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

		static void Sort(std::vector<FitnessAssignedScores<T, TSys>*>& points)
		{
			std::stable_sort(points.begin(), points.end(), FitnessAssignedScores<T, TSys>::BetterThanPtr);
		}

		static void Sort(std::vector<const FitnessAssignedScores<T, TSys>*>& points)
		{
			std::stable_sort(points.begin(), points.end(), FitnessAssignedScores<T, TSys>::BetterThanPtr);
		}

		static void Sort(std::vector<FitnessAssignedScores<T, TSys>>& points)
		{
			std::stable_sort(points.begin(), points.end(), FitnessAssignedScores<T, TSys>::BetterThan);
		}

		string ToString() const
		{
			return FitnessValue().ToString() + ", " + Scores.ToString();
		}

	private:
		
		IObjectiveScores<TSys> scores;
		T fitnessValue;

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

	template<typename T>
	class HyperCube : public IHyperCube<T> //where T : IComparable
	{
	public:
		HyperCube() {}

		HyperCube(const HyperCube& src)
		{
			this->def = src.def;
		}
		vector<string> GetVariableNames() const {
			return mhcpp::utils::GetKeys(def);
		}
		void Define(string name, double min, double max, double value) {
			def[name] = MMV(name, min, max, value);
		}
		size_t Dimensions() const { return def.size(); }
		T GetValue(string variableName) const { return def.at(variableName).Value; }
		T GetMaxValue(string variableName) const { return def.at(variableName).Max; }
		T GetMinValue(string variableName) const { return def.at(variableName).Min; }
		void SetValue(string variableName, T value)    { def[variableName].Value = value; }
		void SetMinValue(string variableName, T value) { def[variableName].Min = value; }
		void SetMaxValue(string variableName, T value) { def[variableName].Max = value; }
		//IHyperCube<T> HomotheticTransform(IHyperCube<T> point, double factor) {}
		string GetConfigurationDescription() const { return ""; }
		void ApplyConfiguration(void* system) {}

		static HyperCube GetCentroid(const std::vector<HyperCube>& points)
		{
			// TODO: surely some vector libraries to reuse (Boost?)
			if (points.size() == 0) throw std::logic_error("Cannot take centroid of empty set of points");
			vector<string> names = points[0].GetVariableNames();
			vector<double> coords(names.size());
			coords.assign(coords.size(), 0);
			for(auto& p : points)
				for (size_t i = 0; i < coords.size(); i++)
					coords[i] += p.GetValue(names[i]);
			for (size_t i = 0; i < coords.size(); i++)
				coords[i] /= points.size();
			HyperCube centroid = points[0];
			for (size_t i = 0; i < coords.size(); i++)
			{
				centroid.SetValue(names[i], coords[i]);
			}
			return centroid;
		}

		HyperCube HomotheticTransform(const HyperCube& from, double factor)
		{
			HyperCube result(from);
			auto varnames = GetVariableNames();
			for(auto& v : varnames)
			{
				//double min = this->GetMinValue(v);
				//double max = this->GetMaxValue(v);
				//result.SetMinValue(v, min);
				//result.SetMaxValue(v, max);
				double newVal = Reflect(from.GetValue(v), this->GetValue(v), factor);
				//var isInBounds = MetaheuristicsHelper.CheckInBounds(newVal, min, max, throwIfFalse: this->ThrowOnOutOfBounds);
				//if (!isInBounds)
				//{
				//	result = null;
				//	break;
				//}
				result.SetValue(v, newVal);
			}
			return result;
		}

		double Reflect(double point, double reference, double factor)
		{
			return reference + ((point - reference) * factor);
		}

		virtual bool IsFeasible() const
		{
			auto varnames = GetVariableNames();
			for (auto& v : varnames)
				if (!this->def.at(v).IsFeasible()) return false;
			return true;
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
			bool IsFeasible() const { return (Value >= Min) && (Value <= Max); }
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

	bool CheckParameterFeasible(const IObjectiveScores<HyperCube<double>>& s)
	{
		if (!s.SystemConfiguration().IsFeasible())
		{
			string ps = s.ToString();
			std::cout << ps;
			return false;
		}
		return true;
	}

	bool CheckParameterFeasible(const HyperCube<double>& p)
	{
		if (!p.IsFeasible())
		{
			string ps = p.ToString();
			std::cout << ps;
			return false;
		}
		return true;
	}

	bool CheckParameterFeasible(const std::vector<IObjectiveScores<HyperCube<double>>>& vec)
	{
		for (size_t i = 0; i < vec.size(); i++)
		{
			if (!CheckParameterFeasible(vec[i])) return false;
		}
		return true;
	}

	bool CheckParameterFeasible(const FitnessAssignedScores<double, HyperCube<double>>& p)
	{
		return CheckParameterFeasible(p.Scores());
	}

	bool CheckParameterFeasible(const std::vector<FitnessAssignedScores<double, HyperCube<double>>>& vec)
	{
		for (size_t i = 0; i < vec.size(); i++)
		{
			if (!CheckParameterFeasible(vec[i])) return false;
		}
		return true;
	}

	template<typename ElemType>
	vector<ElemType> SampleFrom(RngInt& drng, const std::vector<ElemType>& population, size_t n,
		vector<ElemType>& leftOut, bool replace = false)
	{
		if (!replace && population.size() <= n)
			throw std::logic_error("If elements are sampled once, the output size must be less than the population sampled from");

#ifdef _DEBUG
		//CheckParameterFeasible(population);
#endif

		std::set<int> selected;
		std::set<int> notSelected;
		for (size_t i = 0; i < population.size(); i++)
			notSelected.emplace(i);
		std::vector<ElemType> result(n);
		if (replace)
		{
			for (size_t i = 0; i < n; i++)
			{
				result[i] = population[drng()];
				selected.emplace(i);
				notSelected.erase(i);
			}
		}
		else
		{
			auto src = AsPointers(population);
			int counter = 0;
			while (counter < n)
			{
				int i = drng();
				if (!(src[i] == nullptr))
				{
					result[counter] = *(src[i]);
					src[i] = nullptr;
					selected.emplace(i);
					notSelected.erase(i);
					counter++;
				}
#ifdef _DEBUG
				//CheckParameterFeasible(population);
#endif
			}
			leftOut.clear();
			for (auto index : notSelected)
				leftOut.push_back(population[index]);
#ifdef _DEBUG
			//CheckParameterFeasible(population);
			//CheckParameterFeasible(leftOut);
#endif
		}
		return result;
	}


}

