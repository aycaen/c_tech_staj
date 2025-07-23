#include "pch.h"
#include <fstream>
#include <cstdint>
#define BUILD_DLL
#include "bin_to_wav.h"
#pragma pack(push, 1) // boşluk bytları kullanma der, 1 byte hizalama kullanır. WAV dosyası tam byte hizalaması ister.

struct WAVHeader {
    char riff[4] = { 'R', 'I', 'F', 'F' };          // dosya türü 'RIFF' ile başlar.
    uint32_t chunkSize;                      // dosyanın toplam boyutu - 8 byte
    char wave[4] = { 'W', 'A', 'V', 'E' };      // ses dosyasının türünü söyler.
    char fmt[4] = { 'f', 'm', 't', ' ' };       // format bloğu başlığı
    uint32_t subchunk1Size = 16;           // format bloğu boyutu (PCM için 16 byte)
    uint16_t audioFormat = 1;            // PCM (sıkıştırmasız)
    uint16_t numChannels;
    uint32_t sampleRate;
    uint32_t byteRate;
    uint16_t blockAlign;
    uint16_t bitsPerSample;
    char data[4] = { 'd', 'a', 't', 'a' };   // veri bloğu başlığı
    uint32_t subchunk2Size;                  // veri boyutu 
};
#pragma pack(pop) // işlemler bittikten sonra eski hizalama ayarlarına geri döndürür

int ConvertBinToWavSimple(
    const char* binPath,
    const char* wavPath,
    uint16_t numChannels,    // sesin kaç kanalı var (1 mono, 2 stereo)
    uint32_t sampleRate,     // saniyedeki örnek sayısı ( 1 saniyede kaç ses örneği alınacağı)
    uint16_t bitsPerSample,  // her bir örneğin kaç bit olduğu
    uint32_t durationSeconds, // 0 gelirse tam dosya kabul edilecek
    bool isFromEnd)
{
    uint32_t bytePerSample = bitsPerSample / 8;  // her ses örneğinin kaç byte olduğunu hesaplar
    uint32_t byteRate = sampleRate * numChannels * bytePerSample; //1 saniyelik ses verisinin kaç byte tuttuğunu hesaplar.

    std::ifstream in(binPath, std::ios::binary | std::ios::ate);


    uint64_t totalSize = static_cast<uint64_t>(in.tellg()); // dosyanın toplam boyutunu alır

    // Eğer durationSeconds 0 ise tüm dosya kullanılacak şekilde ayarla
    uint64_t dataSize;
    if (durationSeconds == 0) {
        dataSize = totalSize;
    }
    else {
        dataSize = static_cast<uint64_t>(durationSeconds) * byteRate; // istenen süreyi byte cinsine çevirir
        if (dataSize > totalSize) return 3; // istenen süre dosyadan uzun olamaz
    }

    uint64_t startByte;
    if (isFromEnd) {
        if (dataSize > totalSize) return 3;
        startByte = totalSize - dataSize;
    }
    else {
        startByte = 0;
    }

    in.seekg(startByte, std::ios::beg); // dosyanın başından istenilen byte konumuna getirilir.

    WAVHeader header;
    header.numChannels = numChannels;
    header.sampleRate = sampleRate;
    header.bitsPerSample = bitsPerSample;
    header.subchunk2Size = static_cast<uint32_t>(dataSize);
    header.byteRate = byteRate;
    header.blockAlign = numChannels * bytePerSample;
    header.chunkSize = 36 + header.subchunk2Size;

    std::ofstream out(wavPath, std::ios::binary);
    in.rdbuf()->pubsetbuf(nullptr, 0);

    out.write(reinterpret_cast<char*>(&header), sizeof(WAVHeader)); // header isimli yapının bellekteki ham halini dosyaya yazar.

    const size_t bufferSize = 262144; // dosyayı küçük parçalar halinde okuma ve yazma için buffer boyutu 256 KB
    char buffer[bufferSize]; // okunan veriyi geçici olarak tutacak bir bellek alanı (buffer) oluşturur.
    uint64_t remaining = dataSize; // daha kaç byte lık veri okunması gerektiğini tutar.
    while (remaining > 0) {            // veri kalmayana kadar belli miktar veri okur ve kalan veri miktarı azaltılır.
        size_t toRead = static_cast<size_t>(std::min<uint64_t>(bufferSize, remaining)); // kalan veri miktarıyla bellek kapasitesi arasından küçük olanu seç ve toread değişkenine ata
        in.read(buffer, toRead);  // dosyadan toRead kadar veri okur
        out.write(buffer, toRead);
        remaining -= toRead;
    }

    in.close();
    out.close();

    return 0;
}