using UnityEngine;
using System.Collections;

public class CorridorGenerator : MonoBehaviour
{
    [SerializeField] private GameObject corridorPrefab;
    // 방과 방 사이의 간격(마진): 복도의 긴 변 길이로 사용됨
    [SerializeField] private float margin = 10f;
    // 복도의 짧은 변(폭)을 고정할 값 (예: 10)
    [SerializeField] private float fixedWidth = 10f;
    // 방의 크기 (50x1x50)
    [SerializeField] private int roomSize = 50;
    // 메인 룸(루트 오브젝트)의 Transform (RoomGenerator가 붙은 메인 룸)
    [SerializeField] private Transform mainRoomTransform;

    IEnumerator Start()
    {
        // RoomGenerator에서 RoomConnections가 채워질 시간을 위해 한 프레임 대기
        yield return null;
        foreach (RoomConnection connection in RoomGenerator.RoomConnections)
        {
            CreateCorridor(connection);
        }
    }

    void CreateCorridor(RoomConnection connection)
    {
        // RoomGenerator의 배치 방식: spacing = roomSize + margin
        float spacing = roomSize + margin;
        Vector3 mainPos = mainRoomTransform ? mainRoomTransform.position : Vector3.zero;
        Vector3 parentWorld = mainPos + new Vector3(connection.parent.x * spacing, 0, connection.parent.y * spacing);
        Vector3 childWorld = mainPos + new Vector3(connection.child.x * spacing, 0, connection.child.y * spacing);

        Vector3 corridorPos = Vector3.zero;
        bool isHorizontal = false;
        bool isVertical = false;

        // 두 방의 그리드 좌표 차이에 따라 연결 방향 결정
        if (connection.parent.x != connection.child.x)
        {
            isHorizontal = true;
            if (connection.child.x > connection.parent.x)
            {
                // 부모의 오른쪽 벽 중앙과 자식의 왼쪽 벽 중앙
                Vector3 parentEdge = parentWorld + new Vector3(roomSize / 2f, 0, 0);
                Vector3 childEdge = childWorld - new Vector3(roomSize / 2f, 0, 0);
                corridorPos = (parentEdge + childEdge) / 2f;
            }
            else
            {
                Vector3 parentEdge = parentWorld - new Vector3(roomSize / 2f, 0, 0);
                Vector3 childEdge = childWorld + new Vector3(roomSize / 2f, 0, 0);
                corridorPos = (parentEdge + childEdge) / 2f;
            }
        }
        else if (connection.parent.y != connection.child.y)
        {
            isVertical = true;
            if (connection.child.y > connection.parent.y)
            {
                // 부모의 상단(월드 z +) 벽 중앙과 자식의 하단(월드 z -) 벽 중앙
                Vector3 parentEdge = parentWorld + new Vector3(0, 0, roomSize / 2f);
                Vector3 childEdge = childWorld - new Vector3(0, 0, roomSize / 2f);
                corridorPos = (parentEdge + childEdge) / 2f;
            }
            else
            {
                Vector3 parentEdge = parentWorld - new Vector3(0, 0, roomSize / 2f);
                Vector3 childEdge = childWorld + new Vector3(0, 0, roomSize / 2f);
                corridorPos = (parentEdge + childEdge) / 2f;
            }
        }

        GameObject corridor;
        if (corridorPrefab != null)
        {
            corridor = Instantiate(corridorPrefab, corridorPos, Quaternion.identity, transform);
        }
        else
        {
            corridor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            corridor.transform.position = corridorPos;
            corridor.transform.parent = transform;
        }

        // 복도의 스케일 설정:
        // - 수평 복도: x축 길이 = margin, z축 폭 = fixedWidth
        // - 수직 복도: z축 길이 = margin, x축 폭 = fixedWidth
        if (isHorizontal)
        {
            corridor.transform.localScale = new Vector3(margin, 1, fixedWidth);
        }
        else if (isVertical)
        {
            corridor.transform.localScale = new Vector3(fixedWidth, 1, margin);
        }
    }
}
