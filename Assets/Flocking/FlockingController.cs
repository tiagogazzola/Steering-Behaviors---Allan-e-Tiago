using System.Collections.Generic;
using UnityEngine;

public class FlockingController : MonoBehaviour
{
    [Header("Referências")]
    public Transform naveMae;
    public List<Transform> navesFilhas;

    [Header("Velocidades")]
    public float speedMae = 3f;
    public float speedFilha = 2.5f;

    [Header("Parâmetros de Movimento das Filhas")]
    public float cohesionWeight = 1.5f;
    public float separationWeight = 3f;
    public float followWeight = 1.2f;
    public float separationDistance = 1f;
    public float separationFromMaeDistance = 1.5f;
    public float formacaoRadius = 1.5f;

    private Dictionary<Transform, Vector3> offsetFormacao = new Dictionary<Transform, Vector3>();

    void Start()
    {
        foreach (Transform filha in navesFilhas)
        {
            Vector2 offset = Random.insideUnitCircle.normalized * formacaoRadius;
            offsetFormacao[filha] = new Vector3(offset.x, offset.y, 0f);
        }
    }

    void Update()
    {
        SeguirMouse();

        foreach (Transform filha in navesFilhas)
        {
            Vector3 cohesion = GetCohesionVector(filha);
            Vector3 separation = GetSeparationVector(filha);
            Vector3 separationMae = GetSeparationFromMae(filha);

            Vector3 posAlvo = naveMae.position + offsetFormacao[filha];
            Vector3 follow = (posAlvo - filha.position).normalized;

            Vector3 direction = (
                cohesion * cohesionWeight +
                separation * separationWeight +
                separationMae * separationWeight +
                follow * followWeight
            ).normalized;

            Rigidbody2D rb = filha.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                Vector2 novaPos = rb.position + (Vector2)(direction * speedFilha * Time.deltaTime);
                rb.MovePosition(novaPos);
            }

            // Rotação para mirar diretamente na nave mãe
            Vector3 lookDir = naveMae.position - filha.position;
            float angle = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg;
            filha.rotation = Quaternion.Lerp(filha.rotation, Quaternion.Euler(0, 0, angle), Time.deltaTime * 10f);
        }
    }

    void SeguirMouse()
    {
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0;

        Rigidbody2D rb = naveMae.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            Vector2 novaPos = Vector2.MoveTowards(rb.position, mouseWorldPos, speedMae * Time.deltaTime);
            rb.MovePosition(novaPos);
        }
        else
        {
            naveMae.position = Vector3.MoveTowards(naveMae.position, mouseWorldPos, speedMae * Time.deltaTime);
        }
    }

    Vector3 GetCohesionVector(Transform current)
    {
        Vector3 center = Vector3.zero;
        int count = 0;

        foreach (Transform other in navesFilhas)
        {
            if (other == current) continue;
            center += other.position;
            count++;
        }

        if (count == 0) return Vector3.zero;
        center /= count;

        return (center - current.position).normalized;
    }

    Vector3 GetSeparationVector(Transform current)
    {
        Vector3 separation = Vector3.zero;
        int count = 0;

        foreach (Transform other in navesFilhas)
        {
            if (other == current) continue;

            float distance = Vector3.Distance(current.position, other.position);
            if (distance < separationDistance && distance > 0f)
            {
                separation += (current.position - other.position).normalized / distance;
                count++;
            }
        }

        if (count == 0) return Vector3.zero;
        return (separation / count).normalized;
    }

    Vector3 GetSeparationFromMae(Transform filha)
    {
        float distance = Vector3.Distance(filha.position, naveMae.position);
        if (distance < separationFromMaeDistance && distance > 0f)
        {
            return (filha.position - naveMae.position).normalized / distance;
        }
        return Vector3.zero;
    }
}
