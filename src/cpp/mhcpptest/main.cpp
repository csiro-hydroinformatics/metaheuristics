#define CATCH_CONFIG_MAIN  // This tells Catch to provide a main() - only do this in one cpp file
#include <set>
#include <vector>
#include <map>
#include <algorithm>
#include <iostream>
#include <iterator>
#include "catch.hpp"
#include "../mhcpp/core.hpp"
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

SCENARIO("Basic hypercubes", "[sysconfig]") {

	GIVEN("A 2 dimensional hypercube")
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
		(hc.Dimensions() == 2) &&
		(std::abs(hc.GetValue("a") - a) < tolerance) &&
		(std::abs(hc.GetValue("b") - b) < tolerance);
}

bool isIn(const std::string& s, const std::vector<std::string>& b)
{
	return (std::find(b.begin(), b.end(), s) != b.end());
}

bool allIn(const std::vector<std::string>& a, const std::vector<std::string>& b)
{
	for (auto s : a)
	{
		if (!isIn(s, b))
			return false;
	}
	return true;
}

bool sameSets(const std::vector<std::string>& a, const std::vector<std::string>& b)
{
	if (a.size() != b.size()) return false;
	if (!allIn(a, b)) return false;
	if (!allIn(b, a)) return false;
	return true;
}

bool sameValues(const std::vector<std::string>& keys, const HyperCube<double>& a, const HyperCube<double>& b)
{
	for (auto& s : keys)
		if (a.GetValue(s) != b.GetValue(s)) 
			return false;
	return true;
}

bool sameMinValues(const std::vector<std::string>& keys, const HyperCube<double>& a, const HyperCube<double>& b)
{
	for (auto& s : keys)
		if (a.GetMinValue(s) != b.GetMinValue(s))
			return false;
	return true;
}

bool sameMaxValues(const std::vector<std::string>& keys, const HyperCube<double>& a, const HyperCube<double>& b)
{
	for (auto& s : keys)
		if (a.GetMaxValue(s) != b.GetMaxValue(s))
			return false;
	return true;
}

bool assertEqual(const HyperCube<double>& a, const HyperCube<double>& b)
{
	return
		(a.Dimensions() == b.Dimensions()) &&
		(sameSets(a.GetVariableNames(), b.GetVariableNames())) &&
		(sameValues(a.GetVariableNames(), a, b)) &&
		(sameMinValues(a.GetVariableNames(), a, b)) &&
		(sameMaxValues(a.GetVariableNames(), a, b));
}

bool assertValuesNotEqual(const HyperCube<double>& a, const HyperCube<double>& b)
{
	return
		(a.Dimensions() == b.Dimensions()) &&
		(sameSets(a.GetVariableNames(), b.GetVariableNames())) &&
		(!sameValues(a.GetVariableNames(), a, b));
}

template<typename T>
bool requireEqual(const vector<T>& a, const vector<T>& b)
{
	if (a.size() != b.size()) return false;
	for (size_t i = 0; i < a.size(); i++)
	{
		if (a[i] != b[i]) return false;
	}
	return true;
}

template<typename T>
bool requireRelativeEqual(const vector<T>& expected, const vector<T>& b, T tolerance)
{
	if (expected.size() != b.size()) return false;
	for (size_t i = 0; i < expected.size(); i++)
	{
		if ((std::abs(expected[i] - b[i]) / expected[i]) > tolerance) return false;
	}
	return true;
}


SCENARIO("Calculation of a centroid", "[sysconfig]") {

	GIVEN("A population of hypercubes")
	{
		std::vector<HyperCube<double>> points;
		points.push_back(createTestHc(1.1, 2.1));
		points.push_back(createTestHc(1.2, 2.2));
		auto c = HyperCube<double>::GetCentroid(points);
		WHEN("Population of two points") {
			THEN("Expected barycentre"){
				REQUIRE(assertHyperCube(c, 1.15, 2.15));
			}
		}
		points.push_back(createTestHc(1.9, 2.6));
		c = HyperCube<double>::GetCentroid(points);
		WHEN("Population of three points") {
			THEN("Expected barycentre"){
				REQUIRE(assertHyperCube(c, 1.4, 2.3));
			}
		}
	}
}

