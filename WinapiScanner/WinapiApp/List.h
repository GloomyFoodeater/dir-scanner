#pragma once
#include "IList.h"
#include "Node.h"

template <typename TKey, typename TValue>
class List :
	public IList<TKey, TValue>
{
public:
	List();

	~List();

	bool insert(TKey key, TValue value) override;

	bool remove(TKey key) override;

	Node<TKey, TValue>* _head;
};
