using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hun.Obstacle
{
    public class StageDoor : MonoBehaviour, IObstacle
    {
        public void OnEnter()
        {
            OnInteract();
        }

        public void OnExit()
        {

        }

        public void OnInteract()
        {
            Debug.Log("���� �̵� ����");
        }
    }
}