#pragma once
#include "List.h"
#include "Windows.h"
#include "process.h" 


template <typename TKey, typename TValue>
class BlockingList :
    public List<TKey, TValue>
{
public:
    BlockingList() : List<TKey, TValue>()
    {
        InitializeCriticalSection(&_resize_lock);
    }

    bool insert(TKey key, TValue value) override
    {
        EnterCriticalSection(&_resize_lock);
        bool is_inserted = List<TKey, TValue>::insert(key, value);
        LeaveCriticalSection(&_resize_lock);

        return is_inserted;
    }

    bool remove(TKey key) override
    {
        EnterCriticalSection(&_resize_lock);
        bool is_removed = List<TKey, TValue>::remove(key);
        LeaveCriticalSection(&_resize_lock);

        return is_removed;
    }

private:
    CRITICAL_SECTION _resize_lock;
};
