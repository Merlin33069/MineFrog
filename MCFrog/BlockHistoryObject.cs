using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace MCFrog
{
	class TestClass
	{
		internal TestClass()
		{
			Console.WriteLine("loop starting!");
			BlockHistoryObjectCollection bho = new BlockHistoryObjectCollection(100);
			for (int i = 0; i < 120; i++)
			{
				bho.Add(new BlockHistoryObject("BHO: " + i));
			}
			foreach(BlockHistoryObject bho1 in bho)
			{
				Console.WriteLine("loopy");
				Console.WriteLine(bho1.Name);
			}
		}
	}

	struct BlockHistoryObject
	{
		public string Name;
		public BlockHistoryObject(string name)
		{
			Name = name;
		}
	}

	internal class BlockHistoryObjectCollection : ICollection
	{
		BlockHistoryObject[] collectionArray;
		int collectionSize;
		int currentPosition = 0;
		bool isFull = false;

		internal BlockHistoryObjectCollection(int size)
		{
			collectionSize = size;
			collectionArray = new BlockHistoryObject[size];
		}

		internal void Add(BlockHistoryObject incoming)
		{
			collectionArray[currentPosition] = incoming;
			currentPosition++;
			if (currentPosition == 100)
			{
				currentPosition = 0;
				isFull = true;
			}
		}

		void ICollection.CopyTo(Array myArr, int index)
		{
			foreach (BlockHistoryObject bho in collectionArray)
			{
				myArr.SetValue(bho, index);
				index++;
			}
		}
		bool ICollection.IsSynchronized
		{
			get
			{
				return false;
			}
		}
		object ICollection.SyncRoot
		{
			get
			{
				return this;
			}
		}
		int ICollection.Count
		{
			get
			{
				int count = currentPosition;
				if (isFull)
				{
					count = collectionSize;
				}
				return count;
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			int offset = 0;
			int count = currentPosition;
			if (isFull)
			{
				count = collectionSize;
				offset = currentPosition;
			}

			return new BlockHistoryObjectCollectionEnumerator(collectionArray, offset, count);
		}

		
	}

	class BlockHistoryObjectCollectionEnumerator : IEnumerator
	{
		private BlockHistoryObject[] collection;
		private int startPosition = -1;
		private int Cursor = -1;
		private int Count = 0;
		private int MaxCount = 100;

		internal BlockHistoryObjectCollectionEnumerator(BlockHistoryObject[] bho, int bhoPosition, int count)
		{
			collection = bho;
			startPosition = bhoPosition - 1; //We have to start at -1
			Cursor = startPosition;
			MaxCount = count;
		}

		void IEnumerator.Reset()
		{
			Cursor = startPosition;
			Count = 0;
		}
		bool IEnumerator.MoveNext()
		{
			//Check that we are within the bounds of the array
			Count++;
			if (Count > MaxCount) return false;

			Cursor++;
			if (Cursor >= MaxCount) Cursor -= MaxCount;

			//if (!collection[Cursor].isInitialized) return false;

			return true;
		}
		object IEnumerator.Current
		{
			get
			{
				if ((Cursor < 0) || (Cursor == collection.Length))
					throw new InvalidOperationException();
				return collection[Cursor];
			}
		}
	}
}
