// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using System;
using UnityEngine;
using UnityEngine.Events;

public class MapLoadingIndicator : MonoBehaviour
{
    [Tooltip("GameObject to show when the map is loading data")]
    public GameObject mapLoadingIndicator;

    [Header("Events")]
    public UnityEvent onSceneLoaded;

    private bool initialLoadComplete;

    private void Awake()
    {
    }

    private void Start()
    {
        initialLoadComplete = false;
    }

    private void OnDestroy()
    {
    }

    private void AfterMapUpdate(object sender, EventArgs e)
    {
        if (!initialLoadComplete)
        {
            OnSceneLoadComplete();
        }
    }

    private void OnSceneLoadComplete()
    {
        initialLoadComplete = true;
        onSceneLoaded?.Invoke();
    }
}