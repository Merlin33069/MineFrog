using System;
using System.Collections;

namespace MineFrog
{
	internal class TestClass
	{
		internal TestClass()
		{
			Console.WriteLine("loop starting!");
			var bho = new BlockHistoryObjectCollection(100);
			for (int i = 0; i < 120; i++)
			{
				bho.Add(new BlockHistoryObject("BHO: " + i));
			}
			foreach (BlockHistoryObject bho1 in bho)
			{
				Console.WriteLine("loopy");
				Console.WriteLine(bho1.Name);
			}
		}
	}

	internal struct BlockHistoryObject
	{
		public string Name;

		public BlockHistoryObject(string name)
		{
			Name = name;
		}
	}

	internal class BlockHistoryObjectCollection : ICollection
	{
		private readonly BlockHistoryObject[] _collectionArray;
		private readonly int _collectionSize;
		private int _currentPosition;
		private bool _isFull;

		internal BlockHistoryObjectCollection(int size)
		{
			_collectionSize = size;
			_collectionArray = new BlockHistoryObject[size];
		}

		#region ICollection Members

		void ICollection.CopyTo(Array myArr, int index)
		{
			foreach (BlockHistoryObject bho in _collectionArray)
			{
				myArr.SetValue(bho, index);
				index++;
			}
		}

		bool ICollection.IsSynchronized
		{
			get { return false; }
		}

		object ICollection.SyncRoot
		{
			get { return this; }
		}

		int ICollection.Count
		{
			get
			{
				int count = _currentPosition;
				if (_isFull)
				{
					count = _collectionSize;
				}
				return count;
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			int offset = 0;
			int count = _currentPosition;
			if (_isFull)
			{
				count = _collectionSize;
				offset = _currentPosition;
			}

			return new BlockHistoryObjectCollectionEnumerator(_collectionArray, offset, count);
		}

		#endregion

		internal void Add(BlockHistoryObject incoming)
		{
			_collectionArray[_currentPosition] = incoming;
			_currentPosition++;
			if (_currentPosition == 100)
			{
				_currentPosition = 0;
				_isFull = true;
			}
		}
	}

	internal class BlockHistoryObjectCollectionEnumerator : IEnumerator
	{
		private readonly BlockHistoryObject[] _collection;
		private readonly int _maxCount = 100;
		private readonly int _startPosition = -1;
		private int _count;
		private int _cursor = -1;

		internal BlockHistoryObjectCollectionEnumerator(BlockHistoryObject[] bho, int bhoPosition, int count)
		{
			_collection = bho;
			_startPosition = bhoPosition - 1; //We have to start at -1
			_cursor = _startPosition;
			_maxCount = count;
		}

		#region IEnumerator Members

		void IEnumerator.Reset()
		{
			_cursor = _startPosition;
			_count = 0;
		}

		bool IEnumerator.MoveNext()
		{
			//Check that we are within the bounds of the array
			_count++;
			if (_count > _maxCount) return false;

			_cursor++;
			if (_cursor >= _maxCount) _cursor -= _maxCount;

			//if (!collection[Cursor].isInitialized) return false;

			return true;
		}

		object IEnumerator.Current
		{
			get
			{
				if ((_cursor < 0) || (_cursor == _collection.Length))
					throw new InvalidOperationException();
				return _collection[_cursor];
			}
		}

		#endregion
	}
}