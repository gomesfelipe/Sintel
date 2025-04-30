using UnityEngine;
using System; 
public enum BurnBehavior
{
    Finite,
    Infinite
}
public class BurnableObject : MonoBehaviour, IBurnable
{
    [Header("Burnable Settings")]
    public float maxHealth = 100f, burnDamagePerSecond = 10f;
    public BurnBehavior burnBehavior = BurnBehavior.Finite;
    public GameObject explosionPrefab, flamesPrefab;
    protected float currentHealth;
    protected GameObject spawnedFlames;
    protected WindZone[] windZones;

    [Header("Fire Propagation")]
    public bool isExplosive = false;
    public bool IsBurning() => isBurning;
    public float propagationRadius = 2f, propagationInterval = 2f;
    protected float propagationTimer = 0f;
    public LayerMask burnableLayer;
    public event Action OnStartBurn;

    [Header("Damage to Nearby Enemies")]
    public float damageRadius = 1f, damageInterval = 1f;
    public int damageAmount = 10;
    [SerializeField] protected float damageTimer = 0f, explosionForce = 500f, upwardModifier = 1f;
    [SerializeField] protected bool isBurning = false;
    
    private void Awake()
    {
        currentHealth = maxHealth;
        if (isBurning)
        {
            StartBurn();
        }
        windZones = FindObjectsOfType<WindZone>();
    }

    private void LateUpdate()
    {
        if (isBurning)
        {
            ApplyBurnDamage(burnDamagePerSecond);

            propagationTimer += Time.deltaTime;
            if (propagationTimer >= propagationInterval)
            {
                TryPropagateFire();
                propagationTimer = 0f;
            }
        }
    }

    public void OnInteract()
    {
        Debug.Log("Objeto interagido, mas precisa de tocha ativa para queimar.");
    }

    public void TakeDamage(float damage)
    {
        StartBurn();
    }

    public void StartBurn()
    {
        if (isBurning) return;

        isBurning = true;
        Debug.Log($"{gameObject.name} started to burn!");

        if (flamesPrefab != null && spawnedFlames == null)
        {
            spawnedFlames = Instantiate(flamesPrefab, transform.position, Quaternion.identity);
            spawnedFlames.transform.SetParent(transform);
        }
        OnStartBurn?.Invoke();
    }

    private void ApplyBurnDamage(float damage)
    {
        if (burnBehavior == BurnBehavior.Infinite) return;

        currentHealth -= damage * Time.deltaTime;

        if (currentHealth <= 0)
        {
            Die();
        }
    }
    private void TryPropagateFire()
    {
        Vector3 windDir = GetEffectiveWindDirection(out float windStrength);

        Collider[] nearby = Physics.OverlapSphere(transform.position, propagationRadius, burnableLayer);

        foreach (var col in nearby)
        {
            if (col.gameObject == this.gameObject) continue;

            if (col.TryGetComponent<IBurnable>(out var otherBurnable) &&
                col.TryGetComponent<BurnableObject>(out var burnableObj) &&
                !burnableObj.IsBurning())
            {
                Vector3 directionToOther = (col.transform.position - transform.position).normalized;
                float alignment = Vector3.Dot(directionToOther, windDir);

                float propagationChance = 0.4f + Mathf.Clamp01(alignment) * windStrength;

                if (UnityEngine.Random.value < propagationChance)
                {
                    otherBurnable.StartBurn();
                    Debug.Log($"{name} propagou fogo para {col.name} com chance {propagationChance:F2}");
                }
            }
        }
    }

    private Vector3 GetEffectiveWindDirection(out float strength)
    {
        if (windZones == null || windZones.Length == 0)
        {
            strength = 0f;
            return Vector3.zero;
        }

        Vector3 finalDirection = Vector3.zero;
        strength = 0f;

        foreach (var wz in windZones)
        {
            if (wz.mode == WindZoneMode.Directional)
            {
                finalDirection += wz.transform.forward * wz.windMain;
                strength += wz.windMain;
            }
            else if (wz.mode == WindZoneMode.Spherical)
            {
                float distance = Vector3.Distance(transform.position, wz.transform.position);
                if (distance < wz.radius)
                {
                    Vector3 directionFromCenter = (transform.position - wz.transform.position).normalized;
                    float falloff = 1f - (distance / wz.radius);
                    finalDirection += directionFromCenter * wz.windMain * falloff;
                    strength += wz.windMain * falloff;
                }
            }
        }

        if (strength > 0)
        {
            finalDirection.Normalize();
        }

        return finalDirection;
    }
    private void Die()
    {
        if (isExplosive)
        {
            Explode();
        }
        else
        {
            DestroyObject();
        }
    }

    private void Explode()
    {
        if (explosionPrefab != null)
        {
            Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        }

        Debug.Log($"{gameObject.name} explode!");

        float explosionRadius = propagationRadius;
        LayerMask affectedLayers = burnableLayer;

        Collider[] hitColliders = Physics.OverlapSphere(transform.position, explosionRadius, affectedLayers);

        foreach (var hit in hitColliders)
        {
            if (hit.gameObject == gameObject) continue;
            if (hit.TryGetComponent<IDamageable>(out var damageable))
            {
                float distance = Vector3.Distance(transform.position, hit.transform.position);
                float t = Mathf.Clamp01(distance / propagationRadius);
                float scaledDamage = damageAmount * (1f - t);
                if (scaledDamage > 0)
                {
                    damageable.TakeDamage(scaledDamage);
                    Debug.Log($"{hit.name} recebeu {scaledDamage} de dano da explosão.");
                }
            }
            if (hit.attachedRigidbody != null)
            {
                hit.attachedRigidbody.AddExplosionForce(explosionForce, transform.position, explosionRadius, upwardModifier, ForceMode.Impulse);
            }
        }

        DestroyObject();
    }


    private void DestroyObject()
    {
        if (spawnedFlames != null)
        {
            Destroy(spawnedFlames);
        }
        Destroy(gameObject);
    }
    private void OnDrawGizmosSelected()
    {
    #if UNITY_EDITOR
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, propagationRadius);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, damageRadius);

        // Direções da explosão
        Gizmos.color = Color.cyan;
        int segments = 16; // número de setas
        for (int i = 0; i < segments; i++)
        {
            float angle = i * Mathf.PI * 2f / segments;
            Vector3 dir = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle));
            Gizmos.DrawRay(transform.position, dir * damageRadius);
        }

        // Direção para cima (representa upwardModifier)
        if (upwardModifier > 0)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(transform.position, Vector3.up * upwardModifier);
        }
    #endif
    }


}
