using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

public class Controller : MonoBehaviour
{
    public Camera mainCamera;
    public NavMeshAgent agent;

    void Update()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                agent.SetDestination(hit.point);
            }
        }
    }
}
