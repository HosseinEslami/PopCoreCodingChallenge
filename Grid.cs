using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class Grid : MonoBehaviour {

	public int ROWS = 20;

	public int COLUMNS = 14;

    public int minmumLineNumb = 16;
    public int lowestLineNumb = 3;
    public float moveUp = 7f;

	public float TILE_SIZE = 0.68f;

	public float GRID_SPEED = 0.1f;

	public float changeTypeRate = 0.5f;

	public int emptyLines = 16;

	public GameObject gridBallGO;

    public bool scrollAndAddLines = false;
    public bool scrollUp = false;
    public bool moveBalls = false;

    Ball highetMatchBall = null;

	public Vector2 movetoPosition = new Vector2();


	public List<GameObject> activeBalls = new List<GameObject>();

	[HideInInspector]
	public float GRID_OFFSET_X = 0;


	[HideInInspector]
	public List<List<Ball>> gridBalls;

	private List<Ball> matchList;

	private List<Ball.BALL_TYPE> typePool;

	private Ball.BALL_TYPE lastType;

	private int bullets = 0;

	void Start () {

		matchList = new List<Ball> ();
		lastType = (Ball.BALL_TYPE)Random.Range (0, 7);
		typePool = new List<Ball.BALL_TYPE> ();

		var i = 0;
		var total = 100000;
		while (i < total) {
			typePool.Add (GetBallType ());
			i++;
		}

		Shuffle(typePool);

		BuildGrid ();

	}


	void BuildGrid ()	{
		gridBalls = new List<List<Ball>> ();

		GRID_OFFSET_X = (COLUMNS * TILE_SIZE) * 0.5f;
		GRID_OFFSET_X -= TILE_SIZE * 0.5f;


		for (int row = 0; row < ROWS; row++) {

			var rowBalls = new List<Ball>();

			for (int column = 0; column < COLUMNS; column++) 
            {
                var item = Instantiate (gridBallGO) as GameObject;
				var ball = item.GetComponent<Ball>();

				ball.SetBallPosition(this, column, row);
				ball.SetType (typePool [0]);
				typePool.RemoveAt (0);

				ball.transform.parent = gameObject.transform;
				rowBalls.Add (ball);

				if (gridBalls.Count < emptyLines ) 
                {
					ball.gameObject.SetActive (false);
				}
			}
				
			gridBalls.Add(rowBalls);
		}

		var p = transform.position;
		p.y -= 4.7f;
		transform.position = p;

	}

	void AddLine () 
	{
		//Debug.Log("ADDD");
		ROWS++;

		var rowBalls = new List<Ball>();

		for (int column = 0; column < COLUMNS; column++) {

			var item = Instantiate (gridBallGO) as GameObject;

			var ball = item.GetComponent<Ball>();
			ball.transform.parent = gameObject.transform;
			ball.SetBallPosition(this, column, gridBalls.Count-1);
			ball.SetType (typePool [0]);
			ball.connected = true;

			typePool.RemoveAt (0);

			rowBalls.Add (ball);
		}
		gridBalls.Add(rowBalls);
        scrollAndAddLines = false;
        minmumLineNumb++;
        lowestLineNumb++;
    }

    void RemoveLine()
    {
   //     var rowBalls = new List<Ball>();

   //     for (int column = 0; column < COLUMNS; column++)
   //     {
   //         var ball = gridBalls[ROWS-1][column].GetComponent<Ball>();
   //         //Debug.Log(ball.row + " " + ball.column);
   //         rowBalls.Add(ball);

			////Destroy(ball.gameObject);
   //     }

		//gridBalls.Remove(rowBalls);
		MoveAllRowsUp();
        //scrollAndAddLines = false;
        //minmumLineNumb++;
        //lowestLineNumb++;
    }

    void MoveAllRowsUp()
    {
        for (int row = ROWS-1; row > 0; row--)
        {
            var rowBalls = new List<Ball>();

            for (int column = 0; column < COLUMNS; column++)
            {
                //var ball = gridBalls[row][column].GetComponent<Ball>();
                gridBalls[row][column].SetType(gridBalls[row-1][column].type);
                gridBalls[row][column].gameObject.SetActive(gridBalls[row-1][column].gameObject.activeInHierarchy);
                //ball.gameObject.SetActive(false);
            }
        }
    }

    public void AddBall (Ball collisionBall, Bullet bullet) 
	{

		var neighbors = BallEmptyNeighbors(collisionBall);
		var minDistance = 10000.0f;
		Ball minBall = null;
		foreach (var n in neighbors) 
        {
			var d = Vector2.Distance (n.transform.position, bullet.transform.position);
			if ( d < minDistance ) 
            {
				minDistance = d;
				minBall = n;
			}
		}
		bullet.gameObject.SetActive (false);

		if(minBall != null)
        {
            minBall.SetType(bullet.type);
            minBall.gameObject.SetActive(true);

            CheckMatchesForBall(minBall);
        }
	}

	public void CheckMatchesForBall (Ball ball) 
    {
        matchList.Clear ();

		foreach (var r in gridBalls) 
		{
			foreach (var b in r) 
            {
				b.visited = false;
			}
		}

        //search for matches around ball
		var initialResult = GetMatches( ball );
		matchList.AddRange (initialResult);

		while (true) 
		{	
			var allVisited = true;
			for (var i = matchList.Count - 1; i >= 0 ; i--) 
            {
				var b = matchList [i];
				if (!b.visited) {
					AddMatches (GetMatches (b));
					allVisited = false;
				}
			}

			if (allVisited) 
            {
                if ((int) ball.type == 9)
                {
                    ball.ShowSmoke();
                    foreach (var b in matchList)
                    {
						b.gameObject.SetActive(false);
						GameController.gameController.AddScore(1, b.type);
					}
                }
                if (matchList.Count > 1)
                {
                    int highestMatchRow = ball.row;
                    
					foreach (var b in matchList)
                    {
						//Debug.Log(b.row + " " + b.column);
                        if (b.row > highestMatchRow)
                        {
                            highestMatchRow = b.row;
							highetMatchBall = b;
                        }
                        else if(b.row == highestMatchRow && b.column % 2 != 0)
                        {
							highestMatchRow = b.row;
                            highetMatchBall = b;
						}
                    }

					foreach (var b in matchList)
                    {
                        if (highetMatchBall != null && b == highetMatchBall)
                        {
                            int newType = (int)b.type + (matchList.Count - 1);
                            if (newType >= 10) newType = 9;
                            b.SetType((Ball.BALL_TYPE)newType);
                            highetMatchBall = b;
                        }
                        else
                        {
                            //movetoPosition = highetMatchBall.transform.position;
                            b.gameObject.SetActive(false);
                            GameController.gameController.AddScore(1, b.type);
						}
                            
					}
                    //moveBalls = true;

					CheckForDisconnected ();

					//remove disconnected balls
					var i = gridBalls.Count - 1;
					while (i >= 0) 
					{
						foreach (var b in gridBalls[i]) 
                        {
							if (!b.connected)// && matchList.Contains(b))
                            {
                                b.gameObject.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Dynamic;
                                //b.gameObject.SetActive (false);
                            }
						}
						i--;
					}

                    if(highetMatchBall != null) CheckMatchesForBall(highetMatchBall);
				}

                FindActiveBalls();
				return;
			}
		}
	}

	void CheckForDisconnected () {
		//set all balls as disconnected
		foreach (var r in gridBalls) 
        {
			foreach (var b in r) {
				b.connected = false;
			}
		}
		//connect visible balls in last row 
		foreach (var b in gridBalls[ROWS-1]) {
			if (b.gameObject.activeSelf)
				b.connected = true;
		}


		//now set connect property on the rest of the balls
		var i = ROWS-1;
		while (i >= 0) {
			foreach (var b in gridBalls[i]) {
				if (b.gameObject.activeSelf) {
					var neighbors = BallActiveNeighbors (b);
					var connected = false;

					foreach (var n in neighbors) {
						if (n.connected) {
							connected = true;
							break;
						}
					}

					if (connected) {
						b.connected = true;
						foreach (var n in neighbors) {
							if (n.gameObject.activeSelf) {
								n.connected = true;
							}
						}
					} 
				}
			}
			i--;
		}
	}

	List<Ball> GetMatches (Ball ball) {
		ball.visited = true;
		var result = new List<Ball> () { ball };
		var n = BallActiveNeighbors (ball);

		foreach (var b in n) 
		{
			if (b.type == ball.type) {
				result.Add (b);
			}
		}

		return result;
	}

	void AddMatches (List<Ball> matches) {
		foreach (var b in matches) {
			if (!matchList.Contains (b))
				matchList.Add (b);
		}
	}

	Ball.BALL_TYPE GetBallType () {
		var random = Random.Range (0.0f, 1.0f);
		if (random > changeTypeRate) {
			lastType = (Ball.BALL_TYPE)Random.Range (0, 7);
		}
		return lastType;
	}

	List<Ball> BallEmptyNeighbors (Ball ball) {
		var result = new List<Ball> ();
		if (ball.column + 1 < COLUMNS) {
			if (!gridBalls [ball.row] [ball.column + 1].gameObject.activeSelf)
				result.Add (gridBalls [ball.row] [ball.column + 1]);
		}

		//left
		if (ball.column - 1 >= 0) {
			if (!gridBalls [ball.row] [ball.column - 1].gameObject.activeSelf)
				result.Add (gridBalls [ball.row] [ball.column - 1]);
		}
		//top
		if (ball.row - 1 >= 0) {
			if (!gridBalls [ball.row - 1] [ball.column].gameObject.activeSelf)
				result.Add (gridBalls [ball.row - 1] [ball.column]);
		}

		//bottom
		if (ball.row + 1 < gridBalls.Count) {
			if (!gridBalls [ball.row + 1] [ball.column].gameObject.activeSelf)
				result.Add (gridBalls [ball.row + 1] [ball.column]);
		}

		if (ball.column % 2 == 0) {

			//top-left
			if (ball.row - 1 >= 0 && ball.column - 1 >= 0) {
				if (!gridBalls [ball.row - 1] [ball.column - 1].gameObject.activeSelf)
					result.Add (gridBalls [ball.row - 1] [ball.column - 1]);
			}

			//top-right
			if (ball.row - 1 >= 0 && ball.column + 1 < COLUMNS) {
				if (!gridBalls [ball.row - 1] [ball.column + 1].gameObject.activeSelf)
					result.Add (gridBalls [ball.row - 1] [ball.column + 1]);
			}
		} else {
			//bottom-left
			if (ball.row + 1 < gridBalls.Count && ball.column - 1 >= 0) {
				if (!gridBalls [ball.row + 1] [ball.column - 1].gameObject.activeSelf)
					result.Add (gridBalls [ball.row + 1] [ball.column - 1]);
			}

			//bottom-right
			if (ball.row + 1 < gridBalls.Count && ball.column + 1 < COLUMNS) {
				if (!gridBalls [ball.row + 1] [ball.column + 1].gameObject.activeSelf)
					result.Add (gridBalls [ball.row + 1] [ball.column + 1]);
			}

		}


		return result;
	}

	List<Ball> BallActiveNeighbors (Ball ball) {
		
		var result = new List<Ball> ();
		//right
		if (ball.column + 1 < COLUMNS) {
			if (gridBalls [ball.row] [ball.column + 1].gameObject.activeSelf)
				result.Add (gridBalls [ball.row] [ball.column + 1]);
		}

		//left
		if (ball.column - 1 >= 0) {
			if (gridBalls [ball.row] [ball.column - 1].gameObject.activeSelf)
				result.Add (gridBalls [ball.row] [ball.column - 1]);
		}
		//bottom
		if (ball.row - 1 >= 0) {
			if (gridBalls [ball.row - 1] [ball.column].gameObject.activeSelf)
				result.Add (gridBalls [ball.row - 1] [ball.column]);
		}

		//top
		if (ball.row + 1 < gridBalls.Count) {
			if (gridBalls [ball.row + 1] [ball.column].gameObject.activeSelf)
				result.Add (gridBalls [ball.row + 1] [ball.column]);
		}


		if (ball.column % 2 == 0) {

			//top-left
			if (ball.row - 1 >= 0 && ball.column - 1 >= 0) {
				if (gridBalls [ball.row - 1] [ball.column - 1].gameObject.activeSelf)
					result.Add (gridBalls [ball.row - 1] [ball.column - 1]);
			}

			//top-right
			if (ball.row - 1 >= 0 && ball.column + 1 < COLUMNS) {
				if (gridBalls [ball.row - 1] [ball.column + 1].gameObject.activeSelf)
					result.Add (gridBalls [ball.row - 1] [ball.column + 1]);
			}
		} else {
			//bottom-left
			if (ball.row + 1 < gridBalls.Count && ball.column - 1 >= 0) {
				if (gridBalls [ball.row + 1] [ball.column - 1].gameObject.activeSelf)
					result.Add (gridBalls [ball.row + 1] [ball.column - 1]);
			}

			//bottom-right
			if (ball.row + 1 < gridBalls.Count && ball.column + 1 < COLUMNS) {
				if (gridBalls [ball.row + 1] [ball.column + 1].gameObject.activeSelf)
					result.Add (gridBalls [ball.row + 1] [ball.column + 1]);
			}

		}

		return result;
	}

	public Ball BallCloseToPoint (Vector2 point)
	{
		
		point.y -= transform.position.y;

		int c = Mathf.FloorToInt ((point.x + GRID_OFFSET_X + ( TILE_SIZE * 0.5f )) / TILE_SIZE);
		if (c < 0)
			c = 0;
		if (c >= COLUMNS)
			c = COLUMNS - 1;

		int r =  Mathf.FloorToInt (( ( TILE_SIZE * 0.5f ) + point.y )/  TILE_SIZE);
		if (r < 0) r = 0;
		if (r >= gridBalls.Count) r = gridBalls.Count - 1;



		return gridBalls [r] [c];

	}

	private void FindActiveBalls()
	{
		//Debug.Log("FindActiveBalls");
		activeBalls.Clear();
		GameObject[] tmpActive = GameObject.FindGameObjectsWithTag("ball");
		for (int i = 0; i < tmpActive.Length; i++)
		{
			activeBalls.Add(tmpActive[i]);
		}

        int lowestFilledRow = minmumLineNumb;
        for (int i = 1; i < activeBalls.Count; i++)
        {
            Ball lastBall = activeBalls[i].GetComponent<Ball>();
			if (lastBall.row < lowestFilledRow) lowestFilledRow = lastBall.row;
		}

        if (lowestFilledRow >= 10)
        {
			GameController.gameController.popUpFader.SetActive(true);
            GameController.gameController.BlockTouch(true);

            GameController.gameController.popUpFader.GetComponentInChildren<TMP_Text>().text = "PERFECT!! ";
        }

		//Debug.Log(lowestFilledRow + " " + minmumLineNumb);
		scrollAndAddLines = lowestFilledRow >= minmumLineNumb;


        if(lowestFilledRow <= lowestLineNumb) MoveAllRowsUp();
	}

	void Update()
    {
        if (Input.GetKeyDown("space"))
        {
            RemoveLine();
        }

        if (scrollAndAddLines)
        {
            //Debug.Log("ScrollAndAddLine");

		    var p = transform.position;
		    p.y -= Time.deltaTime * GRID_SPEED;
		    transform.position = p;

			//Debug.Log(gridBalls[gridBalls.Count - 1][0].transform.position.y + " " + gridBalls[gridBalls.Count - 1][0].row);

            if (gridBalls [gridBalls.Count - 1] [0].transform.position.y < 5 ) 
            {
                //add new line
			    AddLine();
                //FindActiveBalls();
            }
        }
		//else if (scrollUp)
		//{
		//	var p = transform.position;
		//	p.y += Time.deltaTime * GRID_SPEED;
		//	transform.position = p;

  //          if (gridBalls[gridBalls.Count - 1][0].transform.position.y > moveUp)
  //          {
  //              MoveAllRowsUp();
		//		scrollUp = false;
  //              moveUp += 0.5f;
  //              lowestLineNumb--;
  //              minmumLineNumb--;
  //          }
		//}

	}

	private static System.Random rng = new System.Random(); 
	public static void Shuffle<T>(IList<T> list)  {  
		int n = list.Count;  
		while (n > 1) {  
			n--;  
			int k = rng.Next(n + 1);  
			T value = list[k];  
			list[k] = list[n];  
			list[n] = value;  
		}  
	}

}
