using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Item : MonoBehaviour
{
    [SerializeField]
    private SpriteRenderer spriteRenderer;
    [SerializeField]
    private BoxCollider2D itemCollider;
    [SerializeField]
    public RoomContentGenerator roomContentGenerator=null;
    private bool catchable;
    private bool isKey;
    private bool treasure;
    private float velocidad = 4.0f; // Velocidad del movimiento
    private float amplitud = 0.15f; // Amplitud del movimiento
    private float tiempo = 0.0f;
    private float origen=0.0f;
    public AudioSource soundCoin;


    public void Initialize(ItemData itemData)
    {
        //set sprite
        spriteRenderer.sprite = itemData.sprite;
        //set sprite offset
        spriteRenderer.transform.localPosition = new Vector2(0.5f * itemData.size.x, 0.5f * itemData.size.y);
        itemCollider.size = itemData.size;
        itemCollider.offset = spriteRenderer.transform.localPosition;

        if (itemData.catchable)
            catchable = true;

        if (itemData.isKey)
            isKey = true;

        if (itemData.treasure)
            treasure = true;

        if (roomContentGenerator == null)
        {
            roomContentGenerator = FindObjectOfType<RoomContentGenerator>();
        }
        origen=transform.position.y;

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Check if the object is catchable and collided with a player
        if (catchable && collision.CompareTag("Player"))
        {
            if(isKey){
                roomContentGenerator.addKey();

            }else{
                itemCollider.enabled = false;
                spriteRenderer.enabled = false;
                if(treasure){
                    soundCoin.Play();
                    roomContentGenerator.addCoins(5);
                }else{
                    soundCoin.Play();
                    roomContentGenerator.addCoins(1);
                }
            }
            // Destroy the item
            Invoke(nameof(DestroyObject), 0.18f);//para poder eactivar el sonido
        }
    }

    void Update()//animacion
    {
        if(catchable && !treasure){
            tiempo += Time.deltaTime * velocidad; // Actualizar el tiempo basado en la velocidad
            float offsetY = Mathf.Sin(tiempo) * amplitud;
            
            transform.position = new Vector2(transform.position.x, origen+offsetY);
        }
    }
    private void DestroyObject()
    {
        Destroy(gameObject);
    }

}

