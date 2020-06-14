#include "pipeserver.h"
#include "main.h"

#include <cstdio>
#include <Windows.h>
#include <thread>
#include <algorithm>
#include <utility>


void PipeServer::start(dataReceivedCallback dataReceivedCallback)
{
    instance.server = this; // prepare OVERLAPPED struct with a pointer to ourselves
    onDataReceived = std::move(dataReceivedCallback);
    pipeThread = std::thread(&PipeServer::threadFunc, this);
}

void PipeServer::stop()
{
    pipeThread.join();
}

void PipeServer::threadFunc()
{
    // Prepare unnamed wait events in auto-reset mode
    oConnect.hEvent = CreateEvent(NULL, false, false, NULL);
    oRead.hEvent = CreateEvent(NULL, false, false, NULL);
    oWrite.hEvent = CreateEvent(NULL, false, false, NULL);
    oNewData.hEvent = CreateEvent(NULL, false, false, NULL);

    if (!oConnect.hEvent || !oRead.hEvent || !oWrite.hEvent || !oNewData.hEvent)
    {
        Log("CreateEvent failed with %d.\r\n", GetLastError());
        return;
    }

    // Call a subroutine to create one instance, and wait for the client to connect. 
    (void)createAndConnectPipe();

    while (true)
    {
        // Wait for a client to connect, or for a read or write operation to be completed, which
        // causes a completion routine to be queued for execution. 
        HANDLE waitHandles[] = { oConnect.hEvent, oRead.hEvent, oWrite.hEvent, oNewData.hEvent };
        DWORD completedHandle = WaitForMultipleObjects(4, waitHandles, false, INFINITE);

        switch (completedHandle)
        {
            case 0: // oConnect
                DWORD dummy;
                if (GetOverlappedResult(hPipe, &oConnect, &dummy, false))
                {
                    // Connected to pipe, begin reading.
                    if (!ReadFile(hPipe, bufRead, BUFSIZE, NULL, &oRead))
                        Log("ReadFile after ConnectNamePipe error: %d", GetLastError());

                    // If there's already stuff in buffer to be sent, start flushing.
                    flushWriteQueue();
                }
                else {
                    Log("GetOverlappedResult() after ConnectNamedPipe() error (%d)\r\n", GetLastError());
                }
                break;

            case 1: // oRead
                DWORD bytesRead;
                if (GetOverlappedResult(hPipe, &oRead, &bytesRead, false))
                {
                    // Successfully read, copy to local buffer
                    std::vector<unsigned char> v(bufRead, bufRead + bytesRead);

                    // Immediately start reading again
                    if (!ReadFile(hPipe, bufRead, BUFSIZE, NULL, &oRead))
                        Log("ReadFile error: %d", GetLastError());

                    // Dispatch to subscriber
                    if (bytesRead && onDataReceived)
                        onDataReceived(v);
                }
                else {
                    Log("GetOverlappedResult for read error: %d\r\n", GetLastError());
                    // client has gone; reconnect
                    (void)createAndConnectPipe();
                }

                break;

            case 2: // oWrite
                DWORD bytesWritten;
                if (!GetOverlappedResult(hPipe, &oWrite, &bytesWritten, false))
                {
                    Log("WriteFile error (%d)\r\n", GetLastError());
                }
                else {
                    // If more left in buffer, start another write operation
                    writeInProgress = false;
                    flushWriteQueue();
                }
                break;

            case 3: // oNewData
                // New data written to buffer. Flush it if not busy. Event auto-resets.
                flushWriteQueue();
                break;

            case WAIT_IO_COMPLETION:
                break;

            // An error occurred in the wait function. 
            default:
            {
                Log("WaitForMultipleObjects failed (%d)\r\n", GetLastError());
                return;
            }
        }
    }
}

void PipeServer::flushWriteQueue()
{
    if (!writeInProgress)
    {
        unsigned int bytesToWrite;
        // copy to bufWrite under lock
        std::lock_guard<std::mutex> lock(mtxWriteQueue);
        {
            bytesToWrite = std::min(BUFSIZE, writeQueue.size());
            for (unsigned i = 0; i < bytesToWrite; ++i) {
                bufWrite[i] = writeQueue.front();
                writeQueue.pop_front();
            }
        }

        if (bytesToWrite)
        {
            writeInProgress = WriteFile(hPipe, bufWrite, bytesToWrite, NULL, &oWrite);
            if (!writeInProgress)
            {
                Log("WriteFile() inside flushWriteQueue() failed: %d", GetLastError());
            }
        }
    }
}


// This function creates a pipe instance and connects to the client. It returns TRUE if
// the connect operation is pending, and FALSE if the connection has been completed. 
bool PipeServer::createAndConnectPipe()
{
    // Create pipe
    hPipe = CreateNamedPipe(
        PIPE_NAME,                // pipe name 
        PIPE_ACCESS_DUPLEX |      // read/write access 
        FILE_FLAG_OVERLAPPED,     // overlapped mode 
        PIPE_TYPE_BYTE |          // byte-type pipe 
        PIPE_READMODE_BYTE |      // byte read mode 
        PIPE_WAIT,                // blocking mode 
        PIPE_UNLIMITED_INSTANCES, // unlimited instances 
        BUFSIZE,                  // output buffer size 
        BUFSIZE,                  // input buffer size 
        PIPE_TIMEOUT,             // client time-out 
        NULL);                    // default security attributes

    if (hPipe == INVALID_HANDLE_VALUE)
    {
        Log("CreateNamedPipe failed with %d.\r\n", GetLastError());
        return false;
    }

    std::lock_guard<std::mutex> lock(mtxWriteQueue);
    writeQueue.clear();

    // Overlapped ConnectNamedPipe should return zero. 
    if (ConnectNamedPipe(hPipe, &oConnect))
    {
        Log("ConnectNamedPipe failed with %d.\r\n", GetLastError());
        return false;
    }

    switch (GetLastError())
    {
        // The overlapped connection is in progress. 
        case ERROR_IO_PENDING:
            return true;

            // Client is already connected, so signal an event. 
        case ERROR_PIPE_CONNECTED:
            Log("PIPE_CONNECTED already");
            if (SetEvent(oConnect.hEvent))
                break;

            // If an error occurs during the connect operation... 
        default:
        {
            Log("ConnectNamedPipe failed with %d.\r\n", GetLastError());
        }
    }
    return false;
}

void PipeServer::writeToPipe(const std::vector<unsigned char>& data)
{
    {
        std::lock_guard<std::mutex> lock(mtxWriteQueue);
        for (auto& i : data)
            writeQueue.push_back(i);
    }
    // signal availability of new data
    SetEvent(oNewData.hEvent);
}