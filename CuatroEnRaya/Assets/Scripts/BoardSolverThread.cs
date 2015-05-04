using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets
{
    class BoardSolverThread : ThreadedJob
    {
        public Board board;
        public Board.Player currentPlayer;
        public int maxDepth;

        public int bestMovement;

        protected override void ThreadFunction ()
        {
            bestMovement = board.getBestMovement(currentPlayer, maxDepth).Item2;
            System.Threading.Thread.Sleep(500);
        }        
    }
}
