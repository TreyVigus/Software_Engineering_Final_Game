/* I made the classes in this file to compensate for classes that exist in MFC but
 * don't exist in C#  -- JC */

using System;
using System.Collections.Generic;

namespace ACFramework
{
    // I made this struct to mimic the CRect that is used in MFC, but is not 
    // exactly the same in C#  -- JC
    struct CRect
    {
        public int left;
        public int top;
        public int right;
        public int bottom;

        public CRect(int l, int t, int r, int b)
        {
            left = l;
            top = t;
            right = r;
            bottom = b;
        }
    }


    /* I made this LinkedList class to mimic the CArray class that exists in MFC but
     * doesn't exist in C#.  I used a LinkedList instead of an array, realizing that it 
     * would be more efficient since elements were often inserted or deleted into the 
     * middle of the CArray.  The CArray had the advantage of direct access to a specific 
     * element through an index, but it doesn't happen too often -- when it does happen, 
     * it is often because the code is iterating through the array, so iterating through
     * a linked list is just as fast.  I did redesign a lot of the code throughout
     * the Pop framework to make the use of the LinkedList efficient.  For cases where
     * there is no way to avoid direct access to an element through an index, I created
     * an indexer (a specific feature of C#) for this class which can be used. It should
     * be avoided whenever possible, however, because of inefficiency. -- JC
     */

    public class Node<DataType>
    {
        public DataType info;
        public Node<DataType> next;
    }

    class LinkedList<DataType>
    {
        private Node<DataType> start;
        private Node<DataType> back;
        private Node<DataType> current;  // used to mark the node before the node
        // of interest -- set to null if there is 
        // no current position
        private Node<DataType> save;    // used to save a list position
        public delegate void DelAssign(out DataType a, DataType b);
        private DelAssign Assign;
        private int size;
        private bool removeflag;

        public LinkedList(DelAssign a)
        {
            Assign = a;
            start = back = new Node<DataType>();  // header
            removeflag = false;
        }

        public virtual void Copy(LinkedList<DataType> original)
        {
            current = start;
            original.current = original.start;
            if (size < original.size)
            {
                int i;
                for (i = 0; i < size; i++)
                {
                    Assign(out current.next.info, original.current.next.info);
                    current = current.next;
                    original.current = original.current.next;
                }
                for (; i < original.size; i++)
                {
                    current.next = new Node<DataType>();
                    Assign(out current.next.info, original.current.next.info);
                    current = current.next;
                    original.current = original.current.next;
                }
            }
            else
            {
                for (int i = 0; i < original.size; i++)
                {
                    Assign(out current.next.info, original.current.next.info);
                    current = current.next;
                    original.current = original.current.next;
                }
            }
            back = current;
            size = original.size;
            current.next = null;
        }

        public virtual void Add(DataType element) // adds to end of list
        {
            Node<DataType> newNode = new Node<DataType>();
            Assign(out newNode.info, element);
            back.next = newNode;
            back = newNode;
            size++;
        }

        public void SavePos()
        {
            save = current;
        }

        public void RestorePos()
        {
            current = save;
        }

        public IEnumerator<DataType> GetEnumerator()
        {
            current = start;
            while (current != back)
            {
                yield return current.next.info;
                current = current.next;
            }
        }

        // alternate iterator for RemoveNext

        public void First(out DataType element)
        {
            current = start;
            Assign( out element, current.next.info );
        }

        public bool GetNext(out DataType element)
        {
            element = default( DataType );
            if ( !removeflag )
                current = current.next;
            removeflag = false;
            if (current == back)
                return false;
            Assign(out element, current.next.info);
            return true;
        }

        public void RemoveNext()
        {
            if (back == current.next)
                back = current;
            current.next = current.next.next;
            size--;
            removeflag = true;
        }

        public void InsertAt(DataType element) // inserts a node at the current position
        {
            Node<DataType> newNode = new Node<DataType>();
            Assign(out newNode.info, element);
            newNode.next = current.next;
            current.next = newNode;
            if (back == current)
                back = newNode;
            size++;
        }

        public virtual void InsertAt(int i, DataType element) // inserts at position i
        {
            locate(i);
            InsertAt(element);
        }

        public int Size
        {
            get
            {
                return size;
            }
        }

        public virtual DataType GetAt(int i)
        {
            locate(i);
            DataType copy;
            Assign(out copy, current.next.info);
            return copy;
        }

        public DataType ElementAt()
        {
            return current.next.info;
        }

        public DataType ElementAt(int i)
        {
            locate(i);
            return current.next.info;
        }

        public void SetAt(DataType element)
        {
            Assign(out current.next.info, element);
        }

        public void SetAt(int i, DataType element)
        {
            locate(i);
            Assign(out current.next.info, element);
        }

        public DataType this[int i]
        {
            get
            {
                locate(i);
                return current.next.info;
            }
            set
            {
                locate(i);
                SetAt(value);
            }
        }

        public void RemoveAt()
        {
            if (back == current.next)
                back = current;
            current.next = current.next.next;
            size--;
        }

        public void RemoveAt( int i )
        {
            locate(i);
            RemoveAt();
        }

        public void RemoveAll()
        {
            start.next = null;
            back = start;
            size = 0;
        }

        private void locate( int i )
        {
            current = start;
            int j = 0;
            while (j < i)
            {
                current = current.next;
                j++;
            }
        }

    }

}
