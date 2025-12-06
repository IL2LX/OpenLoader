#define _WIN32_WINNT 0x0A00 // Target Windows 10+ (works perfectly on Windows 11)
#include "pch.h"
#include <windows.h>
#include <metahost.h>
#include <string>
#include <mutex>

#pragma comment(lib, "mscoree.lib")

ICLRMetaHost* pMetaHost = nullptr;
ICLRRuntimeInfo* pRuntimeInfo = nullptr;
ICLRRuntimeHost* pClrRuntimeHost = nullptr;

static std::wstring g_vrchatDirCached;
static std::once_flag g_vrchatInitFlag;
static const wchar_t* REG_KEY_PATH = L"Software\\OpenLoader";
static const wchar_t* REG_VALUE_NAME = L"VRChatPath";

static void Log(const std::wstring& msg)
{
    wchar_t tempPath[MAX_PATH];
    GetTempPathW(MAX_PATH, tempPath);
    std::wstring logPath = std::wstring(tempPath) + L"OpenLoaderLog.txt";
    FILE* f = nullptr;
    _wfopen_s(&f, logPath.c_str(), L"a+, ccs=UTF-8");
    if (f)
    {
        fwprintf(f, L"%s\n", msg.c_str());
        fclose(f);
    }
}

static std::wstring NormalizePath(const std::wstring& path)
{
    if (path.rfind(L"\\\\?\\", 0) == 0)
        return path.substr(4);
    return path;
}

static bool WriteVRChatPathToRegistry(const std::wstring& path)
{
    HKEY hKey = nullptr;
    LONG res = RegCreateKeyExW(HKEY_CURRENT_USER, REG_KEY_PATH, 0, nullptr,
        REG_OPTION_NON_VOLATILE, KEY_WRITE, nullptr, &hKey, nullptr);
    if (res != ERROR_SUCCESS)
    {
        Log(L"Failed to open registry for write: " + std::to_wstring(res));
        return false;
    }

    res = RegSetValueExW(hKey, REG_VALUE_NAME, 0, REG_SZ,
        reinterpret_cast<const BYTE*>(path.c_str()),
        static_cast<DWORD>((path.size() + 1) * sizeof(wchar_t)));

    RegCloseKey(hKey);
    if (res != ERROR_SUCCESS)
    {
        Log(L"Failed to write registry value: " + std::to_wstring(res));
        return false;
    }
    return true;
}

static std::wstring ReadVRChatPathFromRegistry()
{
    std::wstring result;
    HKEY hKey = nullptr;
    LONG res = RegOpenKeyExW(HKEY_CURRENT_USER, REG_KEY_PATH, 0, KEY_READ, &hKey);
    if (res != ERROR_SUCCESS)
        return result;

    DWORD type = 0;
    DWORD cbData = 0;
    res = RegQueryValueExW(hKey, REG_VALUE_NAME, nullptr, &type, nullptr, &cbData);
    if (res == ERROR_SUCCESS && type == REG_SZ && cbData > 0)
    {
        std::wstring buffer(cbData / sizeof(wchar_t), L'\0');
        res = RegQueryValueExW(hKey, REG_VALUE_NAME, nullptr, nullptr,
            reinterpret_cast<LPBYTE>(&buffer[0]), &cbData);
        if (res == ERROR_SUCCESS)
        {
            buffer.resize(wcslen(buffer.c_str()));
            result = buffer;
        }
    }
    RegCloseKey(hKey);
    return result;
}

static std::wstring ComputeVRChatDirectory()
{
    wchar_t exePath[MAX_PATH] = { 0 };
    if (!GetModuleFileNameW(nullptr, exePath, MAX_PATH))
        return {};

    std::wstring pathStr = NormalizePath(exePath);
    size_t pos = pathStr.find_last_of(L"\\/");
    if (pos == std::wstring::npos) return {};
    return pathStr.substr(0, pos);
}

