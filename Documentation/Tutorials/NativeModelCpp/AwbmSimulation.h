#pragma once

#include <string>
#include <vector>
#include <map>
#include "AWBM.h"


class VariablePtr
{
public:
	VariablePtr(double * modelVariable, const std::vector<double>& data) { this->modelVariable = modelVariable; this->data = data; }
	VariablePtr(double * modelVariable) { this->modelVariable = modelVariable; }
	void InitData(int length) {
		if (data.size() != length)
			data.resize(length);
	}
	void Record(int index) { data[index] = *modelVariable; }
	void Play(int index) { *modelVariable = data[index]; }
	~VariablePtr() {}
	VariablePtr() {}
	// Note: the following should be private...
	double * modelVariable = nullptr;
	std::vector<double> data;
};

class AwbmSimulation
{
public:
	AwbmSimulation();
	AwbmSimulation(const AwbmSimulation& src);
	~AwbmSimulation();

	void Execute();
	std::vector<double> GetRecorded(const std::string& variableIdentifier);
	void SetSpan(int from, int to);
	void Play(const std::string& variableIdentifier, const std::vector<double>& values);
	void Record(const std::string& variableIdentifier);
	void SetVariable(const std::string& variableIdentifier, double value);
	double GetVariable(const std::string& variableIdentifier);
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

