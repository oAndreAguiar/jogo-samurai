using UnityEngine;

/// <summary>
/// Move todos os inimigos ativos de uma vez, num único Update (mais rápido que um Update por inimigo).
/// Cada inimigo: anda em direção ao jogador, se afasta dos vizinhos (separação),
/// para em paredes altas e sobe degraus e rampas baixas.
/// </summary>
public class EnemyMovementSystem : MonoBehaviour
{
    [SerializeField] private HordeSpawner spawner;

    [Tooltip("Camadas consideradas chão e parede para os inimigos.")]
    [SerializeField] private LayerMask groundMask = ~0;

    [Tooltip("Degrau mais alto que o inimigo sobe, em metros. Mais alto que isso vira parede.")]
    [SerializeField] private float maxStepHeight = 0.5f;

    // Grade espacial para achar vizinhos rápido (em vez de comparar todos com todos).
    // Cada célula aponta para o primeiro inimigo dela, e "next" encadeia os outros.
    private const int BucketCount = 4096;
    private readonly int[] bucketHead = new int[BucketCount];
    private int[] next = new int[0];
    private Vector3[] positions = new Vector3[0];

    private Transform cameraTransform;

    private void Start()
    {
        if (Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }
    }

    private void Update()
    {
        var enemies = spawner.ActiveEnemies;
        int count = enemies.Count;
        if (count == 0)
        {
            return;
        }

        HordeSettings settings = spawner.Settings;
        float dt = Time.deltaTime;
        Vector3 playerPos = spawner.Player.position;
        Quaternion faceCamera = cameraTransform != null ? cameraTransform.rotation : Quaternion.identity;

        EnsureCapacity(count);
        for (int i = 0; i < count; i++)
        {
            positions[i] = enemies[i].GroundPosition;
        }
        BuildGrid(count, settings.separationRadius);

        float radius = settings.separationRadius;
        float radiusSqr = radius * radius;
        float stopSqr = settings.stopDistance * settings.stopDistance;

        for (int i = 0; i < count; i++)
        {
            Enemy enemy = enemies[i];
            Vector3 pos = positions[i];

            // 1) Direção ao jogador (só no plano do chão)
            Vector3 toPlayer = playerPos - pos;
            toPlayer.y = 0f;
            Vector3 velocity = Vector3.zero;
            float distSqr = toPlayer.sqrMagnitude;
            if (distSqr > stopSqr)
            {
                velocity = toPlayer / Mathf.Sqrt(distSqr) * enemy.Data.moveSpeed;
            }

            // 2) Separação: empurra para longe dos vizinhos muito próximos
            velocity += ComputeSeparation(i, pos, radius, radiusSqr) * settings.separationStrength;

            // 3) Anda, respeitando paredes e seguindo a altura do chão
            Vector3 step = velocity * dt;
            enemy.GroundPosition = MoveOnGround(pos, step);

            if (Mathf.Abs(toPlayer.x) > 0.05f)
            {
                enemy.SetFacingLeft(toPlayer.x < 0f);
            }
            enemy.TickFlash(dt);
            enemy.ApplyTransform(faceCamera);
        }
    }

    private Vector3 ComputeSeparation(int self, Vector3 pos, float radius, float radiusSqr)
    {
        Vector3 push = Vector3.zero;
        int cx = Mathf.FloorToInt(pos.x / radius);
        int cz = Mathf.FloorToInt(pos.z / radius);

        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dz = -1; dz <= 1; dz++)
            {
                int other = bucketHead[Hash(cx + dx, cz + dz)];
                while (other != -1)
                {
                    if (other != self)
                    {
                        Vector3 away = pos - positions[other];
                        away.y = 0f;
                        float dSqr = away.sqrMagnitude;
                        if (dSqr < radiusSqr && dSqr > 0.0001f)
                        {
                            float d = Mathf.Sqrt(dSqr);
                            // Quanto mais perto, mais forte o empurrão
                            push += away / d * (1f - d / radius);
                        }
                    }
                    other = next[other];
                }
            }
        }
        return push;
    }

    // Tenta andar; se bater numa parede alta, tenta deslizar só em X ou só em Z.
    private Vector3 MoveOnGround(Vector3 pos, Vector3 step)
    {
        if (step.sqrMagnitude < 0.0000001f)
        {
            return pos;
        }
        if (TryStep(pos, step, out Vector3 result))
        {
            return result;
        }
        if (TryStep(pos, new Vector3(step.x, 0f, 0f), out result))
        {
            return result;
        }
        if (TryStep(pos, new Vector3(0f, 0f, step.z), out result))
        {
            return result;
        }
        return pos;
    }

    private bool TryStep(Vector3 pos, Vector3 step, out Vector3 result)
    {
        Vector3 target = pos + step;
        // Raio de cima para baixo, começando na altura do maior degrau possível
        Vector3 origin = new Vector3(target.x, pos.y + maxStepHeight + 0.05f, target.z);
        float groundY = 0f;
        bool hasGround = Physics.Raycast(origin, Vector3.down, out RaycastHit hit, maxStepHeight + 10f,
            groundMask, QueryTriggerInteraction.Ignore);
        if (hasGround)
        {
            groundY = hit.point.y;
        }

        // Parede: o raio começou dentro de um bloco alto e não achou o topo dele
        Vector3 kneeHeight = new Vector3(pos.x, pos.y + maxStepHeight, pos.z);
        float stepLength = step.magnitude;
        if (Physics.Raycast(kneeHeight, step / stepLength, stepLength + 0.25f, groundMask, QueryTriggerInteraction.Ignore))
        {
            result = pos;
            return false;
        }

        result = new Vector3(target.x, groundY, target.z);
        return true;
    }

    private void BuildGrid(int count, float cellSize)
    {
        for (int b = 0; b < BucketCount; b++)
        {
            bucketHead[b] = -1;
        }
        for (int i = 0; i < count; i++)
        {
            int h = Hash(Mathf.FloorToInt(positions[i].x / cellSize), Mathf.FloorToInt(positions[i].z / cellSize));
            next[i] = bucketHead[h];
            bucketHead[h] = i;
        }
    }

    private static int Hash(int cx, int cz)
    {
        return ((cx * 73856093) ^ (cz * 19349663)) & (BucketCount - 1);
    }

    private void EnsureCapacity(int count)
    {
        if (positions.Length < count)
        {
            int size = Mathf.NextPowerOfTwo(count);
            positions = new Vector3[size];
            next = new int[size];
        }
    }
}
