#define REGISTRY_PATH "SOFTWARE\\COGAPLEX\\LICENSEKEY"
#define FILE_NAME "\\licenseKey.txt"
#define INTRUSION_NAME "\\cplkv.txt"

#include <iostream>
#include <fstream>
#include <iomanip>
#include <comdef.h>
#include <WbemIdl.h>
#include <Windows.h>
#include <assert.h>
#include <iphlpapi.h>
#include <string>
#include <winreg.h>
#include <Shlwapi.h>
#include <shlobj_core.h>
#include <time.h>

#include "sha.h"
#include "filters.h"
#include "base64.h"

#ifdef _DEBUG
	#pragma comment(lib, "cryptlibD.lib")
	bool LicenseKeyGenerator();
	bool LicenseValidator();
#else
	#pragma comment(lib, "cryptlibR.lib")
    #define LICENSE_DLL_EXPORT
    #include "License.h"
#endif



#pragma comment(lib, "wbemuuid.lib")
#pragma comment(lib, "shlwapi")

static char ChBuff[1024];
static char DefChar = ' ';

std::string MBoardWMI();
std::string USBWMI();
std::string MACWMI();
bool CreateLicenseKey();
bool WriteToRegistry(std::string);
std::string ReadFromRegistry();
std::string SHA256HashString(std::string);
bool WriteToFile(std::string);
std::string ReadFromFile();

std::string CreatePath(std::string);

int main() 
{
	if (LicenseKeyGenerator())
	{
		std::cout << "Generated" << std::endl;
	}
	if (!LicenseValidator())
	{
		std::cout << "Invalid License" << std::endl;
	}
	std::cout << "Validation Complete." << std::endl;

	std::cout << "PATH file:  " << CreatePath(FILE_NAME) << std::endl;
	std::cout << "PATH val:  " << CreatePath(INTRUSION_NAME) << std::endl;
	system("pause");
	return 0;

}

bool LicenseKeyGenerator() 
{
	std::string intrusion = CreatePath(INTRUSION_NAME);
	std::string lkeyfile = CreatePath(FILE_NAME);
	if (PathFileExists(intrusion.c_str()))
	{
		remove(intrusion.c_str());
	}

	if (PathFileExists(lkeyfile.c_str()))
	{
		remove(lkeyfile.c_str());
	}

	std::string licenseKey = CreateLicenseKey();
	return WriteToRegistry(licenseKey) && WriteToFile(licenseKey);
}

bool LicenseValidator() 
{
	std::string regi = ReadFromRegistry();
	std::string file = ReadFromFile();
	std::string sha = CreateLicenseKey();

	std::string intrusion = CreatePath(INTRUSION_NAME); 
	std::string lkeyfile = CreatePath(FILE_NAME);

	if (!regi.empty() && !file.empty() && !sha.empty())
	{
		if (!PathFileExists(intrusion.c_str()) && regi == file && regi == sha)
		{
			return true;
		}
	}

	std::string detector = "More at cogaplex@cogaplex.com";
	std::ofstream LicenseDetector;
	LicenseDetector.open(intrusion);
	SetFileAttributes(intrusion.c_str(), FILE_ATTRIBUTE_HIDDEN);
	LicenseDetector.write(detector.c_str(), detector.size());
	LicenseDetector.close();
	remove(lkeyfile.c_str());
	SHDeleteKey(HKEY_LOCAL_MACHINE, REGISTRY_PATH);
	
	return false;
}

std::string CreateLicenseKey() 
{
	std::string mac = MACWMI();
	std::string usb = USBWMI();
	std::string mBoard = MBoardWMI();
	if (!mBoard.empty() && !usb.empty() && !mac.empty())
	{
		std::string toBeEncrypted = SHA256HashString(mBoard) + usb;
		toBeEncrypted = SHA256HashString(toBeEncrypted) + mac;
		return SHA256HashString(toBeEncrypted);
	}
	return "";
}

