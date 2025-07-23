#pragma once   // birden fazla include yazýmýný önler, sadece bir kez include eder.
#include <cstdint> //sabit uzunluklu türleri tanýmlar, örneðin uint16_t, uint32_t gibi.
#ifdef BUILD_DLL  
#define DLL_API __declspec(dllexport) // dll in içindeki fonksiyonlarý dýþarýya açar, diðer programlar bu dll i kullanabilir.
#else
#define DLL_API __declspec(dllimport) // dll in içindeki fonksiyonlarý içeriye  açar, diðer programlar bu dll i kullanabilir.   
#endif
extern "C" {                                             // dll deki dosyanýn imzasýný gösterir.Her yerde extern "c" yazmak zorunda kalmayýz, include ile çaðrýlýr.
    DLL_API int ConvertBinToWavSimple(
        const char* binPath,
        const char* wavPath,
        uint16_t numChannels,
        uint32_t sampleRate,
        uint16_t bitsPerSample,
        uint32_t durationSeconds,
        bool isFromEnd);
}