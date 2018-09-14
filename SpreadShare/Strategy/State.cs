﻿using System;
using Binance.Net.Objects;
using Microsoft.Extensions.Logging;
using SpreadShare.Models;

namespace SpreadShare.Strategy
{
    abstract class State
    {
        public enum ResponseCodes { SUCCES, NOT_DEFINED }
        protected StateManager _stateManager;
        public Context Context { get; set; }
        protected ILogger Logger;

        protected State()
        {
            Context = new Context();
        }

        /// <summary>
        /// Initialise the state
        /// </summary>
        /// <param name="context">Set of objects that are required for the state to work</param>
        /// <param name="stateManager"></param>
        /// <param name="loggerFactory"></param>
        public void Activate(Context context, StateManager stateManager, ILoggerFactory loggerFactory)
        {
            Context = context;
            _stateManager = stateManager;
            Logger = loggerFactory.CreateLogger(GetType());
            ValidateContext();
        }

        /// <summary>
        /// Validates if all the required parameters exist within the context
        /// </summary>
        protected abstract void ValidateContext();


        /// <summary>
        /// Switching states
        /// </summary>
        /// <param name="s">State to switch to</param>
        protected void SwitchState(State s)
        {
            _stateManager.SwitchState(s);
        }

        public virtual ResponseCodes OnCandle(Candle c) {
            return ResponseCodes.NOT_DEFINED;
        }
        public virtual ResponseCodes OnOrderUpdate(BinanceStreamOrderUpdate order) {
            return ResponseCodes.NOT_DEFINED;
        }

        public virtual ResponseCodes OnTimer() {
            return ResponseCodes.NOT_DEFINED;
        }
    }
}
