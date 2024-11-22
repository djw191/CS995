using System;

public class TurnManager
{
    // ReSharper disable once MemberCanBePrivate.Global
    public int TurnCount { private set; get; }
    public event Action OnTick;

    public void Tick()
    {
        TurnCount++;
        OnTick?.Invoke();
    }
}