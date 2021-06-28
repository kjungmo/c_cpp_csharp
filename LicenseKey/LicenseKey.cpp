#define KEY1_FILE "Profile.txt"
#define KEY2_FILE "Info.txt"
#define INTRUSION_FILE "wtfjm.dat"
#define MAC_GUID_FILE "rnlel.dat"

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

#include <iostream>
#include <fstream>
#include <string>
#include <iomanip>
#include <algorithm>

#ifdef _DEBUG
#pragma comment(lib, "D:/Github/c_cpp_csharp/LicenseKey/cryptlibD.lib")
const char* GenerateLicenseKey(bool initCOM);
bool ValidateLicenseKey(bool initCOM);
#else
#pragma comment(lib, "D:/Github/c_cpp_csharp/LicenseKey/cryptlibR.lib")
#define LICENSE_DLL_EXPORT
#include "License.h"
const char* GenerateLicenseKey(bool initCOM);
bool ValidateLicenseKey(bool initCOM);
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
std::string CreateLicenseKey(std::string, std::string, std::string);
bool CreateMacGuidKey(std::string, std::string, std::string&);
std::string SHA256HashString(std::string);
bool WriteToFile(std::string, std::string);
std::string ReadFromFile(std::string);
std::string CreateIntrusionPath(std::string, std::string);
std::string CreateKey1Path(std::string, std::string);
std::string CreateKey2Path(std::string);
std::string CreateMacGuidPath(std::string, std::string);
std::string GetGPUSerial(std::string);
bool SplitString(std::string&, std::string&);
bool RemoveSymbols(std::string&);
std::string ConvertGUID(std::string);
std::string DecryptGUID(std::string, std::string);
std::string EncryptGUID(std::string, std::string);


int main()
{
    time_t startG, endG, startV, endV;
    double resultG, resultV;
    startG = clock();
    if (GenerateLicenseKey(true))
    {
        std::cout << "Generated" << std::endl;
    }
    endG = clock();
    startV = clock();
    if (!ValidateLicenseKey(true))
    {
        std::cout << "Invalid License" << std::endl;
    }
    endV = clock();

    std::cout << "Validation Complete." << std::endl;

    resultG = (double)(endG - startG);
    resultV = (double)(endV - startV);
    std::cout << "result Gen : " << resultG << std::endl << "result Val : " << resultV << std::endl;

    system("pause");
    return 0;
}

