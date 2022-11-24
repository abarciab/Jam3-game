using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Enemy : MonoBehaviour
{
    [Header("stats")]
    public string enemyName;
    public int maxHealth;
    public float health;
    public float testDamage;        //it's not setup, but it would probably be best if enemy attacks worked the same way as character attacks, making use of the 'Ability' class to allow for much more flexibility. check out character.cs to see more
    [Tooltip("the time, in seconds, it takes for this enemy to complete their attack")]     //enemy attacks are performed sequentially
    public float attackTime = 0.5f;
    public List<string> abilityNames;

    [Header("Ability")]
    public Ability AoEAbility;
    public Ability HealAbility;
    public Ability DoTAbility;
    public Ability OtherAbility;

    [Header("references")]
    public TextMeshProUGUI nameLabel;
    public TextMeshProUGUI healthLabel;
    public TextMeshProUGUI damageIndicator;

    [Header("misc")]
    public int deathSoundID = 5;
    public List<StatusEffect> activeStatusEffects = new List<StatusEffect>();   //hasn't been implemented yet, but these should be created when duration > 1 in damage function, and triggered onEnemyTurnStart

    //basic setup for some UI labels and adding listeners to events
    private void Start() {
        nameLabel.text = enemyName;
        GameManager.instance.AddEnemyToFight(this);
        health = maxHealth;
        healthLabel.text = health + "/" + maxHealth;
        GameManager.onEnemyTurnStart += AddToAttackQueue;
    }

    //this is called at the start of the enemy turn, and then enemies in the queue are called sequentially, each attacking only after the previous one has finished
    void AddToAttackQueue() { 
        if (!GameManager.instance.enemyActionQueue.Contains(this)) {
            GameManager.instance.enemyActionQueue.Add(this);
        }
    }

    //simple handler for the coroutine so that other scripts can call this instead of 'startCoroutine("ReadyAttack")'
    public void StartAttack() {
        StartCoroutine("ReadyAttack");                
    }

    //artificial waiting period so that attacks aren't instant. A more refined version of this would probably use attack animations with animation events to trigger Attack() instead
    IEnumerator ReadyAttack()
    {
        yield return new WaitForSeconds(attackTime);
        EnemyAction();
    }

    //Chontrols the type of action the enemy takes and who it is used on
    void EnemyAction(){
        var probabilityNum = Random.Range(0,100);
        if(probabilityNum < 20) {MultiTargetAttack();}
        else if(probabilityNum < 40) {Heal();}
        else {Attack();}
    }

    //simple attack, where a random character is picked and damage is dealt.
    void Attack() {
        if (GameManager.instance.charactersInFight.Count == 0) { return; }
        Character target = GameManager.instance.charactersInFight[Random.Range(0, GameManager.instance.charactersInFight.Count)];
        GameManager.instance.Log(enemyName + " attacks " + target.characterName + " for " + testDamage + " dmg");
        target.Damage(testDamage);
        AudioManager.instance.PlayGlobal(6, _priority: 1, restart: true);   //the restart:true ensures that, if multiple enemies attack quickly, you hear part of each attack, even if it's inturrupeted. it'll be better to have an audiosource on each enemy, then there's no overlap
        GameManager.instance.CompleteAttack(this);      //this function is called to let the gameManager know that it's time for the next enemy to attack
    }

    //ability that enemies use to do damage to multiple characters
    void MultiTargetAttack(){
        if (GameManager.instance.charactersInFight.Count == 0) { return; }
        else if (GameManager.instance.charactersInFight.Count == 1) { 
            Attack();
            return; 
        }
        var possibleTargets = new List<Character>(GameManager.instance.charactersInFight);
        for(int targetCount = AoEAbility.numTargets; targetCount > 0; targetCount--){
            int selectedIndex = Random.Range(0, possibleTargets.Count);
            possibleTargets[selectedIndex].Damage(AoEAbility.damage, AoEAbility.turnDuration, AoEAbility.damagePerTurn);
            GameManager.instance.Log(enemyName + " attacks " + possibleTargets[selectedIndex].characterName + " for " + AoEAbility.damage + " dmg");
            AudioManager.instance.PlayGlobal(AoEAbility.soundID, _priority: 1);
            possibleTargets.RemoveAt(selectedIndex);
        }
        GameManager.instance.CompleteAttack(this);
    }

    //Healing ability (group or single) which can be an over-time heal
    void Heal(){
        if (GameManager.instance.charactersInFight.Count == 0) { return; }
        var possibleTargets = new List<Enemy>(GameManager.instance.enemiesInFight);
        double highestHealth = -1.0; //kinda assuming 0 means dead and I dont want to revive an enemy lol
        int selectedIndex = 0;
        if(HealAbility.numTargets == 1 || GameManager.instance.enemiesInFight.Count == 1){ //when single healing heals lowest enemy
            for(int i = 0; i < possibleTargets.Count; i++){
                if(possibleTargets[i].health > highestHealth){
                    highestHealth = possibleTargets[i].health;
                    selectedIndex = i;
                }
            }
            possibleTargets[selectedIndex].Damage(HealAbility.damage, HealAbility.turnDuration, HealAbility.damagePerTurn);
            GameManager.instance.Log(enemyName + " heals " + possibleTargets[selectedIndex].enemyName + " for " + HealAbility.damage + " dmg");
            AudioManager.instance.PlayGlobal(HealAbility.soundID, _priority: 1);
        }
        else{
            for(int targetCount = HealAbility.numTargets; targetCount > 0; targetCount--){
                selectedIndex = Random.Range(0, possibleTargets.Count);
                possibleTargets[selectedIndex].Damage(HealAbility.damage, HealAbility.turnDuration, HealAbility.damagePerTurn);
                GameManager.instance.Log(enemyName + " heals " + possibleTargets[selectedIndex].enemyName + " for " + HealAbility.damage + " dmg");
                AudioManager.instance.PlayGlobal(HealAbility.soundID, _priority: 1);
                possibleTargets.RemoveAt(selectedIndex);
            }
        }
        GameManager.instance.CompleteAttack(this);

    }

    //DoT Ability can be single or Multi-Target 

    //this function is called when someone damages (or heals) this character. almost identical to the Damage() function in Character.cs
    public void Damage(float damageAmount, int duration = 1, float damagePerTurn = 1) {
        //actually change health value
        health = Mathf.Max(0, health - damageAmount);
        health = Mathf.Min(health, maxHealth);

        //display it, rounding to 1 decimal point and activating the damageIndicator
        healthLabel.text = (Mathf.Round(health * 10) / 10) + "/" + maxHealth;
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
