using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Premio : MonoBehaviour
{
     private bool _isProcessing = false;
    float tiempoUnity;
    float tiempoReal ; // Tiempo real

    [SerializeField]
    private float tCambio=2f; //si batch_size: 64 entonces 14s de simulacion=1s real para conseguir 2-3 segundos pondremos 28
    //si batch_size: 32 podemos poner los segundos reales
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Mlagent") ||other.CompareTag("Player"))
        {
            if(!_isProcessing){
                _isProcessing = true;
                //tiempoUnity = Time.time;
                //tiempoReal = Time.realtimeSinceStartup;
                //Debug.Log($"Tiempo Unity: {tiempoUnity:F2} s | Tiempo Real: {tiempoReal:F2} s");
                //Invoke("MoverPosicionInicial", tCambio);
                StartCoroutine(EsperarYReubicar());
            }
           
         
        }
    }

    private IEnumerator EsperarYReubicar()
    {
        float tiempoInicial = Time.realtimeSinceStartup;
        while (Time.realtimeSinceStartup < tiempoInicial + tCambio)
        {
            yield return null; // Esperar el siguiente frame
        }

        MoverPosicionInicial();
    }

    private void MoverPosicionInicial()
    {
        //float tiempoUnity2 = Time.time-tiempoUnity;
        //float tiempoReal2 = Time.realtimeSinceStartup-tiempoReal;
        //Debug.Log($"Tiempo Unity: {tiempoUnity2:F2} s | Tiempo Real: {tiempoReal2:F2} s");
        bool posicionEncontrada = false;
        int intentos = 100;
        Vector2 posicionPotencial = Vector2.zero;

        while (!posicionEncontrada || intentos >= 0)
        {
            intentos--;
            posicionPotencial = new Vector2(
                transform.parent.position.x+Random.Range(-7f, 8f),  //transform.parent.position.x+ -7/10
                transform.parent.position.y+Random.Range(-10f, 3f));//-10/11

            //en el caso de que tengamos mas cosas en el escenario checker que no choca
            Collider2D[] colliders = Physics2D.OverlapCircleAll(posicionPotencial, 1f);
            
             bool esBorde = true;
            foreach (var collider in colliders)
            {
                if (!collider.CompareTag("Borders"))
                {
                    esBorde = false;                    
                }
            }

            // Si no hay colisiones o la posición quetoca es solo un borde
            if (colliders.Length == 0 || esBorde)
            {
                transform.position = posicionPotencial;
                posicionEncontrada = true;
                _isProcessing = false;
            }
        }
    }
}
