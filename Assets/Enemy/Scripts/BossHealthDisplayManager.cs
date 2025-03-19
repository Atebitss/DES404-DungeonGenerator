using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BossHealthDisplayManager : MonoBehaviour
{
    [SerializeField] private GameObject bossHealthParent;
    [SerializeField] private TMP_Text bossHealthText;
    [SerializeField] private RectTransform bossHealthBarRect;
    [SerializeField] private Image bossHealthBarImage;
    private float maxBossHealthBarWidth;
    private int bossHealthPointsCurrent = 0, bossHealthPointsMax = 0;
    AbstractEnemy bossController;



    public void EnableBossHealthDisplay()
    {
        bossHealthParent.SetActive(true);
    }
    public void DisableBossHealthDisplay()
    {
        bossHealthParent.SetActive(false);
    }


    public void Wake(AbstractEnemy bossController)
    {
        this.bossController = bossController;
        maxBossHealthBarWidth = bossHealthBarRect.sizeDelta.x;
        bossHealthPointsCurrent = bossController.GetHealth();
        bossHealthPointsMax = bossController.GetHealth();
        UpdateBossHealthBar();
    }

    public void UpdateCurrentBossHealth(int healthPoints)
    {
        bossHealthPointsCurrent = healthPoints;
        UpdateBossHealthBar();
    }

    private void UpdateBossHealthBar()
    {
        //update health bar
        float hpPercentage = (float)bossHealthPointsCurrent / (float)bossHealthPointsMax;
        bossHealthBarRect.sizeDelta = new Vector2(maxBossHealthBarWidth * hpPercentage, bossHealthBarRect.sizeDelta.y);
        bossHealthText.text = bossHealthPointsCurrent + " / " + bossHealthPointsMax;
    }
}