const char* GenerateLicenseKey(bool initCOM)
{
    LkBuf[0] = '\0';
//#ifdef GENERATOR_BUILD

    time_t startGen, endGen, startGETWMI, endGETWMI, startCREATELK, endCREATELK, startMACGUIDKEY, endMACGUIDKEY;  //**********************************
    double resultGen, resultGETWMI, resultCREATELK, resultMACGUIDKEY;  //****************************
    startGen = clock(); // *********************

    std::string userName;
    std::string mBoard;
    std::string uuid;
    std::string guid;
    std::string mac;
    std::string macGuidKey;

    if (!GetWMIUserAccount(userName, initCOM))
    {
        return LkBuf;
    }

    std::string intrusion = CreateIntrusionPath(INTRUSION_FILE, userName);
    std::string lkey1File = CreateKey1Path(KEY1_FILE, userName);
    std::string lkey2File = CreateKey2Path(KEY2_FILE);
    std::string macguidFile = CreateMacGuidPath(MAC_GUID_FILE, userName);

    if (PathFileExists(intrusion.c_str()))
    {
        remove(intrusion.c_str());
    }

    if (PathFileExists(lkey1File.c_str()))
    {
        remove(lkey1File.c_str());
    }

    if (PathFileExists(lkey2File.c_str()))
    {
        remove(lkey2File.c_str());
    }

    if (PathFileExists(macguidFile.c_str()))
    {
        remove(macguidFile.c_str());
    }

    startGETWMI = clock();  //****************************
    if (!GetWMIMboard(mBoard, initCOM) || !GetWMI(uuid, guid, mac, initCOM))
    {
        return LkBuf;
    }
    endGETWMI = clock();  //****************************
    //std::cout << "mboard : " << mBoard << "\nuuid : " << uuid << "\nguid : " << guid << "\nmac : " << mac << std::endl;  //****************************

    startMACGUIDKEY = clock();  //****************************
    std::string licenseKey = CreateLicenseKey(mBoard, uuid, mac);
    if (licenseKey.length() != 0 && !CreateMacGuidKey(mBoard, guid, macGuidKey))
    {
        return LkBuf;
    }

    std::cout << "macGuidKey created : " << macGuidKey << "\nlicenseKey created : " << licenseKey << std::endl;
    endMACGUIDKEY = clock();  //****************************

    startCREATELK = clock();  //****************************
    endCREATELK = clock();  //****************************

    if (!WriteToFile(lkey1File, licenseKey) || !WriteToFile(lkey2File, licenseKey) || !WriteToFile(macguidFile, macGuidKey))
    {
        return LkBuf;
    }

    resultGETWMI = (double)(endGETWMI - startGETWMI);  //****************************
    resultMACGUIDKEY = (double)(endMACGUIDKEY - startMACGUIDKEY);  //****************************
    resultCREATELK = (double)(endCREATELK - startCREATELK);  //****************************


    strcpy_s(LkBuf, licenseKey.length() + 1, licenseKey.c_str());
    endGen = clock(); // *********************
    resultGen = (double)(endGen - startGen); // *********************
    std::cout << "resultGen : " << resultGen << "\nresultGETWMI : " << resultGETWMI << "\nresultMACGUIDKEY : " << resultMACGUIDKEY << "\nresultCREATELK : " << resultCREATELK << std::endl;  //****************************
//#endif
    return LkBuf;
}

bool ValidateLicenseKey(bool initCOM)
{
    time_t start = clock();  // *********************

    std::string userName;
    if (!GetWMIUserAccount(userName, initCOM))
    {
        return false;
    }

    std::string intrusion = CreateIntrusionPath(INTRUSION_FILE, userName);
    std::string lkey1File = CreateKey1Path(KEY1_FILE, userName);
    std::string lkey2File = CreateKey2Path(KEY2_FILE);
    std::string macguidFile = CreateMacGuidPath(MAC_GUID_FILE, userName);

    std::string lKey1 = ReadFromFile(lkey1File);
    std::string lKey2 = ReadFromFile(lkey2File);
    std::string macguidCipher = ReadFromFile(macguidFile);

    std::string mBoard;
    if (GetWMIMboard(mBoard, initCOM))
    {
        std::string guid = DecryptGUID(mBoard, macguidCipher);
        std::string uuid;
        std::string mac;

        if (guid.length() != 0 && GetWMI(uuid, guid, mac, initCOM))
        {
            std::string licenseKey = CreateLicenseKey(mBoard, uuid, mac);
            if (!lKey1.empty() && !lKey2.empty() && !licenseKey.empty())
            {
                if (!PathFileExists(intrusion.c_str()) && lKey1 == licenseKey && lKey2 == licenseKey)
                {
                    time_t end = clock(); // *********************
                    std::cout << "val: " << (double)(end - start) << std::endl; // *********************
                    return true;
                }
            }
        }
    }

    WriteToFile(intrusion, "More at cogaplex@cogaplex.com.");
    remove(lkey1File.c_str());
    remove(lkey2File.c_str());
    remove(macguidFile.c_str());

    time_t end = clock(); // *********************
    std::cout << "val(fail): " << (double)(end - start) << std::endl; // *********************
    return false;
}

std::string CreateLicenseKey(std::string mBoard, std::string uuid, std::string mac)
{
    if (RemoveSymbols(mBoard) && RemoveSymbols(uuid) && RemoveSymbols(mac))
    {
        std::cout << "mBoard(removed) : " << mBoard << "\n"; // *********************
        std::cout << "uuid(removed) : " << uuid << "\n"; // *********************
        std::cout << "mac(removed) : " << mac << "\n"; // *********************
        std::string mBoard2, uuid2, mac2;
        if (SplitString(mBoard, mBoard2) && SplitString(uuid, uuid2) && SplitString(mac, mac2))
        {
            return SHA256HashString(SHA256HashString(mac2 + uuid + mBoard2) + SHA256HashString(mBoard + mac + uuid2));
        }
    }

    return "";
}

