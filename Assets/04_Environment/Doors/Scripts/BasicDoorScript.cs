using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicDoorScript : AbstractDoorScript
{
    void Awake()
    {
        UpdateDoorMaterial();
        health = 1;
    }
}