using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;
using System.Collections.Generic;

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
        achievements[0] = new NumericAchievement(1f, "Try again!", "1st death");
        achievements[1] = new NumericAchievement(5f, "Everyone makes mistakes", "5 deaths");
        achievements[2] = new NumericAchievement(25f, "The revenant", "25 deaths");
        achievements[3] = new NumericAchievement(100f, "Death, sweet death", "100 deaths");
        achievements[4] = new NumericAchievement(500f, "Braiiins...?", "500 deaths");

        //Color changes
        achievements[5] = new NumericAchievement(5f, "Color swapper", "5 color changes");
        achievements[6] = new NumericAchievement(25f, "Color curiosity", "25 color changes");
        achievements[7] = new NumericAchievement(100f, "World in black and white", "100 color changes");
        achievements[8] = new NumericAchievement(500f, "Zenitude", "500 color changes");

        //plays
        achievements[9] = new NumericAchievement(1f, "Hello world!", "1st game!");
        achievements[10] = new NumericAchievement(5f, "Try again!", "5 levels played");
        achievements[11] = new NumericAchievement(20f, "Still here?", "20 levels played");
        achievements[12] = new NumericAchievement(50f, "Again!", "50 levels played");
        achievements[13] = new NumericAchievement(100f, "We're glad you like this :D", "100 levels played");

        //succesful plays
        achievements[14] = new NumericAchievement(1f, "First steps", "1 level cleared");
        achievements[15] = new NumericAchievement(5f, "Beginner", "5 levels cleared");
        achievements[16] = new NumericAchievement(20f, "Intermediate", "20 levels cleared");
        achievements[17] = new NumericAchievement(50f, "Expert", "50 levels cleared");
        achievements[18] = new NumericAchievement(100f, "Master", "100 levels cleared");

        //perfect plays
        achievements[19] = new NumericAchievement(1f, "Perfect!", "1 level perfectly cleared");
        achievements[20] = new NumericAchievement(5f, "Virtuoso", "5 levels perfectly cleared");
        achievements[21] = new NumericAchievement(20f, "Clean freak", "20 levels perfectly cleared");
        achievements[22] = new NumericAchievement(50f, "Certified maniac", "50 levels perfectly cleared");
        achievements[23] = new NumericAchievement(100f, "OCD", "100 levels perfectly cleared");

        //score
        achievements[24] = new NumericAchievement(5f, "Insert Coin", "5 point collected");
        achievements[25] = new NumericAchievement(100f, "Every beat helps", "100 points collected");
        achievements[26] = new NumericAchievement(500f, "Money! Money! Money!", "500 points collected");
        achievements[27] = new NumericAchievement(3500f, "I'm rich!", "3500 points collected");
        achievements[28] = new NumericAchievement(10000f, "...O.O", "10000 points collected!");

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
        while (achievementPopup.anchoredPosition.y > - height - 10 )
        {
            achievementPopup.anchoredPosition = achievementPopup.anchoredPosition - Vector2.up * 3;
            yield return null;
        }

        //popup wait
        yield return WaitForRealSeconds(2.5f);

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

    public void CheckAchievement (int index, bool silent = false)
    {
        if (achievements[index].CheckActivation() && !silent)
        {
            EarnAchievement(index);
        }
    }

    public void ShowAchievements ()
    {
        // Show achievement panel
        achievementsPanel.SetActive(true);

        // Destroy items in list
        for ( int i = contentPanel.gameObject.transform.childCount -1; i >= 0; i--)
        {
            GameObject o = contentPanel.transform.GetChild(i).gameObject;
            Destroy(o);
        }

        // Scroll to the beginning of the list
        contentPanel.parent.gameObject.GetComponent<ScrollRect>().verticalNormalizedPosition = 0.5f;

        // Setup game statistics panel with string
		statsText.text = GameManager.instance.stats.GetString();
		GameObject firstItem = Instantiate(itemPrefab);
		firstItem.transform.SetParent(contentPanel);
		firstItem.GetComponentsInChildren<Text>()[0].text = "ACHIEVEMENTS";
		firstItem.GetComponentsInChildren<Text>()[1].text = "";
		firstItem.GetComponentsInChildren<Text>()[0].fontSize = 20;
		firstItem.GetComponent<VerticalLayoutGroup> ().padding.bottom = 5;
		firstItem.GetComponent<VerticalLayoutGroup> ().childAlignment = TextAnchor.LowerLeft;



        // Add new achievement item in the list
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

    internal void RegisterProperty(int index, Property numericProperty)
    {
        achievements[index].RegisterProperty(numericProperty);
    }
}

public abstract class Achievement
{
    public bool isActive = false;
    public string name = "";
    public string description = "";
    public bool isHidden;


    public Achievement(string name, string description, bool isHidden = false)
    {
        this.name = name.ToUpper();
        this.description = description;
        this.isHidden = isHidden;
    }

    public abstract bool CheckActivation();

    public abstract void RegisterProperty(Property property);

}

public class NumericAchievement : Achievement
{
    List<NumericProperty> properties = new List<NumericProperty>();
    public enum Compare { GreaterThan, Equal, LowerThan };
    protected float threshold = 0;
    protected Compare compareMethod;

