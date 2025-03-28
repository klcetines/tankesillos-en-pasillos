using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Networking.Transport.Samples;
using UnityEngine.UI;
using Projectiles;

public class GameManager : MonoBehaviour
{
    private ClientBehaviour clientBehaviour;
    private int seed;
    private MazeGenerator mazeGeneratorScript;
    private int yourID;
    private int TankID;
    private Tanke_Script myTank;
    public GameObject[] tankesillos;

    // Start is called before the first frame update
    void Start()
    {
        GameObject connection = GameObject.Find("ClientBehaviour");
        GameObject mazeGenerator = GameObject.Find("mazeGenerator");

        if (connection != null)
        {
            clientBehaviour = connection.GetComponent<ClientBehaviour>();
            if (clientBehaviour != null)
            {
                Debug.Log("ClientBehaviour component found.");
                seed = clientBehaviour.seed;
                clientBehaviour.gameManager = this;
                clientBehaviour.sendSceneLoaded();
            }
            else
            {
                Debug.LogError("ClientBehaviour component not found on the connection GameObject.");
            }
        }
        else
        {
            Debug.LogError("Connection GameObject not found.");
        }

        if (mazeGenerator != null)
        {
            mazeGeneratorScript = mazeGenerator.GetComponent<MazeGenerator>();
            if (mazeGeneratorScript != null)
            {
                Debug.Log("MazeGenerator component found.");
                mazeGeneratorScript.SetSeed(seed);
                mazeGeneratorScript.Initialize(seed);
            }
            else
            {
                Debug.LogError("MazeGenerator component not found on the mazeGenerator GameObject.");
            }
        }
        else
        {
            Debug.LogError("MazeGenerator GameObject not found.");
        }

    }

    // Update is called once per frame
    void Update()
    {
        HandleMovement();
        HandleShooting();
    }

    void HandleMovement()
    {
        // Movimiento del tanque con "WASD"
        float moveX = 0f;
        float moveY = 0f;

        if (Input.GetKey(KeyCode.W))
        {
            moveY = 1f;
            Vector2 moveDirection = new Vector2(moveX, moveY).normalized;
            clientBehaviour.sendMoveIntention(moveDirection);

        }
        else if (Input.GetKey(KeyCode.S))
        {
            moveY = -1f;
            Vector2 moveDirection = new Vector2(moveX, moveY).normalized;
            clientBehaviour.sendMoveIntention(moveDirection);

        }

        if (Input.GetKey(KeyCode.A))
        {
            moveX = -1f;
            Vector2 moveDirection = new Vector2(moveX, moveY).normalized;
            clientBehaviour.sendMoveIntention(moveDirection);
        }
        else if (Input.GetKey(KeyCode.D))
        {
            moveX = 1f;
            Vector2 moveDirection = new Vector2(moveX, moveY).normalized;
            clientBehaviour.sendMoveIntention(moveDirection);
        }

        //transform.Translate(moveDirection * moveSpeed * Time.deltaTime, Space.World);
    }

    void HandleShooting()
    {
        // Dirección de disparo con las flechas
        int fireDirection = 0;
        if (Input.GetKey(KeyCode.UpArrow) && myTank.IsAbleToShoot())
        {
            fireDirection = 0;
            clientBehaviour.SendFireIntention(yourID, fireDirection);
        }
        else if (Input.GetKey(KeyCode.DownArrow) && myTank.IsAbleToShoot())
        {
            fireDirection = 2;
            clientBehaviour.SendFireIntention(yourID, fireDirection);
        }
        else if (Input.GetKey(KeyCode.LeftArrow) && myTank.IsAbleToShoot())
        {
            fireDirection = 3;
            clientBehaviour.SendFireIntention(yourID, fireDirection);
        }
        else if (Input.GetKey(KeyCode.RightArrow) && myTank.IsAbleToShoot())
        {
            fireDirection = 1;
            clientBehaviour.SendFireIntention(yourID, fireDirection);
        }
    }

    public void SetYourID(int id)
    {
        yourID = id;
    }

    public void InstantiateTank(int ClientId, int TankID, float x, float y)
    {
        if(ClientId == yourID)
        {
            this.TankID = TankID;
            GameObject tanke = mazeGeneratorScript.addPlayer(ClientId, tankesillos[TankID], x, y);
            myTank = tanke.GetComponent<Tanke_Script>();
            Debug.Log("My tank instantiated");
            Debug.Log(myTank);
        }
        else{
            mazeGeneratorScript.addPlayer(ClientId, tankesillos[TankID], x, y);
        }
    }

    public void setPosition(int clientID, float x, float y)
    {
        if(clientID == yourID){
            myTank.transform.position = new Vector3(x, y, 0);
        }
        else if(mazeGeneratorScript.tankesillos.ContainsKey(clientID)){
            Debug.Log("Setting position for client " + clientID + " to " + x + ", " + y);
            Debug.Log("Tankesillos count: " + mazeGeneratorScript.tankesillos.Count);
            mazeGeneratorScript.setPosition(clientID, x, y);
        }        
    }

    public void Fire(Vector2 position, int direction, int projectileID, float speed)
    {
        mazeGeneratorScript.Fire(TankID, yourID, position, direction, projectileID, speed);
    }

    public void DestroyProjectile(int idProjectile)
    {
        // Encuentra el proyectil en la lista de proyectiles activos y destrúyelo
        Projectile projectileToDestroy = null;
        foreach (var projectile in mazeGeneratorScript.activeProjectiles)
        {
            if (projectile.ID == idProjectile)
            {
                projectileToDestroy = projectile;
                break;
            }
        }

        if (projectileToDestroy != null)
        {
            mazeGeneratorScript.activeProjectiles.Remove(projectileToDestroy);
            projectileToDestroy.Destroy();
            Debug.Log($"Projectile {idProjectile} destroyed.");
        }
        else
        {
            Debug.LogError($"Projectile {idProjectile} not found.");
        }
    }

    public void UpdateProjectilePosition(int projectileID, Vector2 newPosition)
    {
        // Encuentra el proyectil en la lista de proyectiles activos y actualiza su posición
        Projectile projectileToUpdate = null;
        foreach (var projectile in mazeGeneratorScript.activeProjectiles)
        {
            if (projectile.ID == projectileID)
            {
                projectileToUpdate = projectile;
                break;
            }
        }

        if (projectileToUpdate != null)
        {
            projectileToUpdate.Position = newPosition;
            projectileToUpdate.UpdatePosition(0); // Actualiza la posición inmediatamente
            Debug.Log($"Projectile {projectileID} position updated to {newPosition}");
        }
        else
        {
            Debug.LogError($"Projectile {projectileID} not found.");
        }
    }
}
