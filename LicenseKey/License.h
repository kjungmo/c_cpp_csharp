#pragma once
#ifdef LICENSE_DLL_EXPORT
  #define DLLAPI extern "C" __declspec(dllexport)
#else
  #define DLLAPI extern "C" __declspec(dllimport)  
#endif

DLLAPI bool LicenseKeyGenerator();
DLLAPI bool LicenseValidator();