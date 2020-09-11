using CardGame.Utils.Factory;
using CardGame.Utils.Value;

namespace CardGame.Domain.Game
{
    public class Score : StructValue<int>
    {
        protected Score(int value) : base(value)
        {
        }

        public static FactoryResult<Score> Factory(int score = 0)
        {
            const int MaxScore = 7;
            if (score < 0 || score > MaxScore)
            {
                return FactoryResult<Score>.Error($"Score cannot be less than zero or greater than {MaxScore}");
            }
            return FactoryResult<Score>.Success(new Score(score));
        }
    }
}