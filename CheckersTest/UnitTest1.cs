using CheckersLib;
using static CheckersLib.Constants;

namespace CheckersTest
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void Test1()
        {
            var pieces = new List<CheckersPiece>();
            for (var i = 0; i < BoardParameters * BoardParameters; i++)
            {
                pieces.Add(new EmptyPiece(i));
            }
            pieces[2] = new CheckersPiece(2, PieceColors.Black);
            pieces[9] = new CheckersPiece(9, PieceColors.Red);
            pieces[16] = new CheckersPiece(16, PieceColors.Black);

            var moves = pieces[2].GetValidMoves(pieces, false);
            Assert.That(moves, Has.Count.EqualTo(1));
        }
    }
}