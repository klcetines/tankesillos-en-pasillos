using UnityEngine;
using Unity.Networking.Transport;
using UnityEngine.UI;
using System;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

namespace Unity.Networking.Transport.Samples
{
    public class ClientBehaviour : MonoBehaviour
    {
        NetworkDriver m_Driver;
        NetworkConnection m_Connection;
        NetworkPipeline myPipeline;
        public TMP_InputField IPServer;
        public TMP_InputField portServer;
        private bool connecting;
        private bool connected;
        private List<int> availableCharacters = new List<int> {};
        private Dictionary<int, int> playersCharacter = new Dictionary<int, int>();
        public int seed;
        
        public GameManager gameManager;

        void Start()
        {
            connecting = false;
            connected = false;
            DontDestroyOnLoad(this);
        }

        void OnDestroy()
        {
            if (m_Driver.IsCreated)
            {
                m_Driver.Dispose();
            }
        }

        void Update()
        {
            if (connecting || connected)
            {
                m_Driver.ScheduleUpdate().Complete();
                if (!m_Connection.IsCreated)
                {
                    Debug.Log("Lost connection to the server");
                    connected = false;
                    connecting = false;
                    return;
                }

                Unity.Collections.DataStreamReader stream;
                NetworkEvent.Type cmd;
                
                while ((cmd = m_Connection.PopEvent(m_Driver, out stream, out var recievingPipeline)) != NetworkEvent.Type.Empty)
                {
                    if (cmd == NetworkEvent.Type.Connect)
                    {
                        Debug.Log("We are now connected to the server.");
                        connected = true;
                        SceneManager.LoadScene("menuJoc");
                    }
                    else if (cmd == NetworkEvent.Type.Data)
                    {
                        ProcessData(stream);
                    }
                    else if (cmd == NetworkEvent.Type.Disconnect)
                    {
                        Debug.Log("Client got disconnected from server.");
                        m_Connection = default;
                        connected = false;
                        connecting = false;
                    }
                }
            }
        }

        void ProcessData(Unity.Collections.DataStreamReader stream)
        {
            char messageType = (char)stream.ReadByte();
            Debug.Log($"Message type: {messageType}");
            if (messageType == 'A')
            {
                availableCharacters.Clear();
                int availableCount = stream.ReadInt();
                Debug.Log($"Available characters count: {availableCount}");
                for (int i = 0; i < availableCount; i++)
                {
                    int characterID = stream.ReadInt();
                    processAviableCharacters(characterID);
                    Debug.Log($"Character ID: {characterID}");
                }
            }
            if (messageType == 'S'){
                int selectedCount = stream.ReadInt();
                Debug.Log($"Selected characters count: {selectedCount}");
                for (int i = 0; i < selectedCount; i++)
                {
                    int clientID = stream.ReadInt();
                    int characterID = stream.ReadInt();
                    Debug.Log($"Client ID: {clientID}, Selected Character: {characterID}");
                    playersCharacter[clientID] = characterID; 
                }
            }
            if(messageType == 'T'){
                Debug.Log("The Game will start in 3 seconds.");
            }
            if(messageType == 'P'){
                Debug.Log("Stoping the game start.");
            }
            if(messageType == 'G'){
                seed = stream.ReadInt();
                Debug.Log($"Seed: {seed}");
                SceneManager.LoadScene("game");
            }
            if(messageType == 'Y'){
                int clientID = stream.ReadInt();
                int tankID = stream.ReadInt();
                float posX = stream.ReadFloat();
                float posY = stream.ReadFloat();
                gameManager.SetYourID(clientID);
                gameManager.InstantiateTank(clientID, tankID, posX, posY);
               
            }
            if(messageType == 'I'){
                int clientID = stream.ReadInt();
                int tankID = stream.ReadInt();
                float posX = stream.ReadFloat();
                float posY = stream.ReadFloat();
                gameManager.InstantiateTank(clientID, tankID, posX, posY);
               
            }
            if(messageType == 'L'){
                int clientID = stream.ReadInt();
                float posX = stream.ReadFloat();
                float posY = stream.ReadFloat();
                gameManager.setPosition(clientID, posX, posY);
            }
            if(messageType == 'Q'){
                int clientID = stream.ReadInt();
                //gameManager.DestroyTank(clientID);
            }
            if(messageType == 'F'){
                float fireX = stream.ReadFloat();
                float fireY = stream.ReadFloat();
                int direction = stream.ReadInt();
                int projectileID = stream.ReadInt();
                float speed = stream.ReadFloat();
                Debug.Log("Fire received in position " + fireX + ", " + fireY + "direction: " + direction);
                gameManager.Fire(new Vector2(fireX, fireY), direction, projectileID, speed);
            }
            if(messageType == 'O'){
                int projectileID = stream.ReadInt();
                float fireX = stream.ReadFloat();
                float fireY = stream.ReadFloat();
                int direction = stream.ReadInt();
                float speed = stream.ReadFloat();
                gameManager.Fire(new Vector2(fireX, fireY), direction, projectileID, speed);
            }
            if (messageType == 'D')
            {
                int idProjectile = stream.ReadInt();
                gameManager.DestroyProjectile(idProjectile);
            }
            if (messageType == 'N')
            {
                int projectileID = stream.ReadInt();
                float posX = stream.ReadFloat();
                float posY = stream.ReadFloat();
                gameManager.UpdateProjectilePosition(projectileID, new Vector2(posX, posY));
            }
        }

