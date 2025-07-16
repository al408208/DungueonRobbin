using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using TMPro;
using UnityEngine.SceneManagement;

public class RoomContentGenerator : MonoBehaviour
{
    [SerializeField]
    private RoomGenerator playerRoom, defaultRoom;

    List<GameObject> spawnedObjects = new List<GameObject>();

    public Transform itemParent;
    private int totalkeys=0;
    private int totalcoins=0;
    private int enemiesDefeated=0;

    public TextMeshProUGUI keys;
    public TextMeshProUGUI coins;

    [SerializeField]
    private CinemachineVirtualCamera cinemachineCamera;

    public UnityEvent RegenerateDungeon;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            foreach (var item in spawnedObjects)
            {
                Destroy(item);
            }
            RegenerateDungeon?.Invoke();
        }
    }
    public void GenerateRoomContent(DungeonData dungeonData)
    {
        foreach (GameObject item in spawnedObjects)
        {
            DestroyImmediate(item);
        }
        spawnedObjects.Clear();

        SelectPlayerSpawnPoint(dungeonData);
        SelectEnemySpawnPoints(dungeonData);

        foreach (GameObject item in spawnedObjects)
        {
            if(item != null)
                item.transform.SetParent(itemParent, false);
        }
    }

    private void SelectPlayerSpawnPoint(DungeonData dungeonData)
    {
        int randomRoomIndex = UnityEngine.Random.Range(0, dungeonData.roomsDictionary.Count);
        Vector2Int playerSpawnPoint = dungeonData.roomsDictionary.Keys.ElementAt(randomRoomIndex);


        Vector2Int roomIndex = dungeonData.roomsDictionary.Keys.ElementAt(randomRoomIndex);

        List<GameObject> placedPrefabs = playerRoom.ProcessRoom(
            playerSpawnPoint,
            dungeonData.roomsDictionary.Values.ElementAt(randomRoomIndex),
            dungeonData.GetRoomFloorWithoutCorridors(roomIndex)
            );

        FocusCameraOnThePlayer(placedPrefabs[placedPrefabs.Count - 1].transform);

        spawnedObjects.AddRange(placedPrefabs);

        dungeonData.roomsDictionary.Remove(playerSpawnPoint);
    }

    private void FocusCameraOnThePlayer(Transform playerTransform)
    {
        cinemachineCamera.LookAt = playerTransform;
        cinemachineCamera.Follow = playerTransform;
    }

    private void SelectEnemySpawnPoints(DungeonData dungeonData)
    {
        defaultRoom.count=0;
        totalkeys=0;
        foreach (KeyValuePair<Vector2Int,HashSet<Vector2Int>> roomData in dungeonData.roomsDictionary)
        { 
            spawnedObjects.AddRange(
                defaultRoom.ProcessRoom(
                    roomData.Key,
                    roomData.Value, 
                    dungeonData.GetRoomFloorWithoutCorridors(roomData.Key)
                    )
            );

        }
        coins.text = totalcoins.ToString();
        keys.text = totalkeys.ToString()+"/"+defaultRoom.count.ToString();
    }

    public void addKey(){
        totalkeys++;
        keys.text = totalkeys.ToString()+"/"+defaultRoom.count.ToString();
        if(totalkeys==1){
            if(SceneManager.GetActiveScene().name=="Roomsfirst"){
                SaveData.SaveScoreToCSV(totalcoins,2,enemiesDefeated,"No");     // Llama al método para guardar el puntaje
                SceneManager.LoadScene("FinalRoom");                            // Voy a la siguiente escena
            }else if(SceneManager.GetActiveScene().name == "CorridorsFirst"){
                SaveData.SaveScoreToCSV(totalcoins,1,enemiesDefeated,"No"); 
                SceneManager.LoadScene("Roomsfirst");
            }
        }
    }

    public void addCoins(int valor){
        totalcoins+=valor;
        coins.text = totalcoins.ToString();
    }
    public int getCoins(){
        return totalcoins;
    }
    public void addEnemies(){
        enemiesDefeated++;
    }
    public int getEnemies(){
        return enemiesDefeated;
    }


}
