using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Character : MonoBehaviour
{
    [Header("Stats")]
    public string characterName;
    public int maxHealth;
    public float health;
    public string pattern;

    [Header("Ability")]
    public Ability ability;

    [Header("References")]
    public TextMeshProUGUI patternLabel;
    public TextMeshProUGUI damageIndicator;
    public TextMeshProUGUI nameLabel;
    public TextMeshProUGUI healthLabel;
    public TextMeshProUGUI abilityDesciprtionLabel;

    [Header("Misc")]
    public float resetTime = 0.1f;      //when the player turn is over, how long to wait before resetting the color of the letters? if 0, the player never sees the color change of the last letter
    public List<StatusEffect> ActiveStatusEffects = new List<StatusEffect>();


    //private variables
    int currentPatternPos = -1;

    private void Start() {
        health = maxHealth;
        nameLabel.text = characterName;
        pattern = pattern.ToUpper();
        UpdatePatternLabel();

        abilityDesciprtionLabel.text = ability.description;
        healthLabel.text = health + "/" + maxHealth;
        GameManager.onUpdateInput += CheckPattern;
        GameManager.onEnemyTurnStart += ResetCharacter;
        GameManager.instance.charactersInFight.Add(this);
    }

    public void Damage(float damageAmount, int duration = 1, float damagePerTurn = 1) {
        damageIndicator.gameObject.SetActive(false);
        health = Mathf.Max(0, health - damageAmount);
        health = Mathf.Min(health, maxHealth);
        healthLabel.text = (Mathf.Round(health * 10)/10) + "/" + maxHealth;
        damageIndicator.text = Mathf.Abs(Mathf.Round(damageAmount * 10) / 10).ToString();
        damageIndicator.color = damageAmount < 0 ? Color.green : Color.red;
        damageIndicator.gameObject.SetActive(true);
        
        if (health == 0) {
            Die();
        }
    }

    void Die() {
        print(characterName + " died!!!");
        GameManager.onUpdateInput -= CheckPattern;
        GameManager.onEnemyTurnStart -= ResetCharacter;
        GameManager.instance.charactersInFight.Remove(this);
        Destroy(gameObject);
    }

    private void CheckPattern() {
        string currentPattern = GameManager.instance.currentPattern.ToUpper();
        if (currentPattern.Length == 0 || currentPatternPos+1 >= pattern.Length) { return; }
        char letter = currentPattern[currentPattern.Length - 1];
        if (letter == pattern[currentPatternPos + 1]) {
            currentPatternPos += 1;
            if (currentPatternPos + 1 == pattern.Length) {
                CompletePattern();
            }
            UpdatePatternLabel();
            GameManager.instance.InputReactSound(true, currentPatternPos != 0);
        }
        else if (letter == pattern[0]) {
            currentPatternPos = 0;
            UpdatePatternLabel();
            GameManager.instance.InputReactSound(true, false);
        }
        else {
            GameManager.instance.InputReactSound(false, currentPatternPos != -1);
            currentPatternPos = -1;
            UpdatePatternLabel();
        }
    }

    void CompletePattern() {
        if (GameManager.instance.charactersInFight.Count == 0) { return; }
        var selectedEnemyTargets = new List<Enemy>();
        var selectedAllyTargets = new List<Character>();

        var possibleEnemyTargets = new List<Enemy>(GameManager.instance.enemiesInFight);
        var possibleAllyTargets = new List<Character>(GameManager.instance.charactersInFight);

        for (int i = 0; i < ability.numTargets; i++) {
            if (possibleAllyTargets.Count > 0 && ability.targetAllies) {
                int selectedIndex = Random.Range(0, possibleAllyTargets.Count);
                selectedAllyTargets.Add(possibleAllyTargets[selectedIndex]);
                possibleAllyTargets.RemoveAt(selectedIndex);
            }
            else if (i == 0 && !ability.randomTarget && !ability.targetAllies) {
                selectedEnemyTargets.Add(possibleEnemyTargets[GameManager.instance.targetEnemy]);
                possibleEnemyTargets.RemoveAt(GameManager.instance.targetEnemy);
            }
            else if (possibleEnemyTargets.Count > 0) {
                int selectedIndex = Random.Range(0, possibleEnemyTargets.Count);
                selectedEnemyTargets.Add(possibleEnemyTargets[selectedIndex]); ;
                possibleEnemyTargets.RemoveAt(selectedIndex);
            }
        }

        int numTargets = ability.targetAllies ? selectedAllyTargets.Count : selectedEnemyTargets.Count;

        for (int i = 0; i < numTargets; i++) {
            float abilityDamage = ability.damage * (GameManager.instance.comboScript.GetComboValue());
            if (ability.targetAllies) {
                selectedAllyTargets[i].Damage(abilityDamage, ability.turnDuration, ability.damagePerTurn);
                GameManager.instance.Log(characterName + (abilityDamage > 0 ? " attacks " : "heals ") + selectedAllyTargets[i].characterName + " for " + Mathf.Abs(Mathf.Round(abilityDamage * 10) / 10) + " dmg");
            }
            else {
                selectedEnemyTargets[i].Damage(abilityDamage, ability.turnDuration, ability.damagePerTurn);
                GameManager.instance.Log(characterName + (abilityDamage > 0 ? " attacks " : "heals ") + selectedEnemyTargets[i].enemyName + " for " + Mathf.Abs(Mathf.Round(abilityDamage * 10) / 10) + " dmg");
            }
        }

        GameManager.instance.comboScript.AddCombo(ability.comboValue);
        AudioManager.instance.PlayGlobal(ability.soundID, _priority: 1);
        if (gameObject != null)
            StartCoroutine("WaitThenReset");
    }

    private void ResetCharacter() {
        currentPatternPos = -1;
        UpdatePatternLabel();
    }

    IEnumerator WaitThenReset() {
        yield return new WaitForSeconds(ability.castTime);
        currentPatternPos = -1;
        UpdatePatternLabel();
        GameManager.instance.EndPlayerTurn();
    }

    void UpdatePatternLabel() {

        patternLabel.text = "";
        if (currentPatternPos >= 0)
            patternLabel.text = "<color=red>";
        for (int i = 0; i < pattern.Length; i++) {
            patternLabel.text += pattern[i].ToString().ToUpper();
            if (i < pattern.Length - 1) {
                patternLabel.text += ", ";
            }
            if (i == currentPatternPos) {
                patternLabel.text += "</color>";
            }
        }
    }
}
