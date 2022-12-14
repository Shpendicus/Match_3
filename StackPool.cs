namespace Match_3;

[StructLayout(LayoutKind.Explicit, Size = 256 * 1, Pack = sizeof(ulong))]
public unsafe ref struct StackBuffer
{
    [FieldOffset(0)]
    private byte first;

    public ref T GetFirst<T>() where T : unmanaged
    {
        return ref Slice<T>(1, 1)[0];
    }

    private void Store<T>(Span<T> data, int pos) where T : unmanaged
    {
        T* current = (T*)Unsafe.AsPointer(ref first) + pos;
        var destination = new Span<T>(current, data.Length);
        data.CopyTo(destination);
    }

    public void Store<T>(ReadOnlySpan<T> data, int pos) where T : unmanaged
    {
        Store(new Span<T>((T*)Unsafe.AsPointer(ref first), data.Length), pos);
    }

    public Span<T> Slice<T>(int start, int length) where T : unmanaged
    {
        return new((T*)Unsafe.AsPointer(ref first) + start, length);
    }

    public ReadOnlySpan<T> SliceROS<T>(int length) where T : unmanaged
    {
        return new((T*)Unsafe.AsPointer(ref first), length);
    }
}