using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IInteractive
{
    public void HoverIn();

    public void HoverOut();

    public void Interact();

    public void Cancel();
}
