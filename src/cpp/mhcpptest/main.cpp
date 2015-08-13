#define CATCH_CONFIG_MAIN  // This tells Catch to provide a main() - only do this in one cpp file
#include <set>
#include <vector>
#include <map>
#include <algorithm>
#include <iostream>
#include <iterator>
#include "catch.hpp"
#include "../mhcpp/core.h"
#include "../mhcpp/sce.hpp"

using namespace mhcpp;
using namespace std;
using namespace mhcpp::optimization;

SCENARIO("basic hypercubes", "[sysconfig]") {

	GIVEN("A 2 dimensional hypercube (Yeah, square...)")
	{
		HyperCube<double> hc;
		hc.Define("a", 1, 2, 1.5);
		hc.Define("b", 3, 4, 3.3);
		vector<string> keys;
		keys.push_back("a");
		keys.push_back("b");

		WHEN("Read values after initial definition") {
			REQUIRE(hc.Dimensions() == 2);
			REQUIRE(mhcpp::utils::AreSetEqual(keys, hc.GetVariableNames()));
			REQUIRE(hc.GetValue("a") == 1.5);
			REQUIRE(hc.GetMinValue("a") == 1);
			REQUIRE(hc.GetMaxValue("a") == 2);
			REQUIRE(hc.GetValue("b") == 3.3);
			REQUIRE(hc.GetMinValue("b") == 3);
			REQUIRE(hc.GetMaxValue("b") == 4);
		}

		WHEN("Change definition values after initial definition") {

			hc.SetValue("a", 1.1);
			hc.SetMinValue("a", 0.5);
			hc.SetMaxValue("a", 1.99);
			hc.SetValue("b", 5);
			hc.SetMinValue("b", 0);
			hc.SetMaxValue("b", 10);


			REQUIRE(hc.GetValue("a") == 1.1);
			REQUIRE(hc.GetMinValue("a") == 0.5);
			REQUIRE(hc.GetMaxValue("a") == 1.99);
			REQUIRE(hc.GetValue("b") == 5);
			REQUIRE(hc.GetMinValue("b") == 0);
			REQUIRE(hc.GetMaxValue("b") == 10);
		}
	}
}


SCENARIO("Basic objective evaluator", "[objectives]") {

	GIVEN("Single-value score")
	{
		HyperCube<double> hc;
		hc.Define("a", 1, 2, 1.5);
		hc.Define("b", 3, 4, 3.3);

		auto sceParams = SceParameters::CreateForProblemOfDimension(5, 20);
		ShuffledComplexEvolution<HyperCube<double>> opt(nullptr, nullptr, nullptr, sceParams);

		HyperCube<double> goal;
		goal.Define("a", 1, 2, 1);
		goal.Define("b", 3, 4, 3);

		//IObjectiveEvaluator<HyperCube < double > >* evaluator = new TopologicalDistance<HyperCube < double > >(goal);
		TopologicalDistance<HyperCube < double > > evaluator(goal);

		IObjectiveScores<HyperCube<double>> scores = evaluator.EvaluateScore(hc);
		WHEN("") {
			REQUIRE(scores.ObjectiveCount() == 1);
			REQUIRE(scores.Value(0) == std::sqrt(0.25 + 0.09));
		}
	}
}

SCENARIO("RNG basics", "[rng]") {
	HyperCube<double> hc;
	hc.Define("a", 1, 2, 1.5);
	hc.Define("b", 3, 4, 3.3);
	auto rng = UniformRandomSamplingFactory<HyperCube<double>>(IRandomNumberGeneratorFactory(), hc);
	HyperCube<double> p = rng.CreateRandomCandidate();

	REQUIRE(p.GetMinValue("a") == 1.0);
	REQUIRE(p.GetMaxValue("a") == 2.0);
	REQUIRE(p.GetMinValue("b") == 3.0);
	REQUIRE(p.GetMaxValue("b") == 4.0);
	REQUIRE(p.GetValue("a") != 1.5);
	REQUIRE(p.GetValue("b") != 3.3);

	REQUIRE(p.GetValue("a") >= 1.0);
	REQUIRE(p.GetValue("b") >= 3.0);

	REQUIRE(p.GetValue("a") <= 2.0);
	REQUIRE(p.GetValue("b") <= 4.0);
}

SCENARIO("Complex for SCE", "[optimizer]") {
	GIVEN("A 2D Hypercube")
	{
		using T = HyperCube < double > ;
		std::vector < IObjectiveScores<T> > scores;
		int m = 1;
		int q = 1, alpha = 1, beta = 1;
		IObjectiveEvaluator<T>* evaluator = nullptr;
		IRandomNumberGeneratorFactory rng;
		IFitnessAssignment<double, T> fitnessAssignment;
		/*IHyperCubeOperations* hyperCubeOperations, */
		ILoggerMh* logger = nullptr;

		Complex<T> cplx
			(scores, m, q, alpha, beta,
			evaluator, rng,
			fitnessAssignment, logger = nullptr);

		// Create a subcomplex. 
		// The wirst point found is the expected one.
		// Calling Evolve works without exceptions.

		cplx.Evolve();
	}
}

SCENARIO("SCE basic port", "[optimizer]") {

	GIVEN("A 2D Hypercube")
	{
		HyperCube<double> hc;
		hc.Define("a", 1, 2, 1.5);
		hc.Define("b", 3, 4, 3.3);

		auto sceParams = SceParameters::CreateForProblemOfDimension(5, 20);
		// TODO: check above
		sceParams.P = 5;
		sceParams.Pmin = 3;

		HyperCube<double> goal;
		goal.Define("a", 1, 2, 1);
		goal.Define("b", 3, 4, 3);
		TopologicalDistance<HyperCube < double > >  * evaluator = new TopologicalDistance<HyperCube < double > >(goal);
		ICandidateFactory<HyperCube < double > >* populationInitializer = new UniformRandomSamplingFactory<HyperCube<double>>(IRandomNumberGeneratorFactory(), hc);
		ITerminationCondition<HyperCube < double > > terminationCondition;

		ShuffledComplexEvolution<HyperCube<double>> opt(evaluator, populationInitializer, &terminationCondition, sceParams);

		WHEN("") {
			auto results = opt.Evolve();
			auto first = results[0];
			REQUIRE(first.ObjectiveCount() == 1);			
		}

		//delete evaluator;
	}
}
