// Licensed under the MIT license: https://opensource.org/licenses/MIT


namespace EchoSharp.ProvisioningModelUtility;

public class SeekableStreamWrapper(Stream source) : Stream
{
    private readonly MemoryStream cacheStream = new();
    private long position;

    public override bool CanRead => true;
    public override bool CanSeek => true;
    public override bool CanWrite => false;

    public override long Length => cacheStream.Length;

    public override long Position
    {
        get => position;
        set => Seek(value, SeekOrigin.Begin);
    }

    public override void Flush()
    {
        throw new NotSupportedException();
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        if (position < cacheStream.Length)
        {
            cacheStream.Position = position;
            var bytesReadFromCache = cacheStream.Read(buffer, offset, count);
            position += bytesReadFromCache;

            if (bytesReadFromCache == count)
            {
                return bytesReadFromCache;
            }
            offset += bytesReadFromCache;
            count -= bytesReadFromCache;
        }

        var bytesRead = source.Read(buffer, offset, count);
        if (bytesRead > 0)
        {
            cacheStream.Position = cacheStream.Length;
            cacheStream.Write(buffer, offset, bytesRead);
            cacheStream.Position = position += bytesRead;
        }

        return bytesRead;
    }

    public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
    {
        if (position < cacheStream.Length)
        {
            cacheStream.Position = position;
            var bytesReadFromCache = await cacheStream.ReadAsync(buffer, cancellationToken);
            position += bytesReadFromCache;

            if (bytesReadFromCache == buffer.Length)
            {
                return bytesReadFromCache;
            }
            buffer = buffer.Slice(bytesReadFromCache);
        }

        var bytesRead = await source.ReadAsync(buffer, cancellationToken);
        if (bytesRead > 0)
        {
            cacheStream.Position = cacheStream.Length;
            await cacheStream.WriteAsync(buffer.Slice(0, bytesRead), cancellationToken);
            cacheStream.Position = position += bytesRead;
        }

        return bytesRead;
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        switch (origin)
        {
            case SeekOrigin.Begin:
                position = offset;
                break;
            case SeekOrigin.Current:
                position += offset;
                break;
            case SeekOrigin.End:
                position = Length + offset;
                break;
        }

        if (position < 0)
        {
            throw new IOException("Attempted to seek before the beginning of the stream.");
        }

        return position;
    }

    public override void SetLength(long value)
    {

    }

    public override void Write(byte[] buffer, int offset, int count)
    {

    }
}
