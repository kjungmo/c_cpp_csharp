#define _CRT_SECURE_NO_WARNINGS
#include <stdio.h>  

struct USERDATA
{
    int nAge;
    char szName[16];
    char szPhone[16];
};

void main()
{
    struct USERDATA UserData = {0};
    UserData.nAge = 20;
    sprintf(UserData.szName, "%s", "Jung Mo Kang");
    sprintf(UserData.szPhone, "%s", "111-2222");
}