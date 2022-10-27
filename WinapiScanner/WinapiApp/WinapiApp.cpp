// WinapiApp.cpp : Defines the entry point for the application.
//

#include "framework.h"
#include "WinapiApp.h"

#define MAX_LOADSTRING 100

// Global Variables:
HINSTANCE hInst;                                // current instance
WCHAR szTitle[MAX_LOADSTRING];                  // The title bar text
WCHAR szWindowClass[MAX_LOADSTRING];            // the main window class name

// Scanning info
DirectoryScanner scanner;
wstring needle;
wstring dir;

// Handles
HWND hMain;
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
	if (!InitInstance(hInstance, nCmdShow))
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

	return (int)msg.wParam;
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

	wcex.style = CS_HREDRAW | CS_VREDRAW;
	wcex.lpfnWndProc = WndProc;
	wcex.cbClsExtra = 0;
	wcex.cbWndExtra = 0;
	wcex.hInstance = hInstance;
	wcex.hIcon = LoadIcon(hInstance, MAKEINTRESOURCE(IDI_WINAPIAPP));
	wcex.hCursor = LoadCursor(nullptr, IDC_ARROW);
	wcex.hbrBackground = (HBRUSH)(COLOR_WINDOW + 1);
	wcex.lpszMenuName = MAKEINTRESOURCEW(IDC_WINAPIAPP);
	wcex.lpszClassName = szWindowClass;
	wcex.hIconSm = LoadIcon(wcex.hInstance, MAKEINTRESOURCE(IDI_SMALL));

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
	hMain = hWnd;
	ShowWindow(hWnd, nCmdShow);
	UpdateWindow(hWnd);

	return TRUE;
}

LRESULT CALLBACK OnCreate(HWND hWnd, UINT message, WPARAM wParam, LPARAM lParam)
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
	return 0;
}

LRESULT CALLBACK OnSize(HWND hWnd, UINT message, WPARAM wParam, LPARAM lParam)
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
	x = (int)(0.05 * clientW);
	cx = 60;
	SetWindowPos(hLblNeedle, 0, x, y, cx, cy, 0);

	// Resize edit for needle
	x = x + cx;
	cx = 160;
	SetWindowPos(hEditNeedle, 0, x, y, cx, cy, 0);

	// Resize label for dir
	x = max(x + cx + 20, (int)(0.2 * clientW - 80));
	cx = 60;
	SetWindowPos(hLblDir, 0, x, y, cx, cy, 0);

	// Resize edit for directory
	x = x + cx;
	cx = (int)(0.3 * clientW);
	SetWindowPos(hEditDir, 0, x, y, cx, cy, 0);

	// Resize scan button
	x = x + cx + 20;
	cx = 80;
	SetWindowPos(hBtnScan, 0, x, y, cx, cy, 0);

	// Resize open button
	x = x + cx + 10;
	cx = 80;
	SetWindowPos(hBtnOpen, 0, x, y, cx, cy, 0);

	return 0;
}

void OnScanClick()
{
	wchar_t buf[MAX_PATH]{ 0 };
	int length = 0;

	// Extract text from needle edit
	length = GetWindowTextLength(hEditNeedle) + 1;
	if (length == 1)
	{
		MessageBox(NULL, L"Needle was empty", L"Error", MB_OK);
		return;
	}
	GetWindowText(hEditNeedle, buf, length);
	needle = buf;

	// Extract text from dir edit
	length = GetWindowTextLength(hEditDir) + 1;
	if (length == 1)
	{
		MessageBox(NULL, L"Directory was not set", L"Error", MB_OK);
		return;
	}
	GetWindowText(hEditDir, buf, length);
	dir = buf;


	// Scan directory
	NonBlockingList<wstring, int> list;
	bool res = scanner.Scan(dir, needle, list);
	if (!res)
	{
		MessageBox(NULL, L"Failed to scan given directory", L"Error", MB_OK);
		return;
	}

	// Set listbox
	SendMessage(hListBox, LB_RESETCONTENT, 0, 0);
	for (auto it = list.begin(); it != list.end(); it = it->next)
	{
		wstring outputString = it->key + L": " + std::to_wstring(it->value);
		SendMessage(hListBox, LB_ADDSTRING, 0, (LPARAM)outputString.c_str());
	}
}

void OnOpenClick()
{
	// Init COM library
	HRESULT hr = CoInitializeEx(NULL, COINIT_APARTMENTTHREADED |
		COINIT_DISABLE_OLE1DDE);
	if (!SUCCEEDED(hr))
		return;

	// Create the FileOpenDialog object
	IFileOpenDialog* pFileOpen;
	hr = CoCreateInstance(CLSID_FileOpenDialog, NULL, CLSCTX_ALL,
		IID_IFileOpenDialog, reinterpret_cast<void**>(&pFileOpen));
	if (!SUCCEEDED(hr))
	{
		CoUninitialize();
		return;
	}

	// Set option to pick only folders
	hr = pFileOpen->SetOptions(FOS_PICKFOLDERS);
	if (!SUCCEEDED(hr))
	{
		pFileOpen->Release();
		CoUninitialize();
		return;
	}

	// Show the dialog
	hr = pFileOpen->Show(NULL);
	if (!SUCCEEDED(hr))
	{
		pFileOpen->Release();
		CoUninitialize();
		return;
	}

	// Get the file name from the dialog box
	IShellItem* pItem;
	hr = pFileOpen->GetResult(&pItem);
	if (!SUCCEEDED(hr))
	{
		pFileOpen->Release();
		CoUninitialize();
		return;
	}
	PWSTR pszFilePath;
	hr = pItem->GetDisplayName(SIGDN_FILESYSPATH, &pszFilePath);
	if (SUCCEEDED(hr))
	{
		// Set directory edit to selected path
		SendMessage(hEditDir, WM_SETTEXT, 0, (LPARAM)pszFilePath);
	}

	pItem->Release();
	pFileOpen->Release();
	CoUninitialize();
}

LRESULT CALLBACK OnCommand(HWND hWnd, UINT message, WPARAM wParam, LPARAM lParam)
{

	int wmId = LOWORD(wParam);
	int ncCode = HIWORD(wParam);

	// Parse the menu selections:
	switch (wmId)
	{
	case IDM_ABOUT:
		DialogBox(hInst, MAKEINTRESOURCE(IDD_ABOUTBOX), hWnd, About);
		break;
	case ID_BUTTON_OPEN:
		if (ncCode == BN_CLICKED)
			OnOpenClick();
		break;
	case ID_BUTTON_SCAN:
		if (ncCode == BN_CLICKED)
			OnScanClick();
		break;
	case IDM_EXIT:
		DestroyWindow(hWnd);
		break;
	default:
		return DefWindowProc(hWnd, message, wParam, lParam);
	}
	return 0;
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
		return OnCreate(hWnd, message, wParam, lParam);


	case WM_SIZE:
		return OnSize(hWnd, message, wParam, lParam);
	case WM_COMMAND:
		return OnCommand(hWnd, message, wParam, lParam);
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
