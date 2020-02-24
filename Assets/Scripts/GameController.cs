using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TMPro;

public class GameController : MonoBehaviour
{
    public static GameController gameController;

    public int score = 0;
    public int lvl = 1;
    public int sliderVal = 0;

    public TMP_Text scoreText, currentLvlTxt, NextLvlText;
    public Slider scoreSlider;
    public GameObject popUpFader;
    public bool blockTouch = true;

	public RayCastShooter[] shooters;

	private RayCastShooter selectedShooter;

	private bool mouseDown = false;


    void Awake()
    {
        gameController = this.GetComponent<GameController>();

        if(PlayerPrefs.HasKey("Score"))
        {
            score = PlayerPrefs.GetInt("Score");
            lvl = PlayerPrefs.GetInt("Lvl");
            sliderVal = PlayerPrefs.GetInt("SliderVal");
            UpdateUI();
        }
    }

    public void BlockTouch(bool blockVal)
    {
        blockTouch = blockVal;
    }

    public void AddScore(int count, Ball.BALL_TYPE type)
    {
        int typeScore = 0;

        switch (type)
        {
            case Ball.BALL_TYPE.TYPE_1:
                typeScore = 2;
                break;
            case Ball.BALL_TYPE.TYPE_2:
                typeScore = 4;
                break;
            case Ball.BALL_TYPE.TYPE_3:
                typeScore = 8;
                break;
            case Ball.BALL_TYPE.TYPE_4:
                typeScore = 16;
                break;
            case Ball.BALL_TYPE.TYPE_5:
                typeScore = 32;
                break;
            case Ball.BALL_TYPE.TYPE_6:
                typeScore = 64;
                break;
            case Ball.BALL_TYPE.TYPE_7:
                typeScore = 128;
                break;
            case Ball.BALL_TYPE.TYPE_8:
                typeScore = 256;
                break;
            case Ball.BALL_TYPE.TYPE_9:
                typeScore = 512;
                break;
            case Ball.BALL_TYPE.TYPE_10:
                typeScore = 1024;
                break;
        }

        score += count * typeScore;

        sliderVal = score / (lvl * 10);
        //Debug.Log(sliderVal);
        if (sliderVal >= 100)
        {
            sliderVal = 0;
            lvl++;
            popUpFader.gameObject.SetActive(true);
            popUpFader.GetComponentInChildren<TMP_Text>().text = "You Reached Level " + lvl;
            BlockTouch(true);
        }

        UpdateUI();

        PlayerPrefs.SetInt("Score", score);
        PlayerPrefs.SetInt("Lvl", lvl);
        PlayerPrefs.SetInt("SliderVal", sliderVal);

    }

    void UpdateUI()
    {
        scoreText.text = score.ToString();
        currentLvlTxt.text = lvl.ToString();
        NextLvlText.text = (lvl +1).ToString();
        scoreSlider.value = sliderVal;
    }

    // Update is called once per frame
	void Update () {
		if (Input.touches.Length > 0) {

			Touch touch = Input.touches [0];

			if (touch.phase == TouchPhase.Began) {
				TouchDown (touch.position);
			} else if (touch.phase == TouchPhase.Canceled || touch.phase == TouchPhase.Ended) {
				TouchUp (Input.mousePosition);
			} else if (touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary) {
				TouchMove (touch.position);
			}
			TouchMove (touch.position);	
			return;
		} else if (Input.GetMouseButtonDown (0)) {
			mouseDown = true;
			TouchDown (Input.mousePosition);
		} else if (Input.GetMouseButtonUp (0)) {
			mouseDown = false;
			TouchUp (Input.mousePosition);
		} else if (mouseDown) {
			TouchMove (Input.mousePosition);
		}
	}


	void TouchDown (Vector2 touch) 
    {
        if (!GameController.gameController.blockTouch)
        {
            selectedShooter = null;
            Vector2 point = Camera.main.ScreenToWorldPoint(touch);

            //if (point.y < -1f) {
            var minDistance = 100000.0f;
            RayCastShooter shooter = null;

            //look for closest shooter
            foreach (var s in shooters)
            {
                var d = Vector2.Distance(point, s.transform.position);
                if (d < minDistance)
                {
                    minDistance = d;
                    shooter = s;
                }
            }

            selectedShooter = shooter;
            //}
        }


    }

	void TouchUp (Vector2 touch) 
    {
        if (!GameController.gameController.blockTouch)
        {
            if (selectedShooter == null)
                return;
            Vector2 point = Camera.main.ScreenToWorldPoint(touch);
            if (Vector2.Distance(point, selectedShooter.transform.position) < 0.2f)
            {
                selectedShooter.ClearShotPath();
            }
            else
            {
                selectedShooter.HandleTouchUp(touch);
            }
        }
	}

	void TouchMove (Vector2 touch) 
    {
        if (!GameController.gameController.blockTouch)
        {
            if (selectedShooter == null)
                return;
            selectedShooter.HandleTouchMove(touch);
        }
	}
}
