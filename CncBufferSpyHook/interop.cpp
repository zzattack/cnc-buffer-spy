#include <windows.h>
#include <detours.h>
#include <processthreadsapi.h>
#include <Tlhelp32.h>
#include <string>
#include <filesystem>

#include "main.h"

__declspec(dllexport) bool __cdecl StartProcess(const char* processName)
{
	STARTUPINFO si;
	PROCESS_INFORMATION pi;
	ZeroMemory(&si, sizeof(STARTUPINFO));
	ZeroMemory(&pi, sizeof(PROCESS_INFORMATION));
	si.cb = sizeof(STARTUPINFO);
    si.dwFlags |= CREATE_NEW_CONSOLE;

	char dll[MAX_PATH];
	GetModuleFileName(sInstanceHandle, dll, MAX_PATH);
    std::filesystem::path pth(processName);

	bool success = DetourCreateProcessWithDll(
		processName, // lpApplicationName
		NULL, // lpCommandLine
		NULL, // lpProcessAttributes
		NULL, // lpThreadAttributes
		false, // bInheritHandles
		CREATE_DEFAULT_ERROR_MODE | CREATE_NEW_CONSOLE, // dwCreationFlags
		NULL, // lpEnvironment
		pth.parent_path().string().c_str(), //lpCurrentDirectory
		&si, // lpStartupInfo
		&pi, // lpProcessInformation
		dll, // lpDllPath
		NULL); // pfCreateProcess

	return success;	
}

__declspec(dllexport) bool InjectToRunningProcess(const char* processName)
{
	// Find name of this DLL
	char dll[MAX_PATH];
	GetModuleFileName(sInstanceHandle, dll, MAX_PATH);

	PROCESSENTRY32 pe32;
	pe32.dwSize = sizeof(PROCESSENTRY32);
	HANDLE hTool32 = CreateToolhelp32Snapshot(TH32CS_SNAPPROCESS, NULL);
	BOOL bProcess = Process32First(hTool32, &pe32);

    std::filesystem::path pth(processName);
    std::string search = pth.filename().string();
    std::transform(search.begin(), search.end(), search.begin(), [](unsigned char c) { return std::tolower(c); });

	if (bProcess == TRUE)
	{
		while (Process32Next(hTool32, &pe32))
		{
            std::string match(pe32.szExeFile);
            std::transform(match.begin(), match.end(), match.begin(), [](unsigned char c) { return std::tolower(c); });
			if (search == match)
			{
				HANDLE hProcess = OpenProcess(PROCESS_CREATE_THREAD | PROCESS_VM_OPERATION |
					PROCESS_VM_WRITE, FALSE, pe32.th32ProcessID);
				LPVOID LoadLibraryAddr = (LPVOID)GetProcAddress(GetModuleHandle("kernel32.dll"),
					"LoadLibraryA");
				LPVOID LLParam = (LPVOID)VirtualAllocEx(hProcess, NULL, strlen(dll),
					MEM_RESERVE | MEM_COMMIT, PAGE_READWRITE);
				WriteProcessMemory(hProcess, LLParam, dll, strlen(dll), NULL);
				CreateRemoteThread(hProcess, NULL, NULL, (LPTHREAD_START_ROUTINE)LoadLibraryAddr,
					LLParam, NULL, NULL);
				CloseHandle(hProcess);
                return true;
			}
		}
	}
	CloseHandle(hTool32);
	return false;
}
