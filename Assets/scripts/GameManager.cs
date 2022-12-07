using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using UnityEngine.SceneManagement;
using JetBrains.Annotations;

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

    [Header("Enemy Creation")]
    public Transform enemyContainer;
    public GameObject enemyPrefab;
    public Vector3 enemyInitialPos;
    public float enemyXSpacing;
    public float enemyYSpacing;
    public float dialogueTime;

    [Header("Player Party Creation")]
    public Transform characterContainer;
    public GameObject characterPrefab;
    public Vector3 partyInitialPos;
    public float partyXSpacing;

    [Header("combatants")]
    public List<Character> charactersInFight = new List<Character>();
    public List<Enemy> enemiesInFight = new List<Enemy>();
    public List<Enemy> enemyActionQueue = new List<Enemy>();

    [Header("References")]
    public TextMeshProUGUI turnLabel;
    public TextMeshProUGUI LogText;
    public GameObject speechBubble;
    public TextMeshProUGUI dialogueText;
    [HideInInspector]
    public ComboGagueCoordinator comboScript;   //this script assigns itself, so I hid it in the inspector
    public GameObject targetIcon;
    public GameObject winScreen;
    public GameObject loseScreen;
    public string mainMenuSceneName;


    [Header("misc")]
    public int totalScoreForThisFight;
    public float endWaitTime;
    public int turnsPassed;
    public int targetEnemy;
    Color normalPatternColor;        //not currently implemented
    Color typedPatternColor;         //not currently implemented
    public int successSoundID;
    public int failureSoundID;
    public string currentPattern;
    public bool playerTurn;
    public bool textHidden;
    public Color fireColor;
    public Color healColor;
    public Sprite fireImg;
    public Sprite heartImg;
    public bool revealHook;
    public bool revealVines;

    //see InputReactSound for more details about these two
    bool playSuccessSound;
    bool playSound;

    private bool battleOver;

    //setup singleton
    private void Awake() {
        instance = this;
    }

    private void Start(){
        totalScoreForThisFight = 0;
        AudioManager.instance.PlayMusic(8);
        battleOver = false;
        clearLog();

        // get enemies in battle from free roam and set initial position in battle scene
        List<EnemyStats> currentEnemies = BattleManager.instance.currentEnemies;
        Vector3 position = enemyInitialPos;

        // loop through enemies
        for(int i = 0; i < currentEnemies.Count; ++i) {
            // instantiate a new enemy game object
            GameObject newEnemy = Instantiate(enemyPrefab, position, transform.rotation, enemyContainer);
            
            // get stats from free roam and set new enemy's stats to them
            Enemy stats = newEnemy.GetComponent<Enemy>();
            stats.setStats(currentEnemies[i]);

            if(stats.isSpeaker)
                stats.sayLine();

            // move position to space enemies
            position = new Vector3(position.x + enemyXSpacing, position.y + enemyYSpacing, position.z);
        }

        Transform playerParty = BattleManager.instance.playerParty;
        position = partyInitialPos;
        foreach(Transform partyMember in playerParty) {
            // get stats and make a new character
            PartyMemberStats stats = partyMember.GetComponent<PartyMemberStats>();
            GameObject newCharacter = Instantiate(characterPrefab, position, transform.rotation, characterContainer);

            // add character to battle and move position to space characters
            newCharacter.GetComponent<Character>().addToBattle(stats);
            position = new Vector3(position.x + partyXSpacing, position.y, position.z);
        }
    }

    private void Update()
    {
        //first, check win/loss. this would be better to check only when a player or enemy dies
        if (charactersInFight.Count == 0 && turnsPassed > 0 && !battleOver) {
            battleOver = true;
            clearLog();
            AudioManager.instance.PlayGlobal(14, _priority: 2);
            Log("<color=red>You Lose!</color>");
            StartCoroutine(endFight(false));
            return;
        }
        if (enemiesInFight.Count == 0 && turnsPassed > 0 && !battleOver) {
            battleOver = true;
            clearLog();
            BattleManager.instance.xpSystem.GainXP(totalScoreForThisFight);
            //HealthBars.instance.IncreaseScore(totalScoreForThisFight);
            Log("<color=green>You Win!</color>");
            AudioManager.instance.PlayGlobal(15, _priority:5);
            StartCoroutine(endFight(true));
            return;
        }

        //the player can use the number keys to select an enemy to target (only relevant for abilities that have 'randomTarget' set to false. also the targetIcon doesn't reset when an enemy dies, so it needs to be updated
        if (enemiesInFight.Count > 0) {
            int number;
            if (int.TryParse(Input.inputString, out number)) {
                if (number <= enemiesInFight.Count && number != 0) {
                    setTargetIcon(number);
                }
                return;
            }
        }

        //check if a key was pressed and broadcast which it was
        if (playerTurn && !battleOver) {
            if (!string.IsNullOrEmpty(Input.inputString)) {
                currentPattern += Input.inputString;
                playSuccessSound = false;
                onUpdateInput?.Invoke();
                if (playSound)
                    AudioManager.instance.PlayGlobal(playSuccessSound ? successSoundID : failureSoundID, _priority: 0);
            }
        }
    }

    private IEnumerator endFight(bool win) {
        yield return new WaitForSeconds(endWaitTime);

        if (win) {
            BattleManager.instance.endBattleWin(revealVines, revealHook);
        }
        else if (!loseScreen.activeInHierarchy) {
            print("lose");
            loseScreen.SetActive(true);
        }
    }

    public void setTargetIcon(int targetNum) {
        targetEnemy = targetNum - 1;
        targetIcon.transform.position = new Vector3(enemiesInFight[targetEnemy].transform.position.x, targetIcon.transform.position.y, targetIcon.transform.position.z);
    }

    public bool checkForSpeakers() {
        foreach(Enemy enemy in enemiesInFight) {
            if(enemy.isSpeaker)
                return true;
        }
        return false;
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

    //called from enemy.cs. this goes through all enemies sequentially and calls 'startAttack' on them
    public void CompleteAttack(Enemy _enemy) {
        enemyActionQueue.Remove(_enemy);
        //print("removing " + _enemy.name + " from queue. remaining: " + enemyActionQueue.Count);
        
        
        if (enemyActionQueue.Count > 0 && enemyActionQueue[0] != null) {
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
        speechBubble.SetActive(false);
        onEnemyTurnStart?.Invoke();

        for (int i = 0; i < enemyActionQueue.Count; i++) {
            if (enemyActionQueue[0] != null && enemyActionQueue[0].health > 0) {
                enemyActionQueue[i].StartAttack();
                return;
            }
        }
        StartPlayerTurn();
        /*if (enemyActionQueue.Count > 0) {
            enemyActionQueue[0].StartAttack();
        }*/
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
        textHidden = false;
        currentPattern = "";
        playerTurn = true;
        turnLabel.text = "Your turn";
        AudioManager.instance.PlayGlobal(9, _priority: 1);
        onPlayerTurnStart?.Invoke();
    }

    //mostly here for testing, this function logs strings to the in-game display
    public void Log(string newLine) {
        LogText.text += "\n" + newLine;
    }

    public void DiaLog(string line)
    {
        speechBubble.SetActive(true);
        dialogueText.text = line;
    }

    public void clearLog() {
        LogText.text = "";
    }

    //called from the win and lose screens, this function is required to null out the events. otherwise, they would try to call functions attached to destroyed gameObjects.
    public void ResetScene() {
        onUpdateInput = null;
        onEnemyTurnStart = null;
        onPlayerTurnStart = null;
        //SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void resetOnLose() {
        BattleManager.instance.endBattleLose();
    }

    public void quitGame() {
        SceneManager.LoadScene(mainMenuSceneName);
    }
}
