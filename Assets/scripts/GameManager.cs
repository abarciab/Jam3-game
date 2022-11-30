using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using UnityEngine.SceneManagement;

//this class is a flexible way to create diffent kinds of abilities in the game.
[System.Serializable]
public class Ability
{
    public string abilityName = "";
    public int numTargets = 1;
    public bool targetAllies;
    public int turnDuration = 1;
    public float damagePerTurn = 1;
    public float abilityChance = 1;
    public bool randomTarget;       //if randomTarget is false, but numtargets is >1, any additional targets are selected randomly
    [Tooltip("negative damage values heal targets")]
    public float damage = 1;        
    [Tooltip("when this ability is used, what percent is the combo meter increased by (ignored for enemy attacks)")]
    public float comboValue = 0.15f;
    public float castTime = 0.1f;

    [TextArea(2,5)]
    public string description;
    public int soundID;
}

//hasn't been implemented yet, but I was thinking that this class can help enemy.cs and character.cs track status effects from abilities, positive or negative
[System.Serializable]
public class StatusEffect
{
    public int turnsLeft;
    public float damagePerTurn;
}

//this GameManager script is pretty broad, and you might want to split it up into a couple differnt managers if it gets too messy, but it works for now. it's also the only singleton in the project, which is kinda nice
public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    //these are System.Actions, and they're one of the ways unity can do events + listeners. other scripts can subscribe to them, then they can be called with action.invoke()
    public static Action onUpdateInput;
    public static Action onEnemyTurnStart;
    public static Action onPlayerTurnStart;

    [Header("combatants")]
    public List<Character> charactersInFight = new List<Character>();
    public List<Enemy> enemiesInFight = new List<Enemy>();
    public List<Enemy> enemyActionQueue = new List<Enemy>();

    [Header("References")]
    public TextMeshProUGUI turnLabel;
    public TextMeshProUGUI LogText;
    [HideInInspector]
    public ComboGagueCoordinator comboScript;   //this script assigns itself, so I hid it in the inspector
    public GameObject targetIcon;
    public GameObject winScreen;
    public GameObject loseScreen;


    [Header("misc")]
    public int turnsPassed;
    public int targetEnemy;
    public Color normalPatternColor;        //not currently implemented
    public Color typedPatternColor;         //not currently implemented
    public int successSoundID;
    public int failureSoundID;
    public string currentPattern;
    public bool playerTurn;

    //see InputReactSound for more details about these two
    bool playSuccessSound;
    bool playSound;

    //setup singleton
    private void Awake() {
        instance = this;
    }

    void Start(){
        List<EnemyStats> currentEnemies = BattleManager.instance.currentEnemies;
        Enemy newEnemy = new Enemy();
        for(int i = 0; i < currentEnemies.Count; i++){
            newEnemy.enemyName = currentEnemies[i].enemyName;
            newEnemy.portrait = currentEnemies[i].portrait;
            newEnemy.maxHealth = currentEnemies[i].maxHealth;
            newEnemy.attackDamage = currentEnemies[i].attackDamage;
            newEnemy.attackTime = currentEnemies[i].attackTime;
            newEnemy.usableAbilities = currentEnemies[i].usableAbilities;
            newEnemy.isSpeaker = currentEnemies[i].isSpeaker;
            newEnemy.dialogueLines = currentEnemies[i].dialogueLines;
            //enemyChoice.Add(newEnemy);
            //AddEnemyToFight(newEnemy);
        }
        //Test();
    }

    void Update()
    {
        //first, check win/loss. this would be better to check only when a player or enemy dies
        if (charactersInFight.Count == 0 && turnsPassed > 0) {
            if (!loseScreen.activeInHierarchy)
                loseScreen.SetActive(true);
            return;
        }
        if (enemiesInFight.Count == 0 && turnsPassed > 0) {
            if (!winScreen.activeInHierarchy)
                winScreen.SetActive(true);
            return;
        }

        //the player can use the number keys to select an enemy to target (only relevant for abilities that have 'randomTarget' set to false. also the targetIcon doesn't reset when an enemy dies, so it needs to be updated
        if (enemiesInFight.Count > 0) {
            int number;
            if (int.TryParse(Input.inputString, out number)) {
                if (number <= enemiesInFight.Count && number != 0) {
                    targetEnemy = number - 1;
                    targetIcon.transform.position = new Vector3(enemiesInFight[targetEnemy].transform.position.x, targetIcon.transform.position.y, targetIcon.transform.position.z);
                }
                return;
            }
        }

        //check if a key was pressed and broadcast which it was
        if (playerTurn) {
            if (!string.IsNullOrEmpty(Input.inputString)) {
                currentPattern += Input.inputString;
                playSuccessSound = false;

                onUpdateInput?.Invoke();
                if (playSound)
                    AudioManager.instance.PlayGlobal(playSuccessSound ? successSoundID : failureSoundID, _priority: 0);
            }
        }
    }

    //there's probably a better way todo this, but this script ensures that enemies are sorted by xPosition in the list, so that selecting them makes more sense visually (i.e. pressing '1' selects the leftmost one, then '2' selects the enemy to the right)
    public void AddEnemyToFight(Enemy enemy) {
        int correctPosition = 0;
        for (int i = 0; i < enemiesInFight.Count; i++) {
            //print(enemiesInFight[i]);
            if (enemiesInFight[i].transform.position.x < enemy.transform.position.x) {
                correctPosition += 1;
            }
        }
        Enemy displacedEnemy = enemy;
        for (int i = -1; i < enemiesInFight.Count; i++) {
            if (i == correctPosition) {
                displacedEnemy = enemiesInFight[i];
                enemiesInFight.Insert(correctPosition, enemy);
            }
            else if (i > correctPosition) {
                displacedEnemy = enemiesInFight[i];
                enemiesInFight.Insert(correctPosition, displacedEnemy);
            }
        }
        enemiesInFight.Add(displacedEnemy);
    }

    public void Test(){
        print(BattleManager.instance.currentEnemies);
    }

    //called from enemy.cs. this goes through all enemies sequentially and calls 'startAttack' on them
    public void CompleteAttack(Enemy _enemy) {
        enemyActionQueue.Remove(_enemy);
        
        if (enemyActionQueue.Count > 0) {
            enemyActionQueue[0].StartAttack();
        }
        else {
            StartPlayerTurn();
        }
    }

    //resets the current pattern, increments turn counter, and makes the first enemy attack. 
    public void EndPlayerTurn() {
        turnsPassed += 1;
        currentPattern = "";
        playerTurn = false;
        turnLabel.text = "Enemy turn";
        
        onEnemyTurnStart?.Invoke();

        if (enemyActionQueue.Count > 0) {
            enemyActionQueue[0].StartAttack();
        }
    }

    //this function makes sure that the right sound is played when a key is pressed. it should play a good sound when the right key is pressed, and only play a bad sound if there are no patterns that the key matches
    public void InputReactSound(bool sucsess, bool started) {
        if (sucsess) {
            playSound = true;
            playSuccessSound = true;
        }
        if (!sucsess && !started && !playSuccessSound) {
            playSound = true;
            playSuccessSound = false;
        }
    }

    //...starts the player turn...
    public void StartPlayerTurn() {
        currentPattern = "";
        playerTurn = true;
        turnLabel.text = "Your turn";

        onPlayerTurnStart?.Invoke();
    }

    //mostly here for testing, this function logs strings to the in-game display
    public void Log(string newLine) {
        LogText.text += "\n" + newLine;
    }

    //called from the win and lose screens, this function is required to null out the events. otherwise, they would try to call functions attached to destroyed gameObjects.
    public void ResetScene() {
        onUpdateInput = null;
        onEnemyTurnStart = null;
        onPlayerTurnStart = null;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
