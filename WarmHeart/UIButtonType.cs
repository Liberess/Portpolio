using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EUIButtonType { Restart, Resume, Lobby }

public class UIButtonType : MonoBehaviour
{
    [SerializeField] private EUIButtonType eUIButtonType;
    public EUIButtonType EUIButtonType { get => eUIButtonType; }
}