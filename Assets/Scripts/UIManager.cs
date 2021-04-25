using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    private static UIManager _instance;
    public static UIManager Instance => _instance;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
        }
    }

    [SerializeField] private GameObject playerHealth;
    [SerializeField] private GameObject bossHealth;
    [SerializeField] private GameObject gameEndPanel;
    [SerializeField] private GameObject deathText;
    [SerializeField] private GameObject victoryText;
    [SerializeField] private GameObject countdownTimer;
    [SerializeField] private GameObject hitOutline;
    [SerializeField] private TextMeshProUGUI runTimeText;
    private int countdownVal;
    private string runTime;

    public void SetBossHealth(float maxHealth, float health)
    {
        if (!bossHealth.activeSelf)
        {
            bossHealth.SetActive(true);
        }

        bossHealth.GetComponent<Slider>().value = health / maxHealth;

    }

    public void SetPlayerHealth(float maxHealth, float health)
    {
        if (!playerHealth.activeSelf)
        {
            playerHealth.SetActive(true);
        }

        playerHealth.GetComponent<Slider>().value = health / maxHealth;
    }

    public void TriggerDeathScreen()
    {
        gameEndPanel.SetActive(true);
        deathText.SetActive(true);
    }

    public void TriggerVictoryScreen()
    {
        if (bossHealth.activeSelf)
        {
            bossHealth.SetActive(false);
        }

        gameEndPanel.SetActive(true);
        victoryText.SetActive(true);
    }

    public IEnumerator CountdownTimer()
    {
        Time.timeScale = 0;
        countdownVal = 3;
        countdownTimer.SetActive(true);
        while(countdownVal > 0)
        {
            countdownTimer.GetComponent<TextMeshProUGUI>().text = countdownVal.ToString();
            yield return new WaitForSecondsRealtime(1);
            countdownVal--;
        }
        Time.timeScale = 1;
        countdownTimer.SetActive(false);
    }

    public IEnumerator GetHit()
    {
        hitOutline.SetActive(true);
        yield return new WaitForSeconds(0.22f);
        hitOutline.SetActive(false);
    }

    public void Restart()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(1);
    }

    public void BackToMenu()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }

    public void UpdateRunTime(string runTime)
    {
        this.runTime = runTime;
        runTimeText.text = runTime;
    }
}
