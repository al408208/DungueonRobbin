using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

using UnityEngine.UI;

public class PuntuacionPlayer : MonoBehaviour
{
    public TextMeshProUGUI contador = null;
     public float pointsPlayer = 0;
     [SerializeField] private float tiempoMaximo; 
    [SerializeField] private Slider slider; 
    private float tiempoActual; 
    public bool tiempoActivo=false;

    [SerializeField]
    private EnemyML2 enemy;

    public GameObject objetActivable1;
    public GameObject objetActivable2;

    private void Start() {
        ActivarTemporizador();
    }

    void Update()
    {
        if(tiempoActivo){
            CambiarContador();
        }
    }

    private void OnTriggerStay2D(Collider2D other)//queremos que se queda en la zona del premio y que no se quede en los bordes
    {
        
        if (other.CompareTag("Target"))
        {   
            DaPremio(0.5f);
        }
        if (other.CompareTag("Borders"))
        {
            DaPremio(-0.1f);
        }
        
    }

    private void DaPremio(float premio)
    {
        pointsPlayer += premio;
        contador.text = (Mathf.Round(pointsPlayer * 10f) / 10f).ToString();
    }
    
    private void CambiarContador(){
        tiempoActual-=Time.deltaTime;
        if(tiempoActual>=0){
            slider.value=tiempoActual;
        }
        if(tiempoActual<=0){
            CambiarTemporizador(false);
            if(pointsPlayer>enemy.pointsBoss){
                SaveData.SaveScoreToCSVLevel3(pointsPlayer,enemy.pointsBoss,"Player"); 
                objetActivable2.SetActive(true);
            }else{
                SaveData.SaveScoreToCSVLevel3(pointsPlayer,enemy.pointsBoss,"Enemy");
                objetActivable1.SetActive(true);
            }
        }
    }

    private void CambiarTemporizador(bool estado){
        tiempoActivo=estado;
    }

    public void ActivarTemporizador(){
        tiempoActual=tiempoMaximo;
        slider.maxValue=tiempoMaximo;
        CambiarTemporizador(true);
    }

}