bool GetWMI(std::string& ) 
{
	HRESULT hres;
	time_t startCoInEx, endCoInEx;
	double resultCoInEx;
	startCoInEx = clock();
	hres = CoInitializeEx(0, COINIT_MULTITHREADED);
	if (FAILED(hres))
	{
		return "";
	}
	endCoInEx = clock();
	resultCoInEx = (double)(endCoInEx - startCoInEx);
	std::cout << "mb result CoInitializeEx : " << resultCoInEx << std::endl;

	time_t startCoInSe, endCoInSe;
	double resultCoInSe;
	startCoInSe = clock();
	hres = CoInitializeSecurity(
		NULL,
		-1,
		NULL,
		NULL,
		RPC_C_AUTHN_LEVEL_DEFAULT,
		RPC_C_IMP_LEVEL_IMPERSONATE,
		NULL,
		EOAC_NONE,
		NULL
	);

	if (FAILED(hres))
	{
		CoUninitialize();
		return "";
	}
	endCoInSe = clock();
	resultCoInSe = (double)(endCoInSe - startCoInSe);
	std::cout << "mb result CoInitializeSecurity : " << resultCoInSe << std::endl;

	IWbemLocator* pLoc = NULL;

	time_t startCoCreIn, endCoCreIn;
	double resultCoCreIn;
	startCoCreIn = clock();
	hres = CoCreateInstance(
		CLSID_WbemLocator,
		0,
		CLSCTX_INPROC_SERVER,
		IID_IWbemLocator, (LPVOID*)&pLoc);

	if (FAILED(hres))
	{
		CoUninitialize();
		return "";
	}
	endCoCreIn = clock();
	resultCoCreIn = (double)(endCoCreIn - startCoCreIn);
	std::cout << "mb result CoCreateInstance : " << resultCoCreIn << std::endl;
	IWbemServices* pSvc = NULL;
	
	time_t startConnSer, endConnSer;
	double resultConnSer;
	startConnSer = clock();
	hres = pLoc->ConnectServer(
		_bstr_t(L"ROOT\\CIMV2"),
		NULL,
		NULL,
		0,
		NULL,
		0,
		0,
		&pSvc
	);

	if (FAILED(hres))
	{
		pLoc->Release();
		CoUninitialize();
		return "";
	}
	endConnSer = clock();
	resultConnSer = (double)(endConnSer - startConnSer);
	std::cout << "mb result ConnectServer : " << resultConnSer << std::endl;

	time_t startCoSetProxy, endCoSetProxy;
	double resultCoSetProxy;
	startCoSetProxy = clock();
	hres = CoSetProxyBlanket(
		pSvc,
		RPC_C_AUTHN_WINNT,
		RPC_C_AUTHZ_NONE,
		NULL,
		RPC_C_AUTHN_LEVEL_CALL,
		RPC_C_IMP_LEVEL_IMPERSONATE,
		NULL,
		EOAC_NONE
	);

	if (FAILED(hres))
	{
		pSvc->Release();
		pLoc->Release();
		CoUninitialize();
		return "";
	}
	endCoSetProxy = clock();
	resultCoSetProxy = (double)(endCoSetProxy - startCoSetProxy);
	std::cout << "mb result CoSetProxyBlanket : " << resultCoSetProxy << std::endl;

	IEnumWbemClassObject* pEnumerator = NULL;

	time_t startQuery, endQuery;
	double resultQuery;
	startQuery = clock();
	hres = pSvc->ExecQuery(
		bstr_t("WQL"),
		bstr_t("SELECT * FROM Win32_baseboard"),
		WBEM_FLAG_FORWARD_ONLY | WBEM_FLAG_RETURN_IMMEDIATELY,
		NULL,
		&pEnumerator);

	endQuery = clock();
	resultQuery = (double)(endQuery - startQuery);
	std::cout << "mb result ExecQuery : " << resultQuery << std::endl;

	if (FAILED(hres))
	{
		pSvc->Release();
		pLoc->Release();
		CoUninitialize();
		return "";
	}

	IWbemClassObject* pclsObj = NULL;
	ULONG uReturn = 0;
	std::string IDs;

	time_t startEnum, endEnum;
	double resultEnum;
	startEnum = clock();
	while (pEnumerator)
	{
		HRESULT hr = pEnumerator->Next(WBEM_INFINITE, 1,
			&pclsObj, &uReturn);

		if (0 == uReturn)
		{
			break;
		}

		VARIANT vtProp;
		VariantInit(&vtProp);
		hr = pclsObj->Get(L"SerialNumber", 0, &vtProp, 0, 0);
		VariantClear(&vtProp);
		
		WideCharToMultiByte(CP_ACP, 0, vtProp.bstrVal, -1, ch, 30, &DefChar, NULL);

		IDs.append(ch);
		pclsObj->Release();
	}
	endEnum = clock();
	resultEnum = (double)(endEnum - startEnum);
	std::cout << "mb result pEnumerator : " << resultEnum << std::endl;
	pSvc->Release();
	pLoc->Release();
	pEnumerator->Release();
	CoUninitialize();

	return IDs;
}

