namespace Firaw.SnapCopyText.Services;

public sealed class CaptureRequestGate
{
    private int _active;

    public bool TryEnter() => Interlocked.CompareExchange(ref _active, 1, 0) == 0;

    public void Exit() => Volatile.Write(ref _active, 0);
}
