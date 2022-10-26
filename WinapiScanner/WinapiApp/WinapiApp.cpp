// WinapiApp.cpp : Defines the entry point for the application.
//

#include "framework.h"
#include "WinapiApp.h"

#define MAX_LOADSTRING 100

// Global Variables:
HINSTANCE hInst;                                // current instance
WCHAR szTitle[MAX_LOADSTRING];                  // The title bar text
WCHAR szWindowClass[MAX_LOADSTRING];            // the main window class name

// Handles
HWND hListBox;
HWND hEditNeedle;
HWND hEditDir;
HWND hLblNeedle;
HWND hLblDir;
HWND hBtnScan;
HWND hBtnOpen;

// Forward declarations of functions included in this code module:
ATOM                MyRegisterClass(HINSTANCE hInstance);
BOOL                InitInstance(HINSTANCE, int);
LRESULT CALLBACK    WndProc(HWND, UINT, WPARAM, LPARAM);
INT_PTR CALLBACK    About(HWND, UINT, WPARAM, LPARAM);

int APIENTRY wWinMain(_In_ HINSTANCE hInstance,
                     _In_opt_ HINSTANCE hPrevInstance,
                     _In_ LPWSTR    lpCmdLine,
                     _In_ int       nCmdShow)
{
    UNREFERENCED_PARAMETER(hPrevInstance);
    UNREFERENCED_PARAMETER(lpCmdLine);

    // Initialize global strings
    LoadStringW(hInstance, IDS_APP_TITLE, szTitle, MAX_LOADSTRING);
    LoadStringW(hInstance, IDC_WINAPIAPP, szWindowClass, MAX_LOADSTRING);
    MyRegisterClass(hInstance);

    // Perform application initialization:
    if (!InitInstance (hInstance, nCmdShow))
    {
        return FALSE;
    }

    HACCEL hAccelTable = LoadAccelerators(hInstance, MAKEINTRESOURCE(IDC_WINAPIAPP));

    MSG msg;

    // Main message loop:
    while (GetMessage(&msg, nullptr, 0, 0))
    {
        if (!TranslateAccelerator(msg.hwnd, hAccelTable, &msg))
        {
            TranslateMessage(&msg);
            DispatchMessage(&msg);
        }
    }

    return (int) msg.wParam;
}



//
//  FUNCTION: MyRegisterClass()
//
//  PURPOSE: Registers the window class.
//
ATOM MyRegisterClass(HINSTANCE hInstance)
{
    WNDCLASSEXW wcex;

    wcex.cbSize = sizeof(WNDCLASSEX);

    wcex.style          = CS_HREDRAW | CS_VREDRAW;
    wcex.lpfnWndProc    = WndProc;
    wcex.cbClsExtra     = 0;
    wcex.cbWndExtra     = 0;
    wcex.hInstance      = hInstance;
    wcex.hIcon          = LoadIcon(hInstance, MAKEINTRESOURCE(IDI_WINAPIAPP));
    wcex.hCursor        = LoadCursor(nullptr, IDC_ARROW);
    wcex.hbrBackground  = (HBRUSH)(COLOR_WINDOW+1);
    wcex.lpszMenuName   = MAKEINTRESOURCEW(IDC_WINAPIAPP);
    wcex.lpszClassName  = szWindowClass;
    wcex.hIconSm        = LoadIcon(wcex.hInstance, MAKEINTRESOURCE(IDI_SMALL));

    return RegisterClassExW(&wcex);
}

//
//   FUNCTION: InitInstance(HINSTANCE, int)
//
//   PURPOSE: Saves instance handle and creates main window
//
//   COMMENTS:
//
//        In this function, we save the instance handle in a global variable and
//        create and display the main program window.
//
BOOL InitInstance(HINSTANCE hInstance, int nCmdShow)
{
   hInst = hInstance; // Store instance handle in our global variable

   HWND hWnd = CreateWindowW(szWindowClass, szTitle, WS_OVERLAPPEDWINDOW,
      CW_USEDEFAULT, 0, CW_USEDEFAULT, 0, nullptr, nullptr, hInstance, nullptr);

   if (!hWnd)
   {
      return FALSE;
   }

   ShowWindow(hWnd, nCmdShow);
   UpdateWindow(hWnd);

   return TRUE;
}

