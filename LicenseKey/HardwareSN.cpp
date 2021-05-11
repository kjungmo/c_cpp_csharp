#define _WIN32_DCOM
#include <iostream>
#include <comdef.h>
#include <WbemIdl.h>
#include <Windows.h>
#include <stdio.h>
#include <assert.h>
#include <iphlpapi.h>
#include <sstream>

#pragma comment(lib, "iphlpapi.lib")
#pragma comment(lib, "wbemuuid.lib")
//"SOFTWARE\\COGAPLEX\\LICENSEKEY"
//"License Key"
//"SELECT * FROM Win32_baseboard"
//"SELECT * FROM Win32_diskdrive" 

std::string SerialNumWMI(const char*);
std::string mboardWMI(const char*);
std::string diskdriveWMI(const char*);
std::string macWMI();
std::string HardwareIDs(const char*, const char*);
void writeRegistry(const char*, const char*, std::string value);
std::string readRegistry(const char*, const char*);


int main(int argc, char** argv)
{
	std::cout << HardwareIDs("SELECT * FROM Win32_baseboard", "SELECT * FROM Win32_diskdrive") << std::endl;
	writeRegistry("SOFTWARE\\COGAPLEX\\LICENSEKEY", "License Key", HardwareIDs("SELECT * FROM Win32_baseboard", "SELECT * FROM Win32_diskdrive"));
	std::cout << "this is the value read : " << readRegistry("SOFTWARE\\COGAPLEX\\LICENSEKEY", "License Key") << std::endl;
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


std::string mboardWMI(const char* wmiQuery)
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
		bstr_t(wmiQuery), //"SELECT * FROM Win32_baseboard"
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


std::string diskdriveWMI(const char* wmiQuery)
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
		bstr_t(wmiQuery), //"SELECT * FROM Win32_diskdrive" 
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

		char ch[260];
		char DefChar = ' ';
		WideCharToMultiByte(CP_ACP, 0, vtProp.bstrVal, -1, ch, 260, &DefChar, NULL);
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

void writeRegistry(const char* path, const char* keyName, std::string value)
{
	HKEY key;
	RegOpenKey(HKEY_LOCAL_MACHINE, path, &key); //"SOFTWARE\\COGAPLEX\\LICENSEKEY"
	RegSetValueEx(key, keyName, 0, REG_SZ, (LPBYTE)value.c_str(), strlen(value.c_str()) * sizeof(char)); //"License Key"
	RegCloseKey(key);
}

std::string readRegistry(const char* path, const char* keyName)
{
	LONG IResult;
	HKEY hKey;
	DWORD dwType;
	DWORD dwBytes = 250;
	char buffer[250];

	IResult = RegOpenKeyEx(HKEY_LOCAL_MACHINE, path, 0, KEY_ALL_ACCESS | KEY_WOW64_64KEY, &hKey);
	if (IResult != ERROR_SUCCESS)
	{
		MessageBox(NULL, "Register Open Error", "Error", MB_OK);
	}

	IResult = RegQueryValueExA(hKey, keyName, 0, &dwType, (LPBYTE)buffer, &dwBytes);
	if (IResult == ERROR_SUCCESS)
	{
		//std::cout << buffer << std::endl;
		//MessageBox(NULL, buffer, "Registry", MB_OK);
	}
	else
		MessageBox(NULL, "Register Read Error", "Error", MB_OK);

	RegCloseKey(hKey);
	std::string licenseKey(buffer);

	return licenseKey;
}
