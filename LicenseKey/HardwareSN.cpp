//#define _CRT_SECURE_NO_WARNINGS
#include <iostream>
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

int main()
{
	std::cout << HardwareIDs("SELECT * FROM Win32_baseboard", "SELECT * FROM Win32_diskdrive") << std::endl;
	//writeRegistry("SOFTWARE\\COGAPLEX\\LICENSEKEY", "License Key", REG_SZ, HardwareIDs("SELECT * FROM Win32_baseboard", "SELECT * FROM Win32_diskdrive"));
	std::cout << SHA256HashString(HardwareIDs("SELECT * FROM Win32_baseboard", "SELECT * FROM Win32_diskdrive")) << std::endl;
	writeRegistry("SOFTWARE\\COGAPLEX\\LICENSEKEY", "License Key", REG_SZ, SHA256HashString(HardwareIDs("SELECT * FROM Win32_baseboard", "SELECT * FROM Win32_diskdrive")));
	std::cout << "this is the value read : " << readRegistry("SOFTWARE\\COGAPLEX\\LICENSEKEY", "License Key", REG_SZ) << std::endl;
	system("pause");
	return 0;
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
	lReg = RegCreateKeyEx(HKEY_LOCAL_MACHINE, pathName, 0, NULL, REG_OPTION_NON_VOLATILE, KEY_ALL_ACCESS, NULL, &hKey, NULL);
	if (lReg != ERROR_SUCCESS)
	{
		std::cout << "Registry creation failed & Error No - " << GetLastError() << std::endl;
	}
	std::cout << "Registry Creation Success " << std::endl;

	RegOpenKey(HKEY_LOCAL_MACHINE, pathName, &hKey); //"SOFTWARE\\COGAPLEX\\LICENSEKEY"
	RegSetValueEx(hKey, keyName, 0, type, (LPBYTE)(value.c_str()), strlen(value.c_str()) * sizeof(char)); //"License Key"
	RegCloseKey(hKey);
}

std::string readRegistry(std::string path, std::string key, DWORD type)
{
	LONG lReg;
	std::cout << "reading Registry!" << std::endl;
	HKEY hKey;
	const char* myValue = (const char*)malloc(255);
	TCHAR value[255];
	DWORD value_length = 255;
	LPCTSTR pathName = (LPCTSTR)path.c_str();
	LPCTSTR keyName = (LPCTSTR)key.c_str();
	RegOpenKey(HKEY_LOCAL_MACHINE, pathName, &hKey);  //"SOFTWARE\\COGAPLEX\\LICENSEKEY"
 	lReg = RegQueryValueEx(hKey, keyName, NULL, &type, (LPBYTE)myValue, &value_length);
	if (lReg != ERROR_SUCCESS)
	{
		std::cout << "read error " << GetLastError() << std::endl;
	}
	RegCloseKey(hKey);
	std::string licenseKey(myValue);/* = TCHAR2Str(value);*/
	std::cout << "done reading!" << std::endl;
	return licenseKey;

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