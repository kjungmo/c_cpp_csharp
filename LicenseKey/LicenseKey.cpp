#include <stdio.h>
#include <Windows.h>
#include <iostream>
using namespace std;



int main_s()
{
    LONG IResult;
    HKEY hKey;
    DWORD dwType;
    DWORD dwBytes = 100;
    char buffer[100];

    IResult = RegOpenKeyEx(HKEY_LOCAL_MACHINE, "SOFTWARE\\COGAPLEX\\LICENSEKEY", 0, KEY_ALL_ACCESS | KEY_WOW64_64KEY, &hKey);
    if (IResult != ERROR_SUCCESS)
    {
        MessageBox(NULL, "Register Open Error", "Error", MB_OK);
    }

    IResult = RegQueryValueExA(hKey, "License Key", 0, &dwType, (LPBYTE)buffer, &dwBytes);
    if (IResult == ERROR_SUCCESS)
    {
        cout << buffer << endl;
        MessageBox(NULL, buffer, "Registry", MB_OK);
    }
    else
        MessageBox(NULL, "Register Read Error", "Error", MB_OK);

    RegCloseKey(hKey);

    system("pause");
    return 0;
}