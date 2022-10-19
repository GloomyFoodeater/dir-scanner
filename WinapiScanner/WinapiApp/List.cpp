#include "List.h"

template<typename TKey, typename TValue>
List<TKey, TValue>::List()
{
	_head = new Node<TKey, TValue>();
	_head->next = nullptr;
}

template<typename TKey, typename TValue>
List<TKey, TValue>::~List()
{
	Node<TKey, TValue>* removed;
	while (_head)
	{
		removed = _head;
		_head = _head->next;
		delete removed;
	}
}

template<typename TKey, typename TValue>
inline bool List<TKey, TValue>::insert(TKey key, TValue value)
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

template<typename TKey, typename TValue>
bool List<TKey, TValue>::remove(TKey key)
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
