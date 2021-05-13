//#define _CRT_SECURE_NO_WARNINGS
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

#include "sha.h"
#include "filters.h"
#include "base64.h"

#pragma comment(lib, "wbemuuid.lib")

std::string SerialNumWMI(const char*);
std::string macWMI();
std::string HardwareIDs(const char*, const char*);
void writeRegistry(std::string, std::string, DWORD,std::string);
std::string readRegistry(std::string, std::string, DWORD);
std::string SHA256HashString(std::string);
void writeToFile(std::string);
std::string readFromFile(std::string);
void LicenseKeyGenerator(const char*, const char*, std::string, std::string);
void LicenseValidator(std::string, std::string);

int main()
{
	//std::cout << HardwareIDs("SELECT * FROM Win32_baseboard", "SELECT * FROM Win32_diskdrive") << std::endl;
	//std::cout << SHA256HashString(HardwareIDs("SELECT * FROM Win32_baseboard", "SELECT * FROM Win32_diskdrive")) << std::endl;
	//writeRegistry("SOFTWARE\\COGAPLEX\\LICENSEKEY", "License Key", REG_SZ, SHA256HashString(HardwareIDs("SELECT * FROM Win32_baseboard", "SELECT * FROM Win32_diskdrive")));
	//std::cout << "this is the value read : " << readRegistry("SOFTWARE\\COGAPLEX\\LICENSEKEY", "License Key", REG_SZ) << std::endl;
	//writeToFile("D:\\Github\\c_cpp_csharp\\LicenseKey\\text2.txt");
	//readFromFile("D:\\Github\\c_cpp_csharp\\LicenseKey\\text2.txt");
	//LicenseValidator("SOFTWARE\\COGAPLEX\\LICENSEKEY", "D:\\Github\\c_cpp_csharp\\LicenseKey\\text2.txt");
	LicenseKeyGenerator("SELECT * FROM Win32_baseboard", "SELECT * FROM Win32_diskdrive", "SOFTWARE\\COGAPLEX\\LICENSEKEY", "D:\\Github\\c_cpp_csharp\\LicenseKey\\text2.txt");
	LicenseValidator("SOFTWARE\\COGAPLEX\\LICENSEKEY", "D:\\Github\\c_cpp_csharp\\LicenseKey\\text2.txt");
	system("pause");
	return 0;
}

void LicenseKeyGenerator(const char* mbQuery, const char* diskQuery, std::string regiPath, std::string filePath)
{
	writeRegistry(regiPath, "License Key", REG_SZ, SHA256HashString(HardwareIDs(mbQuery, diskQuery)));
	writeToFile(filePath);
}

void LicenseValidator(std::string regiPath, std::string filePath)
{
	std::string regi, file;
	regi = readRegistry(regiPath, "License Key", REG_SZ);
	file = readFromFile(filePath);

	if (regi == file)
	{
		std::cout << "PASS" << std::endl;
	};

}

std::string HardwareIDs(const char* mbQuery, const char* diskQuery)
{
	std::string mboard = SerialNumWMI(mbQuery);
	std::string diskdrive = SerialNumWMI(diskQuery);
	std::string mac = macWMI();
	std::string toBeEncrypted = mboard + mac + diskdrive;
	return toBeEncrypted;
}

