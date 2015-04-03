#include "AwbmSimulation.h"


AwbmSimulation::AwbmSimulation()
{
	model.Reset();
}

AwbmSimulation::AwbmSimulation(const AwbmSimulation& src)
{
	for (auto& x : src.outputs) {
		outputs[x.first] = new VariablePtr(model.GetPtr(x.first));;
	}
	for (auto& x : src.inputs) {
		inputs[x.first] = new VariablePtr(model.GetPtr(x.first), x.second->data);;
	}
	fromIndex = src.fromIndex;
	toIndex = src.toIndex;
	model.Reset();
}

AwbmSimulation::~AwbmSimulation()
{
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
std::vector<double> AwbmSimulation::GetRecorded(const std::string& variableIdentifier)
{
	return this->outputs[variableIdentifier]->data;
}

void AwbmSimulation::SetSpan(int from, int to)
{
	fromIndex = from;
	toIndex = to;
}

void   AwbmSimulation::Play(const std::string& variableIdentifier, const std::vector<double>& values)
{
	// TODO: in prod system you'd check pre-existing key
	inputs[variableIdentifier] = new VariablePtr(model.GetPtr(variableIdentifier), values);
}

void   AwbmSimulation::Record(const std::string& variableIdentifier)
{
	// TODO: in prod system you'd check pre-existing key
	outputs[variableIdentifier] = new VariablePtr(model.GetPtr(variableIdentifier));;
}

void   AwbmSimulation::SetVariable(const std::string& variableIdentifier, double value)
{
	model.SetVariable(variableIdentifier, value);
}

double AwbmSimulation::GetVariable(const std::string& variableIdentifier)
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
