#define FILE_NAME "Profile.txt"
#define FILE_NAME2 "Info.txt"
#define INTRUSION_NAME "wtfjm.dat"
#define MAC_GUID_NAME "id_gu.dat"

#include <iostream>
#include <fstream>
#include <iomanip>
#include <comdef.h>
#include <WbemIdl.h>
#include <Windows.h>
#include <assert.h>
#include <iphlpapi.h>
#include <string>
#include <Shlwapi.h>
#include <shlobj_core.h>
#include <algorithm>

#include "cryptopp850/sha.h"
#include "cryptopp850/filters.h"
#include "cryptopp850/base64.h"

#include "cryptopp850/modes.h"
#include "cryptopp850/aes.h"
#include "cryptopp850/hex.h"

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

bool GetWMI(std::string&, std::string&, std::string&, std::string&, bool initCOM = true);
bool GetWMIUserAccount(std::string&, bool initCOM = true);
bool GetWMIMacGUID(std::string&, bool initCOM = true);
std::string QueryWMI(IWbemServices*, IWbemLocator*, const char*, const wchar_t*);
std::string CreateLicenseKey(std::string, std::string, std::string, bool);
std::string CreateMacGuidKey(std::string, bool);
std::string SHA256HashString(std::string);
bool WriteToFile(std::string, std::string);
std::string ReadFromFile(std::string);
std::string CreateIntrusionPath(std::string, std::string, bool initCOM = true);
std::string CreateKeyPath(std::string, std::string, bool initCOM = true);
std::string CreateKeyPathAnother(std::string, bool initCOM = true);
std::string CreateMacGuidPath(std::string, std::string, bool initCOM = true);
std::string GetGPUSerial(std::string);
bool SplitString(std::string&, std::string&);
bool RemoveSymbols(std::string&);
bool ConvertGUID(std::string&);



int main()
{
	/*time_t startG, endG, startV, endV;
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
	return 0;*/

	time_t startAES, endAES, startHASH, endHASH, startCREATEKEY, endCREATEKEY, startENCRYPT, endENCRYPT, startDECRYPT, endDECRYPT; //****************************************************************************************************************************************************************************************
	double resultAES, resultHASH, resultCREATEKEY, resultENCRYPT, resultDECRYPT; //****************************************************************************************************************************************************************************************

	startAES = clock(); //****************************************************************************************************************************************************************************************

	CryptoPP::SHA256 hash;  //****************************************************************************************************************************************************************************************
	CryptoPP::byte digest[CryptoPP::SHA256::DIGESTSIZE];


	std::string mb, uuid, guid, mac; 
	GetWMI(mb, uuid, guid, mac, true);
	std::string message = mb;
	std::cout << "motherboard : " << mb << std::endl;

	startHASH = clock(); //****************************************************************************************************************************************************************************************
	hash.CalculateDigest(digest, (CryptoPP::byte*)message.c_str(), message.length());

	CryptoPP::HexEncoder encoder;
	std::string sKey;
	encoder.Attach(new CryptoPP::StringSink(sKey));
	encoder.Put(digest, sizeof(digest));
	encoder.MessageEnd();
	endHASH = clock(); //****************************************************************************************************************************************************************************************

	startCREATEKEY = clock(); //****************************************************************************************************************************************************************************************
	CryptoPP::byte key[CryptoPP::AES::MAX_KEYLENGTH]; //16 Bytes MAXKEYLENGTH 32 BYTES(SHA 256)
	CryptoPP::byte  iv[CryptoPP::AES::BLOCKSIZE];
	memcpy(key, sKey.c_str(), CryptoPP::AES::MAX_KEYLENGTH);;
	memset(iv, 0x00, CryptoPP::AES::BLOCKSIZE);
	endCREATEKEY = clock(); //****************************************************************************************************************************************************************************************

	//
	// String and Sink setup
	//
	std::string plaintext = guid;
	std::string ciphertext;
	std::string decryptedtext;

	//
	// Dump Plain Text
	//
	std::cout << "Plain Text (" << plaintext.size() << " bytes)" << std::endl;
	std::cout << plaintext;
	std::cout << std::endl << std::endl;

	//
	// Create Cipher Text
	//
	startENCRYPT = clock(); //****************************************************************************************************************************************************************************************
	CryptoPP::AES::Encryption aesEncryption(key, CryptoPP::AES::MAX_KEYLENGTH);
	CryptoPP::CBC_Mode_ExternalCipher::Encryption cbcEncryption(aesEncryption, iv);

	CryptoPP::StreamTransformationFilter stfEncryptor(cbcEncryption, new CryptoPP::StringSink(ciphertext));
	RemoveSymbols(plaintext);
	stfEncryptor.Put(reinterpret_cast<const unsigned char*>(plaintext.c_str()), plaintext.length());
	stfEncryptor.MessageEnd();
	endENCRYPT = clock(); //****************************************************************************************************************************************************************************************
	//
	// Dump Cipher Text
	//
	std::cout << "Cipher Text (" << ciphertext.size() << " bytes)" << std::endl;

	for (int i = 0; i < ciphertext.size(); i++) {

		std::cout << "0x" << std::hex << (0xFF & static_cast<CryptoPP::byte>(ciphertext[i])) << " ";
	}
	std::cout << "\nCipherText : " << ciphertext << std::endl;

	WriteToFile("D:\\ZipTest\\CipherText.txt", ciphertext);
	std::string textciphered = ReadFromFile("D:\\ZipTest\\CipherText.txt");
	std::cout << "textciphered : " << textciphered << std::endl;

	std::cout << std::endl << std::endl;

	//
	// Decrypt
	//
	startDECRYPT = clock(); 
	CryptoPP::AES::Decryption aesDecryption(key, CryptoPP::AES::MAX_KEYLENGTH);
	CryptoPP::CBC_Mode_ExternalCipher::Decryption cbcDecryption(aesDecryption, iv);

	CryptoPP::StreamTransformationFilter stfDecryptor(cbcDecryption, new CryptoPP::StringSink(decryptedtext));
	stfDecryptor.Put(reinterpret_cast<const unsigned char*>(textciphered.c_str()), textciphered.size());
	stfDecryptor.MessageEnd();
	endDECRYPT = clock(); //****************************************************************************************************************************************************************************************

	ConvertGUID(decryptedtext);
	//
	// Dump Decrypted Text
	//
	std::cout << "Decrypted Text: " << std::endl;
	std::cout << decryptedtext;
	std::cout << std::endl << std::endl;//****************************************************************************************************************************************************************************************


	resultAES = (double)(endAES - startAES);
	resultHASH = (double)(endHASH - startHASH);
	resultCREATEKEY = (double)(endCREATEKEY - startCREATEKEY);
	resultENCRYPT = (double)(endENCRYPT - startENCRYPT);
	resultDECRYPT = (double)(endDECRYPT - startDECRYPT);
	std::cout << "resultAES : " << resultAES << "\nresultHASH : " << resultHASH << "\nresultCREATEKEY : " << resultCREATEKEY << "\nresultENCRYPT : " << resultENCRYPT << "\nresultDECRYPT : " << resultDECRYPT << std::endl;

	return 0;


}

