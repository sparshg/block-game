using UnityEngine;

public enum PowerupType {
    None,
    SpeedBoost,
    HealthBoost,
    Shield
}

public class PowerupScript : MonoBehaviour
{
    // choose a random powerup type from the enum
    public PowerupType type;
    public int duration;
    public int effectValue;
    public Sprite sprite;

    void Start(){
        type = (PowerupType)Random.Range(1, System.Enum.GetNames(typeof(PowerupType)).Length);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            ApplyEffect();
            Destroy(gameObject);
        }
    }

    private void ApplyEffect()
    {
        switch (type)
        {
            case PowerupType.SpeedBoost:
                // Apply speed boost effect to the player
                break;
            case PowerupType.HealthBoost:
                // Apply health boost effect to the player
                break;
            case PowerupType.Shield:
                // Apply shield effect to the player
                break;
            default:
                break;
        }
    }
}