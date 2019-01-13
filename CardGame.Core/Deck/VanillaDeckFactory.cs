using System;
using System.Collections.Generic;
using CardGame.Core.Card;

namespace CardGame.Core.Deck
{
    public class VanillaDeckFactory : IDeckFactory
    {
        private static readonly Guid PrincessGuid = Guid.Parse("e9aaf1fd-23ae-4f44-ab22-b4881c438467");
        private static readonly Guid CountessGuid = Guid.Parse("02cf99e9-12e4-4764-ae7f-0d6de4248d97");
        private static readonly Guid KingGuid = Guid.Parse("de733695-f4c4-4967-bd5c-9615e3f0f920");
        private static readonly IEnumerable<Guid> PrinceGuids = new []{ Guid.Parse("cad25b4b-7635-4f06-abb6-46746e6dcffa"), Guid.Parse("c33a8685-27cf-4970-b722-b960c491135b") };
        private static readonly IEnumerable<Guid> HandmaidGuids = new []{ Guid.Parse("e88f186d-4c60-4eab-8036-eb6965d81748"), Guid.Parse("07fffc87-117a-45c9-bf85-42e84ea1a53d") };
        private static readonly IEnumerable<Guid> BaronGuids = new []{ Guid.Parse("b8ef5a88-78fa-42c7-9879-fb8950a81138"), Guid.Parse("cc4fc484-c4f2-4170-97b7-27a9ebf0206a") };
        private static readonly IEnumerable<Guid> PriestGuids = new []{ Guid.Parse("f62ce9c5-8c5d-4fb8-86b4-058b88315f30"), Guid.Parse("180ec8cb-84d6-4a86-83c0-dbc01ca7af6d") };

        private static readonly IEnumerable<Guid> GuardGuids = new []
        {
            Guid.Parse("5dfca648-cc4b-4a48-9cc1-040fc22935ae"), 
            Guid.Parse("35c065d9-a844-44a6-b2f8-57ef2298f26e"), 
            Guid.Parse("13127153-cc7e-48d3-8527-659e557b57d9"), 
            Guid.Parse("bf9f3eb6-16ab-4ba2-8e28-c7681884972b"), 
            Guid.Parse("315c4fc9-48ab-4074-bf77-8ebcd64087d8"),
        };

        public IEnumerable<Card.Card> Create()
        {
            return CreateDeck();
        }

        public static IEnumerable<Card.Card> CreateDeck()
        {
            yield return new Card.Card(PrincessGuid, CardValue.Princess, nameof(CardValue.Princess),
                nameof(CardValue.Princess));
            yield return new Card.Card(CountessGuid, CardValue.Countess, nameof(CardValue.Countess),
                nameof(CardValue.Countess));
            yield return new Card.Card(KingGuid, CardValue.King, nameof(CardValue.King), nameof(CardValue.King));
            foreach (var princeGuid in PrinceGuids)
                yield return new Card.Card(princeGuid, CardValue.Prince, nameof(CardValue.Prince),
                    nameof(CardValue.Prince));
            foreach (var handmaidGuid in HandmaidGuids)
                yield return new Card.Card(handmaidGuid, CardValue.Handmaid, nameof(CardValue.Handmaid),
                    nameof(CardValue.Handmaid));
            foreach (var baronGuid in BaronGuids)
                yield return new Card.Card(baronGuid, CardValue.Baron, nameof(CardValue.Baron),
                    nameof(CardValue.Baron));
            foreach (var priestGuid in PriestGuids)
                yield return new Card.Card(priestGuid, CardValue.Priest, nameof(CardValue.Priest),
                    nameof(CardValue.Priest));
            foreach (var guardGuid in GuardGuids)
                yield return new Card.Card(guardGuid, CardValue.Guard, nameof(CardValue.Guard),
                    nameof(CardValue.Guard));
        }
    }
}