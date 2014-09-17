#include "AwbmSimulation.h"


AwbmSimulation::AwbmSimulation()
{
	model.Reset();
}


AwbmSimulation::~AwbmSimulation()
{
	// Clean output buffers
	for (auto& x : outputs) {
		x.second->DisposeData();
	}
}

void AwbmSimulation::Execute()
{
	initOutputs();
	for (int i = fromIndex; i < toIndex; i++)
	{
		setInputs(i-fromIndex);
		model.RunOneTimeStep();
		getStates(i - fromIndex);
	}
}
double * AwbmSimulation::GetRecorded(std::string& variableIdentifier)
{
	return this->outputs[variableIdentifier]->data;
}

void   AwbmSimulation::SetSpan(int from, int to)
{
	fromIndex = from;
	toIndex = to;
}

void   AwbmSimulation::Play(std::string& variableIdentifier, double * values)
{
	// TODO: in prod system you'd check pre-existing key
	inputs[variableIdentifier] = new VariablePtr(model.GetPtr(variableIdentifier), values);
}

void   AwbmSimulation::Record(std::string& variableIdentifier)
{
	// TODO: in prod system you'd check pre-existing key
	outputs[variableIdentifier] = new VariablePtr(model.GetPtr(variableIdentifier), new double[]);;
}

void   AwbmSimulation::SetVariable(std::string& variableIdentifier, double value)
{
	model.SetVariable(variableIdentifier, value);
}

double AwbmSimulation::GetVariable(std::string& variableIdentifier)
{
	return model.GetVariable(variableIdentifier);
}

int	   AwbmSimulation::GetStart()
{
	return fromIndex;
}

int	   AwbmSimulation::GetEnd()
{
	return toIndex;
}

void AwbmSimulation::initOutputs()
{
	for (auto& x : outputs) {
		x.second->InitData(this->NumSteps());
	}
}
void AwbmSimulation::setInputs(int index)
{
	for (auto& x : inputs) {
		x.second->Play(index);
	}
}
void AwbmSimulation::getStates(int index)
{
	for (auto& x : outputs) {
		x.second->Record(index);
	}
}
