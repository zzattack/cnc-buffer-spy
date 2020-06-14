#pragma once

#include <Windows.h>

extern HINSTANCE sInstanceHandle;
extern bool sIsHookedInstance;

void DumpAllocatedAreas();
void Log(const char* format, ...);