const char* GenerateLicenseKey(bool initCOM)
{
	time_t startGETUSERACCOUNT, endGETUSERACCOUNT, startGETWMI, endGETWMI, startCREATELK, endCREATELK, startMACGUIDKEY, endMACGUIDKEY;
	double resultGETUSERACCOUNT, resultGETWMI, resultCREATELK, resultMACGUIDKEY;


//#ifdef GENERATOR_BUILD
	std::string userAsBrain;
	startGETUSERACCOUNT = clock();
	GetWMIUserAccount(userAsBrain, true);
	endGETUSERACCOUNT = clock();
	std::string intrusion = CreateIntrusionPath(INTRUSION_NAME, userAsBrain, initCOM);
	std::string lkeyfile = CreateKeyPath(FILE_NAME, userAsBrain, initCOM);
	std::string lkeyfileAnother = CreateKeyPathAnother(FILE_NAME2, initCOM);
	std::string macguidFile = CreateMacGuidPath(MAC_GUID_NAME, userAsBrain, initCOM);

	if (PathFileExists(intrusion.c_str()))
	{
		remove(intrusion.c_str());
	}

	if (PathFileExists(lkeyfile.c_str()))
	{
		remove(lkeyfile.c_str());
	}
	if (PathFileExists(lkeyfileAnother.c_str()))
	{
		remove(lkeyfileAnother.c_str());
	}
	if (PathFileExists(macguidFile.c_str()))
	{
		remove(macguidFile.c_str());
	}

	std::string mBoard, uuid, guid, mac;
	startGETWMI = clock();

	GetWMI(mBoard, uuid, guid, mac, initCOM);
	//std::cout << "mboard : " << mBoard << "\nuuid : " << uuid << "\nguid : " << guid << "\nmac : " << mac << std::endl;  //****************************
	endGETWMI = clock();
	startMACGUIDKEY = clock();

	std::string macGuidKey = CreateMacGuidKey(guid, initCOM);
	endMACGUIDKEY = clock();

	startCREATELK = clock();

	std::string licenseKey = CreateLicenseKey(mBoard, uuid, mac, initCOM);
	endCREATELK = clock();
	std::cout << "macGuidKey : " << macGuidKey  << "\nlicenseKey : " << licenseKey << std::endl;
	if (licenseKey.empty() || !WriteToFile(lkeyfile, licenseKey) || !WriteToFile(lkeyfileAnother, licenseKey) || !WriteToFile(macguidFile, macGuidKey))
	{
		licenseKey = "";
	}
	
	resultGETUSERACCOUNT = (double)(endGETUSERACCOUNT - startGETUSERACCOUNT);
	resultGETWMI = (double)(endGETWMI - startGETWMI);
	resultMACGUIDKEY = (double)(endMACGUIDKEY - startMACGUIDKEY);
	resultCREATELK = (double)(endCREATELK - startCREATELK);

	std::cout << "resultGETUSERACCOUNT : " << resultGETUSERACCOUNT << "\nresultGETWMI : " << resultGETWMI << "\nresultMACGUIDKEY : " << resultMACGUIDKEY << "\nresultCREATELK : " << resultCREATELK << std::endl;
	strcpy_s(LkBuf, licenseKey.length() + 1, licenseKey.c_str());
//#endif
	return LkBuf;
}

