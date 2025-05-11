using CardGame.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CardGame.Application.Common.Interfaces;

public interface IDeckRepository
{
    Task<IEnumerable<CardAsset>> GetDeckByIdAsync(Guid deckId);
}
