#pragma once

template <typename TKey, typename TValue>
class Node
{
public:
	TKey key;
	TValue value;
	Node* next;
};

