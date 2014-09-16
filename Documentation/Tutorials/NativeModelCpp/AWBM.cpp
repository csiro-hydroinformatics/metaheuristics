#include "AWBM.h"


AWBM::AWBM()
{
	partialAreas[0]= 0.134;
	partialAreas[1]= 0.433;
	partialAreas[2]= 0.433;
	capacities[0]= 7;
	capacities[1]= 70;
	capacities[2]= 150;
	Store[0] = 0;
	Store[1] = 0;
	Store[2] = 0;
	PartialExcess[0] = 0;
	PartialExcess[1] = 0;
	PartialExcess[2] = 0;
	
	BFI = 0.35;
	KBase = 0.95;
	KSurf = 0.35;
}


AWBM::~AWBM()
{
}

// Parameters


void AWBM::RunOneTimeStep()
{
	EffectiveRainfall = std::max(0.0, (Rainfall - Evapotranspiration));

	Excess = 0.0;
	for (int i = 0; i < 3; i++)
	{
		double prevS = Store[i];
		PartialExcess[i] = 0.0;
		Store[i] += (Rainfall - Evapotranspiration);
		if (Store[i] >= capacities[i])
		{
			PartialExcess[i] = ((Store[i] - capacities[i]) * partialAreas[i]);
			Store[i] = capacities[i];
			Excess += PartialExcess[i];
		}
		if (Store[i] < 0.0)
		{
			Store[i] = 0.0;
		}
	}

	BaseflowRecharge = BFI * Excess;
	SurfaceRunoff = (1 - BFI) * Excess;

	BaseflowStore += BaseflowRecharge;
	SurfaceStore += SurfaceRunoff;

	RoutedSurfaceRunoff = (1 - KSurf) * SurfaceStore;
	Baseflow = (1 - KBase) * BaseflowStore;

	BaseflowStore *= (KBase);
	SurfaceStore *= (KSurf);

	Runoff = Baseflow + RoutedSurfaceRunoff;

}


void AWBM::Reset()
{
	// inputs
	Rainfall = 0;
	Evapotranspiration = 0;
	// Outputs
	Runoff = 0;
	Baseflow = 0;
	// states
	EffectiveRainfall = 0;
	Excess = 0;
	BaseflowRecharge = 0;
	SurfaceRunoff = 0;
	BaseflowStore = 0;
	SurfaceStore = 0;
	RoutedSurfaceRunoff = 0;
	for (int i = 0; i < 3; i++)
	{
		Store[i] = 0;
		PartialExcess[i] = 0;
	}
}

