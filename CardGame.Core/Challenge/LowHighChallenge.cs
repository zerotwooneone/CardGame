using System;
using System.Text;
using Automatonymous;
using MassTransit;

namespace CardGame.Core.Challenge
{
    public class LowHighChallenge : SagaStateMachineInstance
    {
        public Guid Target { get; }
        public Guid Requester { get; }
        private readonly Random _random;

        public LowHighChallenge(Guid requester, Guid target, Random random) : this(random)
        {
            Target = target;
            Requester = requester;
        }

        public LowHighChallenge(Guid requester, Guid target, Guid correlationId, Random random) : this(requester, target, random)
        {
            Target = target;
            Requester = requester;
            CorrelationId = correlationId;
        }

        private LowHighChallenge(Random random)
        {
            _random = random;
        }

        public Guid CorrelationId { get; set; }
        public byte[] EncryptedRequesterValue { get; private set; }
        public string CurrentState { get; set; }
        public int? RequesterValue { get; private set; }
        public int? TargetValue { get; private set; }
        public bool? TargetIsLowerThanRequester { get; private set; }
        public byte[] RequesterKey { get; private set; }
        public bool? Win { get; private set; }

        public void SetRequest(byte[] encrypted)
        {
            EncryptedRequesterValue = encrypted;
        }
        private int GetRandomInt()
        {
            return _random.Next(int.MinValue, int.MaxValue - 1);
        }

        public void InitRequest(Func<int, byte[]> encrypt, byte[] key)
        {
            RequesterValue = GetRandomInt();
            EncryptedRequesterValue = encrypt(RequesterValue.Value);
            RequesterKey = key;
        }

        public void InitResponse(byte[] encrypted)
        {
            TargetValue = GetRandomInt();
            EncryptedRequesterValue = encrypted;
            TargetIsLowerThanRequester = GetRandomBool();
        }

        private bool GetRandomBool()
        {
            var value = _random.NextDouble();
            return value > 0.5;
        }

        public void SetResponse(bool isLowerThan, int targetValue)
        {
            TargetIsLowerThanRequester = isLowerThan;
            TargetValue = targetValue;
            Win = !TargetWins();
        }

        public void SetResult(byte[] key, Func<byte[], byte[], int> decrypt)
        {
            RequesterValue = decrypt(key, EncryptedRequesterValue);
            Win = TargetWins();
        }

        private bool TargetWins()
        {
            return TargetIsLowerThanRequester.Value ? TargetValue <= RequesterValue : RequesterValue > TargetValue;
        }
    }

    public class LowHighChallengeFactory
    {
        private readonly Random _random;

        public LowHighChallengeFactory()
        {
            _random = new Random();
        }
        public LowHighChallenge CreateFromRequest(Guid requester, Guid target, Guid correlationId, byte[] encrypted)
        {
            var lowHighChallenge = new LowHighChallenge(requester, target, correlationId, _random);
            lowHighChallenge.InitResponse(encrypted);
            return lowHighChallenge;
        }
        public LowHighChallenge CreateRequest(Guid source, Guid target, IPublishEndpoint publishEndpoint)
        {
            var lowHighChallenge = new LowHighChallenge(source, target, new Random());
            lowHighChallenge.InitRequest(i => Encoding.UTF8.GetBytes(i.ToString()), new byte[] { });
            var correlationId = Guid.NewGuid();
            Console.WriteLine($"{nameof(correlationId)}:{correlationId}");
            publishEndpoint.Publish(new LowHighChallengeRequestEvent(
                lowHighChallenge.EncryptedRequesterValue, lowHighChallenge.Target, lowHighChallenge.Requester, correlationId));
            return lowHighChallenge;
        }
    }

