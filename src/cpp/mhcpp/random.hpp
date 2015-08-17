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


}