#pragma once
#include "List.h"
#include <winnt.h>

template <typename TKey, typename TValue>
class NonBlockingList : public List<TKey, TValue>
{
public:
	NonBlockingList() : List<TKey, TValue>()
	{
		_tail = new Node<TKey, TValue>();
		_tail->next = nullptr;
		this->_head->next = _tail;
	}

	Node<TKey, TValue>* end()
	{
		return _tail;
	}

	bool insert(TKey key, TValue value) override
	{
		auto new_node = new Node<TKey, TValue>{ key, value };

		Node<TKey, TValue>* right_node, * left_node;
		do {
			right_node = search(key, &left_node);
			new_node->next = right_node;
			if (CAS(&(left_node->next), new_node, right_node)) /*C2*/
				return true;
		} while (true); /*B3*/
	}

	bool remove(TKey key) override
	{
		Node<TKey, TValue>* right_node, * right_node_next, * left_node;
		do {
			right_node = search(key, &left_node);
			if ((right_node == _tail) || (right_node->key != key)) /*T1*/
				return false;
			right_node_next = right_node->next;
			if (!is_marked_reference(right_node_next))
				if (CAS(&(right_node->next), get_marked_reference(right_node_next), right_node_next))
					break;
		} while (true); /*B4*/

		if (CAS(&(left_node->next), right_node_next, right_node)) /*C4*/
			delete right_node;
		else
			right_node = search(right_node->key, &left_node);
		return true;
	}

private:

	Node<TKey, TValue>* _tail;

	Node<TKey, TValue>* search(TKey search_key, Node<TKey, TValue>** left_node)
	{
		Node<TKey, TValue>* left_node_next = nullptr, * right_node;

	search_again:
		do {
			auto t = this->_head;
			auto t_next = this->_head->next;

			/* 1: Find left_node and right_node */
			do {
				if (!is_marked_reference(t_next)) {
					(*left_node) = t;
					left_node_next = t_next;
				}
				t = get_unmarked_reference(t_next);
				if (t == _tail)
					break;
				t_next = t->next;
			} while (is_marked_reference(t_next) || (t->key < search_key));

			right_node = t;

			/* 2: Check nodes are adjacent */
			if (left_node_next == right_node)
				if ((right_node != _tail) && is_marked_reference(right_node->next))
					goto search_again; /*G1*/
				else
					return right_node; /*R1*/

			/* 3: Remove one or more marked nodes */
			if (CAS(&((*left_node)->next), right_node, left_node_next)) /*C1*/
			{
				delete left_node_next;

				if ((right_node != _tail) && is_marked_reference(right_node->next))
					goto search_again; /*G2*/
				else
					return right_node; /*R2*/
			}

		} while (true); /*B2*/
	}

	Node<TKey, TValue>** CAS(Node<TKey, TValue>** dst, Node<TKey, TValue>* xchg, Node<TKey, TValue>* cmp)
	{
		return (Node<TKey, TValue>**)InterlockedCompareExchange((volatile unsigned int*)dst, (unsigned int)xchg, (unsigned int)cmp);
	}

	bool is_marked_reference(Node<TKey, TValue>* node)
	{
		return (Node<TKey, TValue>*)((uintptr_t)node & 0x1);
	}

	Node<TKey, TValue>* get_marked_reference(Node<TKey, TValue>* node)
	{
		return (Node<TKey, TValue>*)((uintptr_t)node | 0x1);
	}

	Node<TKey, TValue>* get_unmarked_reference(Node<TKey, TValue>* node)
	{
		return (Node<TKey, TValue>*)((uintptr_t)node & ~0x1);
	}
};