bool ValidateLicenseKey(bool initCOM)
{
	time_t startCONVERT, endCONVERT, startGETWMIWITHGUID, endGETWMIWITHGUID;
	double resultCONVERT, resultGETWMIWITHGUID;


	std::string userAsBrain;
	GetWMIUserAccount(userAsBrain, initCOM);
	std::string intrusion = CreateIntrusionPath(INTRUSION_NAME, userAsBrain, initCOM);
	std::string lkeyfile = CreateKeyPath(FILE_NAME, userAsBrain, initCOM);
	std::string lkeyfileAnother = CreateKeyPathAnother(FILE_NAME2, initCOM);
	std::string macguidFile = CreateMacGuidPath(MAC_GUID_NAME, userAsBrain, initCOM);  

	std::string file = ReadFromFile(lkeyfile);
	std::string fileAnother = ReadFromFile(lkeyfileAnother);
	std::string filemacguid = ReadFromFile(macguidFile); 

	std::string mBoard, uuid, mac;

	startCONVERT = clock();
	if (ConvertGUID(filemacguid))
	{
		endCONVERT = clock();
		resultCONVERT = (double)(endCONVERT - startCONVERT);
		startGETWMIWITHGUID = clock();
		GetWMI(mBoard, uuid, filemacguid, mac, initCOM);
		endGETWMIWITHGUID = clock();
		resultGETWMIWITHGUID = (double)(endGETWMIWITHGUID - startGETWMIWITHGUID);
		std::cout << "resultCONVERT : " << resultCONVERT << "\nresultGETWMIWITHGUID : " << resultGETWMIWITHGUID << std::endl;
		//std::cout << "mboard : " << mBoard << "\nuuid : " << uuid << "\nfilemacguid : " << filemacguid << "\nmac : " << mac << std::endl; //**********************************
	}
	std::string sha = CreateLicenseKey(mBoard, uuid, mac, initCOM);

	if (!file.empty() && !fileAnother.empty() && !filemacguid.empty() && !sha.empty())
	{
		if (!PathFileExists(intrusion.c_str()) && file == sha && fileAnother == sha)
		{
			return true;
		}
	}

	std::string detector = "More at cogaplex@cogaplex.com.";
	std::ofstream LicenseDetector;
	LicenseDetector.open(intrusion);
	SetFileAttributes(intrusion.c_str(), FILE_ATTRIBUTE_HIDDEN);
	LicenseDetector.write(detector.c_str(), detector.size());
	LicenseDetector.close();
	remove(lkeyfile.c_str());
	remove(lkeyfileAnother.c_str());
	remove(macguidFile.c_str()); 


	return false;
}

std::string CreateLicenseKey(std::string mBoard, std::string uuid, std::string mac, bool initCOM)
{
	if (!(mBoard.empty() && uuid.empty() && mac.empty()))
	{
		std::string toBeEncrypted;
		std::string mBoard2, uuid2, mac2;
		if (RemoveSymbols(mBoard) && RemoveSymbols(uuid) && RemoveSymbols(mac))
		{
			if (SplitString(mBoard, mBoard2) && SplitString(uuid, uuid2) && SplitString(mac, mac2))
			{
				toBeEncrypted = SHA256HashString(mac2 + uuid + mBoard2) + SHA256HashString(mBoard + mac + uuid2);
			}
		}
		return SHA256HashString(toBeEncrypted);
	}
	return "";
}

