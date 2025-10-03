using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public abstract class Action
{
    public abstract bool isConditionMet();
    public abstract float getWeight();
    public abstract bool isSuccess();
    public abstract void execute(Creature creature);
}
