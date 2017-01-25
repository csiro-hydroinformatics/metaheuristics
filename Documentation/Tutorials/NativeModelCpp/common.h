#pragma once

#ifdef _WIN32
//#ifdef BUILDING_DLL_LIB
#define DLL_LIB __declspec(dllexport)
//#else
//  #define DLL_LIB __declspec(dllimport)
//#endif
#else
#define DLL_LIB // nothing
#endif

