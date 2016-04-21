using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class AchievementManager : MonoBehaviour {
    public static AchievementManager instance;
    public GameObject achievementPopup;
    public GameObject lockedItemPrefab;
    public GameObject itemPrefab;
    public GameObject achievementsPanel;
    public RectTransform contentPanel;
    public Text statsText;

    Achievement[] achievements;
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

        height = achievementPopup.GetComponent<RectTransform>().rect.height/2;
        Vector3 temp = achievementPopup.transform.position;
        temp.y = - height;
        achievementPopup.transform.position = temp;
    }

    void InitAchievements()
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
    IEnumerator PopAchievement(Achievement achievement)
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
        while (achievementPopup.transform.position.y < height - 1 )
        {
            achievementPopup.transform.position = achievementPopup.transform.position + Vector3.up * 3;
            yield return null;
        }

        //popup wait
        float start = Time.realtimeSinceStartup;
        while (Time.realtimeSinceStartup < start + 1.5)
        {
            yield return WaitForRealSeconds(1.5f);
        }

        //popup down
        while (achievementPopup.transform.position.y > -height )
        {
            achievementPopup.transform.position = achievementPopup.transform.position - Vector3.up * 3;
            yield return null;
        }
        popingAchievement = false;

    }


    public void EarnAchievement(Achievement achievement)
    {
        StartCoroutine(PopAchievement(achievement));
    }

    public void EarnAchievement(int achievementIndex)
    {
        StartCoroutine(PopAchievement(achievements[achievementIndex]));
    }

    public void CheckAchievement(int index, float value)
    {
        if (achievements[index].CheckActivation(value))
        {
            EarnAchievement(index);
        }
    }

    public void ShowAchievements()
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
        statsText.text = GameManager.instance.stats.getString();

        //Add new achievement item in the list
        for (int i = 0; i < achievements.Length; i++)
        {
            if (!achievements[i].isHidden)
            {
                GameObject newItem;
                if (achievements[i].isActive)
                {
                    newItem = (GameObject)Instantiate(itemPrefab);
                }
                else
                {
                    newItem = (GameObject)Instantiate(lockedItemPrefab);
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
    float threshold = 0;
    Compare compareMethod;

    public Achievement(float threshold, string name, string description, bool isHidden = false, Compare compareMethod = Compare.GreaterThan)
    {
        this.name = name;
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

public class PlayerStatistics
{
    public Property death;
    public Property colorChange;
    public Property plays;
    public Property succesPlays;
    public Property perfectPlays;
    public Property totalScore;

    public PlayerStatistics()
    {
        death = new Property("Deaths", 0, new int[] { 0, 1, 2, 3, 4 });
        colorChange = new Property("Color Changes", 0, new int[] { 5, 6, 7, 8 });
        plays = new Property("Levels Played", 0, new int[] { 9, 10, 11, 12 ,13 });
        succesPlays = new Property("Succesful Plays", 0, new int[] { 14, 15, 16, 17, 18 });
        perfectPlays = new Property("Perfect Plays", 0, new int[] { 19, 20, 21, 22, 23 });
        totalScore = new Property("Total Score", 0, new int[] { 24, 25, 26, 27, 28 });
    }

    public string getString(Property p)
    {
        return p.name + " : " + p.value + "\n";
    }

    public string getString()
    {
        return
            getString(totalScore) +
            getString(death) +
            getString(colorChange) +
            getString(plays) +
            getString(succesPlays) +
            getString(perfectPlays);   
    }

}

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

    public void Increment()
    {
        value += 1;
        for (int i = 0; i < achievementIndex.Length; i++)
        {
            AchievementManager.instance.CheckAchievement(achievementIndex[i], value);
        }
    }
}
