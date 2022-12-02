using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Enemy : MonoBehaviour
{
    [Header("stats")]
    public int pointValue;
    public string enemyName;
    public EnemyStats.EnemyType type;
    public Color tint;
    public int maxHealth;
    public float health;
    public float attackDamage; 
    public float attackTime = 0.5f;
    public List<Ability> usableAbilities;
    public bool isSpeaker = false;
    [Tooltip("NOTE: First line is intro line said at beginning of battle")]
    public List<string> dialogueLines;
    private int currentLine;
    bool revealHook;
    bool revealVines;

    [Header("references")]
    public TextMeshProUGUI nameLabel;
    public Slider healthBar;
    public TextMeshProUGUI damageIndicator;
    private AudioSource source;
    public TextMeshProUGUI effectAmount;
    public Image effectImage;

    [Header("misc")]
    public int deathSoundID = 5;
    public List<StatusEffect> activeStatusEffects = new List<StatusEffect>();   //hasn't been implemented yet, but these should be created when duration > 1 in damage function, and triggered onEnemyTurnStart


    //basic setup for some UI labels and adding listeners to events
    private void Start() {
        source = gameObject.AddComponent<AudioSource>();
        nameLabel.text = enemyName;
        
        //GetComponent<SpriteRenderer>().sprite = portrait;

        GameManager.instance.AddEnemyToFight(this);
        health = maxHealth;
        healthBar.value = health / maxHealth;
        //healthBar.text = health + "/" + maxHealth;
        GameManager.onEnemyTurnStart += AddToAttackQueue;
    }

    //this is called at the start of the enemy turn, and then enemies in the queue are called sequentially, each attacking only after the previous one has finished
    void AddToAttackQueue() {
        ProcessStatusEffects();
        if (!GameManager.instance.enemyActionQueue.Contains(this) && health > 0) {
            GameManager.instance.enemyActionQueue.Add(this);
        }
    }

    void ProcessStatusEffects()
    {
        float totalDamage = 0;
        foreach (var effect in activeStatusEffects) {
            if (effect.turnsLeft > 0) {
                effect.turnsLeft -= 1;
                totalDamage += effect.damagePerTurn;
            }
        }
        effectImage.gameObject.SetActive(false);
        if (totalDamage > 0) {
            AudioManager.instance.PlayHere(10, source);
            effectImage.sprite = GameManager.instance.fireImg;
            effectAmount.color = GameManager.instance.fireColor;
            
        }
        if (totalDamage < 0) {
            effectImage.sprite = GameManager.instance.heartImg;
            effectAmount.color = GameManager.instance.healColor;
        }

        if (Mathf.Abs(totalDamage) > 0) {
            effectImage.gameObject.SetActive(true);
            effectAmount.text = totalDamage.ToString();
            Damage(totalDamage);
        }
    }

    public void setStats(EnemyStats stats) {
        pointValue = stats.pointValue;
        enemyName = stats.name;
        maxHealth = stats.maxHealth;
        attackDamage = stats.attackDamage;
        attackTime = stats.attackTime;
        usableAbilities = stats.usableAbilities;
        isSpeaker = stats.isSpeaker;
        dialogueLines = stats.dialogueLines;
        currentLine = 0;
        type = stats.type;
        revealHook = stats.revealHook;
        revealVines = stats.revealVines;

        var animator = GetComponent<Animator>();
        animator.SetBool("rock", false);
        animator.SetBool("vine", false);
        animator.SetBool("crab", false);
        switch (type) {
            case EnemyStats.EnemyType.rock:
                animator.SetBool("rock", true);
                break;
            case EnemyStats.EnemyType.vines:
                animator.SetBool("vine", true);
                break;
            case EnemyStats.EnemyType.crab:
                animator.SetBool("crab", true);
                break;
            default:
                break;
        }
    }

    public void sayLine() {
        if (currentLine <= dialogueLines.Count - 1) {
            GameManager.instance.DiaLog(dialogueLines[currentLine]);
            currentLine++;
        }
    }

    //simple handler for the coroutine so that other scripts can call this instead of 'startCoroutine("ReadyAttack")'
    public void StartAttack() {
        StartCoroutine("ReadyAttack");                
    }

    //artificial waiting period so that attacks aren't instant. A more refined version of this would probably use attack animations with animation events to trigger Attack() instead
    IEnumerator ReadyAttack()
    {
        GetComponent<Animator>().SetTrigger("attack");
        if(isSpeaker) {
            //yield return new WaitForSeconds(attackTime);
            //GameManager.instance.clearLog();
            sayLine();
        }
        //yield return new WaitForSeconds(isSpeaker ? GameManager.instance.dialogueTime : attackTime);
        yield return new WaitForSeconds(attackTime);

        /*if(isSpeaker)
            //GameManager.instance.clearLog();
        if(!GameManager.instance.textHidden && !GameManager.instance.checkForSpeakers()) {
            //print("yo");
            GameManager.instance.textHidden = true;
            GameManager.instance.clearLog();
        }*/
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
        //print("Attacking");
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
            else if (possibleEnemyTargets.Count > 0){
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
                GameManager.instance.Log(enemyName + " uses " + selectedAbility.abilityName + (abilityDamage >= 0 ? " to attack " : "to heal ") + selectedAllyTargets[i].enemyName +  " for " + Mathf.Abs(Mathf.Round(abilityDamage * 10) / 10) + " dmg");    //this is what shows up in the little on-screen log
            }
            else {
                selectedEnemyTargets[i].Damage(abilityDamage, selectedAbility.turnDuration, selectedAbility.damagePerTurn);
                GameManager.instance.Log(enemyName + " uses " + selectedAbility.abilityName + (abilityDamage >= 0 ? " to attack " : "to heal ") + selectedEnemyTargets[i].characterName +  " for " + Mathf.Abs(Mathf.Round(abilityDamage * 10) / 10) + " dmg");        //so is this
            }
        }
        int soundID = 11;
        if (type == EnemyStats.EnemyType.rock) { soundID = 13; }
        if (type == EnemyStats.EnemyType.vines) { soundID = 12; }
        AudioManager.instance.PlayHere(soundID, source);
        GameManager.instance.CompleteAttack(this);
    }

    //this function is called when someone damages (or heals) this character. almost identical to the Damage() function in Character.cs
    public void Damage(float damageAmount, int duration = 1, float damagePerTurn = 1) {
        if (duration > 1) {
            var newEffect = new StatusEffect();
            newEffect.damagePerTurn = damagePerTurn;
            newEffect.turnsLeft = duration - 1;
            activeStatusEffects.Add(newEffect);
        }

        //actually change health value
        health = Mathf.Max(0, health - damageAmount);
        health = Mathf.Min(health, maxHealth);
        health = health - damageAmount;

        //display it, rounding to 1 decimal point and activating the damageIndicator
        healthBar.value = health / maxHealth;
        if (damageAmount != 0) {
            damageIndicator.text = Mathf.Abs(Mathf.Round(damageAmount * 10) / 10).ToString();
            damageIndicator.color = damageAmount < 0 ? Color.green : Color.white;
            damageIndicator.gameObject.SetActive(false);    //the animation of the indicator start when the gameobject is turned on, so we turn it off to reset it
            damageIndicator.gameObject.SetActive(true);
        }
       
        if (health <= 0) {
            Die();
        }
    }

    //called when health drops below 0
    void Die() {

        if (!GameManager.instance.playerTurn) {
            //print("ending attack");
            GameManager.instance.CompleteAttack(this);
        }
        GameManager.instance.revealVines = revealVines;
        GameManager.instance.revealHook = revealHook;
        //print(enemyName + " died!!!");
        GameManager.instance.totalScoreForThisFight += pointValue;
        GameManager.instance.enemiesInFight.Remove(this);
        GameManager.onEnemyTurnStart -= AddToAttackQueue;
        if(GameManager.instance.enemiesInFight.Count > 0)
            GameManager.instance.setTargetIcon(1);
        AudioManager.instance.PlayGlobal(deathSoundID);
        Destroy(gameObject);
    }
}