std::string USBWMI() 
{
	HRESULT hres;
	time_t startCoInEx, endCoInEx;
	double resultCoInEx;
	startCoInEx = clock();
	hres = CoInitializeEx(0, COINIT_MULTITHREADED);
	if (FAILED(hres))
	{
		return "";
	}
	endCoInEx = clock();
	resultCoInEx = (double)(endCoInEx - startCoInEx);
	std::cout << "gpu result CoInitializeEx : " << resultCoInEx << std::endl;

	time_t startCoInSe, endCoInSe;
	double resultCoInSe;
	startCoInSe = clock();
	hres = CoInitializeSecurity(
		NULL,
		-1,
		NULL,
		NULL,
		RPC_C_AUTHN_LEVEL_DEFAULT,
		RPC_C_IMP_LEVEL_IMPERSONATE,
		NULL,
		EOAC_NONE,
		NULL
	);

	if (FAILED(hres))
	{
		CoUninitialize();
		return "";
	}
	endCoInSe = clock();
	resultCoInSe = (double)(endCoInSe - startCoInSe);
	std::cout << "gpu result CoInitializeSecurity : " << resultCoInSe << std::endl;

	IWbemLocator* pLoc = NULL;

	time_t startCoCreIn, endCoCreIn;
	double resultCoCreIn;
	startCoCreIn = clock();
	hres = CoCreateInstance(
		CLSID_WbemLocator,
		0,
		CLSCTX_INPROC_SERVER,
		IID_IWbemLocator, (LPVOID*)&pLoc);

	if (FAILED(hres))
	{
		CoUninitialize();
		return "";
	}
	endCoCreIn = clock();
	resultCoCreIn = (double)(endCoCreIn - startCoCreIn);
	std::cout << "gpu result CoCreateInstance : " << resultCoCreIn << std::endl;
	IWbemServices* pSvc = NULL;

	time_t startConnSer, endConnSer;
	double resultConnSer;
	startConnSer = clock();
	hres = pLoc->ConnectServer(
		_bstr_t(L"ROOT\\CIMV2"),
		NULL,
		NULL,
		0,
		NULL,
		0,
		0,
		&pSvc
	);

	if (FAILED(hres))
	{
		pLoc->Release();
		CoUninitialize();
		return "";
	}
	endConnSer = clock();
	resultConnSer = (double)(endConnSer - startConnSer);
	std::cout << "gpu result ConnectServer : " << resultConnSer << std::endl;

	time_t startCoSetProxy, endCoSetProxy;
	double resultCoSetProxy;
	startCoSetProxy = clock();
	hres = CoSetProxyBlanket(
		pSvc,
		RPC_C_AUTHN_WINNT,
		RPC_C_AUTHZ_NONE,
		NULL,
		RPC_C_AUTHN_LEVEL_CALL,
		RPC_C_IMP_LEVEL_IMPERSONATE,
		NULL,
		EOAC_NONE
	);

	if (FAILED(hres))
	{
		pSvc->Release();
		pLoc->Release();
		CoUninitialize();
		return "";
	}
	endCoSetProxy = clock();
	resultCoSetProxy = (double)(endCoSetProxy - startCoSetProxy);
	std::cout << "gpu result CoSetProxyBlanket : " << resultCoSetProxy << std::endl;

	IEnumWbemClassObject* pEnumerator = NULL;

	time_t startQuery, endQuery;
	double resultQuery;
	startQuery = clock();
	hres = pSvc->ExecQuery(
		bstr_t("WQL"),
		bstr_t("SELECT * FROM Win32_baseboard"),
		WBEM_FLAG_FORWARD_ONLY | WBEM_FLAG_RETURN_IMMEDIATELY,
		NULL,
		&pEnumerator);
	

	endQuery = clock();
	resultQuery = (double)(endQuery - startQuery);
	std::cout << "gpu result ExecQuery : " << resultQuery << std::endl;

	if (FAILED(hres))
	{
		pSvc->Release();
		pLoc->Release();
		CoUninitialize();
		return "";
	}

	IWbemClassObject* pclsObj = NULL;
	ULONG uReturn = 0;
	std::string IDs;

	time_t startEnum, endEnum;
	double resultEnum;
	startEnum = clock();
	while (pEnumerator)
	{
		HRESULT hr = pEnumerator->Next(WBEM_INFINITE, 1,
			&pclsObj, &uReturn);

		if (0 == uReturn)
		{
			break;
		}

		VARIANT vtProp;
		VariantInit(&vtProp);
		hr = pclsObj->Get(L"SerialNumber", 0, &vtProp, 0, 0);
		VariantClear(&vtProp);
		char ch[30];
		char DefChar = ' ';
		WideCharToMultiByte(CP_ACP, 0, vtProp.bstrVal, -1, ch, 30, &DefChar, NULL);

		IDs.append(ch);
		pclsObj->Release();
	}

	hres = pSvc->ExecQuery(
		bstr_t("WQL"),
		bstr_t("SELECT * FROM Win32_videocontroller"),
		WBEM_FLAG_FORWARD_ONLY | WBEM_FLAG_RETURN_IMMEDIATELY,
		NULL,
		&pEnumerator);




	if (FAILED(hres))
	{
		pSvc->Release();
		pLoc->Release();
		CoUninitialize();
		return "";
	}

	while (pEnumerator)
	{
		HRESULT hr = pEnumerator->Next(WBEM_INFINITE, 1,
			&pclsObj, &uReturn);

		if (0 == uReturn)
		{
			break;
		}

		VARIANT vtProp;
		VariantInit(&vtProp);
		hr = pclsObj->Get(L"PNPDeviceID", 0, &vtProp, 0, 0);
		VariantClear(&vtProp);
		char ch[30];
		char DefChar = ' ';
		WideCharToMultiByte(CP_ACP, 0, vtProp.bstrVal, -1, ch, 30, &DefChar, NULL);

		IDs.append(ch);
		pclsObj->Release();
	}

	hres = pSvc->ExecQuery(
		bstr_t("WQL"),
		bstr_t("SELECT * FROM Win32_NetworkAdapterConfiguration where IPEnabled ='TRUE'"),
		WBEM_FLAG_FORWARD_ONLY | WBEM_FLAG_RETURN_IMMEDIATELY,
		NULL,
		&pEnumerator);




	endQuery = clock();
	resultQuery = (double)(endQuery - startQuery);
	std::cout << "gpu result ExecQuery : " << resultQuery << std::endl;

	if (FAILED(hres))
	{
		pSvc->Release();
		pLoc->Release();
		CoUninitialize();
		return "";
	}

	while (pEnumerator)
	{
		HRESULT hr = pEnumerator->Next(WBEM_INFINITE, 1,
			&pclsObj, &uReturn);

		if (0 == uReturn)
		{
			break;
		}

		VARIANT vtProp;
		VariantInit(&vtProp);
		hr = pclsObj->Get(L"MACAddress", 0, &vtProp, 0, 0);
		VariantClear(&vtProp);
		char ch[30];
		char DefChar = ' ';
		WideCharToMultiByte(CP_ACP, 0, vtProp.bstrVal, -1, ch, 30, &DefChar, NULL);

		IDs.append(ch);
		pclsObj->Release();
	}

	pSvc->Release();
	pLoc->Release();
	pEnumerator->Release();
	CoUninitialize();

	return IDs;
}