        private int checkConnection()
        {
            if (!connected || !m_Connection.IsCreated)
            {
                Debug.LogError("Not connected to the server.");
                return -1;
            }

            if (!m_Driver.IsCreated || !m_Connection.IsCreated || myPipeline == default)
            {
                Debug.LogError("Network components are not initialized.");
                return -1;
            }

            return 0;
        }

        public void ConnectToServer()
        {
            Debug.Log("Attempting to connect to server...");
            string IP = IPServer.text;
            ushort port;

            if (!ushort.TryParse(portServer.text, out port))
            {
                Debug.LogError("Invalid port number.");
                return;
            }

            try
            {
                m_Driver = NetworkDriver.Create();
                Debug.Log("NetworkDriver created successfully.");
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to create NetworkDriver: {e.Message}");
                return;
            }

            var endpoint = NetworkEndpoint.Parse(IP, port);
            myPipeline = m_Driver.CreatePipeline(typeof(FragmentationPipelineStage), typeof(ReliableSequencedPipelineStage));
            m_Connection = m_Driver.Connect(endpoint);

            if (m_Connection.IsCreated)
            {
                connecting = true;
                Debug.Log($"Connecting to: {endpoint.ToString()}");
            }
            else
            {
                Debug.LogError("Failed to create connection.");
            }
        }

