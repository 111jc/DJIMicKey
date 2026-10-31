#define WIN32_LEAN_AND_MEAN
#include <windows.h>

static HINSTANCE module;
static HHOOK hook;
static const wchar_t caption[] = L"DJIMicKey Shell Volume Guard v1";
static const wchar_t enabledProperty[] = L"DJIMicKey.Shell.Enabled.v1";

/* The callback runs in another process when Windows injects this user-mode DLL.
   Do not use process-local globals for enabled/owner state. A destroyed owner
   window or absent property makes the callback fail open automatically. */
static BOOL should_block(HWND owner, int code, LPARAM data, BOOL notify) {
    if (code != HSHELL_APPCOMMAND || GET_APPCOMMAND_LPARAM(data) != APPCOMMAND_VOLUME_UP) return FALSE;
    if (!owner || !GetPropW(owner, enabledProperty)) return FALSE;
    if (notify) PostMessageW(owner, WM_APP + 42, GetCurrentProcessId(), data);
    return TRUE;
}
static LRESULT CALLBACK shell_proc(int code, WPARAM wp, LPARAM lp) {
    if (should_block(FindWindowW(NULL, caption), code, lp, TRUE)) return 1;
    return CallNextHookEx(NULL, code, wp, lp);
}
__declspec(dllexport) BOOL WINAPI ShellInstall(void) {
    if (hook) return TRUE;
    hook = SetWindowsHookExW(WH_SHELL, shell_proc, module, 0);
    return hook != NULL;
}
__declspec(dllexport) void WINAPI ShellRemove(void) {
    if (hook) { UnhookWindowsHookEx(hook); hook = NULL; }
}
/* Tests exercise the decision without generating a Windows volume command. */
__declspec(dllexport) BOOL WINAPI ShellTestDecision(HWND owner, int code, LPARAM data) {
    return should_block(owner, code, data, FALSE);
}
BOOL WINAPI DllMain(HINSTANCE instance, DWORD reason, LPVOID reserved) {
    (void)reserved;
    if (reason == DLL_PROCESS_ATTACH) { module = instance; DisableThreadLibraryCalls(instance); }
    return TRUE;
}
