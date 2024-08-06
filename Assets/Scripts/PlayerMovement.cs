using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;

public class PlayerMovement : ProjectBehaviour
{
    //[SerializeField] private bool isWalking;

    private NavMeshAgent navMeshAgent;

    private void Start()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        navMeshAgent.speed = PlayerStats.Instance.GetMoveSpeed();
        navMeshAgent.radius = PlayerStats.Instance.GetPlayerRadius();
        navMeshAgent.acceleration = PlayerStats.Instance.GetAccelerateSpeed();
    }

    void Update()
    {
        Movement();
    }

    private void Movement()
    {
        if (PauseGame || InventoryOpen) return;

        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0f;
            navMeshAgent.SetDestination(mousePos);
        }

        //float inputX = 0;
        //float inputY = 0;

        //if (Input.GetKey(KeyCode.D))
        //{
        //    inputX += 1;
        //}

        //if (Input.GetKey(KeyCode.A))
        //{
        //    inputX -= 1;
        //}

        //if (Input.GetKey(KeyCode.W))
        //{
        //    inputY += 1;
        //}

        //if (Input.GetKey(KeyCode.S))
        //{
        //    inputY -= 1;
        //}

        //Vector2 inputVector2 = new Vector2(inputX, inputY);

        //Vector2 moveDir = inputVector2.normalized;

        //float moveDistance = moveSpeed * Time.deltaTime;
        //bool canMove = !Physics2D.CircleCast(this.transform.position, playerRadius, moveDir, moveDistance);

        //if (!canMove)
        //{
        //    Vector2 moveDirX = new Vector2(moveDir.x, 0f).normalized;
        //    canMove = !Physics2D.CircleCast(this.transform.position, playerRadius, moveDirX, moveDistance);

        //    if (canMove)
        //    {
        //        moveDir = moveDirX;
        //    }
        //    else
        //    {
        //        Vector2 moveDirY = new Vector2(0f, moveDir.y).normalized;
        //        canMove = !Physics2D.CircleCast(this.transform.position, playerRadius, moveDir, moveDistance);

        //        if (canMove)
        //        {
        //            moveDir = moveDirY;
        //        }
        //        else
        //        {
        //            // Cannot move in any direction
        //        }
        //    }
        //}

        //Vector3 moveDirVector3 = new Vector3(moveDir.x, moveDir.y, 0f);

        //if (canMove)
        //{
        //    transform.position += moveDirVector3 * moveDistance;
        //}

        //isWalking = moveDir != Vector2.zero;

        ////this.transform.forward = Vector3.Slerp(transform.forward, moveDir, rotateSpeed * Time.deltaTime);
    }
}
