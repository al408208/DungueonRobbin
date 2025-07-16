using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Cinemachine;
using UnityEngine.Events;

public class CorridorFirstDungeonGenerator : SimpleRandomWalkDungeonGenerator
{
    [SerializeField]
    private int corridorLength = 14, corridorCount = 5;//datos para crear los pasillos
    [SerializeField]
    [Range(0.1f,1)]
    private float roomPercent = 0.8f;

    //PCG Data
    private Dictionary<Vector2Int, HashSet<Vector2Int>> roomsDictionary 
        = new Dictionary<Vector2Int, HashSet<Vector2Int>>();
    
    private HashSet<Vector2Int> floorPositions, corridorPositions;

    //Events
    public UnityEvent<DungeonData> OnDungeonFloorReady;

    protected override void RunProceduralGeneration()
    {
        CorridorFirstGeneration();
        DungeonData data = new DungeonData
        {
            roomsDictionary = this.roomsDictionary,
            corridorPositions = this.corridorPositions,
            floorPositions = this.floorPositions
        };
        OnDungeonFloorReady?.Invoke(data);
    }

    private void CorridorFirstGeneration()
    {
        
        floorPositions = new HashSet<Vector2Int>();//para no repetir la primera y ultima baldosa del pasillo
        HashSet<Vector2Int> potentialRoomPositions = new HashSet<Vector2Int>();//posiciones para crear habitaciones

        CreateCorridors(floorPositions,potentialRoomPositions);

        GenerateRooms(potentialRoomPositions);
    }

    private void GenerateRooms(HashSet<Vector2Int> potentialRoomPositions)
    {
        HashSet<Vector2Int> roomPositions = CreateRooms(potentialRoomPositions);

        //despues de crear las habitaciones buscamos callejones
        List<Vector2Int> deadEnds = FindAllDeadEnds(floorPositions);

        //creamos las habitaciones en puntos muertos
        CreateRoomsAtDeadEnd(deadEnds, roomPositions);

        floorPositions.UnionWith(roomPositions); 

        //tilemapVisualizer.Clear();
        tilemapVisualizer.PaintFloorTiles(floorPositions);
        WallGenerator.CreateWalls(floorPositions, tilemapVisualizer);
    }


    private void CreateCorridors(HashSet<Vector2Int> floorPositions,HashSet<Vector2Int> potentialRoomPositions)
    {
        var currentPosition = startPosition;
        potentialRoomPositions.Add(currentPosition);//añado la posicion inicial   

        for (int i = 0; i < corridorCount; i++)//parecido a cuando creamos las habitaciones
        {
            var corridor = ProceduralGenerationAlgorithms.RandomWalkCorridor(currentPosition, corridorLength);
            currentPosition = corridor[corridor.Count - 1];
            potentialRoomPositions.Add(currentPosition);
            floorPositions.UnionWith(corridor);
        }
        corridorPositions=new HashSet<Vector2Int>(floorPositions);// * guardo el pasillo que acabo de crear
    }

    private HashSet<Vector2Int> CreateRooms(HashSet<Vector2Int> potentialRoomPositions) //crear habitaciones a partir de pasillos
    {
        HashSet<Vector2Int> roomPositions = new HashSet<Vector2Int>();
        int roomToCreateCount = Mathf.RoundToInt(potentialRoomPositions.Count * roomPercent);//total habitaciones que crearemos

        List<Vector2Int> roomsToCreate = potentialRoomPositions.OrderBy(x => Guid.NewGuid()).Take(roomToCreateCount).ToList();//habitaciones en puntos aleatorios 
        ClearRoomData();

        foreach (var roomPosition in roomsToCreate) //vamos a crear las habitaciones
        {
            var roomFloor = RunRandomWalk(randomWalkParameters, roomPosition);
            SaveRoomData(roomPosition,roomFloor);
            roomPositions.UnionWith(roomFloor);//para no repetir posiciones
        }
        return roomPositions;
    } 

    private void CreateRoomsAtDeadEnd(List<Vector2Int> deadEnds, HashSet<Vector2Int> roomFloors)
    {
        foreach (var position in deadEnds)
        {
            if(roomFloors.Contains(position) == false)//compruebo que ese punto no esta en una habitacion
            {
                var room = RunRandomWalk(randomWalkParameters, position);
                SaveRoomData(position, room);//guardo la info en el diccionario
                roomFloors.UnionWith(room);
            }
        }
    }
    private List<Vector2Int> FindAllDeadEnds(HashSet<Vector2Int> floorPositions)
    {
        List<Vector2Int> deadEnds = new List<Vector2Int>();
        foreach (var position in floorPositions)
        {
            int neighboursCount = 0;
            foreach (var direction in Direction2D.directionsList)
            {
                if (floorPositions.Contains(position + direction))
                    neighboursCount++;
                
            }
            if (neighboursCount == 1)
                deadEnds.Add(position);
        }
        return deadEnds;
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
