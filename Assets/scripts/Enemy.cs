using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Enemy : MonoBehaviour
{
    [Header("stats")]
    public string enemyName;
    public Sprite portrait;
    public int maxHealth;
    public float health;
    public float attackDamage; 
    public float attackTime = 0.5f;
    public List<Ability> usableAbilities;
    public bool isSpeaker = false;
    [Tooltip("NOTE: First line is intro line said at beginning of battle")]
    public List<string> dialogueLines;
    private int currentLine;

    [Header("references")]
    public TextMeshProUGUI nameLabel;
    public Slider healthBar;
    public TextMeshProUGUI damageIndicator;

    [Header("misc")]
    public int deathSoundID = 5;
    public List<StatusEffect> activeStatusEffects = new List<StatusEffect>();   //hasn't been implemented yet, but these should be created when duration > 1 in damage function, and triggered onEnemyTurnStart


    //basic setup for some UI labels and adding listeners to events
    private void Start() {
        nameLabel.text = enemyName;
        GetComponent<SpriteRenderer>().sprite = portrait;

        GameManager.instance.AddEnemyToFight(this);
        health = maxHealth;
        healthBar.value = maxHealth / health;
        //healthBar.text = health + "/" + maxHealth;
        GameManager.onEnemyTurnStart += AddToAttackQueue;
    }

    //this is called at the start of the enemy turn, and then enemies in the queue are called sequentially, each attacking only after the previous one has finished
    void AddToAttackQueue() { 
        if (!GameManager.instance.enemyActionQueue.Contains(this)) {
            GameManager.instance.enemyActionQueue.Add(this);
        }
    }

    public void setStats(EnemyStats stats) {
        enemyName = stats.enemyName;
        portrait = stats.portrait;
        maxHealth = stats.maxHealth;
        attackDamage = stats.attackDamage;
        attackTime = stats.attackTime;
        usableAbilities = stats.usableAbilities;
        isSpeaker = stats.isSpeaker;
        dialogueLines = stats.dialogueLines;
        currentLine = 0;
    }

    public void sayLine() {
        GameManager.instance.Log("<color=red>" + enemyName + ": </color>" + dialogueLines[currentLine]);

        if(currentLine < dialogueLines.Count - 1)
            currentLine++;
    }

    //simple handler for the coroutine so that other scripts can call this instead of 'startCoroutine("ReadyAttack")'
    public void StartAttack() {
        StartCoroutine("ReadyAttack");                
    }

    //artificial waiting period so that attacks aren't instant. A more refined version of this would probably use attack animations with animation events to trigger Attack() instead
    IEnumerator ReadyAttack()
    {
        if(isSpeaker) {
            GameManager.instance.clearLog();
            sayLine();
        }
        yield return new WaitForSeconds(isSpeaker ? GameManager.instance.dialogueTime : attackTime);

        if(isSpeaker)
            GameManager.instance.clearLog();
        EnemyAction();
    }

    //Controls the type of action the enemy takes and who it is used on
    void EnemyAction(){
        if (GameManager.instance.charactersInFight.Count == 0) { return; }
        var probabilityNum = 0;
        bool abilityUsed = false;
        for(int i = 0; i < usableAbilities.Count && !abilityUsed; i++){
            probabilityNum = Random.Range(0,100);
            if(probabilityNum <= usableAbilities[i].abilityChance ){
                useAbility(usableAbilities[i]);
                
                abilityUsed = true;
            }
        }
        if(!abilityUsed){
            Attack();
        }
    }

    //simple attack, where a random character is picked and damage is dealt.
    void Attack() {
        Character target = GameManager.instance.charactersInFight[Random.Range(0, GameManager.instance.charactersInFight.Count)];
        GameManager.instance.Log(enemyName + " attacks " + target.characterName + " for " + attackDamage + " dmg");
        target.Damage(attackDamage);
        AudioManager.instance.PlayGlobal(6, _priority: 1, restart: true);   //the restart:true ensures that, if multiple enemies attack quickly, you hear part of each attack, even if it's inturrupeted. it'll be better to have an audiosource on each enemy, then there's no overlap
        GameManager.instance.CompleteAttack(this);      //this function is called to let the gameManager know that it's time for the next enemy to attack
    }

    //Handles how abilities are going to be used based on ability selected.
    void useAbility(Ability selectedAbility){
        var selectedEnemyTargets = new List<Character>();
        var selectedAllyTargets = new List<Enemy>();

        var possibleEnemyTargets = new List<Character>(GameManager.instance.charactersInFight);
        var possibleAllyTargets = new List<Enemy>(GameManager.instance.enemiesInFight);

        for (int i = 0; i < selectedAbility.numTargets; i++) {      //first we select the targets we're interested in, based on the settings of the ability. because we might be targeting enemies or allies, we have to check things twice
            if (possibleAllyTargets.Count > 0 && selectedAbility.targetAllies) {
                int selectedIndex = Random.Range(0, possibleAllyTargets.Count);
                selectedAllyTargets.Add(possibleAllyTargets[selectedIndex]);
                possibleAllyTargets.RemoveAt(selectedIndex);
            }
            else {
                int selectedIndex = Random.Range(0, possibleEnemyTargets.Count);
                selectedEnemyTargets.Add(possibleEnemyTargets[selectedIndex]);
                possibleEnemyTargets.RemoveAt(selectedIndex);
            }
        }

        int numTargets = selectedAbility.targetAllies ? selectedAllyTargets.Count : selectedEnemyTargets.Count;
        for (int i = 0; i < numTargets; i++) {                      //then we actually damage/heal the targets
            float abilityDamage = selectedAbility.damage;
            if (selectedAbility.targetAllies) {
                selectedAllyTargets[i].Damage(abilityDamage, selectedAbility.turnDuration, selectedAbility.damagePerTurn);
                GameManager.instance.Log(enemyName + " uses " + selectedAbility.abilityName + (abilityDamage > 0 ? " to attack " : "to heal ") + selectedAllyTargets[i].enemyName +  " for " + Mathf.Abs(Mathf.Round(abilityDamage * 10) / 10) + " dmg");    //this is what shows up in the little on-screen log
            }
            else {
                selectedEnemyTargets[i].Damage(abilityDamage, selectedAbility.turnDuration, selectedAbility.damagePerTurn);
                GameManager.instance.Log(enemyName + " uses " + selectedAbility.abilityName + (abilityDamage > 0 ? " to attack " : "to heal ") + selectedEnemyTargets[i].characterName +  " for " + Mathf.Abs(Mathf.Round(abilityDamage * 10) / 10) + " dmg");        //so is this
            }
        }
        AudioManager.instance.PlayGlobal(selectedAbility.soundID, _priority: 1);
        GameManager.instance.CompleteAttack(this);
    }

    //this function is called when someone damages (or heals) this character. almost identical to the Damage() function in Character.cs
    public void Damage(float damageAmount, int duration = 1, float damagePerTurn = 1) {
        //actually change health value
        health = Mathf.Max(0, health - damageAmount);
        health = Mathf.Min(health, maxHealth);
        health = health - damageAmount;

        //display it, rounding to 1 decimal point and activating the damageIndicator
        //healthBar.text = (Mathf.Round(health * 10) / 10) + "/" + maxHealth;
        healthBar.value = maxHealth / health;
        damageIndicator.text = Mathf.Abs(Mathf.Round(damageAmount * 10) / 10).ToString();
        damageIndicator.color = damageAmount < 0 ? Color.green : Color.white;
        damageIndicator.gameObject.SetActive(false);    //the animation of the indicator start when the gameobject is turned on, so we turn it off to reset it
        damageIndicator.gameObject.SetActive(true);

        if (health <= 0) {
            Die();
        }
    }

    //called when health drops below 0
    void Die() {
        print(enemyName + " died!!!");
        GameManager.instance.enemiesInFight.Remove(this);
        GameManager.onEnemyTurnStart -= AddToAttackQueue;
        AudioManager.instance.PlayGlobal(deathSoundID);
        Destroy(gameObject);
    }
}
