﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

public class AchievementManager : MonoBehaviour {
    public static AchievementManager instance;
    public RectTransform achievementPopup;
    public GameObject lockedItemPrefab;
    public GameObject itemPrefab;
    public GameObject achievementsPanel;
    public RectTransform contentPanel;
    public Text statsText;

    private Achievement[] achievements;
    private float height;
    private bool popingAchievement = false;

    void Start () {

        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }

        InitAchievements();

        height = achievementPopup.rect.height/2;
        Vector3 temp = achievementPopup.anchoredPosition;
        temp.y = height;
        achievementPopup.anchoredPosition = temp;
    }

    void InitAchievements ()
    {
        achievements = new Achievement[29];

        //Deaths
        achievements[0] = new Achievement(1f, "Try again !", "1st death");
        achievements[1] = new Achievement(5f, "Everyone makes mistakes", "5 deaths");
        achievements[2] = new Achievement(25f, "The revenant", "25 deaths");
        achievements[3] = new Achievement(100f, "Death, sweet death", "100 deaths");
        achievements[4] = new Achievement(500f, "Braiiins...?", "500 deaths");

        //Color changes
        achievements[5] = new Achievement(5f, "Color swapper", "5 color changes");
        achievements[6] = new Achievement(25f, "Color curiosity", "25 color changes");
        achievements[7] = new Achievement(100f, "World in black and white", "100 color changes");
        achievements[8] = new Achievement(500f, "Zenitude", "500 color changes");

        //plays
        achievements[9] = new Achievement(1f, "Hello world !", "1st game !");
        achievements[10] = new Achievement(5f, "Try again !", "5 levels played");
        achievements[11] = new Achievement(20f, "Still here ?", "20 levels played");
        achievements[12] = new Achievement(50f, "Again !", "50 levels played");
        achievements[13] = new Achievement(100f, "We're glad you like this :D", "100 levels played");

        //succesful plays
        achievements[14] = new Achievement(1f, "First steps", "1 level cleared");
        achievements[15] = new Achievement(5f, "Beginner", "5 levels cleared");
        achievements[16] = new Achievement(20f, "Intermediate", "20 levels cleared");
        achievements[17] = new Achievement(50f, "Expert", "50 levels cleared");
        achievements[18] = new Achievement(100f, "Master", "100 levels cleared");

        //perfect plays
        achievements[19] = new Achievement(1f, "Perfect !", "1 level perfectly cleared");
        achievements[20] = new Achievement(5f, "Virtuoso", "5 levels perfectly cleared");
        achievements[21] = new Achievement(20f, "Clean freak", "20 levels perfectly cleared");
        achievements[22] = new Achievement(50f, "Certified maniac", "50 levels perfectly cleared");
        achievements[23] = new Achievement(100f, "OCD", "100 levels perfectly cleared");

        //score
        achievements[24] = new Achievement(5f, "Insert Coin", "5 point collected");
        achievements[25] = new Achievement(100f, "Every beat helps", "100 points collected");
        achievements[26] = new Achievement(500f, "Money ! Money ! Money !", "500 points collected");
        achievements[27] = new Achievement(3500f, "I'm rich !", "3500 points collected");
        achievements[28] = new Achievement(10000f, "...", "10000 points collected !");

    }

    public void Reset()
    {
        foreach(Achievement a in achievements)
        {
            a.isActive = false;
        }
    }

    IEnumerator WaitForRealSeconds(float time)
    {
        //popup wait
        float start = Time.realtimeSinceStartup;
        while (Time.realtimeSinceStartup < start + time)
        {
            yield return null;
        }
    }
	
    //Coroutine for achievement popup
    IEnumerator PopAchievement (Achievement achievement)
    {
        while (popingAchievement)
        {
            yield return WaitForRealSeconds(1.5f);
        }

        popingAchievement = true;

        //popup setup
        achievementPopup.GetComponentsInChildren<Text>()[0].text = achievement.name;
        achievementPopup.GetComponentsInChildren<Text>()[1].text = achievement.description;
        SoundManager.instance.PlayAchievementSound();

        //popup up
        while (achievementPopup.anchoredPosition.y > - height )
        {
            achievementPopup.anchoredPosition = achievementPopup.anchoredPosition - Vector2.up * 3;
            yield return null;
        }

        //popup wait
        yield return WaitForRealSeconds(1.5f);

        //popup down
        while (achievementPopup.anchoredPosition.y < height )
        {
            achievementPopup.anchoredPosition = achievementPopup.anchoredPosition + Vector2.up * 3;
            yield return null;
        }
        popingAchievement = false;
    }


    void EarnAchievement (Achievement achievement)
    {
        StartCoroutine(PopAchievement(achievement));
    }

    void EarnAchievement (int achievementIndex)
    {
        StartCoroutine(PopAchievement(achievements[achievementIndex]));
    }

    public void CheckAchievement (int index, float value, bool silent = false)
    {
        if (achievements[index].CheckActivation(value) && !silent)
        {
            EarnAchievement(index);
        }
    }

    public void ShowAchievements ()
    {
        //Show achievement panel
        achievementsPanel.SetActive(true);

        //destroy items in list
        for ( int i = contentPanel.gameObject.transform.childCount -1; i >= 0; i--)
        {
            GameObject o = contentPanel.transform.GetChild(i).gameObject;
            Destroy(o);
        }

        //scroll to the beginning of the list
        contentPanel.parent.gameObject.GetComponent<ScrollRect>().verticalNormalizedPosition = 0.5f;

        //setup game statistics panel with string
		statsText.text = GameManager.instance.stats.GetString();
		GameObject firstItem = Instantiate(itemPrefab);
		firstItem.transform.SetParent(contentPanel);
		firstItem.GetComponentsInChildren<Text>()[0].text = "ACHIEVEMENTS";
		firstItem.GetComponentsInChildren<Text>()[1].text = "";
		firstItem.GetComponentsInChildren<Text>()[0].fontSize = 20;
		firstItem.GetComponent<VerticalLayoutGroup> ().padding.bottom = 5;
		firstItem.GetComponent<VerticalLayoutGroup> ().childAlignment = TextAnchor.LowerLeft;



        //Add new achievement item in the list
        for (int i = 0; i < achievements.Length; i++)
        {
            if (!achievements[i].isHidden || achievements[i].isActive)
            {
                GameObject newItem;
                if (achievements[i].isActive)
                {
                    newItem = Instantiate(itemPrefab);
                }
                else
                {
                    newItem = Instantiate(lockedItemPrefab);
                }
                newItem.transform.SetParent(contentPanel);
                newItem.GetComponentsInChildren<Text>()[0].text = achievements[i].name;
                newItem.GetComponentsInChildren<Text>()[1].text = achievements[i].description;
            }
        }

    }

}

