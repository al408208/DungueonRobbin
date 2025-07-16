using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class RoomGenerator : MonoBehaviour
{
    [SerializeField]
    public int count=0;
    public abstract List<GameObject> ProcessRoom(
        Vector2Int roomCenter, 
        HashSet<Vector2Int> roomFloor, 
        HashSet<Vector2Int> corridors);
}