bool CreateMacGuidKey(std::string mb, std::string guid, std::string& cipher)
{
    if (mb.length() != 0 && guid.length() != 0)
    {
        std::cout << "guid " << guid << "\n";  // *********************
        RemoveSymbols(guid);
        std::cout << "removed guid " << guid << "\n"; // *********************
        cipher = EncryptGUID(mb, guid);
        std::cout << "cipher " << cipher << "\n"; // *********************
        std::cout << "cipher end\n"; // *********************
        return !(cipher.empty());
    }
    return false;
}

bool GetWMI(std::string& uuid, std::string& guid, std::string& mac, bool initCOM)
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
    time_t startUUID, endUUID, startGUID, endGUID, startMAC, endMAC;  //********************************************
    double resultUUID, resultGUID, resultMAC; //********************************************
    startUUID = clock(); //********************************************
    uuid = QueryWMI(pSvc, pLoc, "SELECT * FROM Win32_ComputerSystemProduct", L"UUID");
    endUUID = clock(); //********************************************
    startGUID = clock(); //********************************************
    if (guid.length() == 0)
    {
        guid = QueryWMI(pSvc, pLoc, "SELECT * FROM Win32_NetworkAdapter WHERE NetConnectionID = 'Ethernet' OR NetConnectionID = '이더넷'", L"GUID");
        if (guid.length() == 0)
        {
            guid = QueryWMI(pSvc, pLoc, "SELECT * FROM Win32_NetworkAdapter WHERE NetConnectionID = 'Ethernet 2' OR NetConnectionID = '이더넷 2'", L"GUID");
        }
    }
    endGUID = clock(); //********************************************
    startMAC = clock(); //********************************************
    std::string macQuery = "SELECT * FROM Win32_NetworkAdapter WHERE GUID = '";
    macQuery += guid;
    macQuery += "'";
    mac = QueryWMI(pSvc, pLoc, macQuery.c_str(), L"MACAddress");
    endMAC = clock(); //********************************************

    pSvc->Release();
    pLoc->Release();
    CoUninitialize();

    resultUUID = (double)(endUUID - startUUID); //********************************************
    resultGUID = (double)(endGUID - startGUID); //********************************************
    resultMAC = (double)(endMAC - startMAC); //********************************************
    std::cout << "resultUUID : " << resultUUID << "\nresultGUID : " << resultGUID << "\nresultMAC :" << resultMAC << std::endl; //********************************************

    return !(uuid.empty() || guid.empty() || mac.empty());
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

    pSvc->Release();
    pLoc->Release();
    CoUninitialize();
    return !(mb.empty());
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

std::string ReadFromFile(std::string filepath)
{
    std::ifstream rfile(filepath);
    std::string readStr((std::istreambuf_iterator<char>(rfile)), (std::istreambuf_iterator<char>()));
    rfile.close();
    return readStr;
}

std::string CreateIntrusionPath(std::string filename, std::string userName)
{
    std::string path = "C:\\Users\\";
    path += userName;
    path += "\\AppData\\LocalLow\\";
    path += filename;
    return path;
}

std::string CreateKey1Path(std::string filename, std::string userName)
{
    std::string path = "C:\\Users\\";
    path += userName;
    path += "\\AppData\\Local\\";
    path += filename;
    return path;
}

std::string CreateKey2Path(std::string filename)
{
    std::string path = "C:\\Users\\Public\\Documents\\";
    path += filename;
    return path;
}

