using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using UnityEngine;

public class script_zombie : MonoBehaviour
{

    Rigidbody2D rb;
    float limiteCaminataIzq;
    float limiteCaminataDer;

    public float velCaminata = 5f;
    int direccion = 1;
    Vector3 escalaOriginal;

    public float umbralVelocidad;

    public GameObject prefabMuerto;

    public float magnitudVueloCabeza = 200f;

    enum tipoComportamientoZombie { pasivo, persecución, ataque}

    tipoComportamientoZombie comportamiento = tipoComportamientoZombie.pasivo;

    float entradaZonaPersecución = 60f;
    float salidaZonaPersecución = 100f;
    float distanciaAtaque = 7f;

    float distanciaConPersonaje;
    public Transform Personaje;

    Animator anim;

    bool mordidaEsValida = false;
    public Transform refPiso;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        limiteCaminataDer = transform.position.x + GetComponent<CircleCollider2D>().radius;
        limiteCaminataIzq = transform.position.x - GetComponent<CircleCollider2D>().radius;

        escalaOriginal = transform.localScale;

        anim = transform.GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        bool enPiso = Physics2D.OverlapCircle(refPiso.position, 1f, 1 << 8); //cuando el pie esta cerca del piso

        distanciaConPersonaje = Mathf.Abs(Personaje.position.x - transform.position.x);
        bool pocaDistanciaVertical = Mathf.Abs(Personaje.position.y - transform.position.y) < 15f;
        switch (comportamiento)
        {
            case tipoComportamientoZombie.pasivo:
                //pasivo
                if (rb.velocity.magnitude < umbralVelocidad)
                {
                    //desplazarse caminando
                    rb.velocity = new Vector2(velCaminata * direccion, rb.velocity.y);
                    //girarse 
                    if (transform.position.x < limiteCaminataIzq) direccion = 1;
                    if (transform.position.x > limiteCaminataDer) direccion = -1;

                    //velocidad del animador
                    anim.speed = 1f;
                    //entrar en zona persecucion
                   
                    if (distanciaConPersonaje < entradaZonaPersecución && !pocaDistanciaVertical) comportamiento = tipoComportamientoZombie.persecución;
                }
                break;

            case tipoComportamientoZombie.persecución:
                //persecucion
                if (rb.velocity.magnitude < umbralVelocidad)
                {
                    //desplazarse corriendo
                    rb.velocity = new Vector2(velCaminata * 1.5f * direccion, rb.velocity.y);
                    //girarse 
                    if (Personaje.position.x > transform.position.x) direccion = 1;
                    if (Personaje.position.x < transform.position.x) direccion = -1;

                    //velocidad del animador
                    anim.speed = 1.5f;

                    //volver a la zona pasiva

                    if (distanciaConPersonaje > salidaZonaPersecución || pocaDistanciaVertical) comportamiento = tipoComportamientoZombie.pasivo;

                    //entre a la zona de ataque
                    if (distanciaConPersonaje < distanciaAtaque) comportamiento = tipoComportamientoZombie.ataque;
                }
                break;

            case tipoComportamientoZombie.ataque:
                //ataque
                if (rb.velocity.magnitude < umbralVelocidad)
                {
                    anim.SetTrigger("Atacar");
                    //girarse posicion personaje
                    if (Personaje.position.x > transform.position.x) direccion = 1;
                    if (Personaje.position.x < transform.position.x) direccion = -1;

                    anim.speed = 1f;
                    
                    //volver zona persecucion
                    if (distanciaConPersonaje > distanciaAtaque) {
                    comportamiento = tipoComportamientoZombie.persecución;
                    anim.ResetTrigger("Atacar");
                }
                }
                break;

                //si no hay piso poner en 0 su velocidad
                if (!enPiso) rb.velocity = new Vector3(0, rb.velocity.y);
        }
        

        transform.localScale = new Vector3(escalaOriginal.x * direccion, escalaOriginal.y, escalaOriginal.z);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if(collision.gameObject.CompareTag("Player") && mordidaEsValida)
        {
            mordidaEsValida = false;
            Personaje.GetComponent<script_personaje>().RecibirMordida(collision.contacts[0].point);
        }
    }

    public void muere(Vector3 direccion)
    {
        GameObject instMuerto = Instantiate(prefabMuerto, transform.position, transform.rotation);

        instMuerto.transform.GetChild(0).GetComponent<Rigidbody2D>().AddForce(direccion * magnitudVueloCabeza, ForceMode2D.Impulse);
        instMuerto.transform.GetChild(1).GetComponent<Rigidbody2D>().AddForce(direccion * magnitudVueloCabeza/2, ForceMode2D.Impulse);
        instMuerto.transform.GetChild(0).GetComponent<Rigidbody2D>().AddTorque(10f, ForceMode2D.Impulse);
        instMuerto.transform.GetChild(1).GetComponent<Rigidbody2D>().AddTorque(-10f, ForceMode2D.Impulse);

        Destroy(gameObject);
    }

    public void mordidaValida_inicio(){
        mordidaEsValida = true;
    }

    public void mordidaValida_fin(){
        mordidaEsValida = false;
    }
}
