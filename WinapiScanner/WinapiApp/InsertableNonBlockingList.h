#pragma once
#include "List.h"
#include <winnt.h>
#include <stdexcept>

template <typename TKey, typename TValue>
class InsertableNonBlockingList : public List<TKey, TValue>
{
public:

	bool insert(TKey key, TValue value) override
	{
		auto new_node = new Node<TKey, TValue>{ key, value };
		
		// Failed to allocate memory.
		if (!new_node)
			return false;

		Node<TKey, TValue>* right_node, * left_node;
		do {
			// Search of correct left and right nodes.
			left_node = this->_head;
			while (left_node->next && left_node->next->key < key)
				left_node = left_node->next;
			right_node = left_node->next;

			new_node->next = right_node;

			// Connecting node in list with new node via compare and swap.
			if (CAS(&(left_node->next), new_node, right_node))
				return true;
		} while (true);
	}

	bool remove(TKey key) override
	{
		throw std::exception("Not implemented");
	}

private:

	// Atomic compare and swap with returning of whether swapping tool place.
	bool CAS(Node<TKey, TValue>** dst, Node<TKey, TValue>* new_value, Node<TKey, TValue>* old_value)
	{
		return (Node<TKey, TValue>*)InterlockedCompareExchangePointer((PVOID*)dst, (PVOID)new_value, (PVOID)old_value) != *(Node<TKey, TValue>**)dst;
	}
};