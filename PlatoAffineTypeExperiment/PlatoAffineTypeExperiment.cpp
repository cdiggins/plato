// PlatoAffineTypeExperiment.cpp : This file contains the 'main' function. Program execution begins and ends there.
//

#include <iostream>

template<typename T>
class UniquePtr
{
public:
    UniquePtr()
    {
        _ptr = nullptr;
    }
    T* _ptr;
};

// Borrow
// Release 

template<typename T>
class LinkedListNode
{
public:
    T Data;
    UniquePtr<LinkedListNode<T>> Next;
	
	LinkedListNode(const T& data)
		: Data(data)
	{
	}

    ~LinkedListNode()
    {
        delete Next._ptr;
    }
};


int main()
{
    std::cout << "Hello World!\n";
}

// Run program: Ctrl + F5 or Debug > Start Without Debugging menu
// Debug program: F5 or Debug > Start Debugging menu

// Tips for Getting Started: 
//   1. Use the Solution Explorer window to add/manage files
//   2. Use the Team Explorer window to connect to source control
//   3. Use the Output window to see build output and other messages
//   4. Use the Error List window to view errors
//   5. Go to Project > Add New Item to create new code files, or Project > Add Existing Item to add existing code files to the project
//   6. In the future, to open this project again, go to File > Open > Project and select the .sln file

/*
Remove(node: LinkedListNode, n: Integer): LinkedListNode
{
	Let current = node;
	Let next = node.Next;;
	While (n > 0)
	{
		current = next;
		Next = current.Next;
	}
	Current.Next = Next.Next;
	Return current;
}

If we know that the "node" is a Unique pointer to the Linked list. Then this is possible.

Passing to a function is a "transfer"

// "node" count is 1 (because it is unique)
// Removes the "nth" child (0 means the next child).
Remove(node: LinkedListNode, n: Integer): void
{
	Let current = (share)node; // count goes to 2
	Let next = (share)node.Next; // counter goes to 2
	While (n > 0)
	{
		If (next == null)
		{
			(release)current; // count goes to 1
			(release)next;  // does nothing, its null
			return
		}
			   (release)current; // count goes to 1
		current = (share)next; // count goes to  3
		(release)next; // count goes to 2
		next = (share)current.Next; // count goes to 2
	}
	(release)current.Next;  // counter goes to 1
	current.Next = (share)next.Next; // counter goes to 2
	(release)current; // counter goes to 1
	(release)next;  // counter goes to 0
	(delete)next
	(release)next.Next; // counter goes to 1
}

In this scenario there is one kind of pointer.

	1. Unique references - which can be shared / released / transferred.

The advantage here is that reference counting can be done locally within a function.

There are more complex scenarios liked graphs and double linked listed which require:

	1. Shared references

In these scenarios the reference counting is done in the reference itself.

*/