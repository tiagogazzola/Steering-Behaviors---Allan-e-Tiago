using UnityEngine;

public class PlayerAI : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float speed = 5f;
    [SerializeField] private float detectionRadius = 1.5f;
    [SerializeField] private float avoidanceStrength = 1.0f;
    [SerializeField] private LayerMask obstacleLayer;

    [Header("Desempacar")]
    [SerializeField] private float stuckCheckInterval = 1f;
    [SerializeField] private float stuckDistanceThreshold = 0.05f;
    [SerializeField] private float evasiveTime = 1.5f;

    private Vector2 lastPosition;
    private float stuckTimer = 0f;
    private float evasiveTimer = 0f;
    private bool isEvasive = false;
    private Vector2 fallbackDirection;

    void Start()
    {
        lastPosition = transform.position;
    }

    void Update()
    {
        // Verifica se chegou no destino
        if (Vector2.Distance(transform.position, target.position) < 0.1f)
        return; // Para tudo


        Vector2 currentPos = transform.position;
        Vector2 movement = Vector2.zero;

        // Verificar se está preso
        stuckTimer += Time.deltaTime;
        if (stuckTimer >= stuckCheckInterval)
        {
            float movedDistance = Vector2.Distance(currentPos, lastPosition);
            if (movedDistance < stuckDistanceThreshold)
            {
                isEvasive = true;
                evasiveTimer = evasiveTime;
            }
            lastPosition = currentPos;
            stuckTimer = 0f;
        }

        if (isEvasive)
        {
            evasiveTimer -= Time.deltaTime;
            if (evasiveTimer <= 0f)
            {
                isEvasive = false;
            }
            else
            {
                // Modo evasivo: tenta fugir de obstáculos
                Vector2 evasiveDir = Vector2.zero;
                Collider2D[] obstacles = Physics2D.OverlapCircleAll(transform.position, detectionRadius, obstacleLayer);
                foreach (Collider2D obstacle in obstacles)
                {
                    Vector2 diff = (Vector2)transform.position - (Vector2)obstacle.transform.position;
                    float distance = diff.magnitude;
                    if (distance > 0)
                    {
                        evasiveDir += (diff.normalized / distance) * avoidanceStrength;
                    }
                }

                if (evasiveDir.magnitude < 0.1f)
                {
                    // Vetor evasivo fraco ou nulo: usar direção aleatória
                    if (fallbackDirection == Vector2.zero)
                        fallbackDirection = Random.insideUnitCircle.normalized;

                    evasiveDir = fallbackDirection;
                }
                else
                {
                    fallbackDirection = Vector2.zero; // resetar quando achar caminho útil
                }

                movement = evasiveDir.normalized * speed;
                transform.position += (Vector3)movement * Time.deltaTime;
                return;
            }
        }

        // Movimento normal com desvio
        Vector2 targetDir = ((Vector2)target.position - (Vector2)transform.position).normalized;
        Vector2 avoidanceDir = Vector2.zero;

        Collider2D[] normalObstacles = Physics2D.OverlapCircleAll(transform.position, detectionRadius, obstacleLayer);
        foreach (Collider2D obstacle in normalObstacles)
        {
            Vector2 diff = (Vector2)transform.position - (Vector2)obstacle.transform.position;
            float distance = diff.magnitude;
            if (distance > 0)
            {
                avoidanceDir += (diff.normalized / distance) * avoidanceStrength;
            }
        }

        Vector2 finalDir = (targetDir + avoidanceDir).normalized;
        movement = finalDir * speed;
        transform.position += (Vector3)movement * Time.deltaTime;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}
