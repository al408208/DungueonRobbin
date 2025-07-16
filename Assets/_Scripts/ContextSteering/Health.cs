using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class Health : MonoBehaviour
{
    [SerializeField]
    private int currentHealth, maxHealth;
    
    

    public UnityEvent<GameObject> OnHitWithReference, OnDeathWithReference;

    [SerializeField]
    private bool isDead = false;
    [SerializeField]
    public RoomContentGenerator roomContentGenerator=null;

    public void InitializeHealth(int healthValue)
    {
        currentHealth = healthValue;
        maxHealth = healthValue;
        isDead = false;
        
    }
    private void Start() {
        if (roomContentGenerator == null)
        {
            roomContentGenerator = FindObjectOfType<RoomContentGenerator>();
        }
    }

    public void GetHit(int amount, GameObject sender)
    {
        if (isDead)
            return;
        if (sender.layer == gameObject.layer)
            return;

        currentHealth -= amount;

        if (currentHealth > 0)
        {
            OnHitWithReference?.Invoke(sender);
        }
        else
        {
            if(CompareTag("Player") ){//solo si eres el personaje
                if(SceneManager.GetActiveScene().name=="Roomsfirst"){
                    SaveData.SaveScoreToCSV(roomContentGenerator.getCoins(),2,roomContentGenerator.getEnemies(),"Yes"); //caso de que mueras antes de terminar el nivel
                }else if(SceneManager.GetActiveScene().name == "CorridorsFirst"){
                    SaveData.SaveScoreToCSV(roomContentGenerator.getCoins(),1,roomContentGenerator.getEnemies(),"Yes"); 
                }
            }else{
                roomContentGenerator.addEnemies();//añado enemigo eliminado
            }
            
            OnDeathWithReference?.Invoke(sender);
            isDead = true;
            Destroy(gameObject);
        }
    }
}
