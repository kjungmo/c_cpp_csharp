#include <iostream>
#include <WbemIdl.h>
#include <Windows.h>
#include <shlobj_core.h>
#include <algorithm>
#include <Lmcons.h>
#include <comdef.h>

#pragma comment(lib, "wbemuuid.lib")

static char ChBuff[1024];
static char DefChar = ' ';
static char LkBuf[1024];

std::string CreateKeyPathAnother(std::string);
bool GetWMIUserAccount(std::string&, bool);
std::string QueryWMI(IWbemServices*, IWbemLocator*, const char*, const wchar_t*);
bool SplitString(std::string&, std::string&);
bool RemoveSign(std::string&);
bool ConvertToGUID(std::string&);


int main()
{
	//TCHAR name[UNLEN + 1];
	//DWORD size = UNLEN + 1;

	//if (GetUserName((TCHAR*)name, &size))
	//	std::wcout << L"Hello, " << name << L"!\n";
	//else
	//	std::cout << "Hello, unnamed person!\n";

	//std::cout << CreateKeyPathAnother("") << std::endl;
	////SELECT * FROM Win32_UserAccount WHERE DISABLED=FALSE AND NOT NAME="admin"
	//std::string user;
	//GetWMIUserAccount(user, true);
	//std::cout << GetWMIUserAccount(user, true) << std::endl;
	//std::cout << user << std::endl;

	////std::string brain;
	////GetWMIUserAccount(brain, true);
	////std::string brainPath = "C:\\Users\\" + brain;
	//std::string filename = "\\file.txt";
	//int i = 0;
	//PWSTR path = NULL;
	//WCHAR cp_path[100];

	//std::string brain;
	//GetWMIUserAccount(brain, true);
	//std::string brainPath = "C:\\Users\\" + brain;
	//std::string adminPath = "C:\\Users\\admin";

	//std::string s = filename;
	//SHGetKnownFolderPath(FOLDERID_LocalAppData, 0, NULL, &path);
	//while (path[i] != '\0') cp_path[i] = path[i++];
	//for_each(s.begin(), s.end(), [&](char& c) { cp_path[i++] = c; });
	//cp_path[i] = '\0';

	//CoTaskMemFree(path);

	//std::wstring valpath(cp_path);
	//std::string fullpath(valpath.begin(), valpath.end());
	//fullpath = brainPath + fullpath.substr(adminPath.length());
	//std::cout << "Brain Path : " << fullpath << std::endl;

	//std::string mac = "70:d";
	//std::string mac2guid = "SELECT * FROM Win32_NetworkAdapter WHERE MACAddress = '" + mac + "'";
	//std::cout << "mac : " << mac2guid << std::endl;

	std::string mBoard = "M80-A1001800324";
	std::string mBoard2;

	std::string uuid = "03000200-0400-0500-0006-000700080009";
	std::string uuid2;

	std::string guid = "{B45FEA07-9F64-4A1D-A7B6-0375B626532F}";
	std::string guid2;

	std::string mac = "70:85:C2:31:FB:C4";
	std::string mac2;

	RemoveSign(mBoard);
	std::cout << "mBoard : " << mBoard << std::endl;

	SplitString(mBoard, mBoard2);
	std::cout << "mBoard : " << mBoard << "\nmBoard2 : " << mBoard2 << std::endl;

	RemoveSign(uuid);
	std::cout << "uuid : " << uuid << std::endl;

	SplitString(uuid, uuid2);
	std::cout << "uuid : " << uuid << "\nuuid2 : " << uuid2 << std::endl;

	RemoveSign(guid);
	std::cout << "removed guid : " << guid << std::endl;

	ConvertToGUID(guid);
	std::cout << "converted guid : " << guid << std::endl;

	RemoveSign(guid);

	SplitString(guid, guid2);
	std::cout << "guid : " << guid << "\nguid2 : " << guid2 << std::endl;

	RemoveSign(mac);
	std::cout << "mac : " << mac << std::endl;

	SplitString(mac, mac2);
	std::cout << "mac : " << mac << "\nmac2 : " << mac2 << std::endl;








	system("pause");
 }

std::string CreateKeyPathAnother(std::string filename)
{
	int i = 0;
	PWSTR path = NULL;
	WCHAR cp_path[100];
	std::string s = filename;
	SHGetKnownFolderPath(FOLDERID_Profile, 0, NULL, &path);
	while (path[i] != '\0') cp_path[i] = path[i++];
	for_each(s.begin(), s.end(), [&](char& c) { cp_path[i++] = c; });
	cp_path[i] = '\0';
	CoTaskMemFree(path);
	std::wstring valpath(cp_path);
	std::string fullpath(valpath.begin(), valpath.end());
	return fullpath;
}

