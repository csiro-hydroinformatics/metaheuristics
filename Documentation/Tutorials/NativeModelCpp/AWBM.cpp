#include "AWBM.h"


AWBM::AWBM()
{
	partialAreas[0] = 0.134;
	partialAreas[1] = 0.433;
	partialAreas[2] = 0.433;
	capacities[0] = 7;
	capacities[1] = 70;
	capacities[2] = 150;
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

double * AWBM::GetPtr(const std::string& variableIdentifier)
{
	if (variableIdentifier == "Rainfall") return &Rainfall;
	else if (variableIdentifier == "Evapotranspiration") return &Evapotranspiration;
	else if (variableIdentifier == "Runoff") return &Runoff;
	else if (variableIdentifier == "Baseflow") return &Baseflow;
	else if (variableIdentifier == "BFI") return &BFI;
	else if (variableIdentifier == "KSurf") return &KSurf;
	else if (variableIdentifier == "KBase") return &KBase;
	else throw "Unknown or unsupported model variable";
}

void   AWBM::SetVariable(const std::string& variableIdentifier, double value)
{
	if (variableIdentifier == "Rainfall") Rainfall = value;
	else if (variableIdentifier == "Evapotranspiration") Evapotranspiration = value;
	else if (variableIdentifier == "Runoff") Runoff = value;
	else if (variableIdentifier == "Baseflow") Baseflow = value;
	else if (variableIdentifier == "BFI") BFI = value;
	else if (variableIdentifier == "KSurf") KSurf = value;
	else if (variableIdentifier == "KBase") KBase = value;
	else if (variableIdentifier == "C1") SetC1(value);
	else if (variableIdentifier == "C2") SetC2(value);
	else if (variableIdentifier == "C3") SetC3(value);
	else throw "Unknown model variable";

}

double AWBM::GetVariable(const std::string& variableIdentifier)
{
	if (variableIdentifier == "Rainfall") return Rainfall;
	else if (variableIdentifier == "Evapotranspiration") return Evapotranspiration;
	else if (variableIdentifier == "Runoff") return Runoff;
	else if (variableIdentifier == "Baseflow") return Baseflow;
	else if (variableIdentifier == "BFI") return BFI;
	else if (variableIdentifier == "KSurf") return KSurf;
	else if (variableIdentifier == "KBase") return KBase;
	else if (variableIdentifier == "C1") return GetC1();
	else if (variableIdentifier == "C2") return GetC2();
	else if (variableIdentifier == "C3") return GetC3();
	else throw "Unknown model variable";

}


