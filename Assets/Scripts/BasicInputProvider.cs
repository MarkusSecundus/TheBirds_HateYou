using MarkusSecundus.Utils.Extensions;
using MarkusSecundus.Utils.Input;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicInputProvider : BasicInputProviderBase<InputAxis>
{
    protected override string GetAxisName(InputAxis axis) => axis.ToString();
}
