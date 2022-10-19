#pragma once
#include "List.h"
#include "Windows.h"
#include "process.h" 


template <typename TKey, typename TValue>
class BlockingList :
    public List<TKey, TValue>
{
public:
    BlockingList();

    bool insert(TKey key, TValue value) override;

    bool remove(TKey key) override;

private:
    CRITICAL_SECTION _resize_lock;
};
