#pragma once

#include <algorithm>

class AWBM
{
public:
	AWBM();
	~AWBM();

	// Inputs
	double Rainfall, Evapotranspiration;
	// Primary outputs
	double Runoff, Baseflow;

	void RunOneTimeStep();
	void Reset();

	double BFI;
	double KSurf;
	double KBase;
	double GetC1() { return capacities[0]; }
	double GetC2() { return capacities[1]; }
	double GetC3() { return capacities[2]; }

	void SetC1(double value) { capacities[0] = value; }
	void SetC2(double value) { capacities[1] = value; }
	void SetC3(double value) { capacities[2] = value; }

private:
	// State values, can be considered as outputs depending on the modelling objective.
	double EffectiveRainfall;
	double Excess;
	double Store[3], PartialExcess[3], capacities[3], partialAreas[3];
	double BaseflowRecharge;
	double SurfaceRunoff;
	double BaseflowStore;
	double SurfaceStore;
	double RoutedSurfaceRunoff;


};

