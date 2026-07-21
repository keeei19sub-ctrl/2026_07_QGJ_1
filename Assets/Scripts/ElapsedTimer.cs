public sealed class ElapsedTimer
{
    private float startedAt;
    private float duration;

    public bool IsRunning { get; private set; }
    public float Duration => duration;

    public bool Start(float currentTime, float newDuration)
    {
        if (float.IsNaN(currentTime)
            || float.IsInfinity(currentTime)
            || newDuration <= 0f
            || float.IsNaN(newDuration)
            || float.IsInfinity(newDuration))
        {
            return false;
        }

        startedAt = currentTime;
        duration = newDuration;
        IsRunning = true;
        return true;
    }

    public float GetElapsedTime(float currentTime)
    {
        if (duration <= 0f)
        {
            return 0f;
        }

        return Clamp(currentTime - startedAt, 0f, duration);
    }

    public float GetRemainingTime(float currentTime)
    {
        return duration - GetElapsedTime(currentTime);
    }

    public float GetProgress(float currentTime)
    {
        return duration > 0f
            ? GetElapsedTime(currentTime) / duration
            : 0f;
    }

    public bool TryComplete(float currentTime)
    {
        if (!IsRunning || GetElapsedTime(currentTime) < duration)
        {
            return false;
        }

        IsRunning = false;
        return true;
    }

    public void Stop()
    {
        IsRunning = false;
    }

    private static float Clamp(float value, float minimum, float maximum)
    {
        if (value < minimum)
        {
            return minimum;
        }

        return value > maximum ? maximum : value;
    }
}
