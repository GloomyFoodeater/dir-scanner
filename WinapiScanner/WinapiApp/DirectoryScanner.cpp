#include "DirectoryScanner.h"

// Struct to pass parameters to callback function
typedef struct {
	wstring file_name;				// Name of file to insert as key into entries

	// Refrences to shared between threads data
	wstring& dir_name;				// Path to the file
	wstring& str;					// String to search for
	IList<wstring, int>& entries;	// List of entries
	int& unfinished_counter;		// Counter of unfinished tasks
	CONDITION_VARIABLE& finished;	// Conditional variable
	CRITICAL_SECTION& counter_lock;	// Critical section to change counter & wake variable

} ContextStruct;

int CountOcurrences(wstring& haystack, wstring& needle)
{
	int entries = 0;
	int N = haystack.size();
	int M = needle.size();

	// Iterate over haystack;
	// N - M to avoid out of bounds of haystack
	for (int i = 0; i < N; i++)
	{
		int j;
		for (j = 0; j < M; j++)
			if (haystack[i + j] != needle[j])
				break;

		// Weak typing feature
		entries += (j == M);
	}

	return entries;
}

VOID CALLBACK WorkCallback(
	_Inout_     PTP_CALLBACK_INSTANCE Instance,
	_Inout_opt_ PVOID                 Context,
	_Inout_     PTP_WORK              Work
)
{
	auto context = (ContextStruct*)Context;

	// Check to supress warning
	if (context)
	{
		// Open file in utf-8
		wifstream ifs(context->dir_name + L"\\" + context->file_name);
		ifs.imbue(std::locale(std::locale::empty(), new std::codecvt_utf8<wchar_t>));

		// Count entries line-by-line
		wstring line;
		int entries = 0;
		while (getline(ifs, line))
			entries += CountOcurrences(line, context->str);

		ifs.close();

		context->entries.insert(context->file_name, entries);

		// Locking on counter
		EnterCriticalSection(&(context->counter_lock));
		context->unfinished_counter--;					// Change condition
		WakeConditionVariable(&(context->finished));	// Wake variable
		LeaveCriticalSection(&(context->counter_lock));

		delete context;
	}
}

DirectoryScanner::DirectoryScanner()
{
	_unfinished_counter = 0;
	InitializeConditionVariable(&_is_scan_finished);
	InitializeCriticalSection(&_counter_lock);
}

bool DirectoryScanner::Scan(wstring dir_name, wstring str, IList<wstring, int>& entries)
{
	WIN32_FIND_DATA  file_data{ 0 };

	// Start search
	HANDLE hFindFile = FindFirstFile((dir_name + L"\\*").c_str(), &file_data);
	if (hFindFile == INVALID_HANDLE_VALUE)
		return false;

	// Proceed search
	do {
		if (!(file_data.dwFileAttributes & FILE_ATTRIBUTE_DIRECTORY))
		{
			// Allocate memory for params, releasing must be inside callback
			auto context = new ContextStruct
			{
				file_data.cFileName,
				dir_name,
				str,
				entries,
				_unfinished_counter,
				_is_scan_finished,
				_counter_lock
			};

			// Put work in thread pool
			auto work = CreateThreadpoolWork(WorkCallback, context, NULL);
			_unfinished_counter++;
			SubmitThreadpoolWork(work);
		}
	} while (FindNextFile(hFindFile, &file_data));

	// Locking on counter
	EnterCriticalSection(&_counter_lock);
	while (_unfinished_counter > 0)
		SleepConditionVariableCS(&_is_scan_finished, &_counter_lock, INFINITE);
	LeaveCriticalSection(&_counter_lock);

	return true;
}
