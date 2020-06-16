#pragma once

#include <thread>
#include <Windows.h>
#include <functional>
#include <deque>
#include <vector>
#include <mutex>

class PipeServer
{
public:
    typedef std::function<void(std::vector<unsigned char>)> dataReceivedCallback;
    explicit PipeServer(dataReceivedCallback dataReceivedCallback);
    ~PipeServer();
    void writeToPipe(const std::vector<unsigned char>& data);

private:
    static constexpr unsigned int BUFSIZE = 512;
    static constexpr unsigned int PIPE_TIMEOUT = 0;
    static constexpr char PIPE_NAME[] = "\\\\.\\pipe\\cnc_buffer_spy";

    typedef struct
    {
        OVERLAPPED oOverlap;
        PipeServer* server;
    } PIPEINST; // overlap-expanding struct containing contextual info
    PIPEINST instance;

    unsigned char bufRead[BUFSIZE];
    unsigned char bufWrite[BUFSIZE];

    HANDLE hPipe;
    OVERLAPPED oConnect;
    OVERLAPPED oRead;
    OVERLAPPED oWrite;
    OVERLAPPED oNewData;
    bool keepAlive;
    BOOL writeInProgress = false;

    static DWORD WINAPI threadStart(void* param);
    void start();
    void stop();
    void threadFunc();
    bool createAndConnectPipe();
    void flushWriteQueue();

    HANDLE pipeThread;
    std::deque<unsigned char> writeQueue;
    std::mutex mtxWriteQueue;
    dataReceivedCallback onDataReceived;
};
