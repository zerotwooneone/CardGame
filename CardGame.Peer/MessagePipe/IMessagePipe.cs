﻿using System;
using System.Threading.Tasks;

namespace CardGame.Peer.MessagePipe
{
    public interface IMessagePipe: IDisposable
    {
        Task SendMessage(Message message);
        IObservable<Message> MessageObservable { get; }
    }
}