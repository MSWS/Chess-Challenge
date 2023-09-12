using System;
using System.Linq;
using ChessChallenge.API;

public class EvilBot : IChessBot
{
    int[] pieceValues = { 0, 100, 300, 300, 500, 900, 10000 };

    public Move Think(Board board, Timer timer)
    {
        return board.GetLegalMoves().MaxBy(move => -ScoreMove(board, move, 3, -99999, 99999));
    }

    public double ScoreMove(Board board, Move move, int depth, double alpha, double beta)
    {
        board.MakeMove(move);

        if (depth == 0 || board.GetLegalMoves().Count() == 0)
        {
            alpha = Score(board);
            board.UndoMove(move);
            return alpha;
        }

        foreach (var nextMove in board.GetLegalMoves())
        {
            var eval = -ScoreMove(board, nextMove, depth - 1, -beta, -alpha);
            if (eval >= beta)
            {
                alpha = beta;
                break;
            }

            alpha = Math.Max(eval, alpha);
        }

        board.UndoMove(move);

        return alpha;
    }

    public double Score(Board board)
    {
        if (board.IsInCheckmate())
        {
            return -9999;
        }

        if (board.IsDraw())
        {
            return 0;
        }

        double score = PieceScores(board, board.IsWhiteToMove) - PieceScores(board, !board.IsWhiteToMove);

        // score += board.GetLegalMoves().Length / 50.0;
        // if (board.TrySkipTurn())
        // {
        //     score -= board.GetLegalMoves().Length;
        //     board.UndoSkipTurn();
        // }

        return score;
    }

    public int PieceScores(Board board, bool white)
    {
        return new PieceType[] { PieceType.Pawn, PieceType.Knight, PieceType.Bishop, PieceType.Rook, PieceType.Queen }
          .Sum(type => board.GetPieceList(type, white).Count * pieceValues[(int)type]);
    }
}