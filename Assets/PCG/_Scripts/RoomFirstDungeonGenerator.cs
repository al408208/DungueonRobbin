using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Cinemachine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

public class RoomFirstDungeonGenerator : SimpleRandomWalkDungeonGenerator
{
    [SerializeField]
    private int minRoomWidth = 4, minRoomHeight = 4;
    [SerializeField]
    private int dungeonWidth = 20, dungeonHeight = 20;//area que queremos dividir
    [SerializeField]
    [Range(0,10)]
    private int offset = 1; //desplazamiento para tener habitaciones separadas
    [SerializeField]
    private bool randomWalkRooms = false;

    //PCG Data
    private Dictionary<Vector2Int, HashSet<Vector2Int>> roomsDictionary 
        = new Dictionary<Vector2Int, HashSet<Vector2Int>>();
    
    private HashSet<Vector2Int> floorPositions, corridorPositions;

    public UnityEvent<DungeonData> OnDungeonFloorReady;

    protected override void RunProceduralGeneration()
    {
        CreateRooms();
        DungeonData data = new DungeonData
        {
            roomsDictionary = this.roomsDictionary,
            corridorPositions = this.corridorPositions,
            floorPositions = this.floorPositions
        };
        OnDungeonFloorReady?.Invoke(data);
    }
    private void CreateRooms()
    {
        floorPositions = new HashSet<Vector2Int>();
        var roomsList = ProceduralGenerationAlgorithms.BinarySpacePartitioning(new BoundsInt((Vector3Int)startPosition, 
            new Vector3Int(dungeonWidth, dungeonHeight, 0)), minRoomWidth, minRoomHeight);

        HashSet<Vector2Int> floor = new HashSet<Vector2Int>();

        if (randomWalkRooms)
        {
            floor = CreateRoomsRandomly(roomsList);
        }
        else
        {
            floor = CreateSimpleRooms(roomsList);
        }
        

        List<Vector2Int> roomCenters = new List<Vector2Int>();
        foreach (var room in roomsList)
        {
            roomCenters.Add((Vector2Int)Vector3Int.RoundToInt(room.center));//punto central de cada habitacion
        }

        HashSet<Vector2Int> corridors = ConnectRooms(roomCenters);
        floor.UnionWith(corridors);

        //tilemapVisualizer.Clear();
        tilemapVisualizer.PaintFloorTiles(floor);
        WallGenerator.CreateWalls(floor, tilemapVisualizer);
    }

    private HashSet<Vector2Int> CreateSimpleRooms(List<BoundsInt> roomsList)//creamos habitaciones simples
    {
        HashSet<Vector2Int> floor = new HashSet<Vector2Int>();
        foreach (var room in roomsList)
        {
            for (int col = offset; col < room.size.x - offset; col++)
            {
                for (int row = offset; row < room.size.y - offset; row++)
                {
                    Vector2Int position = (Vector2Int)room.min + new Vector2Int(col, row);
                    floor.Add(position);
                }
                
            }
            
        }
        return floor;
    }
    private HashSet<Vector2Int> CreateRoomsRandomly(List<BoundsInt> roomsList)
    {
        HashSet<Vector2Int> floor = new HashSet<Vector2Int>();
        ClearRoomData();
        for (int i = 0; i < roomsList.Count; i++)
        {
            var roomBounds = roomsList[i]; //habitacion

            // ajusto los limites de la habitacion para aplicar los espacios
            var adjustedRoomBounds = new BoundsInt(
                roomBounds.xMin + offset,
                roomBounds.yMin + offset,
                roomBounds.zMin,
                roomBounds.size.x - 2 * offset, //pongo el 2 porque reduzco arriba y abajo
                roomBounds.size.y - 2 * offset,
                roomBounds.size.z
            );

            //centro de la habitación ajustada
            var roomCenter = new Vector2Int(Mathf.RoundToInt(adjustedRoomBounds.center.x),Mathf.RoundToInt(adjustedRoomBounds.center.y));
            var roomFloor = RunRandomWalk(randomWalkParameters, roomCenter);//creamos el suelo aleatorio
            SaveRoomData(roomCenter,roomFloor);

            // añado todas las posiciones al suelo final
            floor.UnionWith(roomFloor);
        }
        return floor;
    }

    private HashSet<Vector2Int> ConnectRooms(List<Vector2Int> roomCenters)
    {
        HashSet<Vector2Int> corridors = new HashSet<Vector2Int>();
        var currentRoomCenter = roomCenters[Random.Range(0, roomCenters.Count)];//elegimos un punto aleatorio
        roomCenters.Remove(currentRoomCenter);//lo eliminamos

        while (roomCenters.Count > 0)//mientras haya puntos centrales
        {
            Vector2Int closest = FindClosestPointTo(currentRoomCenter, roomCenters);//vamos a buscar el punto mas cercano
            roomCenters.Remove(closest);
            HashSet<Vector2Int> newCorridor = CreateCorridor(currentRoomCenter, closest);
            currentRoomCenter = closest; //pasamos al sigueinte punto
            corridors.UnionWith(newCorridor);
        }
        corridorPositions=new HashSet<Vector2Int>(corridors);//guardo los pasillos
        return corridors;
    }

    private Vector2Int FindClosestPointTo(Vector2Int currentRoomCenter, List<Vector2Int> roomCenters)
    {
        Vector2Int closest = Vector2Int.zero;//punto mas cercano 
        float distance = float.MaxValue;//valor que nos indica el punto mas cercano
        foreach (var position in roomCenters)
        {
            float currentDistance = Vector2.Distance(position, currentRoomCenter);
            if(currentDistance < distance)
            {
                distance = currentDistance;
                closest = position;
            }
        }
        return closest;
    }

    private HashSet<Vector2Int> CreateCorridor(Vector2Int currentRoomCenter, Vector2Int destination)
    {
        HashSet<Vector2Int> corridor = new HashSet<Vector2Int>();
        var position = currentRoomCenter;
        corridor.Add(position);
        while (position.y != destination.y)//hasta que no esten a la misma altura
        {
            if(destination.y > position.y)
            {
                position += Vector2Int.up;
            }
            else if(destination.y < position.y)
            {
                position += Vector2Int.down;
            }
            corridor.Add(position);
        }
        while (position.x != destination.x)//hasta que no esten en la misma x
        {
            if (destination.x > position.x)
            {
                position += Vector2Int.right;
            }else if(destination.x < position.x)
            {
                position += Vector2Int.left;
            }
            corridor.Add(position);
        }
        return corridor;
    }

    private void ClearRoomData()
    {
        roomsDictionary.Clear();
    }

    private void SaveRoomData(Vector2Int roomPosition, HashSet<Vector2Int> roomFloor)
    {
        roomsDictionary[roomPosition] = roomFloor;
    }
}