static const std::wstring& GetCachedVRChatDirectory()
{
    std::call_once(g_vrchatInitFlag, []() {
        std::wstring dir = ComputeVRChatDirectory();
        if (!dir.empty())
        {
            g_vrchatDirCached = dir;
            WriteVRChatPathToRegistry(g_vrchatDirCached);
            Log(L"Cached VRChat dir: " + g_vrchatDirCached);
            return;
        }

        std::wstring regPath = ReadVRChatPathFromRegistry();
        if (!regPath.empty())
        {
            g_vrchatDirCached = regPath;
            Log(L"Loaded VRChat dir from registry: " + g_vrchatDirCached);
        }
        else
        {
            Log(L"Failed to determine VRChat directory.");
        }
        });

    return g_vrchatDirCached;
}

static bool IsTargetProcess()
{
    wchar_t path[MAX_PATH];
    if (!GetModuleFileNameW(nullptr, path, MAX_PATH)) return false;
    wchar_t* filename = wcsrchr(path, L'\\');
    filename = filename ? filename + 1 : path;
    bool isTarget = (_wcsicmp(filename, L"vrchat.exe") == 0);
    if (!isTarget)
        Log(L"Not VRChat.exe — skipping injection.");
    return isTarget;
}

DWORD WINAPI LoaderThread(LPVOID)
{
    Log(L"LoaderThread started.");

    if (FAILED(CLRCreateInstance(CLSID_CLRMetaHost, IID_PPV_ARGS(&pMetaHost))))
    {
        Log(L"CLRCreateInstance failed.");
        return 1;
    }
    if (FAILED(pMetaHost->GetRuntime(L"v4.0.30319", IID_PPV_ARGS(&pRuntimeInfo))))
    {
        Log(L"GetRuntime failed.");
        return 1;
    }

    BOOL fLoadable = FALSE;
    if (FAILED(pRuntimeInfo->IsLoadable(&fLoadable)) || !fLoadable)
    {
        Log(L".NET runtime not loadable.");
        return 1;
    }

    if (FAILED(pRuntimeInfo->GetInterface(CLSID_CLRRuntimeHost, IID_PPV_ARGS(&pClrRuntimeHost))))
    {
        Log(L"GetInterface failed.");
        return 1;
    }

    if (FAILED(pClrRuntimeHost->Start()))
    {
        Log(L"CLRRuntimeHost start failed.");
        return 1;
    }

    const std::wstring& vrchatDir = GetCachedVRChatDirectory();
    if (vrchatDir.empty())
    {
        Log(L"VRChat directory not found.");
        return 1;
    }

    std::wstring assemblyPath = vrchatDir;
    if (assemblyPath.back() != L'\\' && assemblyPath.back() != L'/')
        assemblyPath += L'\\';
    assemblyPath += L"OL\\OpenLoader.dll";

    Log(L"Attempting to load assembly: " + assemblyPath);

    DWORD pReturnValue = 0;
    HRESULT hr = pClrRuntimeHost->ExecuteInDefaultAppDomain(
        assemblyPath.c_str(),
        L"OpenLoader.Entry",
        L"Run",
        L"",
        &pReturnValue);

    if (FAILED(hr))
    {
        Log(L"ExecuteInDefaultAppDomain failed. HRESULT: " + std::to_wstring(hr));
    }
    else
    {
        Log(L"Assembly executed successfully. Return: " + std::to_wstring(pReturnValue));
    }

    return 0;
}

BOOL APIENTRY DllMain(HMODULE hModule, DWORD ul_reason_for_call, LPVOID)
{
    switch (ul_reason_for_call)
    {
    case DLL_PROCESS_ATTACH:
        if (!IsTargetProcess()) return TRUE;
        DisableThreadLibraryCalls(hModule);
        {
            HANDLE hThread = CreateThread(nullptr, 0, LoaderThread, nullptr, 0, nullptr);
            if (hThread)
                CloseHandle(hThread);
            else
                Log(L"Failed to create loader thread.");
        }
        break;
    case DLL_PROCESS_DETACH:
        Log(L"DLL_PROCESS_DETACH called.");
        if (pClrRuntimeHost) { pClrRuntimeHost->Release(); pClrRuntimeHost = nullptr; }
        if (pRuntimeInfo) { pRuntimeInfo->Release(); pRuntimeInfo = nullptr; }
        if (pMetaHost) { pMetaHost->Release(); pMetaHost = nullptr; }
        break;
    }
    return TRUE;
}
