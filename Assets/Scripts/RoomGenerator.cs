using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[DefaultExecutionOrder(-100)] // 다른 스크립트보다 먼저 실행되도록
public class RoomGenerator : MonoBehaviour
{
    [SerializeField] private GameObject roomPrefab;   // 메인 룸 외에 생성할 방 프리팹 (방 크기는 50×1×50)
    [SerializeField] private int treeDegree = 4;        // 최대 4방향 확장 (상하좌우)
    [SerializeField] private float margin = 10f;        // 방 사이의 간격 (복도 및 door 크기로 사용)
    [SerializeField] private int maxDepth = 3;          // 최대 재귀 깊이
    [SerializeField] private int roomSize = 60;         // 방의 크기 (60)

    // 생성된 방의 그리드 좌표와 연결 정보를 저장
    public static HashSet<Vector2Int> RoomPositions = new HashSet<Vector2Int>();
    public static List<RoomConnection> RoomConnections = new List<RoomConnection>();

    // 내부에서 중복 생성 방지를 위한 임시 집합
    private HashSet<Vector2Int> occupiedPositions = new HashSet<Vector2Int>();

    // 메인 룸(루트)의 월드 좌표 (이 스크립트가 붙은 오브젝트의 위치)
    private Vector3 mainRoomWorldPos;

    void Start()
    {
        mainRoomWorldPos = transform.position;
        Vector2Int mainGrid = Vector2Int.zero;
        occupiedPositions.Add(mainGrid);
        RoomPositions.Add(mainGrid);
        // 메인 룸은 이미 씬에 존재하므로, 자식 방만 생성합니다.
        GenerateRoomTree(mainGrid, 0);
    }

    void GenerateRoomTree(Vector2Int currentPos, int depth)
    {
        if (depth >= maxDepth) return;

        // 상, 하, 좌, 우 4방향
        List<Vector2Int> directions = new List<Vector2Int>
        {
            Vector2Int.up,
            Vector2Int.down,
            Vector2Int.left,
            Vector2Int.right
        };

        // 방향을 랜덤하게 섞음
        directions = directions.OrderBy(x => Random.value).ToList();
        int branches = Mathf.Min(treeDegree, directions.Count);

        for (int i = 0; i < branches; i++)
        {
            Vector2Int newPos = currentPos + directions[i];
            if (occupiedPositions.Contains(newPos))
                continue;

            occupiedPositions.Add(newPos);
            RoomPositions.Add(newPos);
            RoomConnections.Add(new RoomConnection(currentPos, newPos));
            InstantiateRoom(newPos);
            GenerateRoomTree(newPos, depth + 1);
        }
    }

    void InstantiateRoom(Vector2Int gridPos)
    {
        float spacing = roomSize + margin;
        Vector3 worldPos = mainRoomWorldPos + new Vector3(gridPos.x * spacing, 0, gridPos.y * spacing);
        if (roomPrefab != null)
        {
            Instantiate(roomPrefab, worldPos, Quaternion.identity, transform);
        }
        else
        {
            GameObject room = GameObject.CreatePrimitive(PrimitiveType.Cube);
            room.transform.position = worldPos;
            room.transform.localScale = new Vector3(roomSize, 1, roomSize);
            room.transform.parent = transform;
        }
    }
}

[System.Serializable]
public class RoomConnection
{
    public Vector2Int parent;
    public Vector2Int child;
    public RoomConnection(Vector2Int p, Vector2Int c)
    {
        parent = p;
        child = c;
    }
}
