using BNG;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using System.Linq;

public class Game : MonoBehaviour
{
    public bool startGame = false;
    public bool inActivatedArea = false;
    private GameObject[] merds;
    [field: SerializeField]
    private int time = 60; // change based on mode
    private float gameTimeLeft;
    private TextMeshProUGUI endScoreText;
    private TextMeshProUGUI timeLeft;
    private TextMeshProUGUI currentScore;
    private TextMeshProUGUI deadFishText;
    private TextMeshProUGUI foodWasteText;
    private UnityEngine.UI.Slider foodWasteSlider;
    Scoring scoring;
    Modes modes; 
    Mode mode;


    // Start is called before the first frame update
    void Start()
    {
        merds = GameObject.FindGameObjectsWithTag("Fish System");
        scoring = FindObjectOfType<Scoring>();
        endScoreText = GameObject.FindGameObjectWithTag("ScoreText").GetComponent<TextMeshProUGUI>();
        GameObject canvas = GameObject.FindGameObjectWithTag("MonitorMerdCanvas");
        timeLeft = canvas.transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>();
        currentScore = canvas.transform.GetChild(1).gameObject.GetComponent<TextMeshProUGUI>();
        deadFishText = canvas.transform.GetChild(2).gameObject.GetComponent<TextMeshProUGUI>();
        foodWasteText = canvas.transform.GetChild(3).gameObject.GetComponent<TextMeshProUGUI>();
        foodWasteSlider = canvas.transform.GetChild(4).gameObject.GetComponent<UnityEngine.UI.Slider>();
        modes = FindObjectOfType<Modes>();
    }

    /* Update is called once per frame. If the key 'g' is pressed or the A button on the controller is pressed and the
     * game hasn't started, start the game and the coroutine Timer and start scoring. */
    void Update()
    {
        /*if (!modes.modesList.Any()) // modes aren't loaded yet
        {
            modes = FindObjectOfType<Modes>(); // reload modes (unnecessary?)
            return; // wait 'till modes are loaded
        }*/
        if ((Input.GetKeyDown(KeyCode.M) || InputBridge.Instance.RightTriggerUp) && !startGame && inActivatedArea)
        {
            modes.ChangeToNextMode();
            mode = modes.mode;
            time = mode.timeLimit;
        }
        if ((Input.GetKeyDown(KeyCode.N) || InputBridge.Instance.LeftTriggerUp) && !startGame && inActivatedArea)
        {
            modes.ChangeToPreviousMode();
            mode = modes.mode;
            time = mode.timeLimit;
        }

        if ((Input.GetKeyDown(KeyCode.G) || InputBridge.Instance.AButtonUp) && !startGame && inActivatedArea)
        {
            if (mode.tutorial.Equals(Tut.NO)) // Disable all tutorials
            {
                foreach (GameObject tut in holders)
                {
                    // Debug.Log("disabled" + tut);
                    tut.SetActive(false);
                }
            }

            startGame = true;
            foreach (GameObject merd in merds)
            {
                FishSystemScript merdScript = merd.GetComponent<FishSystemScript>();
                merdScript.modifier = mode.modifier; // Set modifier on timetohungry etc based on mode difficulty
                merdScript.ReleaseIdle();   // change all fish systems' states from Idle to Full
            }
            Debug.Log("Started the game");
            scoring.StartScoring();
            StartCoroutine(Timer());
        }
        if (startGame)
        {
            UpdateScreenStats();
        }
    }

    /* Starts a timer and gives the score after a certain amount of time. */
    IEnumerator Timer()
    {
        if (time == -1) yield return null; // return if game mode has endless time limit
        for (gameTimeLeft = time; gameTimeLeft > 0; gameTimeLeft -= Time.deltaTime)
        {
            yield return null;
        }
        scoring.StopScoring();
        startGame = false;
        foreach (GameObject merd in merds)
        {
            FishSystemScript merdScript = merd.GetComponent<FishSystemScript>();
            merdScript.SetIdle();
        }
        endScoreText.text = "YOUR SCORE:\n" + scoring.Score;
        Debug.Log("End of game");
        Debug.Log("Score: " + scoring.Score);
    }

    /* Updates the timer, score, food waste and the amount of dead fish on the merd screen. */
    public void UpdateScreenStats()
    {
        if (time != -1)
        {
            timeLeft.text = "Time left: " + Mathf.FloorToInt(gameTimeLeft / 60) + ":" +
                Mathf.FloorToInt(gameTimeLeft % 60).ToString("00");
        } else {
            timeLeft.text = "";
        }

        currentScore.text = "Score: " + scoring.Score;
        foodWasteText.text = "Food wastage: " + scoring.FoodWasted + " / Sec.";
        foodWasteSlider.value = scoring.FoodWastedPercentage;
        deadFishText.text = "Dead fish: " + scoring.DeadFish;
    }

}
