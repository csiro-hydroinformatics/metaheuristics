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


HyperCube<double> createTestHc(double a, double b, double aMin = 1, double bMin = 3, double aMax = 2, double bMax = 4) {
	HyperCube<double> hc;
	hc.Define("a", aMin, aMax, a);
	hc.Define("b", bMin, bMax, b);
	return hc;
}

SCENARIO("basic hypercubes", "[sysconfig]") {

	GIVEN("A 2 dimensional hypercube (Yeah, square...)")
	{
		HyperCube<double> hc = createTestHc(1.5, 3.3);
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
			REQUIRE(hc.GetValue("b") == 5);
			REQUIRE(hc.GetMinValue("a") == 0.5);
			REQUIRE(hc.GetMaxValue("a") == 1.99);
			REQUIRE(hc.GetMinValue("b") == 0);
			REQUIRE(hc.GetMaxValue("b") == 10);
		}
	}
}

bool assertHyperCube(const HyperCube<double>& hc, double a, double b, double tolerance = 1.0e-9)
{
	return 
		(std::abs(hc.GetValue("a") - a) < tolerance) && 
		(std::abs(hc.GetValue("b") - b) < tolerance);
}

SCENARIO("Calculation of a centroid", "[sysconfig]") {

	GIVEN("A population of hypercubes")
	{
		std::vector<HyperCube<double>> points;
		points.push_back(createTestHc(1.1, 2.1));
		points.push_back(createTestHc(1.2, 2.2));
		auto c = HyperCube<double>::GetCentroid(points);
		REQUIRE(assertHyperCube(c, 1.15, 2.15));
		points.push_back(createTestHc(1.9, 2.6));
		c = HyperCube<double>::GetCentroid(points);
		REQUIRE(assertHyperCube(c, 1.4, 2.3));

	}
}

SCENARIO("Basic objective evaluator", "[objectives]") {

	GIVEN("Single-value score")
	{
		HyperCube<double> hc = createTestHc(1.5, 3.3 );
		HyperCube<double> goal = createTestHc(1, 3);

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

template<typename T>
UniformRandomSamplingFactory<T> createTestUnifrand(int seed=0)
{
	IRandomNumberGeneratorFactory rng(seed);
	HyperCube<double> goal = createTestHc(1.5, 3.4);
	UniformRandomSamplingFactory<T> prand(rng, goal);
	return prand;
}

template<typename T>
std::vector < IObjectiveScores<T> >createTestScores(int m, int seed = 0)
{
	std::vector < IObjectiveScores<T> > scores;
	auto prand = createTestUnifrand<T>();
	HyperCube<double> goal = createTestHc(1.5, 3.4);
	TopologicalDistance<T> evaluator(goal);

	for (size_t i = 0; i < m; i++)
		scores.push_back(evaluator.EvaluateScore(prand.CreateRandomCandidate()));

	return scores;
}

SCENARIO("Sub-Complex for SCE", "[optimizer]") {
	GIVEN("A 2D Hypercube")
	{
		using T = HyperCube < double >;
		int m = 20;
		int q = 10, alpha = 1, beta = 1;
		IRandomNumberGeneratorFactory rng(2);
		auto unif = createTestUnifrand<T>(421);

		std::vector < IObjectiveScores<T> > scores = createTestScores<T>(m, 123);
		IFitnessAssignment<double, T> fitnessAssignment;
		//IHyperCubeOperations* hyperCubeOperations
		//ILoggerMh* logger = nullptr;
		HyperCube<double> goal = createTestHc(1.5, 3.4);
		TopologicalDistance<T> evaluator(goal);

		SubComplex<T> scplx
			(scores, &evaluator, q, alpha, rng, &unif,
				fitnessAssignment);

		// Create a subcomplex. 
		// The wirst point found is the expected one.
		// Calling Evolve works without exceptions.

		scplx.Evolve();
	}
}


SCENARIO("Complex for SCE", "[optimizer]") {
	GIVEN("A 2D Hypercube")
	{
		using T = HyperCube < double >;

		int m = 20;
		int q = 10, alpha = 1, beta = 1;
		IRandomNumberGeneratorFactory rng(2);
		auto unif = createTestUnifrand<T>(421);

		std::vector < IObjectiveScores<T> > scores = createTestScores<T>(m, 123);
		IFitnessAssignment<double, T> fitnessAssignment;
		/*IHyperCubeOperations* hyperCubeOperations, */
		ILoggerMh* logger = nullptr;

		HyperCube<double> goal = createTestHc(1.5, 3.4);
		TopologicalDistance<T> evaluator(goal);

		Complex<T> cplx
			(scores, m, q, alpha, beta,
				&evaluator, rng, &unif,
				fitnessAssignment, logger = nullptr);

		// Create a subcomplex. 
		// The wirst point found is the expected one.
		// Calling Evolve works without exceptions.

		cplx.Evolve();
	}
}


template<typename T>
class CounterTestFinished
{
	int counter = 0;
	int maxChecks = 0;
public:
	CounterTestFinished(int maxChecks)
	{
		this->maxChecks = maxChecks;
	}
	bool IsFinished( IEvolutionEngine<T>* engine)
	{
		counter++;
		return (counter >= maxChecks);
	}

	std::function<bool(IEvolutionEngine<T>*)> CreateNew(CounterTestFinished& c)
	{
		return [&c](IEvolutionEngine<T>* e)
		{
			return c.IsFinished(e);
		};
	}
};

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

		CounterTestFinished<HyperCube<double>> c(200);
		ITerminationCondition<HyperCube < double > > terminationCondition(c.CreateNew(c));

		ShuffledComplexEvolution<HyperCube<double>> opt(evaluator, populationInitializer, &terminationCondition, sceParams);

		WHEN("") {
			auto results = opt.Evolve();
			//auto first = results[0];
			//REQUIRE(first.ObjectiveCount() == 1);			
		}

		//delete evaluator;
	}
}
