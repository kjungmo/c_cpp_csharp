#define KEY1_FILE "Profile.txt"

#include "comdef.h"
#include "WbemIdl.h"
#include "Windows.h"
#include "assert.h"
#include "iphlpapi.h"
#include "Shlwapi.h"
#include "shlobj_core.h"

#include "cryptopp/sha.h"
#include "cryptopp/filters.h"
#include "cryptopp/base64.h"
#include "cryptopp/modes.h"
#include "cryptopp/aes.h"
#include "cryptopp/hex.h"
#include "cryptopp/files.h"

#include <iostream>
#include <fstream>
#include <iomanip>
#include <string>
#include <algorithm>

#ifdef _DEBUG
#pragma comment(lib, "cryptopp/cryptlibD.lib")
const char* GenerateLicenseKey(bool initCOM);
#else
#pragma comment(lib, "cryptopp/cryptlibR.lib")
#define LICENSE_DLL_EXPORT
#include "License.h"
#endif

#pragma comment(lib, "wbemuuid.lib")
#pragma comment(lib, "shlwapi")

static char ChBuff[1024];
static char DefChar = ' ';
static char LkBuf[1024];

bool GetWMI(std::string&, std::string&, std::string&, bool initCOM = true);
bool GetWMIUserAccount(std::string&, bool initCOM = true);
bool GetWMIMboard(std::string&, bool initCOM = true);
std::string QueryWMI(IWbemServices*, IWbemLocator*, const char*, const wchar_t*);
bool WriteToFile(std::string, std::string);
std::string CreateKey1Path(std::string);
std::string CreateInfoLine(std::string, std::string, std::string, std::string, std::string);

int main()
{
    if (GenerateLicenseKey(true))
    {
        std::cout << "Generated" << std::endl;
    }

    system("pause");
    return 0;
}

const char* GenerateLicenseKey(bool initCOM)
{
    LkBuf[0] = '\0';
#ifdef GENERATOR_BUILD

    std::string userName;
    std::string mBoard;
    std::string uuid;
    std::string guid;
    std::string mac;

    if (!GetWMIUserAccount(userName, initCOM))
    {
        return LkBuf;
    }

    std::string lkey1File = CreateKey1Path(KEY1_FILE);

    if (PathFileExists(lkey1File.c_str()))
    {
        remove(lkey1File.c_str());
    }

    if (!GetWMIMboard(mBoard, initCOM) || !GetWMI(uuid, guid, mac, initCOM))
    {
        return LkBuf;
    }
    userName = " user : " + userName;
    mBoard = "\n mb : " + mBoard;
    uuid = "\n uuid : " + uuid;
    guid = "\n guid : " + guid;
    mac = "\n mac : " + mac;
    
    std::string writtenInFile = userName;
    writtenInFile += mBoard;
    writtenInFile += uuid;
    writtenInFile += guid;
    writtenInFile += mac;

    if (!WriteToFile(lkey1File, writtenInFile))
    {
        return LkBuf;
    }

    std::string infos = CreateInfoLine(userName, mBoard, uuid, guid, mac);
    strcpy_s(LkBuf, infos.length() + 1, infos.c_str());
#endif
    return LkBuf;
}

