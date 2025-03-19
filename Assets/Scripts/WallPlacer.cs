using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WallPlacer : MonoBehaviour
{
    [SerializeField] private GameObject wallPrefab;        // 벽 프리팹 (없으면 기본 Cube 사용)
    [SerializeField] private Transform mainRoomTransform;    // 메인 룸(루트)의 Transform (RoomGenerator 기준)
    [SerializeField] private float margin = 10f;             // 방 사이의 마진 (RoomGenerator와 동일)
    [SerializeField] private int roomSize = 50;              // 방의 크기 (50)
    [SerializeField] private float wallHeight = 3f;          // 벽 높이
    [SerializeField] private float wallThickness = 2f;       // 벽 두께
    [SerializeField] private float doorWidth = 10f;          // 복도의 단면 크기 (문(door) 영역으로 남길 크기)

    void Start()
    {
        StartCoroutine(PlaceWallsCoroutine());
    }

    IEnumerator PlaceWallsCoroutine()
    {
        // RoomGenerator가 모든 방을 생성할 시간을 줍니다.
        yield return new WaitForSeconds(0.2f);
        Debug.Log("Placing Walls. RoomPositions count: " + RoomGenerator.RoomPositions.Count);
        float spacing = roomSize + margin;

        foreach (Vector2Int gridPos in RoomGenerator.RoomPositions)
        {
            Vector3 roomCenter = mainRoomTransform.position + new Vector3(gridPos.x * spacing, 0, gridPos.y * spacing);
            PlaceWallForSide(gridPos, roomCenter, Vector2Int.right);  // 동쪽
            PlaceWallForSide(gridPos, roomCenter, Vector2Int.left);   // 서쪽
            PlaceWallForSide(gridPos, roomCenter, Vector2Int.up);       // 북쪽 (그리드 up → 월드 +Z)
            PlaceWallForSide(gridPos, roomCenter, Vector2Int.down);     // 남쪽 (그리드 down → 월드 -Z)
        }
    }

    void PlaceWallForSide(Vector2Int roomGridPos, Vector3 roomCenter, Vector2Int dir)
    {
        // 인접한 방과 연결(door)이 있으면 문 영역으로 doorGap을 doorWidth로 설정
        bool hasDoor = HasDoor(roomGridPos, dir);
        float doorGap = hasDoor ? doorWidth : 0f;
        float halfRoom = roomSize / 2f;

        // 동/서쪽 면 (x축 고정, z축 길이 결정)
        if (dir == Vector2Int.right || dir == Vector2Int.left)
        {
            float sideSign = (dir == Vector2Int.right) ? 1f : -1f;
            float wallX = roomCenter.x + sideSign * (halfRoom + wallThickness / 2f);

            if (doorGap > 0f)
            {
                // 위쪽 세그먼트 (월드 z 방향 +)
                float upperStart = roomCenter.z + doorGap / 2f;
                float upperEnd = roomCenter.z + halfRoom;
                float upperLength = upperEnd - upperStart;
                if (upperLength > 0)
                {
                    float segmentCenterZ = (upperStart + upperEnd) / 2f;
                    CreateWall(new Vector3(wallX, wallHeight / 2f, segmentCenterZ), new Vector3(wallThickness, wallHeight, upperLength));
                }
                // 아래쪽 세그먼트 (월드 z 방향 -)
                float lowerStart = roomCenter.z - halfRoom;
                float lowerEnd = roomCenter.z - doorGap / 2f;
                float lowerLength = lowerEnd - lowerStart;
                if (lowerLength > 0)
                {
                    float segmentCenterZ = (lowerStart + lowerEnd) / 2f;
                    CreateWall(new Vector3(wallX, wallHeight / 2f, segmentCenterZ), new Vector3(wallThickness, wallHeight, lowerLength));
                }
            }
            else
            {
                // door가 없으면 전체 면에 대해 하나의 벽 생성
                CreateWall(new Vector3(wallX, wallHeight / 2f, roomCenter.z), new Vector3(wallThickness, wallHeight, roomSize));
            }
        }
        // 북/남쪽 면 (z축 고정, x축 길이 결정)
        else if (dir == Vector2Int.up || dir == Vector2Int.down)
        {
            float sideSign = (dir == Vector2Int.up) ? 1f : -1f;
            float wallZ = roomCenter.z + sideSign * (halfRoom + wallThickness / 2f);

            if (doorGap > 0f)
            {
                // 오른쪽 세그먼트 (월드 x 방향 +)
                float rightStart = roomCenter.x + doorGap / 2f;
                float rightEnd = roomCenter.x + halfRoom;
                float rightLength = rightEnd - rightStart;
                if (rightLength > 0)
                {
                    float segmentCenterX = (rightStart + rightEnd) / 2f;
                    CreateWall(new Vector3(segmentCenterX, wallHeight / 2f, wallZ), new Vector3(rightLength, wallHeight, wallThickness));
                }
                // 왼쪽 세그먼트 (월드 x 방향 -)
                float leftStart = roomCenter.x - halfRoom;
                float leftEnd = roomCenter.x - doorGap / 2f;
                float leftLength = leftEnd - leftStart;
                if (leftLength > 0)
                {
                    float segmentCenterX = (leftStart + leftEnd) / 2f;
                    CreateWall(new Vector3(segmentCenterX, wallHeight / 2f, wallZ), new Vector3(leftLength, wallHeight, wallThickness));
                }
            }
            else
            {
                CreateWall(new Vector3(roomCenter.x, wallHeight / 2f, wallZ), new Vector3(roomSize, wallHeight, wallThickness));
            }
        }
    }

    bool HasDoor(Vector2Int roomPos, Vector2Int dir)
    {
        Vector2Int adjacent = roomPos + dir;
        foreach (RoomConnection conn in RoomGenerator.RoomConnections)
        {
            if ((conn.parent == roomPos && conn.child == adjacent) ||
                (conn.child == roomPos && conn.parent == adjacent))
            {
                return true;
            }
        }
        return false;
    }

    void CreateWall(Vector3 position, Vector3 scale)
    {
        Debug.Log("Creating wall at: " + position + " scale: " + scale);
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
