﻿using System.Collections;
using UnityEngine;

public class PostGameMessageResponder : MessageResponder
{
    public TwitchLeaderboard twitchLeaderboardPrefab = null;
    public Leaderboard leaderboard = null;

    private PostGameCommander _postGameCommander = null;
    private TwitchLeaderboard _leaderboardDisplay = null;

    #region Unity Lifecycle
    private void OnEnable()
    {
        StartCoroutine(CheckForResultsPage());

        _leaderboardDisplay = Instantiate<TwitchLeaderboard>(twitchLeaderboardPrefab);
        _leaderboardDisplay.leaderboard = leaderboard;

        leaderboard.SaveDataToFile();
    }

    private void OnDisable()
    {
        StopAllCoroutines();

        _postGameCommander = null;

        DestroyObject(_leaderboardDisplay);
        _leaderboardDisplay = null;
    }
    #endregion

    #region Protected/Private Methods
    protected override void OnMessageReceived(string userNickName, string userColorCode, string text)
    {
        
        if (_postGameCommander != null)
        {
            _coroutineQueue.AddToQueue(_postGameCommander.RespondToCommand(userNickName, text, null, _ircConnection));
        }
    }

    private IEnumerator CheckForResultsPage()
    {
        yield return null;

        while (true)
        {
            ResultPage[] resultPages = FindObjectsOfType<ResultPage>();
            if (resultPages != null && resultPages.Length > 0)
            {
                ResultPage resultPageBehaviour = resultPages[0];
                _postGameCommander = new PostGameCommander(resultPageBehaviour);
                break;
            }

            yield return null;
        }
        InputInterceptor.EnableInput();
    }
    #endregion
}
