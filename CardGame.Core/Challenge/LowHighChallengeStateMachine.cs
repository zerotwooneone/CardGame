using System;
using System.Text;
using Automatonymous;

namespace CardGame.Core.Challenge
{
    public class LowHighChallengeStateMachine : MassTransitStateMachine<LowHighChallenge>
    {
        public LowHighChallengeStateMachine(LowHighChallengeFactory lowHighChallengeFactory, Guid id, ICryptoService cryptoService)
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
                        //Console.WriteLine($"creating new {Environment.NewLine} requester:{context.Message.Requester} target:{context.Message.Target}");
                        return lowHighChallengeFactory.CreateFromRequest(context.Message.Requester, context.Message.Target, context.Message.CorrelationId, context.Message.Encrypted);
                    })
            );

            Event(() => Response, configurator =>
                configurator
                    .CorrelateById(context =>
                    {
                        return context.Message.CorrelationId;
                    }));

            Event(() => Result, configurator =>
                configurator.CorrelateById(context =>
                {
                    return context.Message.CorrelationId;
                }));

            Initially(
                When(Request, context=>context.Data.Target == id)
                    .Then(context =>
                    {
                        context.Instance.SetRequest(context.Data.Encrypted);
                    })
                    //.ThenAsync(context =>
                    //{
                    //    return Console.Out.WriteLineAsync($"Setting Encrypted Bytes iid:{context.Instance.CorrelationId}");
                    //})
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
                    //.ThenAsync(context =>
                    //{
                    //    return Console.Out.WriteLineAsync($"Adding to repo:{context.Instance.CorrelationId}");
                    //})
                    .TransitionTo(AwaitingResponse)
            );

            During(AwaitingResponse,
                When(Response, context=>context.Data.Requester == id)
                    .Then(context =>
                    {
                        context.Instance.SetResponse(context.Data.IsLowerThan, context.Data.Value);
                    })
                    //.ThenAsync(context =>
                    //{
                    //    return Console.Out.WriteLineAsync($"Setting Response  eid:{context.Data.CorrelationId} iid:{context.Instance.CorrelationId}");
                    //})
                    .TransitionTo(RequesterComplete)
                    .Publish(context =>
                    {
                        return new LowHighChallengeResultEvent(context.Data.CorrelationId, context.Instance.RequesterKey,
                            context.Instance.Target);
                    })
                    //.ThenAsync(context =>
                    //{
                    //    return Console.Out.WriteLineAsync($"id:{id} win:{context.Instance.Win}");
                    //})
                    .Finalize()
            );

            During(AwaitingResult,
                When(Result, context=>context.Data.Target == id)
                    .Then(context =>
                    {
                        var requesterValue =
                            cryptoService.DecryptInt(context.Instance.EncryptedRequesterValue, context.Data.Key);
                        context.Instance.SetResult(context.Data.Key, requesterValue);
                    })
                    //.ThenAsync(context =>
                    //{
                    //    return Console.Out.WriteLineAsync($"Setting Result  eid:{context.Data.CorrelationId} iid:{context.Instance.CorrelationId}");
                    //})
                    .TransitionTo(TargetComplete)
                    //.ThenAsync(context =>
                    //{
                    //    return Console.Out.WriteLineAsync($"id:{id} win:{context.Instance.Win}");
                    //})
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
}