using System;

[Serializable]
public class RunState
{
    public int version = 1;

    public int fightIndex;

    public Unit player;
}