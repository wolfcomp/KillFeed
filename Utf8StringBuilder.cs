using System.Runtime.CompilerServices;

namespace KillFeed;

public class Utf8StringBuilder
{
    private byte[] chunkChars = [];
    private int capacity = 0;
    private int chunkLength = 0;

    public Utf8StringBuilder() { }

    public override string ToString() => Encoding.UTF8.GetString(chunkChars, 0, chunkLength);

    public int Length => chunkLength;

    public void Clear() => chunkLength = 0;

    public void Append(string value) => Append(value, 0, value.Length);

    public void Append(string value, int startIndex, int length) => AppendInternal(Encoding.UTF8.GetBytes(value, startIndex, length).AsSpan());

    public unsafe void Append(ReadOnlySpan<byte> value) => AppendInternal(value.IndexOf(byte.MinValue) < 0 ? value : value[..value.IndexOf(byte.MinValue)]);

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    [SkipLocalsInit]
    private void AppendInternal(ReadOnlySpan<byte> value)
    {
        if (chunkLength + value.Length > capacity)
        {
            capacity = Math.Max(capacity * 2, chunkLength + value.Length);
            var newChunkChars = new byte[capacity];
            chunkChars.AsSpan().CopyTo(newChunkChars);
            chunkChars = newChunkChars;
        }

        value.CopyTo(chunkChars.AsSpan(chunkLength));
        chunkLength += value.Length;
    }

    public ReadOnlySpan<byte> AsSpan() => chunkChars.AsSpan(0, chunkLength);

    public byte[] ToArray() => AsSpan().ToArray();
}