public class Achievement
{
    public bool isActive = false;
    public enum Compare { GreaterThan, Equal, LowerThan};
    public string name = "";
    public string description = "";
    public bool isHidden;
    private float threshold = 0;
    private Compare compareMethod;

    public Achievement(float threshold, string name, string description, bool isHidden = false, Compare compareMethod = Compare.GreaterThan)
    {
        this.name = name.ToUpper();
        this.description = description;
        this.threshold = threshold;
        this.compareMethod = compareMethod;
        this.isHidden = isHidden;
    }

    public bool CheckActivation(float value)
    {
        if (!isActive)
        {
            switch (compareMethod)
            {
                case Compare.Equal:
                    if (value == threshold)
                    {
                        isActive = true;
                        return true;
                    }
                    break;
                case Compare.GreaterThan:
                    if (value >= threshold)
                    {
                        isActive = true;
                        return true;
                    }
                    break;
                case Compare.LowerThan:
                    if (value <= threshold)
                    {
                        isActive = true;
                        return true;
                    }
                    break;
            }

        }
        return false;
    }
}

[Serializable]
public class PlayerStatistics
{
    public Property death;
    public Property colorChange;
    public Property plays;
    public Property succesPlays;
    public Property perfectPlays;
    public Property totalScore;
    public Property highScore;

    public PlayerStatistics ()
    {
        death = new Property("Deaths", 0, new int[] { 0, 1, 2, 3, 4 });
        colorChange = new Property("Color Changes", 0, new int[] { 5, 6, 7, 8 });
        plays = new Property("Levels Played", 0, new int[] { 9, 10, 11, 12 ,13 });
        succesPlays = new Property("Succesful Plays", 0, new int[] { 14, 15, 16, 17, 18 });
        perfectPlays = new Property("Perfect Plays", 0, new int[] { 19, 20, 21, 22, 23 });
        totalScore = new Property("Total Score", 0, new int[] { 24, 25, 26, 27, 28 });
        highScore = new Property("High Score", 0, new int[] { });
    }

    public string GetString ()
    {
        return
            totalScore.GetString() +
            death.GetString() +
            colorChange.GetString() +
            plays.GetString() +
            succesPlays.GetString() +
            perfectPlays.GetString() +
            highScore.GetString();
    }

    public void CheckAchievements ()
    {
        totalScore.CheckAchievements(true);
        death.CheckAchievements(true);
        colorChange.CheckAchievements(true);
        plays.CheckAchievements(true);
        succesPlays.CheckAchievements(true);
        perfectPlays.CheckAchievements(true);
    }

    public void Reset()
    {
        death.SetValue(0f);
        colorChange.SetValue(0f);
        plays.SetValue(0f);
        succesPlays.SetValue(0f);
        perfectPlays.SetValue(0f);
        totalScore.SetValue(0f);
        highScore.SetValue(0f);
    }

}

[Serializable]
public class Property
{
    public string name = "";
    public float value = 0;
    private int[] achievementIndex;

    public Property(string name, float value, int[] achievements)
    {
        this.name = name;
        this.value = value;
        this.achievementIndex = achievements;
    }

    public string GetString()
    {
        return name + " : " + value + "\n";
    }

    public void CheckAchievements (bool silent = false)
    {
        for (int i = 0; i < achievementIndex.Length; i++)
        {
            if(AchievementManager.instance)
            {
                AchievementManager.instance.CheckAchievement(achievementIndex[i], value, silent);
            }
        }
    }

    public void Increment()
    {
        value += 1;
        CheckAchievements();
    }

    public void SetValue(float value)
    {
        this.value = value;
    }
}