std::string MACWMI() 
{
	HRESULT hres;
	time_t startCoInEx, endCoInEx;
	double resultCoInEx;
	startCoInEx = clock();
	hres = CoInitializeEx(0, COINIT_MULTITHREADED);
	if (FAILED(hres))
	{
		return "";
	}
	endCoInEx = clock();
	resultCoInEx = (double)(endCoInEx - startCoInEx);
	std::cout << "mac result CoInitializeEx : " << resultCoInEx << std::endl;

	time_t startCoInSe, endCoInSe;
	double resultCoInSe;
	startCoInSe = clock();
	hres = CoInitializeSecurity(
		NULL,
		-1,
		NULL,
		NULL,
		RPC_C_AUTHN_LEVEL_DEFAULT,
		RPC_C_IMP_LEVEL_IMPERSONATE,
		NULL,
		EOAC_NONE,
		NULL
	);

	if (FAILED(hres))
	{
		CoUninitialize();
		return "";
	}
	endCoInSe = clock();
	resultCoInSe = (double)(endCoInSe - startCoInSe);
	std::cout << "mac result CoInitializeSecurity : " << resultCoInSe << std::endl;

	IWbemLocator* pLoc = NULL;

	time_t startCoCreIn, endCoCreIn;
	double resultCoCreIn;
	startCoCreIn = clock();
	hres = CoCreateInstance(
		CLSID_WbemLocator,
		0,
		CLSCTX_INPROC_SERVER,
		IID_IWbemLocator, (LPVOID*)&pLoc);

	if (FAILED(hres))
	{
		CoUninitialize();
		return "";
	}
	endCoCreIn = clock();
	resultCoCreIn = (double)(endCoCreIn - startCoCreIn);
	std::cout << "mac result CoCreateInstance : " << resultCoCreIn << std::endl;
	IWbemServices* pSvc = NULL;

	time_t startConnSer, endConnSer;
	double resultConnSer;
	startConnSer = clock();
	hres = pLoc->ConnectServer(
		_bstr_t(L"ROOT\\CIMV2"),
		NULL,
		NULL,
		0,
		NULL,
		0,
		0,
		&pSvc
	);

	if (FAILED(hres))
	{
		pLoc->Release();
		CoUninitialize();
		return "";
	}
	endConnSer = clock();
	resultConnSer = (double)(endConnSer - startConnSer);
	std::cout << "mac result ConnectServer : " << resultConnSer << std::endl;

	time_t startCoSetProxy, endCoSetProxy;
	double resultCoSetProxy;
	startCoSetProxy = clock();
	hres = CoSetProxyBlanket(
		pSvc,
		RPC_C_AUTHN_WINNT,
		RPC_C_AUTHZ_NONE,
		NULL,
		RPC_C_AUTHN_LEVEL_CALL,
		RPC_C_IMP_LEVEL_IMPERSONATE,
		NULL,
		EOAC_NONE
	);

	if (FAILED(hres))
	{
		pSvc->Release();
		pLoc->Release();
		CoUninitialize();
		return "";
	}
	endCoSetProxy = clock();
	resultCoSetProxy = (double)(endCoSetProxy - startCoSetProxy);
	std::cout << "mac result CoSetProxyBlanket : " << resultCoSetProxy << std::endl;

	IEnumWbemClassObject* pEnumerator = NULL;

	time_t startQuery, endQuery;
	double resultQuery;
	startQuery = clock();
	hres = pSvc->ExecQuery(
		bstr_t("WQL"),
		bstr_t("SELECT * FROM Win32_NetworkAdapterConfiguration where IPEnabled ='TRUE'"),
		WBEM_FLAG_FORWARD_ONLY | WBEM_FLAG_RETURN_IMMEDIATELY,
		NULL,
		&pEnumerator);

	endQuery = clock();
	resultQuery = (double)(endQuery - startQuery);
	std::cout << "mac result ExecQuery : " << resultQuery << std::endl;

	if (FAILED(hres))
	{
		pSvc->Release();
		pLoc->Release();
		CoUninitialize();
		return "";
	}

	IWbemClassObject* pclsObj = NULL;
	ULONG uReturn = 0;
	std::string IDs;

	time_t startEnum, endEnum;
	double resultEnum;
	startEnum = clock();
	while (pEnumerator)
	{
		HRESULT hr = pEnumerator->Next(WBEM_INFINITE, 1,
			&pclsObj, &uReturn);

		if (0 == uReturn)
		{
			break;
		}

		VARIANT vtProp;
		VariantInit(&vtProp);
		hr = pclsObj->Get(L"MACAddress", 0, &vtProp, 0, 0);
		VariantClear(&vtProp);
		char ch[30];
		char DefChar = ' ';
		WideCharToMultiByte(CP_ACP, 0, vtProp.bstrVal, -1, ch, 30, &DefChar, NULL);

		IDs.append(ch);
		pclsObj->Release();
	}
	endEnum = clock();
	resultEnum = (double)(endEnum - startEnum);
	std::cout << "mac result pEnumerator : " << resultEnum << std::endl;
	pSvc->Release();
	pLoc->Release();
	pEnumerator->Release();
	CoUninitialize();

	return IDs;

}