std::string SerialNumWMI(const char* wmiQuery)
{
	HRESULT hres;
	std::string failed = "failed";
	hres = CoInitializeEx(0, COINIT_MULTITHREADED);
	if (FAILED(hres))
	{
		std::cout << "Failed to initialize COM library. Error code = 0x" << std::hex << hres << std::endl;
		return "";
	}

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
		std::cout << "Failed to initialize security. Error code = 0x"
			<< std::hex << hres << std::endl;
		CoUninitialize();
		return "";
	}

	IWbemLocator* pLoc = NULL;

	hres = CoCreateInstance(
		CLSID_WbemLocator,
		0,
		CLSCTX_INPROC_SERVER,
		IID_IWbemLocator, (LPVOID*)&pLoc);

	if (FAILED(hres))
	{
		std::cout << "Failed to create IWbemLocator object. "
			<< "Err code = 0x"
			<< std::hex << hres << std::endl;
		CoUninitialize();
		return "";
	}

	IWbemServices* pSvc = NULL;

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
		std::cout << "Could not connect. Error code = 0x"
			<< std::hex << hres << std::endl;
		pLoc->Release();
		CoUninitialize();
		return "";
	}

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
		std::cout << "Could not set proxy blanket. Error code = 0x"
			<< std::hex << hres << std::endl;
		pSvc->Release();
		pLoc->Release();
		CoUninitialize();
		return "";
	}

	IEnumWbemClassObject* pEnumerator = NULL;

	hres = pSvc->ExecQuery(
		bstr_t("WQL"),
		bstr_t(wmiQuery), 
		WBEM_FLAG_FORWARD_ONLY | WBEM_FLAG_RETURN_IMMEDIATELY,
		NULL,
		&pEnumerator);

	if (FAILED(hres))
	{
		std::cout << "Query for operating system name failed."
			<< " Error code = 0x"
			<< std::hex << hres << std::endl;
		pSvc->Release();
		pLoc->Release();
		CoUninitialize();
		return "";
	}

	IWbemClassObject* pclsObj = NULL;
	ULONG uReturn = 0;
	std::string IDs;

	while (pEnumerator)
	{
		HRESULT hr = pEnumerator->Next(WBEM_INFINITE, 1,
			&pclsObj, &uReturn);

		if (0 == uReturn)
		{
			break;
		}

		VARIANT vtProp;

		hr = pclsObj->Get(L"SerialNumber", 0, &vtProp, 0, 0);
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

std::string macWMI()
{
	HRESULT hres;

	hres = CoInitializeEx(0, COINIT_MULTITHREADED);
	if (FAILED(hres))
	{
		std::cout << "Failed to initialize COM library. Error code = 0x" << std::hex << hres << std::endl;
		return "";
	}

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
		std::cout << "Failed to initialize security. Error code = 0x"
			<< std::hex << hres << std::endl;
		CoUninitialize();
		return "";
	}

	IWbemLocator* pLoc = NULL;

	hres = CoCreateInstance(
		CLSID_WbemLocator,
		0,
		CLSCTX_INPROC_SERVER,
		IID_IWbemLocator, (LPVOID*)&pLoc);

	if (FAILED(hres))
	{
		std::cout << "Failed to create IWbemLocator object. "
			<< "Err code = 0x"
			<< std::hex << hres << std::endl;
		CoUninitialize();
		return "";
	}

	IWbemServices* pSvc = NULL;

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
		std::cout << "Could not connect. Error code = 0x"
			<< std::hex << hres << std::endl;
		pLoc->Release();
		CoUninitialize();
		return "";        
	}

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
		std::cout << "Could not set proxy blanket. Error code = 0x"
			<< std::hex << hres << std::endl;
		pSvc->Release();
		pLoc->Release();
		CoUninitialize();
		return "";               
	}

	IEnumWbemClassObject* pEnumerator = NULL;

	hres = pSvc->ExecQuery(
		bstr_t("WQL"),
		bstr_t("SELECT * from Win32_NetworkAdapterConfiguration where IPEnabled ='TRUE'"), // 
		WBEM_FLAG_FORWARD_ONLY | WBEM_FLAG_RETURN_IMMEDIATELY,
		NULL,
		&pEnumerator);

	if (FAILED(hres))
	{
		std::cout << "Query for operating system name failed."
			<< " Error code = 0x"
			<< std::hex << hres << std::endl;
		pSvc->Release();
		pLoc->Release();
		CoUninitialize();
		return "";           
	}

	IWbemClassObject* pclsObj = NULL;
	ULONG uReturn = 0;
	std::string IDs;

	while (pEnumerator)
	{
		HRESULT hr = pEnumerator->Next(WBEM_INFINITE, 1,
			&pclsObj, &uReturn);

		if (0 == uReturn)
		{
			break;
		}

		VARIANT vtProp;

		hr = pclsObj->Get(L"MACAddress", 0, &vtProp, 0, 0);
		VariantClear(&vtProp);
		char ch[20];
		char DefChar = ' ';
		WideCharToMultiByte(CP_ACP, 0, vtProp.bstrVal, -1, ch, 20, &DefChar, NULL);

		IDs.append(ch);
		pclsObj->Release();
	}

	pSvc->Release();
	pLoc->Release();
	pEnumerator->Release();
	CoUninitialize();

	return IDs;
}

