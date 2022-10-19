#include "DirectoryScanner.h"

// Struct to pass parameters to callback function
typedef struct {
	wstring file_name;				// Name of file to insert as key into entries
	wstring str;					// String to search for

	// Refrences to shared between threads data
	IList<wstring, int>& entries;	// List of entries
	int& unfinished_counter;		// Counter of unfinished tasks
	CONDITION_VARIABLE& finished;	// Conditional variable
	CRITICAL_SECTION& counter_lock;	// Critical section to change counter & wake variable

} ContextStruct;

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
		int entries = 0;
		// TODO: Add entry count
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

void DirectoryScanner::Scan(wstring dir_name, wstring str, IList<wstring, int>& entries)
{
	WIN32_FIND_DATA  file_data{ 0 };

	// Start search
	HANDLE hFindFile = FindFirstFile((dir_name + L"\\*").c_str(), &file_data);
	if (!hFindFile)
		return;

	// Proceed search
	do {
		if (!(file_data.dwFileAttributes & FILE_ATTRIBUTE_DIRECTORY))
		{
			// Allocate memory for params, releasing must be inside callback
			auto context = new ContextStruct{ file_data.cFileName, str, entries, _unfinished_counter, _is_scan_finished, _counter_lock };
			
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
}