bool WriteToRegistry(std::string value) 
{
	HKEY hKey;
	std::string path = REGISTRY_PATH;
	std::string key = "License Key";
	LPCTSTR pathName = (LPCTSTR)path.c_str();
	LPCTSTR keyName = (LPCTSTR)key.c_str();
	DWORD type = REG_SZ;
	DWORD disposition = 0;
	if (!value.empty())
	{
		if (RegCreateKeyEx(HKEY_LOCAL_MACHINE, pathName, 0, NULL, REG_OPTION_NON_VOLATILE, KEY_ALL_ACCESS, NULL, &hKey, &disposition) == ERROR_SUCCESS)
		{
			if (disposition == REG_CREATED_NEW_KEY)
			{
				RegOpenKey(HKEY_LOCAL_MACHINE, pathName, &hKey);
				RegSetValueEx(hKey, keyName, 0, type, (LPBYTE)(value.c_str()), (DWORD)(strlen(value.c_str()) * sizeof(char)));
			}
			else
			{
				RegSetValueEx(hKey, keyName, 0, type, (LPBYTE)(value.c_str()), (DWORD)(strlen(value.c_str()) * sizeof(char)));
			}
			RegCloseKey(hKey);
			return true;
		}
	}
	return false;
}

std::string ReadFromRegistry() 
{
	HKEY hKey;
	TCHAR value[255];
	DWORD value_length = 255;
	LPCTSTR pathName, keyName;
	std::string path = REGISTRY_PATH;
	std::string key = "License Key";
	DWORD type = REG_SZ;
	pathName = (LPCTSTR)path.c_str();
	keyName = (LPCTSTR)key.c_str();
	if (RegOpenKeyEx(HKEY_LOCAL_MACHINE, pathName, 0, KEY_READ | KEY_WOW64_64KEY, &hKey) == ERROR_SUCCESS)
	{
		if (RegQueryValueEx(hKey, keyName, NULL, &type, (LPBYTE)value, &value_length) != ERROR_SUCCESS)
		{
			return "";
		}
		std::string licenseKey(value);
		RegCloseKey(hKey);
		return licenseKey;
	}
	return "";
}

