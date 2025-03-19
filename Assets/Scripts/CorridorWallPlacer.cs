using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CorridorWallPlacer : MonoBehaviour
{
    [SerializeField] private GameObject wallPrefab;         // 벽 프리팹 (없으면 기본 Cube 사용)
    [SerializeField] private Transform mainRoomTransform;     // 메인 룸(루트)의 Transform (RoomGenerator 기준)
    [SerializeField] private float margin = 10f;              // 복도의 긴 변 길이 (방 사이 간격)
    [SerializeField] private int roomSize = 50;               // 방의 크기 (50)
    [SerializeField] private float fixedWidth = 10f;          // 복도의 너비 (짧은 변, 예: 10)
    [SerializeField] private float corridorWallThickness = 1f;  // 복도 벽 두께
    [SerializeField] private float corridorWallHeight = 3f;     // 복도 벽 높이

    IEnumerator Start()
    {
        // RoomGenerator가 모든 연결 데이터를 채울 시간을 줍니다.
        yield return new WaitForSeconds(0.2f);
        Debug.Log("Placing Corridor Walls. RoomConnections count: " + RoomGenerator.RoomConnections.Count);

        float spacing = roomSize + margin;

        foreach (RoomConnection connection in RoomGenerator.RoomConnections)
        {
            PlaceCorridorWalls(connection, spacing);
        }
    }

    void PlaceCorridorWalls(RoomConnection connection, float spacing)
    {
        // 메인 룸의 월드 좌표를 기준으로 부모와 자식 방의 월드 좌표 계산
        Vector3 mainPos = mainRoomTransform ? mainRoomTransform.position : Vector3.zero;
        Vector3 parentWorld = mainPos + new Vector3(connection.parent.x * spacing, 0, connection.parent.y * spacing);
        Vector3 childWorld  = mainPos + new Vector3(connection.child.x * spacing, 0, connection.child.y * spacing);

        Vector3 corridorPos = Vector3.zero;
        bool isHorizontal = false;
        bool isVertical = false;

        // 그리드 좌표 차이에 따라 복도의 방향 결정
        if (connection.parent.x != connection.child.x)
        {
            isHorizontal = true;
            if (connection.child.x > connection.parent.x)
            {
                // 부모의 오른쪽 벽 중앙과 자식의 왼쪽 벽 중앙의 평균을 구합니다.
                Vector3 parentEdge = parentWorld + new Vector3(roomSize / 2f, 0, 0);
                Vector3 childEdge  = childWorld - new Vector3(roomSize / 2f, 0, 0);
                corridorPos = (parentEdge + childEdge) / 2f;
            }
            else
            {
                Vector3 parentEdge = parentWorld - new Vector3(roomSize / 2f, 0, 0);
                Vector3 childEdge  = childWorld + new Vector3(roomSize / 2f, 0, 0);
                corridorPos = (parentEdge + childEdge) / 2f;
            }
        }
        else if (connection.parent.y != connection.child.y)
        {
            isVertical = true;
            if (connection.child.y > connection.parent.y)
            {
                Vector3 parentEdge = parentWorld + new Vector3(0, 0, roomSize / 2f);
                Vector3 childEdge  = childWorld - new Vector3(0, 0, roomSize / 2f);
                corridorPos = (parentEdge + childEdge) / 2f;
            }
            else
            {
                Vector3 parentEdge = parentWorld - new Vector3(0, 0, roomSize / 2f);
                Vector3 childEdge  = childWorld + new Vector3(0, 0, roomSize / 2f);
                corridorPos = (parentEdge + childEdge) / 2f;
            }
        }

        // 복도의 긴 부분에 대해 양쪽으로 벽 생성
        if (isHorizontal)
        {
            // 수평 복도의 경우, 복도의 긴 변은 x축(길이: margin)이고, 너비는 z축(fixedWidth)
            float halfWidth = fixedWidth / 2f;
            // 위쪽 벽 (z 방향 양의)
            Vector3 topWallPos = new Vector3(
                corridorPos.x, 
                corridorWallHeight / 2f, 
                corridorPos.z + halfWidth + corridorWallThickness / 2f
            );
            Vector3 topWallScale = new Vector3(margin, corridorWallHeight, corridorWallThickness);
            CreateWall(topWallPos, topWallScale);

            // 아래쪽 벽 (z 방향 음의)
            Vector3 bottomWallPos = new Vector3(
                corridorPos.x, 
                corridorWallHeight / 2f, 
                corridorPos.z - halfWidth - corridorWallThickness / 2f
            );
            Vector3 bottomWallScale = new Vector3(margin, corridorWallHeight, corridorWallThickness);
            CreateWall(bottomWallPos, bottomWallScale);
        }
        else if (isVertical)
        {
            // 수직 복도의 경우, 복도의 긴 변은 z축(길이: margin)이고, 너비는 x축(fixedWidth)
            float halfWidth = fixedWidth / 2f;
            // 오른쪽 벽 (x 방향 양의)
            Vector3 rightWallPos = new Vector3(
                corridorPos.x + halfWidth + corridorWallThickness / 2f, 
                corridorWallHeight / 2f, 
                corridorPos.z
            );
            Vector3 rightWallScale = new Vector3(corridorWallThickness, corridorWallHeight, margin);
            CreateWall(rightWallPos, rightWallScale);

            // 왼쪽 벽 (x 방향 음의)
            Vector3 leftWallPos = new Vector3(
                corridorPos.x - halfWidth - corridorWallThickness / 2f, 
                corridorWallHeight / 2f, 
                corridorPos.z
            );
            Vector3 leftWallScale = new Vector3(corridorWallThickness, corridorWallHeight, margin);
            CreateWall(leftWallPos, leftWallScale);
        }
    }

    void CreateWall(Vector3 position, Vector3 scale)
    {
        Debug.Log("Creating corridor wall at: " + position + " scale: " + scale);
        GameObject wall;
        if (wallPrefab != null)
        {
            wall = Instantiate(wallPrefab, position, Quaternion.identity, transform);
        }
        else
        {
            wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.transform.position = position;
            wall.transform.parent = transform;
        }
        wall.transform.localScale = scale;
    }
}
