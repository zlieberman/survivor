using UnityEngine;
using System.Collections.Generic;
using Survivor.Characters.UI;
using System.Linq;

namespace Survivor.Characters
{
    public class PlayerManager : MonoBehaviour
    {
        public static PlayerManager Instance { get; private set; }

        private List<Character> registeredPlayers = new List<Character>();
        private Character mainPlayer;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void RegisterPlayer(Character player)
        {
            if (!registeredPlayers.Contains(player))
            {
                registeredPlayers.Add(player);
                
                // If this is the first player, set them as the main player
                if (mainPlayer == null)
                {
                    mainPlayer = player;
                }
            }
        }

        public void UnregisterPlayer(Character player)
        {
            registeredPlayers.Remove(player);
            
            // If the main player is being unregistered, update the main player reference
            if (player == mainPlayer)
            {
                mainPlayer = registeredPlayers.Count > 0 ? registeredPlayers[0] : null;
            }
        }

        public Character GetMainPlayer()
        {
            return mainPlayer;
        }

        public List<Character> GetAllPlayers()
        {
            return new List<Character>(registeredPlayers);
        }
    }
} 