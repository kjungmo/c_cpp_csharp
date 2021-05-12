//#include "sha.h"
//#include "filters.h"
//#include "base64.h"
//
//#include <iostream>
//#include <iomanip>
//
//std::string SHA256HashString(std::string aString) {
//    std::string digest;
//    CryptoPP::SHA256 hash;
//
//    CryptoPP::StringSource foo(aString, true,
//        new CryptoPP::HashFilter(hash,
//            new CryptoPP::Base64Encoder(
//                new CryptoPP::StringSink(digest))));
//
//    return digest;
//}
//int mains()
//{
//    std::cout << SHA256HashString("abc") << std::endl;
//    system("pause");
//}
