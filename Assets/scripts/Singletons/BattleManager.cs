using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BattleManager : MonoBehaviour
{
    public static BattleManager instance;

    [SerializeField] private string battleSceneName;
    [SerializeField] private string freeRoamSceneName;
    public List<EnemyStats> currentEnemies { get; private set; }
    public FreeRoamEnemy currentFreeRoamEnemy { get; private set; }

    private void Awake() {
        instance = this;
    }

    private void toggleScene(string sceneName, bool active) {
        foreach(GameObject gmobj in SceneManager.GetSceneByName(sceneName).GetRootGameObjects())
            if(gmobj.tag != "GameController")
                gmobj.SetActive(active);
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
        currentFreeRoamEnemy.gameObject.SetActive(false);
    }

    // FOR DEBUGGING
    private void Update() {
        if(Input.GetKeyDown(KeyCode.P)) {
            endBattleWin();
        }
    }
}
