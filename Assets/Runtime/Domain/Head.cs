using System;
using Runtime;

public class Head
{
    private readonly PressCounter _press;
    private Action<int> OnAdd;
    
    public Head()
    {
        _press = PressCounter.StartWith(0, 1);
    }

    public void Add()
    {
        _press.Press();
        OnAdd?.Invoke(_press.Presses);
    }
}
