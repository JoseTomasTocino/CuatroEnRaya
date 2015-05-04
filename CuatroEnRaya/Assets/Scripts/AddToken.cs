using UnityEngine;
using System.Collections;
using Assets;


public class AddToken : MonoBehaviour {

    GameObject redTokenPrefab, yellowTokenPrefab;

    Board board;

    Board.Player turn;

    BoardSolverThread solverThread;

	// Use this for initialization
	void Start () 
    {
        board = new Board();

        redTokenPrefab = (GameObject)Resources.Load("RedToken", typeof(GameObject));
        yellowTokenPrefab = (GameObject)Resources.Load("YellowToken", typeof(GameObject));

        turn = Board.Player.Red;

        StatusLabelManager.statusText = "It's your turn!";
	}
	
	// Update is called once per frame
	void Update () 
    {
        if (Input.GetKey("escape"))
            QuitGame();

        // Launch a ray and get the column is mouse is pointing to
        int columnNumber = -1;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 100) && hit.collider.tag == "Column")
        {
            columnNumber = hit.transform.GetComponent<Column>().columnNumber;            

            var arrowObject = GameObject.Find("Flecha");
            var previousPosition = arrowObject.transform.position;
            previousPosition.x = -0.1373f + columnNumber * 0.045f;
            arrowObject.transform.position = previousPosition;


            // Place a token if the mouse was pressed
            if (Input.GetMouseButtonDown(0) && turn == Board.Player.Red && !board.gameFinished)
            {
                // Switch turn
                turn = turn == Board.Player.Red ? Board.Player.Yellow : Board.Player.Red;
                
                // Instantiate the token
                Instantiate(redTokenPrefab, new Vector3(-0.1373f + columnNumber * 0.045f, 0.35f, 0.004f), Quaternion.Euler(-90, 0, 0));

                // Place the token in the board
                board.placeToken(columnNumber, Board.Player.Red);

                // Check if it's a winner movement
                if (board.getWinner() == Board.Player.Red)
                {
                    StatusLabelManager.statusText = "Congratulations! You've won!";
                }

                // Check if there are remaining movements
                else if (board.gameFinished)
                {
                    StatusLabelManager.statusText = "Oops! That's a draw!";
                }

                // There are remaining movements, the game goes on!
                else
                {
                    StatusLabelManager.statusText = "The computer is thinking...";

                    // Get movement for CPU
                    solverThread = new BoardSolverThread();
                    solverThread.board = board;
                    solverThread.currentPlayer = Board.Player.Yellow;
                    solverThread.maxDepth = 0;
                    solverThread.Start();                
                }

            }
        }

        if (solverThread != null)
        {
            if (solverThread.Update())
            {
                var movement = solverThread.bestMovement;

                // Release the threaded job class
                solverThread = null;

                // Play that movement in the board
                board.placeToken(movement, Board.Player.Yellow);

                // Instantiate the token
                Instantiate(yellowTokenPrefab, new Vector3(-0.1373f + movement * 0.045f, 0.35f, 0.004f), Quaternion.Euler(-90, 0, 0));

                // Check if it's a winner movement
                if (board.getWinner() == Board.Player.Yellow)
                {
                    StatusLabelManager.statusText = "Ow! The computer has won!";
                }

                // Check if there are remaining movements
                else if (board.gameFinished)
                {
                    StatusLabelManager.statusText = "Oops! That's a draw!";
                }

                else
                {
                    // Switch turn
                    turn = turn == Board.Player.Red ? Board.Player.Yellow : Board.Player.Red;

                    StatusLabelManager.statusText = "It's your turn!";
                }
            }
        }
    }

    public void QuitGame ()
    {
        Application.Quit();
    }

    public void ResetGame ()
    {
        // Reset the board
        board.reset();

        // Reset the turn
        turn = Board.Player.Red;

        // Reset the label
        StatusLabelManager.statusText = "It's your turn!";

        // Remove (if exists) the background AI thread
        if (solverThread != null)
        {
            solverThread.Abort();
            solverThread = null;
        }

        // Remove all tokens
        var tokens = GameObject.FindGameObjectsWithTag("Token");

        foreach (var token in tokens)
        {
            Object.Destroy(token);
        }
    }
}

