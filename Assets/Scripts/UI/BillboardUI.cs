/// <summary>
/// 카메라를 향하도록 회전을 매 프레임 동기화하는 빌보드 컴포넌트.
/// 소유: 월드 스페이스 UI GameObject (MaxIndicator, PrisonerBubbleUI 등)
/// 의존: Camera.main
/// </summary>
using UnityEngine;

public class BillboardUI : MonoBehaviour
{
    private void LateUpdate()
    {
        if (Camera.main == null) { return; }

        transform.rotation = Camera.main.transform.rotation;
    }
}
