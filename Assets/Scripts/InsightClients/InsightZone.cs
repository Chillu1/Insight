﻿using UnityEngine;

public class InsightZone : MonoBehaviour
{
    InsightNetworkClient insight;

	// Use this for initialization
	void Start ()
    {
        DontDestroyOnLoad(gameObject);
        insight = new InsightNetworkClient();
        insight.StartClient("localhost", 5000);

    }
	
	// Update is called once per frame
	void Update ()
    {
        insight.HandleNewMessages();

    }

    private void OnApplicationQuit()
    {
        insight.StopClient();
    }
}
