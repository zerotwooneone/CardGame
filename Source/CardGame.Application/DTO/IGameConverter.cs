using CardGame.Domain.Abstractions.Game;

namespace CardGame.Application.DTO
{
    public interface IGameConverter
    {
        CommonKnowledgeGame ConvertToCommonKnowledgeGame(GameDao gameDao);
    }
}