std::string CreateMacGuidPath(std::string filename, std::string userName)
{
    std::string path = "C:\\Users\\";
    path += userName;
    path += "\\AppData\\Local\\Packages\\";
    path += filename;
    return path;
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

bool RemoveSymbols(std::string& letters)
{
    if (!letters.empty())
    {
        letters.erase(
            std::remove_if(letters.begin(), letters.end(), [](char c) { return std::isspace(c) || std::ispunct(c); }),
            letters.end()
        );
    }
    return !(letters.empty());
}

std::string ConvertGUID(std::string guidRead)
{
    std::string guid = "{";
    guid += guidRead.substr(0, 8);
    guid += "-";
    guid += guidRead.substr(8, 4);
    guid += "-";
    guid += guidRead.substr(12, 4);
    guid += "-";
    guid += guidRead.substr(16, 4);
    guid += "-";
    guid += guidRead.substr(20, 12);
    guid += "}";
    return guid;
}

std::string EncryptGUID(std::string mb, std::string guid)
{
    if (mb.length() != 0 && guid.length() != 0)
    {
        std::string plaintext = guid;
        std::string ciphertext;

        CryptoPP::SHA256 hash;
        CryptoPP::byte digest[CryptoPP::SHA256::DIGESTSIZE];
        hash.CalculateDigest(digest, (CryptoPP::byte*)mb.c_str(), mb.length());

        CryptoPP::HexEncoder encoder;
        std::string sKey;
        encoder.Attach(new CryptoPP::StringSink(sKey));
        encoder.Put(digest, sizeof(digest));
        encoder.MessageEnd();

        CryptoPP::byte key[CryptoPP::AES::MAX_KEYLENGTH];
        CryptoPP::byte  iv[CryptoPP::AES::BLOCKSIZE];
        memcpy(key, sKey.c_str(), CryptoPP::AES::MAX_KEYLENGTH);;
        memset(iv, 0x00, CryptoPP::AES::BLOCKSIZE);

        CryptoPP::AES::Encryption aesEncryption(key, CryptoPP::AES::MAX_KEYLENGTH);
        CryptoPP::CBC_Mode_ExternalCipher::Encryption cbcEncryption(aesEncryption, iv);

        CryptoPP::StreamTransformationFilter stfEncryptor(cbcEncryption, new CryptoPP::StringSink(ciphertext));
        stfEncryptor.Put(reinterpret_cast<const unsigned char*>(plaintext.c_str()), plaintext.length());
        stfEncryptor.MessageEnd();

        return ciphertext;
    }
    return "";
}

std::string DecryptGUID(std::string mb, std::string ciphertext)
{
    if (mb.length() != 0 && ciphertext.length() != 0)
    {
        std::string decryptedtext;

        CryptoPP::SHA256 hash;
        CryptoPP::byte digest[CryptoPP::SHA256::DIGESTSIZE];
        hash.CalculateDigest(digest, (CryptoPP::byte*)mb.c_str(), mb.length());

        CryptoPP::HexEncoder encoder;
        std::string sKey;
        encoder.Attach(new CryptoPP::StringSink(sKey));
        encoder.Put(digest, sizeof(digest));
        encoder.MessageEnd();

        CryptoPP::byte key[CryptoPP::AES::MAX_KEYLENGTH];
        CryptoPP::byte  iv[CryptoPP::AES::BLOCKSIZE];
        memcpy(key, sKey.c_str(), CryptoPP::AES::MAX_KEYLENGTH);;
        memset(iv, 0x00, CryptoPP::AES::BLOCKSIZE);

        CryptoPP::AES::Decryption aesDecryption(key, CryptoPP::AES::MAX_KEYLENGTH);
        CryptoPP::CBC_Mode_ExternalCipher::Decryption cbcDecryption(aesDecryption, iv);

        CryptoPP::StreamTransformationFilter stfDecryptor(cbcDecryption, new CryptoPP::StringSink(decryptedtext));
        stfDecryptor.Put(reinterpret_cast<const unsigned char*>(ciphertext.c_str()), ciphertext.size());
        stfDecryptor.MessageEnd();

        return ConvertGUID(decryptedtext);
    }
    return "";
}