bool GetWMIUserAccount(std::string& user, bool initCOM)
{
	HRESULT hres;
	if (initCOM)
	{
		hres = CoInitializeEx(0, COINITBASE_MULTITHREADED);
		if (FAILED(hres))
		{
			return false;
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
			CoUninitialize();
			return false;
		}
	}

	IWbemLocator* pLoc = NULL;

	hres = CoCreateInstance(
		CLSID_WbemLocator,
		0,
		CLSCTX_INPROC_SERVER,
		IID_IWbemLocator, (LPVOID*)&pLoc);

	if (FAILED(hres))
	{
		CoUninitialize();
		return false;
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
		pLoc->Release();
		CoUninitialize();
		return false;
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
		pSvc->Release();
		pLoc->Release();
		CoUninitialize();
		return false;
	}
	user = QueryWMI(pSvc, pLoc, "SELECT * FROM Win32_UserAccount WHERE DISABLED = FALSE AND NOT NAME = 'admin' AND FULLNAME = ''", L"Name");

	pSvc->Release();
	pLoc->Release();
	CoUninitialize();
	return !(user.empty());
}

std::string QueryWMI(IWbemServices* pSvc, IWbemLocator* pLoc, const char* query, const wchar_t* desc)
{
	HRESULT hres;
	IEnumWbemClassObject* pEnumerator = NULL;

	hres = pSvc->ExecQuery(
		bstr_t("WQL"),
		bstr_t(query),
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

	IWbemClassObject* pclsObj = NULL;
	ULONG uReturn = 0;
	std::string IDs;

	while (pEnumerator)
	{
		hres = pEnumerator->Next(WBEM_INFINITE, 1,
			&pclsObj, &uReturn);

		if (0 == uReturn)
		{
			break;
		}

		VARIANT vtProp;
		VariantInit(&vtProp);
		hres = pclsObj->Get(desc, 0, &vtProp, 0, 0);
		VariantClear(&vtProp);
		WideCharToMultiByte(CP_ACP, 0, vtProp.bstrVal, -1, ChBuff, 1024, &DefChar, NULL);

		IDs.append(ChBuff);
		pclsObj->Release();
	}

	pEnumerator->Release();
	return IDs;
}

std::string CreateKeyPath(std::string filename, bool initCOM)
{
	int i = 0;
	PWSTR path = NULL;
	WCHAR cp_path[100];

	std::string brain;
	GetWMIUserAccount(brain, initCOM);
	std::string brainPath = "C:\\Users\\" + brain;
	std::string adminPath = "C:\\Users\\admin";

	std::string s = filename;
	SHGetKnownFolderPath(FOLDERID_LocalAppData, 0, NULL, &path);
	while (path[i] != '\0') cp_path[i] = path[i++];
	for_each(s.begin(), s.end(), [&](char& c) { cp_path[i++] = c; });
	cp_path[i] = '\0';

	CoTaskMemFree(path);

	std::wstring valpath(cp_path);
	std::string fullpath(valpath.begin(), valpath.end());
	fullpath = brainPath + fullpath.substr(adminPath.length());
	return fullpath;
}

bool SplitString(std::string& firstHalf, std::string& secondHalf)
{
	std::string Original = firstHalf;
	if (Original.length() != 0)
	{
		firstHalf = Original.substr(0, Original.length() / 2);
		secondHalf = Original.substr(Original.length() / 2);
	}
	return !(firstHalf.empty() || secondHalf.empty());
}

bool RemoveSign(std::string& letters)
{
	if (!letters.empty())
	{
		std::string chars = "!@#$%^&*:;()_-=+{}[].,/\\";
		letters.erase(std::remove_if(letters.begin(), letters.end(),
			[&chars](const char& c) {
				return chars.find(c) != std::string::npos;
			}),
			letters.end());
		std::cout << letters << std::endl;
	}
	return !(letters.empty());
}

bool ConvertToGUID(std::string& guidRead)
{

	guidRead = "{" +
		guidRead.substr(0, 8) + "-" +
		guidRead.substr(8, 4) + "-" +
		guidRead.substr(12, 4) + "-" +
		guidRead.substr(16, 4) + "-" +
		guidRead.substr(20, 12) + "}";
	return true;
}