using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Networking.Transport.Samples;
using UnityEngine.UI;

public class characterHandler : MonoBehaviour
{
    private ClientBehaviour clientBehaviour;
    public Button[] characters;
    private bool ready;
    public Image player1;
    public Image player2;
    public Image player3;
    public Image player4;

    private Dictionary<int, Sprite> characterSprites = new Dictionary<int, Sprite>();


    // Start is called before the first frame update
    void Start()
    {
        GameObject connection = GameObject.Find("ClientBehaviour");
        ready = false;

        if (connection != null)
        {
            clientBehaviour = connection.GetComponent<ClientBehaviour>();
            if (clientBehaviour != null)
            {
                Debug.Log("ClientBehaviour component found.");
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

        initializeSprites();

        initializeCharacters();

        if (characters != null)
        {
            for (int i = 0; i < characters.Length; i++)
            {
                if (characters[i] is Button)
                {
                    characters[i].interactable = false;
                }
                else
                {
                    Debug.LogError($"Character {i} is not of type Button.");
                }
            }
        }
        
        if(clientBehaviour.getConnected())
        {
            clientBehaviour.SendCheckActives();
            Debug.Log("Demanat llistat de personatges disponibles");
        }
        else{
            Debug.Log("Not connected yet.");
        }

    }

    void Update(){
        markAvailables();
        updateImages();
    }
    
    public void selectCharacter(int character)
    {
        if (clientBehaviour != null)
        {
            clientBehaviour.SendCharacterSelected(character);
        }
        else
        {
            Debug.LogError("ClientBehaviour component is not assigned.");
        }
    }

    public void markAvailables()
    {
        if (!ready){
            int[] availables = clientBehaviour.GetAvailables();
            for (int i = 0; i < characters.Length; i++)
            {
                if (characters[i] == null)
                {
                    Debug.LogError($"Character button {i} is null.");
                    continue;
                }

                // Check if the current character index (i + 1) is in the availables array
                bool isAvailable = System.Array.Exists(availables, available => available == (i));
                characters[i].interactable = isAvailable;
            } 
        }
        else{
            for (int i = 0; i < characters.Length; i++)
            {
                characters[i].interactable = false;
            }
        }
    }

    private void updateImages(){
        player1.sprite = null;
        player2.sprite = null;
        player3.sprite = null;
        player4.sprite = null;

        // Get the selected characters from the client behaviour
        var selecteds = clientBehaviour.GetPlayersCharacter();

        // Update the images based on the selected characters
        foreach (var entry in selecteds)
        {
            int clientID = entry.Key;
            int characterID = entry.Value;

            // Get the sprite for the selected character ID
            if (characterSprites.TryGetValue(characterID, out Sprite sprite))
            {
                // Assign the sprite to the corresponding Image component
                switch (clientID)
                {
                    case 0:
                        player1.sprite = sprite;
                        break;
                    case 1:
                        player2.sprite = sprite;
                        break;
                    case 2:
                        player3.sprite = sprite;
                        break;
                    case 3:
                        player4.sprite = sprite;
                        break;
                    default:
                        Debug.LogWarning($"Unknown client ID: {clientID}");
                        break;
                }
            }
            else
            {
                Debug.LogWarning($"No sprite found for character ID: {characterID}");
            }
        }
    }

    private void initializeCharacters()
    {
        characters = new Button[4];
        for (int i = 0; i < characters.Length; i++)
        {
            string buttonName = "C" + (i + 1);
            GameObject buttonObject = GameObject.Find(buttonName);
            if (buttonObject != null)
            {
                characters[i] = buttonObject.GetComponent<Button>();
                if (characters[i] != null)
                {
                    characters[i].interactable = false;
                }
            }
        }
    }

    private void initializeSprites(){
        // Load images from the Resources folder
        characterSprites[-1] = Resources.Load<Sprite>("Buttons/Pressed");   // ID -1 -> Default.png
        characterSprites[0] = Resources.Load<Sprite>("Buttons/Orange");     // ID 1 -> Orange.png
        characterSprites[1] = Resources.Load<Sprite>("Buttons/Green");      // ID 2 -> Green.png
        characterSprites[2] = Resources.Load<Sprite>("Buttons/Blue");       // ID 3 -> Blue.png
        characterSprites[3] = Resources.Load<Sprite>("Buttons/Purple");     // ID 4 -> Purple.png

        foreach (var entry in characterSprites)
        {
            if (entry.Value == null)
            {
                Debug.LogError($"Failed to load sprite for character ID: {entry.Key}");
            }
        }
    }

    public void setReady()
    {
        ready = !ready;
        clientBehaviour.SendReady(ready);
    }
}
