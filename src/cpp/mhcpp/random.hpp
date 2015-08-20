#pragma once

#include <random>
#include <numeric>
#include <set>
//#include <boost/random.hpp>

using namespace std;

namespace mhcpp
{

	template<typename RNG, typename Distribution>
	class VariateGenerator
	{
	public:
		typedef typename Distribution::result_type result_type;

		VariateGenerator() { }

		VariateGenerator(const Distribution& d) : _dist(d) { }

		/**
		* Constructs a @c VariateGenerator object with the associated
		* \uniform_random_number_generator eng and the associated
		* \random_distribution d.
		*
		* Throws: If and what the copy constructor of RNG or
		* Distribution throws.
		*/

		VariateGenerator(const RNG& e, const Distribution& d)
			: _eng(e), _dist(d) { }

		VariateGenerator(const VariateGenerator& vg)
			: _eng(vg._eng), _dist(vg._dist) { }

		VariateGenerator(const VariateGenerator&& vg)
		{
			this->_eng = std::move(src._eng);
			this->_dist = std::move(src._dist);
		}

		VariateGenerator& operator=(const VariateGenerator& vg)
		{
			if (&vg == this) {
				return *this;
			}
			this->_eng = vg._eng;
			this->_dist = vg._dist;
			return *this;
		}

		VariateGenerator& operator=(const VariateGenerator&& vg)
		{
			if (&vg == this) {
				return *this;
			}
			this->_eng = std::move(vg._eng);
			this->_dist = std::move(vg._dist);
			return *this;
		}

		bool RngEngineEquals(const VariateGenerator& other) const
		{
			if (&other == this) {
				return true;
			}
			return _eng._Equals(other._eng);
		}

		result_type operator()() { return _dist(_eng); }

		const Distribution& distribution()
		{
			return _dist;
		}

	private:
		RNG _eng;
		Distribution _dist;
	};

	using RngInt = VariateGenerator < std::default_random_engine, std::discrete_distribution<int> > ;

	class IRandomNumberGeneratorFactory
	{

	private:
		/** \brief	A random number generator factory. */

		template<typename RNG = std::default_random_engine>
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

			RandomNumberGeneratorFactory() : seedEngine(0)
			{
			}

			RandomNumberGeneratorFactory(const RandomNumberGeneratorFactory& src)
			{
				this->seedEngine = src.seedEngine;
			}

			RandomNumberGeneratorFactory(const RandomNumberGeneratorFactory&& src)
			{
				this->seedEngine = std::move(src.seedEngine);
			}

			RandomNumberGeneratorFactory& operator = (const RandomNumberGeneratorFactory& src)
			{
				if (&src == this) {
					return *this;
				}
				this->seedEngine = src.seedEngine;
				return *this;
			}

			RandomNumberGeneratorFactory& operator = (const RandomNumberGeneratorFactory&& src)
			{
				if (&src == this) {
					return *this;
				}
				this->seedEngine = std::move(src.seedEngine);
				return *this;
			}

			virtual ~RandomNumberGeneratorFactory()
			{
			}

