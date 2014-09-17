#pragma once

#include <string>
#include <map>
#include "AWBM.h"


class VariablePtr
{
public:
	VariablePtr(double * modelVariable, double * data) { this->modelVariable = modelVariable; this->data = data; }
	void DisposeData() { if (data != nullptr) delete[] data; }
	void InitData(int length) { DisposeData(); data = new double[length]; }
	void Record(int index) { data[index] = *modelVariable; }
	void Play(int index) { *modelVariable = data[index]; }
	~VariablePtr() {}
	VariablePtr() {}
	double * modelVariable = nullptr;
	double * data = nullptr;
};

class AwbmSimulation
{
public:
	AwbmSimulation();
	~AwbmSimulation();

	void Execute();
	double * GetRecorded(std::string& variableIdentifier);
	void SetSpan(int from, int to);
	void Play(std::string& variableIdentifier, double * values);
	void Record(std::string& variableIdentifier);
	void SetVariable(std::string& variableIdentifier, double value);
	double GetVariable(std::string& variableIdentifier);
	int GetStart();
	int GetEnd();
	int NumSteps() { return GetEnd() - GetStart() + 1; }

private:
	AWBM model;
	std::map<std::string, VariablePtr*> inputs;
	std::map<std::string, VariablePtr*> outputs;
	int fromIndex, toIndex;

	void initOutputs();
	void setInputs(int index);
	void getStates(int index);
};

