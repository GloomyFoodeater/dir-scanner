#include "BlockingList.h"

template<typename TKey, typename TValue>
inline BlockingList<TKey, TValue>::BlockingList() : List<TKey, TValue>()
{
    InitializeCriticalSection(&_resize_lock);
}

template<typename TKey, typename TValue>
inline bool BlockingList<TKey, TValue>::insert(TKey key, TValue value)
{
    EnterCriticalSection(&_resize_lock);
    bool is_inserted = List<TKey, TValue>::insert(key, value);
    LeaveCriticalSection(&_resize_lock);

    return is_inserted;
}

template<typename TKey, typename TValue>
inline bool BlockingList<TKey, TValue>::remove(TKey key)
{
    EnterCriticalSection(&_resize_lock);
    bool is_removed = List<TKey, TValue>::remove(key);
    LeaveCriticalSection(&_resize_lock);

    return is_removed;
}
