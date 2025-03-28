using UnityEngine;
using Projectiles;

public class Tanke_Script : MonoBehaviour
{
    public GameObject bulletPrefab; // Prefab del proyectil
    public Transform firePoint;     // Punto desde donde se disparan los proyectiles
    public float fireRate = 1f;   // Tiempo entre disparos (segundos)
    public float bulletSpeed = 50f; // Velocidad del proyectil
    public float moveSpeed = 5f;    // Velocidad de movimiento del tanque
    private float fireCooldown = 0f; // Temporizador para el disparo
    private Vector2 lastFireDirection = Vector2.right; // Dirección inicial de disparo

    void Update()
    {
        HandleMovement();
        HandleShooting();
    }

    void HandleMovement()
    {
        // Implementar lógica de movimiento aquí
    }

    void HandleShooting()
    {
        // Reduce el temporizador
        fireCooldown -= Time.deltaTime;
    }

    public bool IsAbleToShoot()
    {
        return fireCooldown <= 0f;
    }

    public void Fire(Projectile projectile)
    {
        if (IsAbleToShoot())
        {
            // Determina la dirección del disparo basado en el valor de projectile.direction
            Vector2 direction = Vector2.zero;
            switch (projectile.Direction)
            {
                case 0: // Arriba
                    direction = Vector2.up;
                    break;
                case 1: // Derecha
                    direction = Vector2.right;
                    break;
                case 2: // Abajo
                    direction = Vector2.down;
                    break;
                case 3: // Izquierda
                    direction = Vector2.left;
                    break;
            }
            RotateSpriteToDirection(direction);
            // Crea el proyectil en el punto de disparo con la rotación actual
            Vector3 globalFirePointPosition = firePoint.position; // Obtener la posición global del firePoint
            projectile.Position = globalFirePointPosition;
            projectile.UpdatePosition(0);

            // Añade velocidad al proyectil
            Rigidbody2D rb = projectile.GameObject.AddComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.velocity = direction * bulletSpeed; // Dispara en la dirección del punto de disparo
            }
            // Rota el proyectil hacia la dirección de disparo
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            projectile.GameObject.transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));
            Debug.Log("Firing projectile on Position: " + globalFirePointPosition + " with Direction: " + direction + " and Speed: " + bulletSpeed);

            // Maneja el tiempo entre disparos sin usar coroutines
            fireCooldown = fireRate;
        }
    }

    void RotateSpriteToDirection(Vector2 direction)
    {
        // Calcula el ángulo en grados hacia la dirección de disparo
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // Aplica la rotación al sprite del tanque
        transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));
    }
}