//
//  FUNCTION: WndProc(HWND, UINT, WPARAM, LPARAM)
//
//  PURPOSE: Processes messages for the main window.
//
//  WM_COMMAND  - process the application menu
//  WM_PAINT    - Paint the main window
//  WM_DESTROY  - post a quit message and return
//
//
LRESULT CALLBACK WndProc(HWND hWnd, UINT message, WPARAM wParam, LPARAM lParam)
{
    switch (message)
    {
    case WM_CREATE:
    {
        // Create list box to output scanner result
        hListBox = CreateWindow(L"listbox", NULL,
            WS_CHILD | WS_VISIBLE | LBS_STANDARD | LBS_WANTKEYBOARDINPUT,
            0, 0, 0, 0,
            hWnd, (HMENU)ID_LIST, hInst, NULL);

        hEditNeedle = CreateWindow(L"edit", NULL, 
            WS_CHILD | WS_VISIBLE | WS_BORDER | ES_AUTOHSCROLL,
            0, 0, 0, 0,
            hWnd, (HMENU)ID_EDIT_NEEDLE, hInst, NULL);

        hLblNeedle = CreateWindow(L"static", L"Needle",
            WS_VISIBLE | WS_CHILD | SS_RIGHT,
            0, 0, 0, 0, 
            hWnd, NULL, hInst, NULL);

        hEditDir = CreateWindow(L"edit", NULL,
            WS_CHILD | WS_VISIBLE | WS_BORDER | ES_AUTOHSCROLL,
            0, 0, 0, 0,
            hWnd, (HMENU)ID_EDIT_DIR, hInst, NULL);

        hLblDir = CreateWindow(L"static", L"Directory",
            WS_VISIBLE | WS_CHILD | SS_RIGHT,
            0, 0, 0, 0,
            hWnd, NULL, hInst, NULL);

        hBtnScan = CreateWindow(L"button", L"Scan",
            WS_VISIBLE | WS_CHILD,
            0, 0, 0, 0,
            hWnd, (HMENU)ID_BUTTON_SCAN, hInst, NULL);

        hBtnOpen = CreateWindow(L"button", L"Open",
            WS_VISIBLE | WS_CHILD,
            0, 0, 0, 0,
            hWnd, (HMENU)ID_BUTTON_OPEN, hInst, NULL);

        // TODO: Move to button
        DirectoryScanner scanner;
        BlockingList<wstring, int> list;
        WCHAR buf[MAX_PATH];
        GetCurrentDirectory(MAX_PATH, buf);
        scanner.Scan(buf, L"hello", list);
        for (auto node = list._head->next; node != nullptr; node = node->next)
            SendMessage(hListBox, LB_ADDSTRING, 0, (LPARAM)node->key.c_str());
    }
    break;
    case WM_SIZE:
    {
        int x, y, cx, cy;
        RECT rect;
        GetClientRect(hWnd, &rect);
        int clientW = rect.right - rect.left;
        int clientH = rect.bottom - rect.top;

        // Resize listbox
        x = 0;
        y = 40;
        cx = clientW;
        cy = clientH;
        SetWindowPos(hListBox, 0, x, y, cx, cy, 0);

        y = 10;
        cy = 20;

        // Resize label for needle
        x = 0.05 * clientW;
        cx = 60;
        SetWindowPos(hLblNeedle, 0, x, y, cx, cy, 0);

        // Resize edit for needle
        x = x + cx;
        cx = 160;
        SetWindowPos(hEditNeedle, 0, x, y, cx, cy, 0);

        // Resize label for dir
        x = max(x + cx + 20, 0.2 * clientW - 80);
        cx = 60;
        SetWindowPos(hLblDir, 0, x, y, cx, cy, 0);

        // Resize edit for directory
        x = x + cx;
        cx = 0.3 * clientW;
        SetWindowPos(hEditDir, 0, x, y, cx, cy, 0);

        // Resize scan button
        x = x + cx + 20;
        cx = 80;
        SetWindowPos(hBtnScan, 0, x, y, cx, cy, 0);

        // Resize open button
        x = x + cx + 10;
        cx = 80;
        SetWindowPos(hBtnOpen, 0, x, y, cx, cy, 0);
    }
        break;
    case WM_COMMAND:
        {
            int wmId = LOWORD(wParam);
            // Parse the menu selections:
            switch (wmId)
            {
            case IDM_ABOUT:
                DialogBox(hInst, MAKEINTRESOURCE(IDD_ABOUTBOX), hWnd, About);
                break;
            case IDM_EXIT:
                DestroyWindow(hWnd);
                break;
            default:
                return DefWindowProc(hWnd, message, wParam, lParam);
            }
        }
        break;
    case WM_DESTROY:
        PostQuitMessage(0);
        break;
    default:
        return DefWindowProc(hWnd, message, wParam, lParam);
    }
    return 0;
}

// Message handler for about box.
INT_PTR CALLBACK About(HWND hDlg, UINT message, WPARAM wParam, LPARAM lParam)
{
    UNREFERENCED_PARAMETER(lParam);
    switch (message)
    {
    case WM_INITDIALOG:
        return (INT_PTR)TRUE;

    case WM_COMMAND:
        if (LOWORD(wParam) == IDOK || LOWORD(wParam) == IDCANCEL)
        {
            EndDialog(hDlg, LOWORD(wParam));
            return (INT_PTR)TRUE;
        }
        break;
    }
    return (INT_PTR)FALSE;
}
