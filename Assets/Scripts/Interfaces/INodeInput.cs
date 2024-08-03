using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface INodeInput : INode
{
    //public NodeHook AttachedNodeHook { get; set; }

    public void Activate();
}
