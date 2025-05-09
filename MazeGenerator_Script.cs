using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Projectiles;

public class MazeGenerator : MonoBehaviour
{
    // Configuración del laberinto
    public GameObject wallPrefab;
    public int rows = 50;
    public int cols = 50;
    public float cellSize = 1.0f;
    
    // Estado del laberinto
    private int seed;
    private int[,] maze;
    
    // Jugadores y proyectiles
    public Tanke_Script myTank;
    public Dictionary<int, GameObject> tankesillos = new Dictionary<int, GameObject>();
    public List<Projectile> activeProjectiles = new List<Projectile>();
    private int projectileCounter = 0;

    void Update() { }

    public void Initialize(int seed)
    {
        this.seed = seed;
        maze = InitializeMaze();
        GenerateMazeFromCenter(seed);
        CreateCentralAndSpawnAreas();
        RenderMaze();
    }

    int[,] InitializeMaze()
    {
        int[,] maze = new int[rows, cols];
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            maze[i, j] = (i == 0 || i == rows - 1 || j == 0 || j == cols - 1) ? 1 : 1;
        }
        return maze;
    }

    void CreateCentralAndSpawnAreas()
    {
        // Zona central (5x5)
        int centerX = rows / 2;
        int centerY = cols / 2;
        for (int i = centerX - 2; i <= centerX + 2; i++)
            for (int j = centerY - 2; j <= centerY + 2; j++)
                maze[i, j] = 0;

        // Zonas de spawn (3x3)
        for (int j = cols / 2 - 1; j <= cols / 2 + 1; j++)
        {
            maze[1, j] = 0; // Spawn arriba
            maze[rows - 2, j] = 0; // Spawn abajo
        }

        for (int i = rows / 2 - 1; i <= rows / 2 + 1; i++)
        {
            maze[i, 1] = 0; // Spawn izquierda
            maze[i, cols - 2] = 0; // Spawn derecha
        }
    }

    void GenerateMazeFromCenter(int seed)
    {
        System.Random rand = new System.Random(seed);
        Stack<Vector2Int> stack = new Stack<Vector2Int>();
        Vector2Int current = new Vector2Int(rows / 2, cols / 2);

        stack.Push(current);
        maze[current.x, current.y] = 0;

        while (stack.Count > 0)
        {
            current = stack.Pop();
            List<Vector2Int> neighbors = GetNeighbors(current);
            
            if (neighbors.Count > 0)
            {
                stack.Push(current);
                Vector2Int next = neighbors[rand.Next(neighbors.Count)];

                maze[(current.x + next.x) / 2, (current.y + next.y) / 2] = 0;
                maze[next.x, next.y] = 0;

                stack.Push(next);
            }
        }
    }

    List<Vector2Int> GetNeighbors(Vector2Int cell)
    {
        List<Vector2Int> neighbors = new List<Vector2Int>();
        Vector2Int[] directions = {
            new Vector2Int(-2, 0), // Arriba
            new Vector2Int(2, 0),  // Abajo
            new Vector2Int(0, -2), // Izquierda
            new Vector2Int(0, 2)   // Derecha
        };

        foreach (var dir in directions)
        {
            int nx = cell.x + dir.x;
            int ny = cell.y + dir.y;

            if (nx > 0 && nx < rows - 1 && ny > 0 && ny < cols - 1 && maze[nx, ny] == 1)
                neighbors.Add(new Vector2Int(nx, ny));
        }

        return neighbors;
    }

    void RenderMaze()
    {
        for (int i = 0; i < rows; i++)
            for (int j = 0; j < cols; j++)
                if (maze[i, j] == 1)
                    Instantiate(wallPrefab, new Vector3(j * cellSize, i * cellSize, 0), Quaternion.identity, transform);
    }
    
    // Métodos de jugadores
    public void SetSeed(int seed) => this.seed = seed;

    public void setPosition(int clientID, float x, float y) => 
        tankesillos[clientID].transform.position = new Vector3(x, y, 0);

    public GameObject addPlayer(int id, GameObject tank, float x, float y)
    {
        tankesillos.Add(id, tank);
        GameObject tanke = Instantiate(tankesillos[id], new Vector3(x, y, 0), Quaternion.identity);
        Debug.Log(tanke);
        return tanke;
    }

    public void InstantiateTank(int clientID) => 
        Instantiate(tankesillos[clientID], new Vector3(0, 0, 0), Quaternion.identity);

    // Métodos de disparo
    public void Fire(int characterID, int clientID, Vector2 position, int direction, int projectileID, float speed)
    {
        Debug.Log($"Firing projectile in position {position} with direction {direction} and speed {speed}");
        
        Projectile projectile;
        GameObject projectileObject = new GameObject("Projectile");
        
        projectile = (characterID < 2) 
            ? new Bullet(projectileID, position, direction, 10, speed, projectileObject)
            : new Rocket(projectileID, position, direction, 20, speed, projectileObject);
            
        projectileCounter++;
        activeProjectiles.Add(projectile);
        tankesillos[clientID].GetComponent<Tanke_Script>().Fire(projectile);
    }

    public Projectile ShootProjectile(Vector2 position, int direction, int playerID)
    {
        if (tankesillos.ContainsKey(playerID))
        {
            Tanke_Script tank = tankesillos[playerID].GetComponent<Tanke_Script>();
            Debug.Log($"Player {playerID} is able to shoot {tank.IsAbleToShoot()}");
            
            if (tank.IsAbleToShoot())
            {
                int projectileID = projectileCounter++;
                Projectile projectile = new Projectile(
                    projectileID, 
                    position, 
                    direction, 
                    20, 
                    tank.bulletPrefab
                );

                activeProjectiles.Add(projectile);
                tank.Fire(projectile);
                return projectile;
            }
        }
        return null;
    }
}
