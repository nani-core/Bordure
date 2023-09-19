namespace NaniCore {
	public interface Range<T> {
		public T Min { get; }
		public T Max { get; }

		public bool Contains(T value);
	}

	public interface PivotRange<T> : Range<T> {
		public T Pivot { get; }
	}
}