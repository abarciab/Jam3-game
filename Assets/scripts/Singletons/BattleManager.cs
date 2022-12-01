using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BattleManager : MonoBehaviour
{
    public static BattleManager instance;

    [SerializeField] private string battleSceneName;
    [SerializeField] private string freeRoamSceneName;
    [SerializeField] private Transform enemyContainer;
    [SerializeField] private Transform playerParty;
    public List<EnemyStats> currentEnemies { get; private set; }
    public FreeRoamEnemy currentFreeRoamEnemy { get; private set; }
    public List<Enemy> enemyChoice = new List<Enemy>();

    private void Awake() {
        instance = this;
    }
    

    private void toggleScene(string sceneName, bool active) {
        foreach(GameObject gmobj in SceneManager.GetSceneByName(sceneName).GetRootGameObjects())
            if(gmobj.tag != "GameController") {
                gmobj.SetActive(active);
                
                if(gmobj.tag == "PlayerParty")
                    gmobj.GetComponentInChildren<IsometricPlayerMovement>().toggleMovement(active);
            }
    }

    private void resetPlayerParty() {
        foreach(Transform partyMember in playerParty) {
            IsometricPlayerMovement playerMovement = partyMember.GetComponent<IsometricPlayerMovement>();
            if(playerMovement)
                playerMovement.resetPosition();
            else
                partyMember.GetComponent<FollowCharacter>().resetPosition();
            
            partyMember.GetComponent<MovementLog>()?.log.Clear();
        }
    }

    public void toggleAllEnemyMovement(bool active) {
        foreach(Transform enemy in enemyContainer) {
            enemy.GetComponent<EnemyMovement>().enabled = active;
        }
    }

    public void startBattleScene(FreeRoamEnemy enemy) {
        // load enemies
        currentFreeRoamEnemy = enemy;
        currentEnemies = enemy.enemiesInEncounter;
        
        // pause free roam scene and start battle scene
        toggleScene(freeRoamSceneName, false);
        if(!SceneManager.GetSceneByName(battleSceneName).isLoaded)
            SceneManager.LoadScene(battleSceneName, LoadSceneMode.Additive); 
    }

    public void endBattleWin() {
        // unload battle scene
        if(SceneManager.GetSceneByName(battleSceneName).isLoaded)
            SceneManager.UnloadSceneAsync(battleSceneName);

        // unpause free roam and disable enemy
        toggleScene(freeRoamSceneName, true);
        toggleAllEnemyMovement(true);
        currentFreeRoamEnemy.setDefeated(true);
        currentFreeRoamEnemy.gameObject.SetActive(false);
    }

    public void endBattleLose() {
        // unload battle scene
        if(SceneManager.GetSceneByName(battleSceneName).isLoaded)
            SceneManager.UnloadSceneAsync(battleSceneName);

        // unpause free roam and disable enemy
        toggleScene(freeRoamSceneName, true);
        resetPlayerParty();
        toggleAllEnemyMovement(true);
    }

    // FOR DEBUGGING
    private void Update() {
        if(Input.GetKeyDown(KeyCode.P)) {
            endBattleWin();
        }
    }
}
