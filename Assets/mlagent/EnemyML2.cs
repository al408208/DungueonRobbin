using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators; // Necesario para ActionBuffers

using TMPro;

public class EnemyML2 : Agent
{
    [Header("Velocidad")]
    [Range(0f, 5f)]
    public float _speed;

    public bool _training = true;

    private Rigidbody2D _rb;

    [SerializeField]
    private Transform _target;

    [SerializeField]
    private Transform _myEnemy; //si myenemy es GameObject usar myenemy.trasform.position
    private float _EnemyDistance2Target = 0.4f;

    [SerializeField] public TextMeshProUGUI rewardsText = null;
    public float pointsBoss = 0; 

    private SpriteRenderer spriteRenderer;
    public override void Initialize()
    {
        _rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        //MaxStep forma parte de la clase Agent
        if (!_training) MaxStep = 0;
    }

    public override void OnEpisodeBegin()
    {
        _rb.velocity = Vector2.zero;

        MoverPosicionInicial();
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
         // Obtener las acciones discretas
        int moveX = actionBuffers.DiscreteActions[0]; // Movimiento horizontal (-1, 0, 1)
        int moveY = actionBuffers.DiscreteActions[1]; // Movimiento vertical (-1, 0, 1)
        
        // Convertir las acciones en un vector de movimiento
        Vector2 movement = new Vector2(moveX - 1, moveY - 1).normalized; // -1, 0, 1 mapeado a (-1, 0, 1)
        //realmente recibioms 0 1 2 por eso resto

        // Aplicar movimiento al Rigidbody2D
       //_rb.velocity = movement * _speed;
        _rb.MovePosition((Vector2)transform.position + movement * _speed * Time.deltaTime);
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActions = actionsOut.DiscreteActions;

        // Obtener entradas del teclado
        int moveX = 1; // Por defecto, no moverse horizontalmente
        int moveY = 1; // Por defecto, no moverse verticalmente

        if (Input.GetKey(KeyCode.LeftArrow)) moveX = 0; // Izquierda (-1)
        if (Input.GetKey(KeyCode.RightArrow)) moveX = 2; // Derecha (+1)

        if (Input.GetKey(KeyCode.DownArrow)) moveY = 0; // Abajo (-1)
        if (Input.GetKey(KeyCode.UpArrow)) moveY = 2; // Arriba (+1)

         // Asignar acciones al buffer discreto
        discreteActions[0] = moveX;
        discreteActions[1] = moveY;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Distancia al objetivo (1 observación)
        sensor.AddObservation(Vector2.Distance(_target.position, transform.position));
        
        Vector2 direction = (_target.position - transform.position).normalized;
        sensor.AddObservation(direction.x);
        sensor.AddObservation(direction.y);
        
        // Posición relativa del objetivo (2 observaciones)
        sensor.AddObservation((Vector2)(_target.position - transform.position));
        // Total: 5 observaciones

        // Distancia del enemigo al objetivo (1 observación)
        sensor.AddObservation(Vector2.Distance(_target.transform.position, _myEnemy.position));

        // Dirección del enemigo al objetivo (2 observaciones: x e y)
        Vector2 direccionEnemigoObjetivo = (_target.position - _myEnemy.position).normalized;
        sensor.AddObservation(direccionEnemigoObjetivo.x);
        sensor.AddObservation(direccionEnemigoObjetivo.y);

        // Distancia al enemigo (1 observación)
        sensor.AddObservation(Vector2.Distance(_myEnemy.position, transform.position));

        // Dirección hacia el enemigo (2 observaciones en 2D)
        Vector2 direccionAlEnemigo = (_myEnemy.position - transform.position).normalized;
        sensor.AddObservation(direccionAlEnemigo.x);
        sensor.AddObservation(direccionAlEnemigo.y);
        //total 6+5=11
    }

    void Update()
    {
        if (_target != null)
        {
            // Si el objetivo está a la izquierda, invierte el sprite (flipX = true)
            if (_target.position.x < transform.position.x)
            {
                spriteRenderer.flipX = true;
            }
            // Si el objetivo está a la derecha, desactiva el flip (flipX = false)
            else if (_target.position.x > transform.position.x)
            {
                spriteRenderer.flipX = false;
            }
        }
        if (_myEnemy != null)
        {
            // Calculamos la distancia entre el enemigo y el objetivo
            if (Vector2.Distance(_target.transform.position, _myEnemy.position) > _EnemyDistance2Target)
            {
                 DaPremio(0.002f); // Recompensa si el enemigo está lejos del objetivo
            }
            else
            {
                 DaPremio(-0.5f); // Penalización si el enemigo está cerca del objetivo
            }
        }
    }

    //Asegurarse de que el circulo es de las dismensiones del entrenamiento porque sino tendremos el problema de que el enemigo me echa del centro
    //pero yo puntuo en los extremos porque para su entrenamiento es 0,5 de radio y el circulo es 1 de radio entonces el piensa que avale
    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Target"))
        {
             DaPremio(0.5f);
        }
        if (other.CompareTag("Borders"))
        {
            DaPremio(-0.05f);
        }
        
    }
    private void DaPremio(float premio)
    {
        AddReward(premio);
        if (rewardsText != null)
        {
            pointsBoss += premio;
            rewardsText.text = (Mathf.Round(pointsBoss * 10f) / 10f).ToString();
        }
    }

    private void MoverPosicionInicial()
    {
        bool posicionEncontrada = false;
        int intentos = 100;
        Vector2 posicionPotencial = Vector2.zero;

        while (!posicionEncontrada || intentos >= 0)
        {
            intentos--;
            posicionPotencial = new Vector2(
                 transform.parent.position.x+Random.Range(-6f, 3f),
                transform.parent.position.y+Random.Range(-8f, 2f));

            //en el caso de que tengamos mas cosas en el escenario checker que no choca
            //Collider2D[] colliders = Physics2D.OverlapCircleAll(posicionPotencial, 0.05f);
            Collider2D[] colliders = Physics2D.OverlapCircleAll(posicionPotencial, 0.5f);

            // Si no hay colisiones o la posición no está en un borde, aceptarla
            if (colliders.Length == 0)
            {
                transform.position = posicionPotencial;
                posicionEncontrada = true;
            }
        }
    }
}
