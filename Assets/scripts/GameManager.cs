using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using UnityEngine.SceneManagement;

[System.Serializable]
public class Ability
{
    public int numTargets = 1;
    public bool targetAllies;
    public int turnDuration = 1;
    public float damagePerTurn = 1;
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

[System.Serializable]
public class StatusEffect
{
    public int turnsLeft;
    public float damagePerTurn;
}

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

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
    public ComboGagueCoordinator comboScript;
    public GameObject targetIcon;
    public GameObject winScreen;
    public GameObject loseScreen;

    [Header("misc")]
    public int turnsPassed;
    public int targetEnemy;
    public Color normalPatternColor;
    public Color typedPatternColor;
    public int successSoundID;
    public int failureSoundID;
    bool playSuccessSound;
    bool playSound;

    public string currentPattern;
    public bool playerTurn;

    //public float testEnemyTime = 1;
    //private float testEnemyCounter;

    private void Awake() {
        instance = this;
    }

    void Update()
    {
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

    public void AddEnemyToFight(Enemy enemy) {
        int correctPosition = 0;
        for (int i = 0; i < enemiesInFight.Count; i++) {
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

    public void CompleteAttack(Enemy _enemy) {
        enemyActionQueue.Remove(_enemy);
        
        if (enemyActionQueue.Count > 0) {
            enemyActionQueue[0].StartAttack();
        }
        else {
            StartPlayerTurn();
        }
    }

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

    public void StartPlayerTurn() {
        currentPattern = "";
        playerTurn = true;
        turnLabel.text = "Your turn";

        onPlayerTurnStart?.Invoke();
    }

    public void Log(string newLine) {
        LogText.text += "\n" + newLine;
    }

    public void ResetScene() {
        onUpdateInput = null;
        onEnemyTurnStart = null;
        onPlayerTurnStart = null;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
