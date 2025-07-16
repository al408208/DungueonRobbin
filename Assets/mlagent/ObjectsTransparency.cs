using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectsTransparency : MonoBehaviour
{
    // Start is called before the first frame update
    private Collider2D myCollider;

    void Start()
    {
        // Obtén el collider del objeto (asegúrate de que no sea un trigger al inicio)
        myCollider = GetComponent<Collider2D>();
        myCollider.enabled = false; // Lo desactivamos al inicio
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Si el objeto que entra tiene el tag "Player"
        if (other.CompareTag("Player"))
        {
            myCollider.enabled = true; // Activa el collider normal
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        // Si el objeto que sale tiene el tag "Player"
        if (other.CompareTag("Player"))
        {
            myCollider.enabled = false; // Vuelve a desactivar el collider normal
        }
    }
}