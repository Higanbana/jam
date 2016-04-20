using UnityEngine;
using System.Collections;

public class AchievementManager : MonoBehaviour {
    public GameObject achievementPopup;
    private float height;
    public static AchievementManager instance;

    // Use this for initialization
    void Start () {

        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }

        height = achievementPopup.GetComponent<RectTransform>().rect.height/2;
        Vector3 temp = achievementPopup.transform.position;
        temp.y = - height;
        achievementPopup.transform.position = temp;
    }
	
	// Update is called once per frame
	void Update () {
	
	}

    IEnumerator PopAchievement()
    {
        while (achievementPopup.transform.position.y < height - 1 )
        {
            achievementPopup.transform.position = achievementPopup.transform.position + new Vector3(0, 2, 0);
            yield return null;
        }

        yield return new WaitForSeconds(1.5f);

        while (achievementPopup.transform.position.y > -height )
        {
            achievementPopup.transform.position = achievementPopup.transform.position - new Vector3(0, 2, 0);
            yield return null;
        }

    }


    public void EarnAchievement()
    {
        StartCoroutine(PopAchievement());
       
    }


}
