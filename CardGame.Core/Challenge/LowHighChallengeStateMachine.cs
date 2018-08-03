using System;
using System.Text;
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
                When(Request, context=>context.Data.Target == id)
                    .Then(context =>
                    {
                        context.Instance.SetRequest(context.Data.Encrypted);
                    })
                    //.Then(context => Console.Out.WriteLineAsync($"Receive {nameof(Request)} {context.Instance.CorrelationId.ToString().Substring(0,8)}"))//{Environment.NewLine}  R:{context.Data.Requester}{Environment.NewLine}  T:{context.Data.Target}"))
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
                    //.Then(context => Console.Out.WriteLineAsync($"Make {nameof(Request)} {context.Instance.CorrelationId.ToString().Substring(0,8)}"))//{Environment.NewLine}  R:{context.Data.Requester}{Environment.NewLine}  T:{context.Data.Target}"))
                    .TransitionTo(AwaitingResponse)
            );

            Initially(
                When(Result)
                    .Then(context => { }
                        //Console.Out.WriteLineAsync($"{nameof(Initial)}->{nameof(Result)} {context.Instance.CorrelationId.ToString().Substring(0, 8)}{Environment.NewLine}  R:{context.Instance.Requester}{Environment.NewLine}  T:{context.Data.Target}{Environment.NewLine}  id:{id}")
                        )
                ,
                When(Response)
                    .Then(context => { }
                        //Console.Out.WriteLineAsync($"{nameof(Initial)}->{nameof(Response)} {context.Instance.CorrelationId.ToString().Substring(0, 8)}{Environment.NewLine}  R:{context.Instance.Requester}{Environment.NewLine}  T:{context.Instance.Target}{Environment.NewLine}  id:{id}")
                        )
            );

            During(AwaitingResponse,
                When(Response, context=>context.Data.Requester == id)
                    .Then(context =>
                    {
                        context.Instance.SetResponse(context.Data.IsLowerThan, context.Data.Value);
                    })
                    //.Then(context => Console.Out.WriteLineAsync($"{nameof(Response)} {context.Instance.CorrelationId.ToString().Substring(0,8)}"))//{Environment.NewLine}  R:{context.Data.Requester}{Environment.NewLine}  R:{context.Instance.Target}"))
                    .TransitionTo(RequesterComplete)
                    .Publish(context =>
                    {
                        return new LowHighChallengeResultEvent(context.Data.CorrelationId, context.Instance.RequesterKey,
                            context.Instance.Target);
                    })
                    //.Finalize()
            );

            During(AwaitingResult,
                When(Result, context=>context.Data.Target == id)
                    .Then(context =>
                    {
                        var requesterValue =
                            cryptoService.DecryptInt(context.Instance.EncryptedRequesterValue, context.Data.Key);
                        context.Instance.SetResult(context.Data.Key, requesterValue);
                    })
                    //.Then(context => Console.Out.WriteLineAsync($"{nameof(Result)} {context.Instance.CorrelationId.ToString().Substring(0,8)}"))
                    .TransitionTo(TargetComplete)
                    .Publish(context=>new LowHighChallengeCompletedEvent(context.Data.CorrelationId, context.Instance.RequesterWin.Value))
                //    .Finalize()
                ,
                When(Response, context=>context.Data.Requester != id)
            );

            During(RequesterComplete,
                When(Result)
                    .Then(context => { }
                        //Console.Out.WriteLineAsync($"{nameof(RequesterComplete)}->{nameof(Result)} {context.Instance.CorrelationId.ToString().Substring(0, 8)}{Environment.NewLine}  R:{context.Instance.Requester}{Environment.NewLine}  T:{context.Data.Target}{Environment.NewLine}  id:{id}")
                        ));

            SetCompletedWhenFinalized();
        }

        public State AwaitingResponse { get; private set; }
        public State AwaitingResult { get; private set; }
        public State RequesterComplete { get; private set; }
        public State TargetComplete { get; private set; }

        public Event<LowHighChallengeRequestEvent> Request { get; private set; }
        public Event<LowHighChallengeResponseEvent> Response { get; private set; }
        public Event<LowHighChallengeResultEvent> Result { get; private set; }
        //public Event<LowHighChallengeCompletedEvent> Complete { get; private set; }
    }
}