std::string CreateMacGuidKey(std::string guid, bool initCOM)
{
	if (guid.length() != 0)
	{
		RemoveSymbols(guid);
		return guid;
	}
	/*std::string toBeEncrypted = SHA256HashString(guid)*/;
	//std::cout << "CreateMacGuidKey GUID : " << guid << std::endl; //********************************************
	return "";
}

bool GetWMI(std::string& mb, std::string& uuid, std::string& guid, std::string& mac, bool initCOM)
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
	time_t startMB, endMB, startUUID, endUUID, startGUID, endGUID, startMAC, endMAC;  //********************************************
	double resultMB, resultUUID, resultGUID, resultMAC; //********************************************
	startMB = clock(); //********************************************
	mb = QueryWMI(pSvc, pLoc, "SELECT * FROM Win32_BaseBoard", L"SerialNumber");
	endMB = clock(); //********************************************
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
	mac = QueryWMI(pSvc, pLoc, ("SELECT * FROM Win32_NetworkAdapter WHERE GUID = '" + guid + "'").c_str(), L"MACAddress");
	endMAC = clock(); //********************************************

	pSvc->Release();
	pLoc->Release();
	CoUninitialize();

	resultMB = (double)(endMB - startMB); //********************************************
	resultUUID = (double)(endUUID - startUUID); //********************************************
	resultGUID = (double)(endGUID - startGUID); //********************************************
	resultMAC = (double)(endMAC - startMAC); //********************************************
	std::cout << "resultMB : " << resultMB << "\nresultUUID : " << resultUUID << "\nresultGUID : " << resultGUID << "\nresultMAC :" << resultMAC << std::endl; //********************************************

	return !(mb.empty() || uuid.empty() || guid.empty() || mac.empty());
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

bool GetWMIMacGUID(std::string& guid, bool initCOM)
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

	guid = QueryWMI(pSvc, pLoc, "SELECT * FROM Win32_NetworkAdapter WHERE NetConnectionID = 'Ethernet' OR NetConnectionID = '이더넷'", L"GUID");
	if (guid.length() == 0)
	{
		guid = QueryWMI(pSvc, pLoc, "SELECT * FROM Win32_NetworkAdapter WHERE NetConnectionID = 'Ethernet 2' OR NetConnectionID = '이더넷 2'", L"GUID");
	}

	pSvc->Release();
	pLoc->Release();
	CoUninitialize();
	return !(guid.empty());
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

bool WriteToFile(std::string filepath, std::string liKey)
{
	std::ofstream writeFile;

	writeFile.open(filepath);
	SetFileAttributes(filepath.c_str(), FILE_ATTRIBUTE_HIDDEN);
	if (liKey.empty())
	{
		writeFile.close();
		return false;
	}
	writeFile.write(liKey.c_str(), liKey.size());
	writeFile.close();
	return true;
}

std::string ReadFromFile(std::string filepath)
{
	std::ifstream rfile(filepath);
	std::string licenseKey;
	if (rfile.is_open())
	{
		rfile >> licenseKey;
	}
	rfile.close();
	return licenseKey;
}

std::string CreateIntrusionPath(std::string filename, std::string brain, bool initCOM)
{
	std::string brainPath = "C:\\Users\\" + brain + "\\AppData\\LocalLow\\" + filename;
	return brainPath;
}

std::string CreateKeyPath(std::string filename, std::string brain, bool initCOM)
{
	std::string brainPath = "C:\\Users\\" + brain + "\\AppData\\Local\\" + filename;
	return brainPath;
}

std::string CreateKeyPathAnother(std::string filename, bool initCOM)
{
	std::string brainPath = "C:\\Users\\Public\\Documents\\" + filename;
	return brainPath;
}

std::string CreateMacGuidPath(std::string filename, std::string brain, bool initCOM)
{
	std::string brainPath = "C:\\Users\\" + brain + "\\AppData\\Local\\Packages\\" + filename;
	return brainPath;
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
		std::string symbols = "!@#$%^&*:;()_-=+{}[].,/\\";
		letters.erase(std::remove_if(letters.begin(), letters.end(),
			[&symbols](const char& c) {
				return symbols.find(c) != std::string::npos;
			}),
			letters.end());
		//std::cout << letters << std::endl;
	}
	return !(letters.empty());
}

bool ConvertGUID(std::string& guidRead)
{
	guidRead = "{" +
		guidRead.substr(0, 8) + "-" +
		guidRead.substr(8, 4) + "-" +
		guidRead.substr(12, 4) + "-" +
		guidRead.substr(16, 4) + "-" +
		guidRead.substr(20, 12) + "}";
	return true;
}