    public class LowHighChallengeStateMachine : MassTransitStateMachine<LowHighChallenge>
    {
        public LowHighChallengeStateMachine(LowHighChallengeFactory lowHighChallengeFactory, Guid id)
        {
            InstanceState(instance => instance.CurrentState);

            Event(() => Request, configurator =>
                configurator
                    .CorrelateById(context =>
                    {
                        return context.Message.CorrelationId;
                    })
                    .SelectId(context =>
                    {
                        return context.Message.CorrelationId;
                    })
                    .SetSagaFactory(context =>
                    {
                        Console.WriteLine($"creating new {Environment.NewLine} requester:{context.Message.Requester} target:{context.Message.Target}");
                        return lowHighChallengeFactory.CreateFromRequest(context.Message.Requester, context.Message.Target, context.Message.CorrelationId, context.Message.Encrypted);
                    })
                );

            Event(() => Response, configurator =>
                configurator
                    .CorrelateById(context =>
                    {
                        return context.Message.Id;
                    }));

            Event(() => Result, configurator =>
                  configurator.CorrelateById(context =>
                  {
                      return context.Message.Id;
                  }));

            Initially(
                When(Request, context=>context.Data.Target == id)
                    .Then(context =>
                    {
                        context.Instance.SetRequest(context.Data.Encrypted);
                    })
                    .ThenAsync(context =>
                    {
                        return Console.Out.WriteLineAsync($"Setting Encrypted Bytes iid:{context.Instance.CorrelationId}");
                    })
                    .TransitionTo(AwaitingResult)
                    .Publish(context =>
                    {
                        return new LowHighChallengeResponseEvent(context.Instance.CorrelationId,
                                context.Instance.TargetValue.Value, context.Instance.TargetIsLowerThanRequester.Value,
                                context.Instance.Requester);
                    })
                );

            Initially(
                When(Request, context=>context.Data.Requester == id)
                    .ThenAsync(context =>
                    {
                        return Console.Out.WriteLineAsync($"Adding to repo:{context.Instance.CorrelationId}");
                    })
                    .TransitionTo(AwaitingResponse)
            );

            During(AwaitingResponse,
                When(Response, context=>context.Data.Requester == id)
                    .Then(context =>
                    {
                        context.Instance.SetResponse(context.Data.IsLowerThan, context.Data.Value);
                    })
                    .ThenAsync(context =>
                    {
                        return Console.Out.WriteLineAsync($"Setting Response  eid:{context.Data.Id} iid:{context.Instance.CorrelationId}");
                    })
                    .TransitionTo(RequesterComplete)
                    .Publish(context =>
                    {
                        return new LowHighChallengeResultEvent(context.Data.Id, context.Instance.RequesterKey,
                                context.Instance.Target);
                    })
                    .ThenAsync(context =>
                    {
                        return Console.Out.WriteLineAsync($"id:{id} win:{context.Instance.Win}");
                    })
                    .Finalize()
                );

            During(AwaitingResult,
                When(Result, context=>context.Data.Target == id)
                    .Then(context =>
                    {
                        context.Instance.SetResult(context.Data.Key, (key, encrypted) => int.Parse(Encoding.UTF8.GetString(encrypted)));
                    })
                    .ThenAsync(context =>
                    {
                        return Console.Out.WriteLineAsync($"Setting Result  eid:{context.Data.Id} iid:{context.Instance.CorrelationId}");
                    })
                    .TransitionTo(TargetComplete)
                    .ThenAsync(context =>
                    {
                        return Console.Out.WriteLineAsync($"id:{id} win:{context.Instance.Win}");
                    })
                    .Finalize(),
                When(Response, context=>context.Data.Requester == id)
                );

            SetCompletedWhenFinalized();
        }

        public State AwaitingResponse { get; private set; }
        public State AwaitingResult { get; private set; }
        public State RequesterComplete { get; private set; }
        public State TargetComplete { get; private set; }

        public Event<LowHighChallengeRequestEvent> Request { get; private set; }
        public Event<LowHighChallengeResponseEvent> Response { get; private set; }
        public Event<LowHighChallengeResultEvent> Result { get; private set; }
    }

    public class LowHighChallengeCompletedEvent
    {
        public Guid Id { get; }
        public bool Win { get; }

        public LowHighChallengeCompletedEvent(Guid id, bool win)
        {
            Id = id;
            Win = win;
        }
    }

    public class LowHighChallengeResponseEvent
    {
        public LowHighChallengeResponseEvent(Guid id, int value, bool isLowerThan, Guid requester)
        {
            Id = id;
            Value = value;
            IsLowerThan = isLowerThan;
            Requester = requester;
        }

        public Guid Id { get; }
        public Guid Requester { get; set; }
        public int Value { get; }
        public bool IsLowerThan { get; }
    }

    public class LowHighChallengeRequestEvent
    {
        public LowHighChallengeRequestEvent(byte[] encrypted, Guid target, Guid requester, Guid correlationId)
        {
            Encrypted = encrypted;
            Target = target;
            Requester = requester;
            CorrelationId = correlationId;
        }

        public Guid Target { get; set; }
        public byte[] Encrypted { get; }
        public Guid Requester { get; }
        public Guid CorrelationId { get; }
    }

    public class LowHighChallengeResultEvent
    {
        public LowHighChallengeResultEvent(Guid id, byte[] key, Guid target)
        {
            Id = id;
            Key = key;
            Target = target;
        }

        public Guid Id { get; }
        public byte[] Key { get; }
        public Guid Target { get; }
    }
}
