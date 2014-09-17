#pragma once

#include "common.h"
#include "AwbmSimulation.h"

#define NATIVE_AWBM_EXPORTS // see http://msdn.microsoft.com/en-us/library/as6wyhwt.aspx, best practice
#ifdef NATIVE_AWBM_EXPORTS
#define NATIVE_AWBM_API DLL_LIB
#else
#define NATIVE_AWBM_API __declspec(dllimport)
#endif

#ifdef __cplusplus
extern "C" {
#endif

	NATIVE_AWBM_API void Execute(AwbmSimulation * modelSimulation);
	NATIVE_AWBM_API void GetRecorded(AwbmSimulation * modelSimulation, char * variableIdentifier, double * values, int arrayLength);
	NATIVE_AWBM_API void SetSpan(AwbmSimulation * modelSimulation, int from, int to);
	NATIVE_AWBM_API void Play(AwbmSimulation * modelSimulation, char * variableIdentifier, double * values, int arrayLength);
	NATIVE_AWBM_API void Record(AwbmSimulation * modelSimulation, char * variableIdentifier);
	NATIVE_AWBM_API void SetVariable(AwbmSimulation * modelSimulation, char * variableIdentifier, double value);
	NATIVE_AWBM_API double GetVariable(AwbmSimulation * modelSimulation, char * variableIdentifier);
	NATIVE_AWBM_API int GetStart(AwbmSimulation * modelSimulation);
	NATIVE_AWBM_API int GetEnd(AwbmSimulation * modelSimulation);
	NATIVE_AWBM_API AwbmSimulation * CreateSimulation();
	NATIVE_AWBM_API void Dispose(AwbmSimulation * modelSimulation);

#ifdef __cplusplus
}
#endif