    public NumericAchievement(float threshold, string name, string description, bool isHidden = false, Compare compareMethod = Compare.GreaterThan) : base(name, description, isHidden)
    {
        this.threshold = threshold;
        this.compareMethod = compareMethod;
    }

    // TODO : Simplify with templates ?
    public override void RegisterProperty(Property property)
    {
        if (property is NumericProperty)
        {
            properties.Add(property as NumericProperty);
        } else
        {
            throw new InvalidOperationException("Numeric Achievement must have numeric property");
        }
    }

    public override bool CheckActivation()
    {
        return CheckActivation(GetValue());
    }

    protected float GetValue()
    {
        float result = 0;
        foreach ( NumericProperty prop in properties)
        {
            result += prop.value;
        }
        return result;
    }

    protected bool CheckActivation(float value)
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
    public NumericProperty death;
    public NumericProperty colorChange;
    public NumericProperty plays;
    public NumericProperty successPlays;
    public NumericProperty perfectPlays;
    public NumericProperty totalScore;
    public NumericProperties highScores;

    public PlayerStatistics ()
    {
        death = new NumericProperty("Deaths", 0, new int[] { 0, 1, 2, 3, 4 });
        colorChange = new NumericProperty("Color Changes", 0, new int[] { 5, 6, 7, 8 });
        plays = new NumericProperty("Levels Played", 0, new int[] { 9, 10, 11, 12 ,13 });
        successPlays = new NumericProperty("Successful Plays", 0, new int[] { 14, 15, 16, 17, 18 });
        perfectPlays = new NumericProperty("Perfect Plays", 0, new int[] { 19, 20, 21, 22, 23 });
        totalScore = new NumericProperty("Total Score", 0, new int[] { 24, 25, 26, 27, 28 });
        highScores = new NumericProperties("High Scores", new Dictionary<string, NumericProperty>(), new int[] { });
    }

    public void InitHighScore(string id)
    {
        highScores.InitValue(id);
    }

    public string GetString ()
    {
        return
            totalScore.GetString() +
            death.GetString() +
            colorChange.GetString() +
            plays.GetString() +
            successPlays.GetString() +
            perfectPlays.GetString() +
            highScores.GetString();
    }

    public void CheckAchievements ()
    {
        totalScore.NotifyAchievements(true);
        death.NotifyAchievements(true);
        colorChange.NotifyAchievements(true);
        plays.NotifyAchievements(true);
        successPlays.NotifyAchievements(true);
        perfectPlays.NotifyAchievements(true);
    }

    public void Reset()
    {
        death.SetValue(0f);
        colorChange.SetValue(0f);
        plays.SetValue(0f);
        successPlays.SetValue(0f);
        perfectPlays.SetValue(0f);
        totalScore.SetValue(0f);
        highScores.SetValue(0f);
    }

}

[Serializable]
public abstract class Property
{
    public string name = "";
    protected int[] achievementIndex;
   
    public void NotifyAchievements (bool silent = false)
    {
        for (int i = 0; i < achievementIndex.Length; i++)
        {
            if(AchievementManager.instance)
            {
                AchievementManager.instance.CheckAchievement(achievementIndex[i], silent);
            }
        }
    }

    public abstract string GetString();

}

[Serializable]
public class NumericProperty : Property {
    public float value;

    public NumericProperty(string name, int value, int[] achievements)
    {
        this.value = value;
        this.name = name;
        this.achievementIndex = achievements;
        foreach (int index in achievements)
        {
            if (AchievementManager.instance)
            {
                AchievementManager.instance.RegisterProperty(index, this);
            }
        }
    }

    public void Increment()
    {
        value += 1;
        NotifyAchievements();
    }

    public void SetValue(float value)
    {
        this.value = value;
    }

    public override string GetString()
    {
        return name + ": " + value + "\n";
    }
}

[Serializable]
public class NumericProperties : Property
{
    private Dictionary<string, NumericProperty> value;

    public NumericProperties(string name, Dictionary<string, NumericProperty> properties, int[] achievements)
    {
        this.name = name;
        this.achievementIndex = achievements;
        this.value = properties;
    }

    public override string GetString()
    {
        string result = "";
        foreach (String prop in value.Keys)
        {
            result += value[prop].GetString();
        }
        return result;
    }

    public void SetValue(string id, float newValue)
    {
        value[id].SetValue(newValue);
    }

    public void SetValue(float newValue)
    {
        foreach(string id in value.Keys)
        {
            SetValue(id, newValue);
        }
    }

    public void SetValues(Dictionary<string, NumericProperty> values)
    {
        value = values;
    }

    public NumericProperty GetValue(string id)
    {
        return value[id];
    }

    public void Increment(string id)
    {
        value[id].Increment();
    }

    internal void InitValue(String id)
    {
        if (!value.ContainsKey(id))
        {
            // TODO : Handle this with a expandable list and without hard coded "High Score" here
            value.Add(id, new NumericProperty("High Score - " + id , 0, new int[] { }));
        }
    }

}
