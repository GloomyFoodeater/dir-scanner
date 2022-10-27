#pragma once
#include "IList.h"
#include "Node.h"

template <typename TKey, typename TValue>
class List :
	public IList<TKey, TValue>
{
public:
	List()
	{
		_head = new Node<TKey, TValue>();
		_head->next = nullptr;
	}

	virtual ~List()
	{
		Node<TKey, TValue>* removed;
		while (_head)
		{
			removed = _head;
			_head = _head->next;
			delete removed;
		}
	}

	Node<TKey, TValue>* begin() override
	{
		return _head->next;
	}

	Node<TKey, TValue>* end() override
	{
		return nullptr;
	}

	bool insert(TKey key, TValue value) override
	{
		// Search node to insert after
		auto left = _head;
		while (left->next && left->next->key < key)
			left = left->next;

		auto inserted = new Node<TKey, TValue>();

		// Failed to allocate memory
		if (!inserted)
			return false;

		// Init node
		inserted->key = key;
		inserted->value = value;

		// Change links
		inserted->next = left->next;
		left->next = inserted;

		return true;
	}

	bool remove(TKey key) override
	{
		// Search node to remove after
		auto left = _head;
		while (left->next && left->next->key != key)
			left = left->next;

		// Key not found
		auto removed = left->next;
		if (!removed)
			return false;

		// Change links
		auto right = removed->next;
		delete removed;
		left->next = right;

		return true;
	}

	Node<TKey, TValue>* _head;
};
