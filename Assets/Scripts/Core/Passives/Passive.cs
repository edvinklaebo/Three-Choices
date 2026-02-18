using System;

[Serializable]
public abstract class Passive
{
    protected Unit Owner;

    public void Attach(Unit owner)
    {
        Owner = owner;
        OnAttach();
    }

    public void Detach()
    {
        OnDetach();
        Owner = null;
    }

    protected virtual void OnAttach()
    {
    }

    protected virtual void OnDetach()
    {
    }
}