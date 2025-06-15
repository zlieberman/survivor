using UnityEngine;

[System.Serializable]
public class PlayerStats
{
    // Personality Traits
    [Range(0, 100)] public int perception;
    [Range(0, 100)] public int deception;
    [Range(0, 100)] public int persuasion;
    [Range(0, 100)] public int swimming;
    [Range(0, 100)] public int speed;
    [Range(0, 100)] public int puzzleSolving;
    [Range(0, 100)] public int strength;
    [Range(0, 100)] public int charisma;
    [Range(0, 100)] public int honesty;
    [Range(0, 100)] public int trust;
    [Range(0, 100)] public int honor;

    // Physical Attributes
    [Range(0, 300)] public float weight; // in pounds
    [Range(0, 96)] public float height; // in inches

    // Basic Info
    public string playerName;
    public bool isMainPlayer;
    public bool isInGame; // Whether the player is on the island or not

    public PlayerStats()
    {
        // Initialize with default values
        perception = 50;
        deception = 50;
        persuasion = 50;
        swimming = 50;
        speed = 50;
        puzzleSolving = 50;
        strength = 50;
        charisma = 50;
        honesty = 50;
        trust = 50;
        honor = 50;
        
        weight = 150;
        height = 72;
        
        isInGame = true;
    }

    public void InitializeRandomStats()
    {
        // Initialize with random values
        perception = Random.Range(30, 91);
        deception = Random.Range(30, 91);
        persuasion = Random.Range(30, 91);
        swimming = Random.Range(30, 91);
        speed = Random.Range(30, 91);
        puzzleSolving = Random.Range(30, 91);
        strength = Random.Range(30, 91);
        charisma = Random.Range(30, 91);
        honesty = Random.Range(30, 91);
        trust = Random.Range(30, 91);
        honor = Random.Range(30, 91);
        
        weight = Random.Range(100, 250);
        height = Random.Range(60, 84);
    }
} 