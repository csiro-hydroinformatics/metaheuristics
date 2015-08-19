#pragma once

#include <random>
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

	private:
		RNG _eng;
		Distribution _dist;
	};

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


	VariateGenerator<std::default_random_engine, std::discrete_distribution<int>> CreateTrapezoidalRng(size_t n, const std::default_random_engine& generator, double trapezoidalFactor = -1)
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
				weights[i-1] = (n + 1 - i);
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


}