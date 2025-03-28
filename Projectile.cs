using UnityEngine;

namespace Projectiles
{
    public class Projectile
    {
        public int ID { get; private set; }
        public Vector2 Position { get; set; }
        public int Direction { get; private set; }
        public float Speed { get; private set; }
        public GameObject GameObject { get; private set; }

        public Projectile(int id, Vector2 position, int direction, float speed, GameObject prefab)
        {
            ID = id;
            Position = position;
            Direction = direction;
            Speed = speed;
            GameObject = Object.Instantiate(prefab, position, Quaternion.identity);
        }

        public virtual void UpdatePosition(float deltaTime)
        {
            switch (Direction)
            {
                case 0: // UP
                    Position += new Vector2(0, Speed * deltaTime);
                    break;
                case 1: // RIGHT
                    Position += new Vector2(Speed * deltaTime, 0);
                    break;
                case 2: // DOWN
                    Position += new Vector2(0, -Speed * deltaTime);
                    break;
                case 3: // LEFT
                    Position += new Vector2(-Speed * deltaTime, 0);
                    break;
                default:
                    throw new System.ArgumentOutOfRangeException("Invalid direction value");
            }
            GameObject.transform.position = new Vector3(Position.x, Position.y, 0);
        }

        public void Destroy()
        {
            Object.Destroy(GameObject);
        }
    }
}