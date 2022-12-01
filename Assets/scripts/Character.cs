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
    public float health;        //health is set to maxHealth at the start of play
    public string pattern;

    [Header("Ability")]
    public Ability ability;

    [Header("References")]
    public TextMeshProUGUI patternLabel;
    public TextMeshProUGUI damageIndicator;
    public TextMeshProUGUI nameLabel;
    public TextMeshProUGUI healthLabel;
    public Slider healthSlider;
    public TextMeshProUGUI abilityDesciprtionLabel;

    [Header("Misc")]
   public List<StatusEffect> ActiveStatusEffects = new List<StatusEffect>(); //hasn't been implemented yet, but these should be created when duration > 1 in damage function, and triggered onPlayerTurnStart

    //private variables
    int currentPatternPos = -1;

    //basic setup for some UI labels and adding listeners to events
    private void Start() {
        health = maxHealth;
        nameLabel.text = characterName;
        pattern = pattern.ToUpper();
        UpdatePatternLabel();

        abilityDesciprtionLabel.text = ability.description;
        healthLabel.text = health + "/" + maxHealth;
        healthSlider.value = maxHealth/health;
        GameManager.onUpdateInput += CheckPattern;
        GameManager.onEnemyTurnStart += ResetCharacter;
        GameManager.instance.charactersInFight.Add(this);
    }

    
    //this is called every time a letter is typed, and if typed letter is the next one in this chararcter's pattern, update the graphic. otherwise, reset progress on the pattern
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
            GameManager.instance.InputReactSound(true, currentPatternPos != 0); //check out GameManager.cs to learn about this function
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

    //called when the player has typed the complete pattern for this character. This function is responsible for activiating the character's ability and ending the player turn
    void CompletePattern() {
        if (GameManager.instance.charactersInFight.Count == 0 || GameManager.instance.enemiesInFight.Count == 0) { return; }

        var selectedEnemyTargets = new List<Enemy>();
        var selectedAllyTargets = new List<Character>();

        var possibleEnemyTargets = new List<Enemy>(GameManager.instance.enemiesInFight);
        var possibleAllyTargets = new List<Character>(GameManager.instance.charactersInFight);

        for (int i = 0; i < ability.numTargets; i++) {      //first we select the targets we're interested in, based on the settings of the ability. because we might be targeting enemies or allies, we have to check things twice
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

        GameManager.instance.clearLog();
        int numTargets = ability.targetAllies ? selectedAllyTargets.Count : selectedEnemyTargets.Count;
        for (int i = 0; i < numTargets; i++) {                      //then we actually damage/heal the targets
            float abilityDamage = ability.damage * (GameManager.instance.comboScript.GetComboValue());
            if (ability.targetAllies) {
                selectedAllyTargets[i].Damage(abilityDamage, ability.turnDuration, ability.damagePerTurn);
                GameManager.instance.Log(characterName + (abilityDamage > 0 ? " attacks " : "heals ") + selectedAllyTargets[i].characterName + " for " + Mathf.Abs(Mathf.Round(abilityDamage * 10) / 10) + " dmg");    //this is what shows up in the little on-screen log
            }
            else {
                selectedEnemyTargets[i].Damage(abilityDamage, ability.turnDuration, ability.damagePerTurn);
                GameManager.instance.Log(characterName + (abilityDamage > 0 ? " attacks " : "heals ") + selectedEnemyTargets[i].enemyName + " for " + Mathf.Abs(Mathf.Round(abilityDamage * 10) / 10) + " dmg");        //so is this
            }
        }
        
        //lastly, we update the combo meter, play the right sounds, and call waitThenReset(which ends the player turn)
        GameManager.instance.comboScript.AddCombo(ability.comboValue);
        AudioManager.instance.PlayGlobal(ability.soundID, _priority: 1);
        if (gameObject != null)
            StartCoroutine("WaitThenReset");
    }

    //this is called on every character very turn that didn't use their ability, resetting them instantly
    private void ResetCharacter() {
        currentPatternPos = -1;
        UpdatePatternLabel();
    }

    //this waits for a moment and then resets the character, giving some time to see the impact of the action. it should probably eventually be replaced by an animation and animation event
    IEnumerator WaitThenReset() {
        yield return new WaitForSeconds(ability.castTime);
        currentPatternPos = -1;
        UpdatePatternLabel();
        GameManager.instance.EndPlayerTurn();
    }

    //this script is responsible for coloring the patternText appropriatly. eventually, it might be nice to have the patterns shown with sprites or icons, which would require this to change
    void UpdatePatternLabel() {

        patternLabel.text = "[ ";
        if (currentPatternPos >= 0)
            patternLabel.text = "<color=red>";
        for (int i = 0; i < pattern.Length; i++) {
            patternLabel.text += pattern[i].ToString().ToUpper();
            if (i < pattern.Length - 1) {
                patternLabel.text += " ";
            }
            if (i == currentPatternPos) {
                patternLabel.text += "</color>";
            }
        }
        patternLabel.text += " ]";
    }

    //this function is called when someone damages (or heals) this character. almost identical to the Damage() function in Enemy.cs
    public void Damage(float damageAmount, int duration = 1, float damagePerTurn = 1)
    {
        damageIndicator.gameObject.SetActive(false);
        health = health - damageAmount;
        healthLabel.text = (Mathf.Round(health * 10) / 10) + "/" + maxHealth;
        healthSlider.value = maxHealth / health;
        damageIndicator.text = Mathf.Abs(Mathf.Round(damageAmount * 10) / 10).ToString();
        damageIndicator.color = damageAmount < 0 ? Color.green : Color.red;
        damageIndicator.gameObject.SetActive(true);

        if (health == 0) {
            Die();
        }
    }

    //kills this character and unsubscribes from events
    void Die()
    {
        print(characterName + " died!!!");
        GameManager.onUpdateInput -= CheckPattern;
        GameManager.onEnemyTurnStart -= ResetCharacter;
        GameManager.instance.charactersInFight.Remove(this);
        Destroy(gameObject);
    }
}