SCENARIO("Basic objective evaluator", "[objectives]") {

	GIVEN("Single-objective calculator, L2 distance")
	{
		HyperCube<double> hc = createTestHc(1.5, 3.3);
		HyperCube<double> goal = createTestHc(1, 3);

		//IObjectiveEvaluator<HyperCube < double > >* evaluator = new TopologicalDistance<HyperCube < double > >(goal);
		TopologicalDistance<HyperCube < double > > evaluator(goal);

		WHEN("Evaluating distance") {
			IObjectiveScores<HyperCube<double>> scores = evaluator.EvaluateScore(hc);
			THEN("Gets one objective value with expected value"){
				REQUIRE(scores.ObjectiveCount() == 1);
				REQUIRE(scores.Value(0) == std::sqrt(0.25 + 0.09));
			}
		}
	}
}

SCENARIO("RNG basics", "[rng]") {
	IRandomNumberGeneratorFactory factory(123);
	GIVEN("A random integer generator IRandomNumberGeneratorFactory")
	{
		WHEN("Another is created with a different seed") {
			IRandomNumberGeneratorFactory f2(456);
			THEN("The sequence of random numbers is different")
			{
				REQUIRE_FALSE(factory.Equals(f2));
				auto x1 = factory.Next(10);
				auto x2 = f2.Next(10);
				REQUIRE(x1[0] != x1[1]);
				for (size_t i = 0; i < 10; i++)
					REQUIRE(x1[i] != x2[i]);
			}
		}
		WHEN("A copy is created by assignment") {
			IRandomNumberGeneratorFactory f2 = factory;
			THEN("The sequence of random numbers is the same")
			{
				REQUIRE(factory.Equals(f2));
				auto x1 = factory.Next(10);
				auto x2 = f2.Next(10);
				REQUIRE(x1[0] != x1[1]);
				for (size_t i = 0; i < 10; i++)
					REQUIRE(x1[i] == x2[i]);
			}
		}
	}
}

SCENARIO("discrete RNG to sample from a population of points", "[rng]")
{
	std::default_random_engine generator(234);
	const int ncandidates = 10;
	RngInt rng = CreateTrapezoidalRng(ncandidates, generator);

	const int nrolls = 100000; // number of experiments
	auto p = SampleFrom(rng, nrolls);
	std::cout << "a discrete_distribution:" << std::endl;

	//PrintHistogram(p, std::cout);
	//PrintValues(p, std::cout, true);
	//PrintValues(p, std::cout, false);

	int n = ncandidates;
	std::vector<double> expectedProportions(n);
	double total = n*(n + 1) / 2; // sum ints from 1 to n;
	for (size_t i = 1; i <= n; i++)
		expectedProportions[i - 1] = (n + 1 - i) / total;

	//PrintVec(RelativeDiff(weights, Normalize(p)), std::cout);

	REQUIRE(requireRelativeEqual(expectedProportions, Normalize(p), 3 * 1e-2));

//	vector<int> expected = { 
//18144
//, 16446
//, 14460
//, 12723
//, 10954
//, 9079
//, 7302
//, 5312
//, 3744
//, 1836
//	REQUIRE(requireEqual(expected, p));
}

SCENARIO("URS RNG basics", "[rng]") {
	HyperCube<double> hc;
	hc.Define("a", 1, 2, 1.5);
	hc.Define("b", 3, 4, 3.3);
	GIVEN("An uniform random sampler")
	{
		auto rng = UniformRandomSamplingFactory<HyperCube<double>>(IRandomNumberGeneratorFactory(), hc);

		WHEN("Creating a random point with default template") {
			HyperCube<double> p = rng.CreateRandomCandidate();
			THEN("Feasible parameter space is the same as the template, but values are different from template")
			{
				REQUIRE(p.GetMinValue("a") == 1.0);
				REQUIRE(p.GetMaxValue("a") == 2.0);
				REQUIRE(p.GetMinValue("b") == 3.0);
				REQUIRE(p.GetMaxValue("b") == 4.0);

				REQUIRE(assertValuesNotEqual(p, hc));

				REQUIRE(p.GetValue("a") >= 1.0);
				REQUIRE(p.GetValue("b") >= 3.0);

				REQUIRE(p.GetValue("a") <= 2.0);
				REQUIRE(p.GetValue("b") <= 4.0);
			}
		}

		WHEN("A copy of the URS factory is made, and samples taken from each") {
			auto rngCopy = rng;
			THEN("The two URS objects are equal") {
				REQUIRE(rng.Equals(rngCopy));
			}
			auto p1 = rng.CreateRandomCandidate();
			auto p2 = rngCopy.CreateRandomCandidate();
			AND_THEN("The sampled values are the same as returned by the original one") {
				REQUIRE(rng.Equals(rngCopy));
				REQUIRE(assertEqual(p1, p2));
				REQUIRE(assertValuesNotEqual(p2, hc));
			}
			AND_WHEN("A new factory is created from these existing ones") {
				auto urs1 = rng.CreateNew();
				auto urs2 = rngCopy.CreateNew();
				auto p1_2 = urs1->CreateRandomCandidate();
				auto p2_2 = urs2->CreateRandomCandidate();
				THEN("The sampled values from these two new factories are identical") {
					REQUIRE(assertEqual(p1_2, p2_2));
				}
				AND_THEN("But differ from ones from the original factories") {
					REQUIRE(assertValuesNotEqual(p1_2, p1));
					REQUIRE(assertValuesNotEqual(p2_2, p2));
				}
				delete urs1;
				delete urs2;
			}
		}
	}
}

