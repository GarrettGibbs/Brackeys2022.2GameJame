using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public abstract class EnemyFigure : Figure
{
    public abstract Task TakeAction();
}
