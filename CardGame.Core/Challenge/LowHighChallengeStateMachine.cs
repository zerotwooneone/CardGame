using System;
using Automatonymous;

namespace CardGame.Core.Challenge
{
    public class LowHighChallengeStateMachine : MassTransitStateMachine<LowHighChallenge>
    {
        public Guid Id { get; }

        public LowHighChallengeStateMachine(LowHighChallengeFactory lowHighChallengeFactory, Guid id, ICryptoService cryptoService)
        {
            Id = id;
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
                    .CorrelateById(context => context.Message.CorrelationId));

            Event(() => Result, configurator =>
                configurator.CorrelateById(context => context.Message.CorrelationId));

            Initially(
                When(Request, context => context.Data.Target == id)
                    .Then(context =>
                    {
                        //Console.Out.WriteLine($"{nameof(Initial)} handling {Request} {Environment.NewLine}   R:{context.Data.Requester}{Environment.NewLine}   T:{context.Data.Target}{Environment.NewLine}   I:{id}");
                        context.Instance.SetRequest(context.Data.Encrypted);
                    })
                    .Publish(context =>
                    {
                        return new LowHighChallengeResponseEvent(context.Instance.CorrelationId,
                            context.Instance.TargetValue.Value, context.Instance.TargetIsLowerThanRequester.Value,
                            context.Instance.Requester);
                    })
                ,
                When(Response, context => context.Instance.Target == id)
                    .TransitionTo(AwaitingResult)
                ,
                When(Request, context => context.Data.Requester != id && context.Data.Target != id)
                    //.Then(context => 
                    //    Console.Out.WriteLineAsync($"{nameof(Initial)} handling {Request}  Going to observe{Environment.NewLine}   R:{context.Data.Requester}{Environment.NewLine}   T:{context.Data.Target}{Environment.NewLine}   I:{id}"))
                    .TransitionTo(Observaing)
                ,
                When(Request, context => context.Data.Requester == id)
                    //.Then(context => Console.Out.WriteLineAsync($"{nameof(Initial)} handling {Request}  {context.Instance.CorrelationId.ToString().Substring(0, 8)}{Environment.NewLine}   R:{context.Data.Requester}{Environment.NewLine}   T:{context.Data.Target}{Environment.NewLine}   I:{id}"))
                    .TransitionTo(AwaitingResponse)
            );

            During(AwaitingResponse,
                When(Response, context => context.Data.Requester == id)
                    .Then(context =>
                    {
                        context.Instance.SetResponse(context.Data.IsLowerThan, context.Data.Value);
                    })
                    //.Then(context => Console.Out.WriteLineAsync($"{nameof(AwaitingResponse)} handling {Response} going to final{Environment.NewLine}   R:{context.Data.Requester}{Environment.NewLine}   T:{context.Instance.Target}{Environment.NewLine}   I:{id}"))
                    .Publish(context =>
                    {
                        return new LowHighChallengeResultEvent(context.Data.CorrelationId, context.Instance.RequesterKey,
                            context.Instance.Target);
                    })
                ,
                When(Result, context => context.Instance.Requester == id)
                    .TransitionTo(Final)
                    .Finalize()
            );

            During(AwaitingResult,
                When(Result, context => context.Data.Target == id)
                    .Then(context =>
                    {
                        var requesterValue =
                            cryptoService.DecryptInt(context.Instance.EncryptedRequesterValue, context.Data.Key);
                        context.Instance.SetResult(context.Data.Key, requesterValue);
                    })
                    //.Then(context => Console.Out.WriteLineAsync($"{nameof(AwaitingResult)} handling {Result} going to final{Environment.NewLine}   R:{context.Instance.Requester}{Environment.NewLine}   T:{context.Data.Target}{Environment.NewLine}   I:{id}"))
                    .Publish(context => new LowHighChallengeCompletedEvent(context.Data.CorrelationId, context.Instance.RequesterWin.Value))
                    .TransitionTo(Final)
                    .Finalize()
            );

            During(Observaing,
                When(Response)
                    .Then(context => { }
                        //Console.Out.WriteLine($"{nameof(Observaing)} handling {Response}  do nothing{Environment.NewLine}   R:{context.Data.Requester}{Environment.NewLine}   T:{context.Instance.Target}{Environment.NewLine}   I:{id}")
                        ),
                When(Result)
                    .Then(context => { }
                        //Console.Out.WriteLine($"{nameof(Observaing)} handling {Result}  do nothing{Environment.NewLine}   R:{context.Instance.Requester}{Environment.NewLine}   T:{context.Data.Target}{Environment.NewLine}   I:{id}")
                        )
                    .Finalize()
                );

            SetCompletedWhenFinalized();
        }

        public State AwaitingResponse { get; private set; }
        public State AwaitingResult { get; private set; }
        public State Observaing { get; private set; }

        public Event<LowHighChallengeRequestEvent> Request { get; private set; }
        public Event<LowHighChallengeResponseEvent> Response { get; private set; }
        public Event<LowHighChallengeResultEvent> Result { get; private set; }
    }
}