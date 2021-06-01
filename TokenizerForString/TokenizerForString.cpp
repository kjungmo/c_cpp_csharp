#include <iostream>
#include <string>
#include <vector>
#include <algorithm>

void tokenize(const std::string& s, const char delim,
    std::vector<std::string>& out)
{
    std::string::size_type beg = 0;
    for (auto end = 0; (end = s.find(delim, end)) != std::string::npos; ++end)
    {
        out.push_back(s.substr(beg, end - beg));
        beg = end + 1;
    }

    out.push_back(s.substr(beg));
}

std::string Tokenizer(std::string id, std::string delimeterA, std::string delimeterB)
{
    std::string strNew;
    unsigned split = id.find_last_of(delimeterA);
    strNew = id.substr(split + 1);
    split = strNew.find(delimeterB);
    strNew = strNew.substr(split + 1);
    split = strNew.find_first_of(delimeterB);
    strNew = strNew.substr(0, split);
    std::cout << strNew << std::endl;
    return strNew;
}

int main()
{

    std::string str = "PCI\\VEN_10DE&DEV_1E07&SUBSYS_1E0710DE&REV_A1\\4&28A4740D&0&0018";
    //unsigned first = str.find("\\");
    //std::string strNew = str.substr(first + 1);
    //unsigned second = strNew.find("\\");
    //strNew = strNew.substr(second + 1);
    //unsigned third = strNew.find("&");
    //strNew = strNew.substr(third + 1);
    //unsigned fourth = strNew.find("&");
    //strNew = strNew.substr(0, fourth);
    //std::cout << strNew << std::endl;
    Tokenizer(str, "\\", "&");
    system("pause");
    return 0;
}