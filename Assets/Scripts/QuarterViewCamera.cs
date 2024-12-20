using UnityEngine;

public class QuarterViewCamera : MonoBehaviour
{
    public Transform player;       // �÷��̾��� Transform
    private Vector3 offset = new Vector3(-5, 7, -5); // ī�޶� ��ġ ������
    private float smoothSpeed = 5f; // ī�޶� �̵� �ε巴�� ó��

    void LateUpdate()
    {
        // ī�޶� ��ǥ ��ġ ����
        Vector3 targetPosition = player.position + offset;

        // �ε巴�� ī�޶� �̵�
        transform.position = Vector3.Lerp(transform.position, targetPosition, smoothSpeed * Time.deltaTime);

        // ī�޶�� �÷��̾ �ٶ󺸵��� ����
        transform.LookAt(player.position);
    }
}
