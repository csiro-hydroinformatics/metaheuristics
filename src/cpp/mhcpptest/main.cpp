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

		IObjectiveScores<HyperCube<double>>* scores = evaluator.EvaluateScore(hc);
		WHEN("") {
			REQUIRE(scores->ObjectiveCount() == 1);
			REQUIRE(scores->Value(0) == std::sqrt(0.25 + 0.09));
		}
		delete scores;
	}
}


SCENARIO("SCE basic port", "[optimizer]") {

	GIVEN("A 2D Hypercube")
	{
		HyperCube<double> hc;
		hc.Define("a", 1, 2, 1.5);
		hc.Define("b", 3, 4, 3.3);

		auto sceParams = SceParameters::CreateForProblemOfDimension(5, 20);

		HyperCube<double> goal;
		goal.Define("a", 1, 2, 1);
		goal.Define("b", 3, 4, 3);
		TopologicalDistance<HyperCube < double > >  * evaluator = new TopologicalDistance<HyperCube < double > >(goal);
		//ICandidateFactory<HyperCube < double > >* populationInitializer;
		//ITerminationCondition<HyperCube < double > >* terminationCondition,

		ShuffledComplexEvolution<HyperCube<double>> opt(evaluator, nullptr, nullptr, sceParams);

		WHEN("") {
			opt.Evolve();
		}
	}
}
