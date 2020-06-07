using System.Collections.Generic;
using UnityEngine;

namespace Insight
{
    public enum CallbackStatus
    {
        Ok,
        Error,
        Timeout
    }

    public abstract class InsightCommon : MonoBehaviour
	{
        public bool DontDestroy = true; //Sets DontDestroyOnLoad for this object. Default to true. Can be disabled via Inspector or runtime code.
        public bool AutoStart = true;
        public bool logNetworkMessages = false;
        public string networkAddress = "localhost";
        
        protected Dictionary<short, InsightNetworkMessageDelegate> messageHandlers = new Dictionary<short, InsightNetworkMessageDelegate>(); //Default handlers

        protected enum ConnectState
        {
            None,
            Connecting,
            Connected,
            Disconnected,
        }
        protected ConnectState connectState = ConnectState.None;

        protected int callbackIdIndex = 0; // 0 is a _special_ id - it represents _no callback_. 
        protected Dictionary<int, CallbackData> callbacks = new Dictionary<int, CallbackData>();

        public delegate void CallbackHandler(CallbackStatus status, InsightNetworkMessage netMsg);
        public delegate void SendToAllFinishedCallbackHandler(CallbackStatus status);

        public const float callbackTimeout = 30f; // all callbacks have a 30 second time out. 

        public bool isConnected { get { return connectState == ConnectState.Connected; } }

        public void RegisterHandler(short msgType, InsightNetworkMessageDelegate handler)
        {
            if (messageHandlers.ContainsKey(msgType))
            {
                Debug.Log("NetworkConnection.RegisterHandler replacing " + msgType);
            }
            messageHandlers[msgType] = handler;
        }

        public void UnRegisterHandler(short msgType, InsightNetworkMessageDelegate handler)
        {
            if (messageHandlers.ContainsKey(msgType))
            {
                messageHandlers[msgType] -= handler;
            }
        }

        protected virtual void CheckCallbackTimeouts()
        {
            foreach (KeyValuePair<int, CallbackData> callback in callbacks)
            {
                if (callback.Value.timeout < Time.realtimeSinceStartup)
                {
                    callback.Value.callback.Invoke(CallbackStatus.Timeout, null);
                    callbacks.Remove(callback.Key);
                    break;
                }
            }
        }

        public void ClearHandlers()
        {
            messageHandlers.Clear();
        }

        public abstract void StartInsight();

        public abstract void StopInsight();

        public struct CallbackData
        {
            public CallbackHandler callback;
            public float timeout;
        }

        [System.Serializable]
        public class SendToAllFinishedCallbackData
        {
            public SendToAllFinishedCallbackHandler callback;
            public HashSet<int> requiredCallbackIds;
            public int callbacks;
            public float timeout;
        }

        public int GetId<T>()
        {
            return typeof(T).FullName.GetStableHashCode() & 0xFFFF;
        }

        public int GetId(Type type)
        {
            return type.FullName.GetStableHashCode() & 0xFFFF;
        }
    }
}