void writeRegistry(std::string path, std::string key, DWORD type, std::string value)
{
	LONG lReg;
	HKEY hKey;
	LPCTSTR pathName = (LPCTSTR)path.c_str();
	LPCTSTR keyName = (LPCTSTR)key.c_str();
	DWORD disposition = 0;
	lReg = RegCreateKeyEx(HKEY_LOCAL_MACHINE, pathName, 0, NULL, REG_OPTION_NON_VOLATILE, KEY_ALL_ACCESS, NULL, &hKey, &disposition);
	if (lReg != ERROR_SUCCESS)
	{
		std::cout << "Registry creation failed & Error No - " << GetLastError() << std::endl;
		return;
	}
	if (disposition == REG_CREATED_NEW_KEY) // RegCreateKeyEx opens the key if the key 
	{
		RegOpenKey(HKEY_LOCAL_MACHINE, pathName, &hKey); //"SOFTWARE\\COGAPLEX\\LICENSEKEY"
		RegSetValueEx(hKey, keyName, 0, type, (LPBYTE)(value.c_str()), strlen(value.c_str()) * sizeof(char)); //"License Key"
	}
	else 
	{
		RegSetValueEx(hKey, keyName, 0, type, (LPBYTE)(value.c_str()), strlen(value.c_str()) * sizeof(char)); //"License Key"
	}
	std::cout << "Registry Creation Success " << std::endl;

	RegCloseKey(hKey);
}

std::string readRegistry(std::string path, std::string key, DWORD type)
{
	LONG lReg;
	HKEY hKey;
	const char* myValue = (const char*)malloc(255);
	TCHAR value[255];
	DWORD value_length = 255;
	LPCTSTR pathName, keyName;
	pathName = (LPCTSTR)path.c_str();
	keyName = (LPCTSTR)key.c_str();
	
	if (RegOpenKeyEx(HKEY_LOCAL_MACHINE, pathName, 0, KEY_READ | KEY_WOW64_64KEY, &hKey) == ERROR_SUCCESS)
	{
		lReg = RegQueryValueEx(hKey, keyName, NULL, &type, (LPBYTE)myValue, &value_length);
		if (lReg != ERROR_SUCCESS)
		{
			std::cout << "read error " << GetLastError() << std::endl;
			return "";
		}
		std::string licenseKey(myValue);
		licenseKey.erase(std::find_if(licenseKey.rbegin(), licenseKey.rend(), std::not1(std::ptr_fun<int, int>(std::isspace))).base(), licenseKey.end());
		std::cout << "done Reading!" << std::endl;
		RegCloseKey(hKey);
		return licenseKey;
	}
	std::cout << "Cannot open Registry" << std::endl;
	return "NULL";
}

std::string SHA256HashString(std::string aString) {
    std::string digest;
    CryptoPP::SHA256 hash;

    CryptoPP::StringSource foo(aString, true,
        new CryptoPP::HashFilter(hash,
            new CryptoPP::Base64Encoder(
                new CryptoPP::StringSink(digest))));

    return digest;
}

void writeToFile(std::string path)
{
	std::ofstream writeFile;

	writeFile.open(path);

	std::string liKey = SHA256HashString(HardwareIDs("SELECT * FROM Win32_baseboard", "SELECT * FROM Win32_diskdrive"));
	writeFile.write(liKey.c_str(), liKey.size());

	writeFile.close();	
}

std::string readFromFile(std::string path)
{
	std::ifstream rfile(path);
	std::string licenseKey;
	if (true == rfile.is_open())
	{
		rfile >> licenseKey;
		std::cout << "read keys : " << licenseKey << std::endl;
	}
	else
	{
		std::cout << "no files " << std::endl;
	}
	rfile.close();
	return licenseKey;
}