template<typename T>
UniformRandomSamplingFactory<T> createTestUnifrand(int seed = 0)
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

SCENARIO("Complex for SCE, single objective", "[optimizer]") {
	using T = HyperCube < double >;
	int m = 20;
	int q = 10, alpha = 2, beta = 3;
	IRandomNumberGeneratorFactory rng(2);
	auto unif = createTestUnifrand<T>(421);

	std::vector < IObjectiveScores<T> > scores = createTestScores<T>(m, 123);
	IFitnessAssignment<double, T> fitnessAssignment;
	//IHyperCubeOperations* hyperCubeOperations
	//ILoggerMh* logger = nullptr;
	HyperCube<double> goal = createTestHc(1.5, 3.4);
	TopologicalDistance<T> evaluator(goal);

	WHEN("looking for the point with the worst fitness value")
	{
		std::vector<FitnessAssignedScores<double, T>> fvec;
		std::vector<FitnessAssignedScores<double, T>> subpopulation;
		for (size_t i = 0; i < scores.size(); i++)
		{
			fvec.push_back(FitnessAssignedScores<double, T>(scores[i], i));
		}
		auto worst = SubComplex<T>::FindWorstPoint(fvec, subpopulation);
		THEN("The score of the worst point found is indeed the worst fitness we assigned in the population")
		{
			REQUIRE(worst.FitnessValue() == (double)(scores.size() - 1));
			REQUIRE(subpopulation.size() == (scores.size() - 1));
			for (size_t i = 0; i < subpopulation.size(); i++)
				REQUIRE(subpopulation[i].FitnessValue() != worst.FitnessValue());
		}
	}
	WHEN("Building and running a subcomplex") {
		auto discreteRng = CreateTrapezoidalRng(scores.size(), rng.CreateNewStd());
		SubComplex<T> scplx
			(scores, &evaluator, q, alpha, rng, &unif, discreteRng,
				fitnessAssignment);
		THEN("The subcomplex evolution completes without exception")
		{
			// Calling Evolve works without exceptions.
			scplx.Evolve();
			AND_THEN("Retrieving the final population does return a vector of expected size = q")
			{
				std::vector<IObjectiveScores<T>> finalPop;
				REQUIRE_NOTHROW(finalPop = scplx.WholePopulation());
				REQUIRE(finalPop.size() == m);
				// TODO: further tests.
			}
		}
	}
	WHEN("Building and running a complex") 
	{
		auto unif = createTestUnifrand<T>(421);
		ITerminationCondition<HyperCube < double > > terminationCondition;

		Complex<T> cplx
			(scores, m, q, alpha, beta,
				&evaluator, rng, &unif,
				fitnessAssignment, &terminationCondition);
		THEN("The complex evolution completes without exception")
		{
			REQUIRE_NOTHROW(cplx.Evolve());
			// TODO: further tests.
		}
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
	bool IsFinished(IEvolutionEngine<T>* engine)
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

		CounterTestFinished<HyperCube<double>> c(100);
		ITerminationCondition<HyperCube < double > > terminationCondition(c.CreateNew(c));

		ShuffledComplexEvolution<HyperCube<double>> opt(evaluator, populationInitializer, &terminationCondition, sceParams);

		WHEN("") {
			auto results = opt.Evolve();
			REQUIRE(results.size() > 0);
			results.PrintTo(std::cout);
			auto first = results[0];
			REQUIRE(first.ObjectiveCount() == 1);			
		}
	}
}
