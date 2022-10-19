#pragma once

template <typename TKey, typename TValue>
class IList
{
public:
	virtual bool insert(TKey key, TValue value) = 0;
	virtual bool remove(TKey key) = 0;
};

