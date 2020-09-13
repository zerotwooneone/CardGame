﻿using System.Threading.Tasks;
using CardGame.CommonModel.Bus;

namespace CardGame.Utils.Abstractions.Bus
{
    public interface IServiceCallRouter
    {
        void Configure();
    }
}