			bool Equals(const RandomNumberGeneratorFactory& other) const
			{
				return seedEngine._Equals(other.seedEngine);
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
			static VariateGenerator<RNG*, DistributionType> * CreateVariateGenerator(RandomNumberGeneratorFactory<RNG>& rngf, DistributionType& dist)
			{
				return new VariateGenerator<RNG*, DistributionType>(rngf.CreateNewEngine(), dist);
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

		IRandomNumberGeneratorFactory(const IRandomNumberGeneratorFactory& src)
		{
			this->rng = src.rng;
		}

		IRandomNumberGeneratorFactory(const IRandomNumberGeneratorFactory&& src)
		{
			this->rng = std::move(src.rng);
		}

		IRandomNumberGeneratorFactory& operator = (const IRandomNumberGeneratorFactory& src)
		{
			if (&src == this) {
				return *this;
			}
			this->rng = src.rng;
			return *this;
		}

		IRandomNumberGeneratorFactory& operator = (const IRandomNumberGeneratorFactory&& src)
		{
			if (&src == this) {
				return *this;
			}
			this->rng = std::move(src.rng);
			return *this;
		}

		bool Equals(const IRandomNumberGeneratorFactory& other) const
		{
			return rng.Equals(other.rng);
		}

		unsigned int Next()
		{
			return rng();
		}

		std::vector<unsigned int> Next(size_t size)
		{
			std::vector<unsigned int> result;
			for (size_t i = 0; i < size; i++)
			{
				result.push_back(Next());
			}
			return result;
		}

		IRandomNumberGeneratorFactory CreateNew()
		{
			return IRandomNumberGeneratorFactory(Next());
		}

		std::default_random_engine CreateNewStd()
		{
			unsigned int seed = Next();
			return CreateNewStd(seed);
		}

		static std::default_random_engine CreateNewStd(unsigned int seed)
		{
			return std::default_random_engine(seed);
		}

		// http://stackoverflow.com/questions/7166799/specialize-template-function-for-template-class
		template<class DistributionType = std::uniform_real_distribution<double>>
		VariateGenerator<std::default_random_engine, DistributionType> CreateVariateGenerator(DistributionType& dist, unsigned int seed) const
		{
			return VariateGenerator<std::default_random_engine, DistributionType>(CreateNewStd(seed), dist);
		}

		template<class DistributionType = std::uniform_real_distribution<double>>
		VariateGenerator<std::default_random_engine, DistributionType> CreateVariateGenerator(DistributionType& dist)
		{
			return VariateGenerator<std::default_random_engine, DistributionType>(CreateNewStd(), dist);
		}
	};

	RngInt CreateTrapezoidalRng(size_t n, const std::default_random_engine& generator, double trapezoidalFactor = -1)
	{
		std::vector<double> weights(n);
		double m = n;
		double sumWeights = n*(n + 1) / 2.0;
		double avgWeight = sumWeights / m;

		if ((trapezoidalFactor <= 0) || (trapezoidalFactor >= 2))
		{
			// default as per the original SCE paper. Note that we do not need 
			// to normalize: std::discrete_distribution takes care of it
			for (size_t i = 1; i <= n; i++)
				weights[i - 1] = (n + 1 - i);
		}
		else
		{
			// y = ax + b
			double b = trapezoidalFactor; // y(0) = b 
			double a = 2 * (1 - b) / (m - 1);  // because y(n-1) = (2-b) = a * (n-1) + b 
			for (size_t i = 0; i < n; i++)
				weights[i] = a * i + b;
		}
		// Would expect to be able to do:
		// std::discrete_distribution<int> distribution(weights.begin(), weights.end());
		// but missing constructor in MS implementation. Using workaround derived from
		// http://stackoverflow.com/questions/21959404/initialising-stddiscrete-distribution-in-vs2013
		std::size_t i(0);
		std::discrete_distribution<> distribution(weights.size(),
			0.0, // dummy!
			1.0, // dummy!
			[&weights, &i](double)
		{
			auto w = weights[i];
			++i;
			return w;
		});
		return VariateGenerator<std::default_random_engine, std::discrete_distribution<int>>(generator, distribution);
	}

	template<typename X>
	static std::vector<const X*> AsPointers(const std::vector<X>& vec)
	{
		std::vector<const X*> result;
		for (size_t i = 0; i < vec.size(); i++)
		{
			const X& e = vec[i];
			result.push_back(&e);
		}
		return result;
	}

	template<typename ElemType>
	vector<ElemType> SampleFrom(RngInt& drng, const std::vector<ElemType>& population, size_t n,
		bool replace = false, vector<ElemType> *leftOut = nullptr)
	{
		if (!replace && population.size() <= n)
			throw std::logic_error("If elements are sampled once, the output size must be less than the population sampled from");
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
			}
			if (leftOut != nullptr)
			{
				leftOut->clear();
				for (auto index : notSelected)
					leftOut->push_back(population[index]);
			}
		}
		return result;
	}

	vector<int> SampleFrom(RngInt& drng, size_t nsampling)
	{
		size_t size = drng.distribution().max() - drng.distribution().min() + 1;
		std::vector<int>p(size);

		for (size_t i = 0; i < nsampling; ++i) {
			int number = drng();
			++p[number];
		}
		return p;
	}

	template<typename ElemType>
	void PrintHistogram(const std::vector<ElemType>& hist, std::ostream& stream, int nstars = 100, char c = '*')
	{
		ElemType total = std::accumulate(hist.begin(), hist.end(), 0);
		size_t n = hist.size();
		std::vector<string> s(n);
		for (size_t i = 0; i < n; ++i)
			s[i] = std::string(hist[i] * nstars / total, c);
		PrintVec(s);
	}

	template<typename ElemType>
	std::vector<double> Normalize(const std::vector<ElemType>& hist)
	{
		size_t n = hist.size();
		std::vector<double> p(n);
		ElemType total = std::accumulate(hist.begin(), hist.end(), 0);
		for (size_t i = 0; i < n; ++i)
			p[i] = (double)hist[i] / total;
		return p;
	}

	template<typename T>
	vector<T> RelativeDiff(const vector<T>& expected, const vector<T>& b)
	{
		vector<T> result(expected.size());
		for (size_t i = 0; i < expected.size(); i++)
		{
			result[i] = (std::abs(expected[i] - b[i]) / expected[i]);
		}
		return result;
	}


	template<typename ElemType>
	void PrintVec(const std::vector<ElemType>& hist, std::ostream& stream)
	{
		int n = hist.size();
		for (size_t i = 0; i < n; ++i)
			stream << i << ": " << std::to_string(hist[i]) << std::endl;
	}

	template<typename ElemType>
	void PrintValues(const std::vector<ElemType>& hist, std::ostream& stream, bool proportions = false)
	{
		int n = hist.size();
		if (!proportions)
		{
			PrintVec(hist, stream);
		}
		else
		{
			auto p = Normalize(hist);
			PrintVec(p, stream);
		}
	}

}