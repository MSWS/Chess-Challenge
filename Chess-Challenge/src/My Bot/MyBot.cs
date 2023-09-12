using System;
using System.Collections.Generic;
using System.Linq;
using ChessChallenge.API;

public class MyBot : IChessBot
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

        score += board.GetLegalMoves().Length / 100.0;
        if (board.TrySkipTurn())
        {
            score -= board.GetLegalMoves().Length / 95.0;


            foreach (PieceType type in Enum.GetValues<PieceType>())
            {
                if (type == PieceType.None)
                    continue;
                PieceList list = board.GetPieceList(type, board.IsWhiteToMove);
                foreach (Piece piece in list)
                {
                    Move? protector = IsSpaceProtected(board, piece.Square);
                    score -= protector == null ? pieceValues[(int)piece.PieceType] : pieceValues[(int)protector.Value.MovePieceType];
                    // if (protector != null)
                    // {
                    //     score += pieceValues[(int)piece.PieceType] - pieceValues[(int)protector.Value.MovePieceType];
                    // }
                }
            }
            board.UndoSkipTurn();
        }

        return score;
    }

    public int PieceScores(Board board, bool white)
    {
        return new PieceType[] { PieceType.Pawn, PieceType.Knight, PieceType.Bishop, PieceType.Rook, PieceType.Queen }
          .Sum(type => board.GetPieceList(type, white).Count * pieceValues[(int)type]);
    }

    private Move? IsSpaceProtected(Board board, Square square)
    {
        List<Move> moves = new List<Move>(board.GetLegalMoves(true)).FindAll(m => m.TargetSquare == square);
        if (moves.Count == 0)
            return null;
        moves.Sort((a, b) => pieceValues[(int)a.MovePieceType].CompareTo(pieceValues[(int)b.MovePieceType]));
        return moves[0];
    }
}