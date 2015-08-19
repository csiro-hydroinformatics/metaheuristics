#pragma once

#include <random>
#include <boost/random.hpp>

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

		std::mt19937 CreateNewStd()
		{
			unsigned int seed = Next();
			return CreateNewStd(seed);
		}

		std::mt19937 CreateNewStd(unsigned int seed)
		{
			return std::mt19937(seed);
		}

		// http://stackoverflow.com/questions/7166799/specialize-template-function-for-template-class
		template<class DistributionType = std::uniform_real_distribution<double>>
		boost::variate_generator<std::mt19937, DistributionType> * CreateVariateGenerator(DistributionType& dist, unsigned int seed)
		{
			return new boost::variate_generator<std::mt19937, DistributionType>(CreateNewStd(seed), dist);
		}
	};


}