bool GetWMI(std::string& uuid, std::string& guid, std::string& mac, bool initCOM)
{
    HRESULT hres;
    std::string getGuid;
    std::string getMac;
    std::string netID;
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
    uuid = QueryWMI(pSvc, pLoc, "SELECT * FROM Win32_ComputerSystemProduct", L"UUID");
    if (uuid.length() == 0)
    {
        uuid = "None";
    }

    if (guid.length() == 0)
    {
        getGuid = QueryWMI(pSvc, pLoc, u8"SELECT * FROM Win32_NetworkAdapter WHERE NetConnectionID = 'CoPick-Ethernet'", L"GUID");
        if (getGuid.length() != 0)
        {
            guid = u8"u8 CoPick-Ethernet : " + getGuid;
        }
        else
        {
            getGuid = QueryWMI(pSvc, pLoc, u8"SELECT * FROM Win32_NetworkAdapter WHERE NetConnectionID = 'Ethernet' OR NetConnectionID = '이더넷'", L"GUID");
            if (getGuid.length() !=0)
            {
                guid = u8"u8 이더넷 : " + getGuid;
            }
            else
            {
                getGuid = QueryWMI(pSvc, pLoc, u8"SELECT * FROM Win32_NetworkAdapter WHERE NetConnectionID = 'Ethernet 2' OR NetConnectionID = '이더넷 2'", L"GUID");
                if (getGuid.length() != 0)
                {
                    guid = u8"u8 이더넷 2 : " + getGuid;
                }
                else
                {
                    getGuid = QueryWMI(pSvc, pLoc, "SELECT * FROM Win32_NetworkAdapter WHERE NetEnabled = 'true'", L"GUID");
                    if (getGuid.length() == 0)
                    {
                        guid = "None";
                    }
                    else
                    {
                        guid = "NetEnabled : " + getGuid;
                    }
                }
            }
        }
    }

    if (mac.length() == 0)
    {
        getMac = QueryWMI(pSvc, pLoc, u8"SELECT * FROM Win32_NetworkAdapter WHERE NetConnectionID = 'CoPick-Ethernet'", L"MACAddress");
        if (getMac.length() != 0)
        {
            mac = u8"u8 CoPick-Ethernet : " + getMac;
        }
        else
        {
            getMac = QueryWMI(pSvc, pLoc, u8"SELECT * FROM Win32_NetworkAdapter WHERE NetConnectionID = 'Ethernet' OR NetConnectionID = '이더넷'", L"MACAddress");
            if (getMac.length() != 0)
            {
                mac = u8"u8 이더넷 : " + getMac;
            }
            else
            {
                getMac = QueryWMI(pSvc, pLoc, u8"SELECT * FROM Win32_NetworkAdapter WHERE NetConnectionID = 'Ethernet 2' OR NetConnectionID = '이더넷 2'", L"MACAddress");
                if (getMac.length() != 0)
                {
                    mac = u8"u8 이더넷 2 : " + getMac;
                }
                else
                {
                    getMac = QueryWMI(pSvc, pLoc, "SELECT * FROM Win32_NetworkAdapter WHERE NetEnabled = 'true'", L"MACAddress");
                    if (getMac.length() == 0)
                    {
                        mac = "None";
                    }
                    else
                    {
                        netID = QueryWMI(pSvc, pLoc, "SELECT * FROM Win32_NetworkAdapter WHERE NetEnabled = 'true'", L"NetConnectionID");
                        mac = "NetEnabled : ";
                        mac += "[";
                        mac += netID;
                        mac += "]";
                        mac += getMac;
                    }
                }
            }
        }
    }

    pSvc->Release();
    pLoc->Release();
    CoUninitialize();

    return true;
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
    if (user.length() == 0)
    {
        user = "None";
    }
    pSvc->Release();
    pLoc->Release();
    CoUninitialize();
    return true;
}

bool GetWMIMboard(std::string& mb, bool initCOM)
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

    mb = QueryWMI(pSvc, pLoc, "SELECT * FROM Win32_BaseBoard", L"SerialNumber");
    if (mb.length() == 0)
    {
        mb = "None";
    }
    pSvc->Release();
    pLoc->Release();
    CoUninitialize();
    return true;
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

bool WriteToFile(std::string filepath, std::string content)
{
    std::ofstream writeFile(filepath);

    SetFileAttributes(filepath.c_str(), FILE_ATTRIBUTE_HIDDEN);
    if (content.empty())
    {
        writeFile.close();
        return false;
    }
    writeFile << content;
    writeFile.close();
    return true;
}

std::string CreateKey1Path(std::string filename)
{
    std::string path = "./";
    path += filename;
    return path;
}

std::string CreateInfoLine(std::string user, std::string mboard, std::string uuid, std::string guid, std::string mac)
{   
    
    std::string infoLine = user;
    infoLine += mboard;
    infoLine += uuid;
    infoLine += guid;
    infoLine += mac;
    return infoLine;
}

