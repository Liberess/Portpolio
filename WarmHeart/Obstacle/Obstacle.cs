using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Obstacle : MonoBehaviour
{
    abstract public void OnEnter();
    abstract public void OnStay();
    abstract public void OnExit();
    abstract public void OnInteract();
}
