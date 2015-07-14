#define CATCH_CONFIG_MAIN  // This tells Catch to provide a main() - only do this in one cpp file
#include "catch.hpp"
#include "../mhcpp/core.h"

using namespace mhcpp;

SCENARIO("basic hypercubes", "[sysconfig]") {

	GIVEN("A 2 dimensional hypercube (Yeah, square...)")
	{
		HyperCube<double> hc;
		hc.Define("a", 1, 2, 1.5);
		hc.Define("b", 3, 4, 3.3);

		WHEN("Read values after initial definition") {
			REQUIRE(hc.Dimensions() == 2);
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
