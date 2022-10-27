#pragma once
#include "Node.h"

template <typename TKey, typename TValue>
class IList
{
public:
	virtual Node<TKey, TValue>* begin() = 0;
	virtual Node<TKey, TValue>* end() = 0;

	virtual bool insert(TKey key, TValue value) = 0;
	virtual bool remove(TKey key) = 0;
};

