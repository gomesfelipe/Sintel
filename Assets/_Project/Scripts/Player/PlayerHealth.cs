using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System;

namespace Rotwang.Sintel.Core.Player
{

    public class PlayerHealth : MonoBehaviour, IDamageable
    {
        [SerializeField] protected PlayerLocomotion _locomotion;
        [SerializeField] protected Animator _anim;
        [Header("Health Settings")]
        public float maxHealth = 100f;
        private float currentHealth;
        public event Action<float> OnDamaged;
        public event Action OnDeath;
        [Header("UI")]
        [SerializeField] private Slider healthSlider;

        private void Awake()
        {
            _locomotion ??= GetComponent<PlayerLocomotion>();
            _anim ??= GetComponentInChildren<Animator>();
            currentHealth = maxHealth;
            if (healthSlider != null)
                healthSlider.value = 1f;
        }

        public void TakeDamage(float damage)
        {
            currentHealth -= damage;
            currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

            UpdateHealthUI();
            Debug.Log($"Jogador levou {damage} de dano. HP atual: {currentHealth}");
            OnDamaged?.Invoke(damage);
            if (currentHealth <= 0)
            {
                Die();
            }
        }

        private void UpdateHealthUI()
        {
            if (healthSlider != null)
                healthSlider.value = currentHealth / maxHealth;
        }

        private void Die()
        {
            Debug.Log("Player has died.");
            _anim?.SetTrigger("Death");
            _locomotion.CanMove = false;
            _locomotion.enabled = false;
            OnDeath?.Invoke();
        }

        public float GetHealthPercentage() => currentHealth / maxHealth;
    }
}
