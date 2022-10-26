#include <string>
#include "IList.h"
#include "Windows.h"

using std::wstring;

class DirectoryScanner
{
public:
	DirectoryScanner();
	bool Scan(wstring dir_name, wstring str, IList<wstring, int>& entries);
private:
	int _unfinished_counter;
	CONDITION_VARIABLE _is_scan_finished;
	CRITICAL_SECTION _counter_lock;
};
