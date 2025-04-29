using UnityEngine;

public class BurnableObject : MonoBehaviour, IBurnable
{
    [Header("Burnable Settings")]
    public float maxHealth = 100f;
    public float burnDamagePerSecond = 10f;
    public bool isExplosive = false;
    public GameObject explosionPrefab;
    public GameObject flamesPrefab;

    private float currentHealth;
    private bool isBurning = false;
    private GameObject spawnedFlames;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    private void Update()
    {
        if (isBurning)
        {
            ApplyBurnDamage();
        }
    }

    public void OnInteract()
    {
        // Se a tocha estiver ativa e com combustível, o PlayerManager vai chamar StartBurn() 
        Debug.Log("Objeto interagido, mas precisa de tocha ativa para queimar.");
    }

    public void TakeDamage()
    {
        StartBurn();
    }

    public void StartBurn()
    {
        if (isBurning) return;

        isBurning = true;
        Debug.Log($"{gameObject.name} começou a queimar!");

        if (flamesPrefab != null && spawnedFlames == null)
        {
            spawnedFlames = Instantiate(flamesPrefab, transform.position, Quaternion.identity);
            spawnedFlames.transform.SetParent(transform);
        }
    }

    private void ApplyBurnDamage()
    {
        currentHealth -= burnDamagePerSecond * Time.deltaTime;

        if (currentHealth <= 0)
        {
            Die();
        }
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
        Debug.Log($"{gameObject.name} explodiu!");
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
}