std::string SHA256HashString(std::string value) 
{
    std::string digest;
    CryptoPP::SHA256 hash;

    CryptoPP::StringSource foo(value, true,
        new CryptoPP::HashFilter(hash,
            new CryptoPP::Base64Encoder(
                new CryptoPP::StringSink(digest))));
	digest.erase(std::find_if(digest.rbegin(), digest.rend(), std::not1(std::ptr_fun<int, int>(std::isspace))).base(), digest.end());
    return digest;
}

bool WriteToFile(std::string liKey) 
{
	std::ofstream writeFile;

	writeFile.open(CreatePath(FILE_NAME));
	SetFileAttributes(CreatePath(FILE_NAME).c_str(), FILE_ATTRIBUTE_HIDDEN);
	if (liKey.empty())
	{
		writeFile.close();
		return false;
	}
	writeFile.write(liKey.c_str(), liKey.size());
	writeFile.close();
	return true;
}

std::string ReadFromFile() 
{
	std::ifstream rfile(CreatePath(FILE_NAME));
	std::string licenseKey;
	if (rfile.is_open())
	{
		rfile >> licenseKey;
	}
	rfile.close();
	return licenseKey;
}

std::string CreatePath(std::string filename) 
{
	int i = 0;
	PWSTR path = NULL;
	WCHAR cp_path[100];
	std::string s = filename;
	SHGetKnownFolderPath(FOLDERID_LocalAppData, 0, NULL, &path); 
	while (path[i] != '\0') cp_path[i] = path[i++];
	for_each(s.begin(), s.end(), [&](char& c) { cp_path[i++] = c; });
	cp_path[i] = '\0';
	CoTaskMemFree(path);
	std::wstring valpath(cp_path);
	std::string fullpath(valpath.begin(), valpath.end());
	return fullpath;
}