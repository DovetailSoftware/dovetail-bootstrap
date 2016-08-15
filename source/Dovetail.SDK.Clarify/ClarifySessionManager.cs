using System;
using System.Collections.Generic;
using FChoice.Foundation.Clarify;

namespace Dovetail.SDK.Clarify
{
    public class ClarifySessionManager : IClarifySessionManager
    {
        private static Action<ClarifySession> _closeSession;

        static ClarifySessionManager()
        {
            Reset();
        }

        private readonly IEnumerable<IClarifySessionListener> _listeners;
        private readonly ILogger _logger;

        public ClarifySessionManager(IEnumerable<IClarifySessionListener> listeners, ILogger logger)
        {
            _listeners = listeners;
            _logger = logger;
        }

        public void Configure(IClarifySession session)
        {
            _logger.LogInfo("Configuring new session " + session.Id);
            
            foreach (var listener in _listeners)
            {
                _logger.LogDebug("Using {0} to configure newly created session {1}", listener.GetType().Name, session.Id);

                try
                {
                    listener.Created(session);
                }
                catch (Exception e)
                {
                    _logger.LogError("Error in listener when configuring newly created session", e);
                    throw;
                }
                
            }

            foreach (var listener in _listeners)
            {
                _logger.LogDebug("Using {0} to configure newly started session {1}", listener.GetType().Name, session.Id);
                
                try
                {
                    listener.Started(session);
                }
                catch (Exception e)
                {
                    _logger.LogError("Error in listener when starting session", e);
                    throw;
                }
            }
        }

        public void Eject(IClarifySession session)
        {
            _logger.LogInfo("Ejecting session " + session.Id);

            foreach (var listener in _listeners)
            {
                _logger.LogDebug("Using {0} to observe the closing of session {1}", listener.GetType().Name, session.Id);

                try
                {
                    listener.Closed(session);
                }
                catch (Exception e)
                {
                    _logger.LogError("Error in listener when closing session", e);
                    throw;
                }
            }
        }

        public static void Close(ClarifySession session)
        {
            _closeSession(session);
        }

        public static void CloseWith(Action<ClarifySession> action)
        {
            _closeSession = action;
        }

        public static void Reset()
        {
            CloseWith(Close);
        }
    }
}