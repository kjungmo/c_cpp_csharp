//#include <stdio.h>
//#include <Windows.h>
//#include <iostream>
//
//void readRegistry(DWORD);
//void writeRegistry(const char*);
//
//int mains()
//{
//    //HKEY hKey;
//    //LONG result = 0;
//    //char filename[] = "C:\test.jpg";
//    //const char* path = "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run\\test";
//
//    //if (RegCreateKeyEx(HKEY_LOCAL_MACHINE, path, 0, NULL, REG_OPTION_NON_VOLATILE, KEY_WRITE, NULL, &hKey, NULL) == ERROR_SUCCESS)
//    //    printf("1. success \n"); // ← doesn`t work 
//    //else printf("fail\n");
//
//    //if (RegCreateKeyEx(HKEY_CURRENT_USER, "Console\papadaks", 0, NULL,
//    //    REG_OPTION_NON_VOLATILE, KEY_WRITE, NULL, &hKey, NULL) == ERROR_SUCCESS)
//    //    printf("2. success \n");  // ← works well 
//    //else printf("fail\n");
//
//    //return 0;
//
//    writeRegistry();
//    readRegistry2(REG_SZ);
//    return 0;
//
//}
//
//void readRegistry2(DWORD type)
//{
//    HKEY key;
//    TCHAR value[255];
//    DWORD value_length = 255;
//    RegOpenKey(HKEY_LOCAL_MACHINE, "SOFTWARE\\COGAPLEX\\LICENSEKEY", &key);
//    RegQueryValueEx(key, "License Key", NULL, &type, (LPBYTE)&value, &value_length);
//    RegCloseKey(key);
//    std::cout << value << std::endl;
//    LONG IResult;
//    HKEY hKey;
//    DWORD dwType;
//    DWORD dwBytes = 100;
//    char buffer[100];
//
//    IResult = RegOpenKeyEx(HKEY_LOCAL_MACHINE, "SOFTWARE\\COGAPLEX\\LICENSEKEY", 0, KEY_ALL_ACCESS | KEY_WOW64_64KEY, &hKey);
//    if (IResult != ERROR_SUCCESS)
//    {
//        MessageBox(NULL, "Register Open Error", "Error", MB_OK);
//    }
//
//    IResult = RegQueryValueExA(hKey, "License Key", 0, &dwType, (LPBYTE)buffer, &dwBytes);
//    if (IResult == ERROR_SUCCESS)
//    {
//        std::cout << buffer << std::endl;
//        MessageBox(NULL, buffer, "Registry", MB_OK);
//    }
//    else
//        MessageBox(NULL, "Register Read Error", "Error", MB_OK);
//
//    RegCloseKey(hKey);
//
//    system("pause");
//    return;
//}
//
//void writeRegistry2(const char* value)
//{
//    HKEY key;
//    RegOpenKey(HKEY_LOCAL_MACHINE, "SOFTWARE\\COGAPLEX\\LICENSEKEY", &key);
//    RegSetValueEx(key, "License Key",0, REG_SZ, (LPBYTE)value, strlen(value) * sizeof(char));
//    RegCloseKey(key);
//
//    result = RegCreateKey(HKEY_LOCAL_MACHINE,"SOFTWARE\\COGAPLEX\\LICENSEKEY", 0, NULL, REG_OPTION_NON_VOLATILE, KEY_WRITE, NULL, &hKey, NULL);
//
//    if (result == ERROR_SUCCESS)
//        std::cout << "Success!" << std::endl;
//
//}