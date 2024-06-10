using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundDetection : MonoBehaviour
{
    public Vector3 groundCheckPosition { get => transform.position + offset; }
    public float groundCheckRadius = 0.5f; // ������ ������
    public Vector3 offset = Vector3.zero;
    public LayerMask groundLayerMask; // ������ ������ ���̾� ����ũ

    [HideInInspector] public bool isGrounded; // ���� �پ� �ִ��� ����

    private void Update()
    {
        // �÷��̾� �ֺ��� �� ����
        Collider[] colliders = Physics.OverlapSphere(groundCheckPosition, groundCheckRadius, groundLayerMask);
        isGrounded = colliders.Length > 0;
    }

    private void OnDrawGizmos()
    {
        // Scene �信�� ���� ������ �ð������� ǥ���ϱ� ���� DrawWireSphere ���
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(groundCheckPosition, groundCheckRadius);
    }
}
