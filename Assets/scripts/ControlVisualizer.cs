using UnityEngine;
using UnityEngine.UI;

public class ControlVisualizer : MonoBehaviour
{
    [Header("Configuración Principal")]
    public KeyCode tecla;
    private Image imagenRelleno;

    [Header("Colores")]
    public Color colorActivo = Color.white;
    public Color colorInactivo = new Color(1, 1, 1, 0.1f);

    [Header("Animación de Aplastamiento")]
    public Vector3 escalaPresionado = new Vector3(0.9f, 0.9f, 1f);
    public float velocidadAnimacion = 15f;
    private Vector3 escalaOriginal;

    void Start()
    {
        imagenRelleno = GetComponent<Image>();
        escalaOriginal = transform.localScale;
    }

    void Update()
    {
        if (Input.GetKey(tecla))
        {
            imagenRelleno.color = colorActivo;
            transform.localScale = Vector3.Lerp(transform.localScale, escalaPresionado, Time.deltaTime * velocidadAnimacion);
        }
        else
        {
            imagenRelleno.color = colorInactivo;
            transform.localScale = Vector3.Lerp(transform.localScale, escalaOriginal, Time.deltaTime * velocidadAnimacion);
        }
    }
}