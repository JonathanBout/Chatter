using System.Buffers;

namespace Chatter.Core
{
	public static class Extensions
	{
		public static IArrayPoolRent<T> RentDisposable<T>(this ArrayPool<T> pool, int minSize)
		{
			return new ArrayPoolRent<T>(pool, minSize);
		}
	}

	public interface IArrayPoolRent<T> : IDisposable
	{
		T[] Array { get; }
		Span<T> Span { get; }
		ArraySegment<T> Segment { get; }
	}

	file sealed class ArrayPoolRent<T> : IDisposable, IArrayPoolRent<T>
	{
		private T[]? _array;
		private readonly int _minSize;
		private bool _disposed;

		private readonly ArrayPool<T> _pool;

		public T[] Array
		{
			get
			{
				ObjectDisposedException.ThrowIf(_disposed, this);
				return _array ??= _pool.Rent(_minSize);
			}
		}

		public Span<T> Span
		{
			get
			{
				return new Span<T>(Array, 0, _minSize);
			}
		}

		public ArraySegment<T> Segment
		{
			get
			{
				return new ArraySegment<T>(Array, 0, _minSize);
			}
		}

		internal ArrayPoolRent(ArrayPool<T> pool, int minSize)
		{
			_pool = pool;
			_minSize = minSize;
		}

		public void Dispose()
		{
			_disposed = true;
			if (_array is not null)
			{
				_pool.Return(_array);
				_array = null;
			}
		}
	}
}
