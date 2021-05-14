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

std::string MBoardWMI();
std::string DiskDriveWMI();
std::string MACWMI();
std::string HardwareIDs();
void WriteToRegistry();
std::string ReadFromRegistry();
std::string SHA256HashString(std::string);
void WriteToFile();
std::string ReadFromFile();
void LicenseKeyGenerator();
void LicenseValidator();

int main()
{
	LicenseKeyGenerator();
	LicenseValidator();
	system("pause");
	return 0;
}

void LicenseKeyGenerator()
{
	WriteToRegistry();
	WriteToFile();
}

void LicenseValidator()
{
	std::string regi, file, sha;
	regi = ReadFromRegistry();
	file = ReadFromFile();
	sha = SHA256HashString(HardwareIDs());
	std::string path = "D:\\Github\\c_cpp_csharp\\LicenseKey\\text3.txt";
	std::ifstream checkerRead(path);
	std::ofstream checkerWrite;

	if (!checkerRead.good() && regi == file && regi == sha)
	{
		std::cout << "PASS" << std::endl;
		checkerRead.close();
		checkerWrite.close();
	}
	else
	{
		checkerWrite.open(path);
		std::string invalid = "NO TRESSPASSING";
		checkerWrite.close();
	}

	std::cout << "Valid License" << std::endl;

}

std::string HardwareIDs()
{
	std::string mboard = MBoardWMI();
	std::string diskdrive = SHA256HashString(mboard) + DiskDriveWMI();
	std::string mac = SHA256HashString(diskdrive) + MACWMI();
	std::string toBeEncrypted = SHA256HashString(mac);
	return toBeEncrypted;
}

std::string MBoardWMI()
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
		bstr_t("SELECT * FROM Win32_baseboard"),
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

std::string DiskDriveWMI()
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
		bstr_t("SELECT * FROM Win32_diskdrive"),
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

std::string MACWMI()
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
		bstr_t("SELECT * from Win32_NetworkAdapterConfiguration where IPEnabled ='TRUE'"), 
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

void WriteToRegistry()
{
	HKEY hKey;
	std::string path = "SOFTWARE\\COGAPLEX\\LICENSEKEY";
	std::string key = "License Key";
	std::string value = SHA256HashString(HardwareIDs());
	LPCTSTR pathName = (LPCTSTR)path.c_str();
	LPCTSTR keyName = (LPCTSTR)key.c_str();
	DWORD type = REG_SZ;
	DWORD disposition = 0;
	if (RegCreateKeyEx(HKEY_LOCAL_MACHINE, pathName, 0, NULL, REG_OPTION_NON_VOLATILE, KEY_ALL_ACCESS, NULL, &hKey, &disposition) == ERROR_SUCCESS)
	{
		if (disposition == REG_CREATED_NEW_KEY)
		{
			RegOpenKey(HKEY_LOCAL_MACHINE, pathName, &hKey); 
			RegSetValueEx(hKey, keyName, 0, type, (LPBYTE)(value.c_str()), strlen(value.c_str()) * sizeof(char));
		}
		else
		{
			RegSetValueEx(hKey, keyName, 0, type, (LPBYTE)(value.c_str()), strlen(value.c_str()) * sizeof(char));
		}
		std::cout << "Registry Creation Success " << std::endl;
		RegCloseKey(hKey);
		return;
	}
	std::cout << "Registry creation failed & Error No - " << GetLastError() << std::endl;
	return;
}

std::string ReadFromRegistry()
{
	HKEY hKey;
	const char* myValue = (const char*)malloc(255);
	TCHAR value[255];
	DWORD value_length = 255;
	LPCTSTR pathName, keyName;
	std::string path = "SOFTWARE\\COGAPLEX\\LICENSEKEY";
	std::string key = "License Key";
	DWORD type = REG_SZ;
	pathName = (LPCTSTR)path.c_str();
	keyName = (LPCTSTR)key.c_str();
	if (RegOpenKeyEx(HKEY_LOCAL_MACHINE, pathName, 0, KEY_READ | KEY_WOW64_64KEY, &hKey) == ERROR_SUCCESS)
	{
		if (RegQueryValueEx(hKey, keyName, NULL, &type, (LPBYTE)myValue, &value_length) != ERROR_SUCCESS)
		{
			std::cout << "read error " << GetLastError() << std::endl;
			return "";
		}
		std::string licenseKey(myValue);
		std::cout << "done Reading!" << std::endl;
		RegCloseKey(hKey);
		return licenseKey;
	}
	std::cout << "Cannot open Registry" << std::endl;
	return "NULL";
}

std::string SHA256HashString(std::string value) {
    std::string digest;
    CryptoPP::SHA256 hash;

    CryptoPP::StringSource foo(value, true,
        new CryptoPP::HashFilter(hash,
            new CryptoPP::Base64Encoder(
                new CryptoPP::StringSink(digest))));
	digest.erase(std::find_if(digest.rbegin(), digest.rend(), std::not1(std::ptr_fun<int, int>(std::isspace))).base(), digest.end());
    return digest;
}

void WriteToFile()
{
	std::ofstream writeFile;

	writeFile.open("D:\\Github\\c_cpp_csharp\\LicenseKey\\text2.txt");

	std::string liKey = SHA256HashString(HardwareIDs());
	writeFile.write(liKey.c_str(), liKey.size());

	writeFile.close();	
}

std::string ReadFromFile()
{
	std::ifstream rfile("D:\\Github\\c_cpp_csharp\\LicenseKey\\text2.txt");
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
