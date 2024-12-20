using UnityEngine;

public class QuarterViewCamera : MonoBehaviour
{
    public Transform player;       // 플레이어의 Transform
    private Vector3 offset = new Vector3(-5, 7, -5); // 카메라 위치 오프셋
    private float smoothSpeed = 5f; // 카메라 이동 부드럽게 처리

    void LateUpdate()
    {
        // 카메라 목표 위치 설정
        Vector3 targetPosition = player.position + offset;

        // 부드럽게 카메라 이동
        transform.position = Vector3.Lerp(transform.position, targetPosition, smoothSpeed * Time.deltaTime);

        // 카메라는 플레이어를 바라보도록 유지
        transform.LookAt(player.position);
    }
}
