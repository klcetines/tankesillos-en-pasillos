using UnityEngine;

namespace Projectiles
{
    public class Rocket : Projectile
    {
        public float Damage { get; private set; }

        public Rocket(int id, Vector2 position, int direction, float speed, float damage, GameObject gameObject)
            : base(id, position, direction, speed, gameObject)
        {
            Damage = damage;
        }

        public override void UpdatePosition(float deltaTime)
        {
            base.UpdatePosition(deltaTime);
            // Aquí puedes agregar lógica específica para Rocket si es necesario
        }
    }
}