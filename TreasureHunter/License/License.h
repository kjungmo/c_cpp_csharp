#pragma once
#ifdef LICENSE_DLL_EXPORT
  #define DLLAPI extern "C" __declspec(dllexport)
#else
  #define DLLAPI extern "C" __declspec(dllimport)  
#endif

DLLAPI const char* GenerateLicenseKey(bool initCOM);
DLLAPI bool ValidateLicenseKey(bool initCOM);