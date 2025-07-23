#include <zmq.hpp>
#include <nlohmann/json.hpp>
#include <iostream>
#include <fstream>
#include <filesystem>
#include "bin_to_wav.h"
using json = nlohmann::json;


int ConvertBinToWavSimple(
    const char* binPath,
    const char* wavPath,
    uint16_t numChannels,
    uint32_t sampleRate,
    uint16_t bitsPerSample,
    uint32_t durationSeconds,
    bool isFromEnd);


int main() {
    zmq::context_t ctx(1);
    zmq::socket_t sock(ctx, zmq::socket_type::rep);
    sock.bind("tcp://*:5555");
    std::cout << "ZMQ Sunucu başlatıldı (port 5555)\n";

    while (true) {
        zmq::message_t req;
        sock.recv(req, zmq::recv_flags::none);

        std::string reqStr(static_cast<char*>(req.data()), req.size());
        nlohmann::json input;
        try {
            input = nlohmann::json::parse(reqStr);
        }
        catch (...) {
            sock.send(zmq::buffer("{\"status\":\"json_error\"}"), zmq::send_flags::none);
            continue;
        }

        std::string binPath = input["binPath"];
        std::string outputFolder = input["outputFolder"];
        uint16_t    numChannels = input["numChannels"];
        uint32_t    sampleRate = input["sampleRate"];
        uint16_t    bitsPerSample = input["bitsPerSample"];

        std::ifstream in(binPath, std::ios::binary | std::ios::ate);
        if (!in) {
            sock.send(zmq::buffer("{\"status\":\"file_error\"}"), zmq::send_flags::none);
            continue;
        }
        uint64_t totalSize = in.tellg();
        in.close();

        uint64_t bytesPerSecond = uint64_t(sampleRate) * numChannels * (bitsPerSample / 8); // bir saniyelik ses verisi kaç byte
        uint32_t totalSeconds = static_cast<uint32_t>(totalSize / bytesPerSecond); //toplam saniye 

        std::vector<std::string> outputs;

        // 2 saatlik dosyaları oluştur (2h, 4h, 6h…)
        for (uint32_t hour = 2; hour < totalSeconds / 3600; hour += 2) {
            uint32_t durationSec = hour * 3600; // kaç saniyelik dosya oluşturulacak
            std::string wavPath = outputFolder + "/ilk_" + std::to_string(hour) + "saat.wav";
            std::cout << "Donusturuluyor: " << wavPath << std::endl;



            if (ConvertBinToWavSimple(
                binPath.c_str(), wavPath.c_str(),
                numChannels, sampleRate, bitsPerSample,
                durationSec, false) == 0)
            {

                outputs.push_back(wavPath);
            }
        }

        // Tamamı (saat ve kalan dakika)
        if (totalSeconds > 0) {
            uint32_t totalHours = totalSeconds / 3600; // toplam saat 
            uint32_t remainingMin = (totalSeconds % 3600) / 60; // kalan dakika sayısını hesaplar.
            std::string finalName = remainingMin == 0
                ? "ilk_" + std::to_string(totalHours) + "saat.wav"
                : "tamami_" + std::to_string(totalHours) + "saat_" + std::to_string(remainingMin) + "dk.wav";
            std::string finalPath = outputFolder + "/" + finalName;
            std::cout << "Donusturuluyor (tamami): " << finalPath << std::endl;
            if (ConvertBinToWavSimple(
                binPath.c_str(), finalPath.c_str(),
                numChannels, sampleRate, bitsPerSample,
                totalSeconds, false) == 0)
            {
                outputs.push_back(finalPath);
            }
        }

        nlohmann::json reply = {
            {"status", "success"},
            {"outputs", outputs}
        };
        sock.send(zmq::buffer(reply.dump()), zmq::send_flags::none);
    }

    return 0;
}