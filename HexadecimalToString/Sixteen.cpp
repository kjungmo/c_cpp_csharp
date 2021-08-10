#define KEY1_FILE "Profile.txt"
#define KEY2_FILE "Info.txt"
#define INTRUSION_FILE "wtfjm.dat"
#define MAC_GUID_FILE "rnlel.dat"
#define MoreAt "More at cogaplex@cogaplex.com."
#define CUSERS "C:\\Users\\"
#define LOCALLOW "\\AppData\\LocalLow"
#define LOCAL "\\AppData\\Local"

#include <iostream> 
#include <sstream>  
#include "obfuscator.hpp"

int HexCharToInt(char);
std::string HexToString(std::string);

int main()
{
    std::string str;
    std::string strPro = OBFUSCATE(KEY1_FILE);
    std::string strInfo = OBFUSCATE(KEY2_FILE);
    std::string strWtf = OBFUSCATE(INTRUSION_FILE);
    std::string strRnlel = OBFUSCATE(MAC_GUID_FILE);
    std::string strMore = OBFUSCATE(MoreAt);
    std::stringstream str1;
    std::stringstream profi;
    std::stringstream info;
    std::stringstream wtfjm;
    std::stringstream rnlel;
    std::stringstream more;

    str = "hello World";
    for (int i = 0; i < str.length(); i++)
    {
        str1 << std::hex << (int)str.at(i);
    }

    for (int i = 0; i < strPro.length(); i++)
    {
        profi << std::hex << (int)strPro.at(i);
    }

    for (int i = 0; i < strInfo.length(); i++)
    {
        info << std::hex << (int)strInfo.at(i);
    }
    
    for (int i = 0; i < strWtf.length(); i++)
    {
        wtfjm << std::hex << (int)strWtf.at(i);
    }
    
    for (int i = 0; i < strRnlel.length(); i++)
    {
        rnlel << std::hex << (int)strRnlel.at(i);
    }  

    for (int i = 0; i < strMore.length(); i++)
    {
        more << std::hex << (int)strMore.at(i);
    }


    std::cout << "str1 is hex " << str1.str() << "\n";
    std::cout << "strPro is hex " << profi.str() << "\n";
    std::cout << "strInfo is hex " << info.str() << "\n";
    std::cout << "strWtf is hex " << wtfjm.str() << "\n";
    std::cout << "strRnlel is hex " << rnlel.str() << "\n";
    std::cout << "strRnlel is hex " << more.str() << "\n";

    //strPro is hex 50726f66696c652e747874
    //strInfo is hex 496e666f2e747874
    //strWtf is hex 7774666a6d2e646174
    //strRnlel is hex 726e6c656c2e646174
    //strRnlel is hex 4d6f726520617420636f6761706c657840636f6761706c65782e636f6d2e



    std::string test = "48656c6c6f20576f726c64";
    std::string testPro = "50726f66696c652e747874";
    std::string testInfo = "496e666f2e747874";
    std::string testWtf = "7774666a6d2e646174";
    std::string testRnl = "726e6c656c2e646174";
    std::string testMore = "4d6f726520617420636f6761706c657840636f6761706c65782e636f6d2e";

    std::cout << "from hex " << HexToString(test) << "\n";
    std::cout << "from hex testPro " << HexToString(testPro) << "\n";
    std::cout << "from hex testInfo " << HexToString(testInfo) << "\n";
    std::cout << "from hex testWtf " << HexToString(testWtf) << "\n";
    std::cout << "from hex testRnl " << HexToString(testRnl) << "\n";
    std::cout << "from hex testMore " << HexToString(testMore) << "\n";

    std::cout << OBFUSCATE("snowapril") << std::endl;
    return 0;
}

std::string HexToString(std::string str)
{
    std::stringstream HexString;
    for (int i = 0; i < str.length(); i++)
    {
        char a = str.at(i++);
        char b = str.at(i);
        int x = HexCharToInt(a);
        int y = HexCharToInt(b);
        HexString << (char)((16 * x) + y);
    }
    return HexString.str();
}

int HexCharToInt(char a)
{
    if (a >= '0' && a <= '9')
        return (a - 48);
    else if (a >= 'A' && a <= 'Z')
        return (a - 55);
    else
        return (a - 87);
}