        public void SendCharacterSelected(int idCharacter)
        {
            if (checkConnection() != 0)
            {
                return;
            }
            
            var result = m_Driver.BeginSend(myPipeline, m_Connection, out var writer);
            if (result != 0) // 0 significa éxito
            {
                Debug.LogError($"BeginSend failed with error code: {result}");
                return;
            }

            try
            {
                ushort id = Convert.ToUInt16(idCharacter);

                writer.WriteByte((byte)'C'); // Ensure 'writer' is valid before writing
                writer.WriteUInt(id);

                result = m_Driver.EndSend(writer); // Always attempt to end the send operation
                if (result == 0)
                {
                    Debug.Log("Message sent successfully.");
                }
                else
                {
                    Debug.LogError($"EndSend failed with error code: {result}");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Exception during send: {ex.Message}");
            }
        }

        private void processAviableCharacters(int idCharacter)
        {
            if (!availableCharacters.Contains(idCharacter))
            {
                availableCharacters.Add(idCharacter);
            }
        }

        public void SendCheckActives()
        {
            if (checkConnection() != 0)
            {
                Debug.LogError("Cannot send message: Connection is not valid.");
                return;
            }

            var result = m_Driver.BeginSend(myPipeline, m_Connection, out var writer);
            if (result != 0)
            {
                Debug.LogError($"BeginSend failed with error code: {result}");
                return;
            }

            try
            {
                writer.WriteByte((byte)'B');
                result = m_Driver.EndSend(writer);
                if (result != 0)
                {
                    Debug.LogError($"EndSend failed with error code: {result}");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Exception during send: {ex.Message}");
                m_Driver.AbortSend(writer); // Asegúrate de abortar el envío en caso de excepción
            }
        }

        public int[] GetAvailables()
        {
            return availableCharacters.ToArray();
        }

        public Dictionary<int, int> GetPlayersCharacter()
        {
            return playersCharacter;
        }

        public void sendSceneLoaded()
        {
            if (checkConnection() != 0)
            {
                return;
            }
            var result = m_Driver.BeginSend(myPipeline, m_Connection, out var writer);
            if (result != 0) // 0 significa éxito
            {
                Debug.LogError($"BeginSend failed with error code: {result}");
                return;
            }

            try
            {
                writer.WriteByte((byte)'L'); // Ensure 'writer' is valid before writing

                result = m_Driver.EndSend(writer); // Always attempt to end the send operation
                if (result == 0)
                {
                    Debug.Log("Message sent successfully.");
                }
                else
                {
                    Debug.LogError($"EndSend failed with error code: {result}");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Exception during send: {ex.Message}");
            }
        }

        public bool getConnected()
        {
            return connected;
        } 

        public void SendReady(bool ready){
            if (checkConnection() != 0)
            {
                return;
            }
            
            var result = m_Driver.BeginSend(myPipeline, m_Connection, out var writer);
            if (result != 0) // 0 significa éxito
            {
                Debug.LogError($"BeginSend failed with error code: {result}");
                return;
            }

            try
            {
                int readymsg = ready ? 1 : 0;
                writer.WriteByte((byte)'R'); // Ensure 'writer' is valid before writing
                writer.WriteInt(readymsg);

                result = m_Driver.EndSend(writer); // Always attempt to end the send operation
                if (result == 0)
                {
                    Debug.Log("Message sent successfully.");
                }
                else
                {
                    Debug.LogError($"EndSend failed with error code: {result}");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Exception during send: {ex.Message}");
            }
        } 

        public void SendFireIntention(int tankID, int fireDirection){
            if (checkConnection() != 0)
            {
                return;
            }
            
            var result = m_Driver.BeginSend(myPipeline, m_Connection, out var writer);
            if (result != 0) // 0 significa éxito
            {
                Debug.LogError($"BeginSend failed with error code: {result}");
                return;
            }

            try
            {
                writer.WriteByte((byte)'F'); // Ensure 'writer' is valid before writing
                writer.WriteInt(fireDirection); //0 UP, 1 RIGHT, 2 DOWN, 3 LEFT

                result = m_Driver.EndSend(writer); // Always attempt to end the send operation
                Debug.Log("Fire intention sent, direction:" + fireDirection);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Exception during send: {ex.Message}");
            }
        }

        public void sendMoveIntention(Vector2 moveDirection){
            if (checkConnection() != 0)
            {
                return;
            }
            
            var result = m_Driver.BeginSend(myPipeline, m_Connection, out var writer);
            if (result != 0) // 0 significa éxito
            {
                Debug.LogError($"BeginSend failed with error code: {result}");
                return;
            }

            try
            {
                writer.WriteByte((byte)'M'); // Ensure 'writer' is valid before writing
                writer.WriteFloat(moveDirection.x);
                writer.WriteFloat(moveDirection.y);

                result = m_Driver.EndSend(writer); // Always attempt to end the send operation
            }
            catch (Exception ex)
            {
                Debug.LogError($"Exception during send: {ex.Message}");
            }
        }
    }
}