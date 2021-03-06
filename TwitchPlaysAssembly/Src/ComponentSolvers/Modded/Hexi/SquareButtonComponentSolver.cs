﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class SquareButtonComponentSolver : ComponentSolver
{
    public SquareButtonComponentSolver(BombCommander bombCommander, BombComponent bombComponent, IRCConnection ircConnection, CoroutineCanceller canceller) :
        base(bombCommander, bombComponent, ircConnection, canceller)
    {
        _button = (MonoBehaviour)_buttonField.GetValue(bombComponent.GetComponent(_componentType));
        modInfo = ComponentSolverFactory.GetModuleInfo(GetModuleType());
    }

    protected override IEnumerator RespondToCommandInternal(string inputCommand)
    {
		inputCommand = inputCommand.ToLowerInvariant();

		if (!_held && inputCommand.EqualsAny("tap", "click"))
        {
            yield return "tap";
            yield return DoInteractionClick(_button);
        }
        if (!_held && (inputCommand.StartsWith("tap ") ||
                       inputCommand.StartsWith("click ")))
        {
            yield return "tap2";

            IEnumerator releaseCoroutine = ReleaseCoroutine(inputCommand.Substring(inputCommand.IndexOf(' ')));
            while (releaseCoroutine.MoveNext())
            {
                yield return releaseCoroutine.Current;
            }
        }
        else if (!_held && inputCommand.EqualsAny("hold", "press"))
        {
            yield return "hold";

            _held = true;
            DoInteractionStart(_button);
            yield return new WaitForSeconds(2.0f);
        }
        else if (_held && inputCommand.StartsWith("release "))
        {
            IEnumerator releaseCoroutine = ReleaseCoroutine(inputCommand.Substring(inputCommand.IndexOf(' ')));
            while (releaseCoroutine.MoveNext())
            {
                yield return releaseCoroutine.Current;
            }
        }
    }

    private IEnumerator ReleaseCoroutine(string second)
    {
        string[] list = second.Split(' ');
        List<int> sortedTimes = new List<int>();
        foreach(string value in list)
        {
            if(!int.TryParse(value, out int time))
            {
                int pos = value.LastIndexOf(':');
                if(pos == -1) continue;
                int hour = 0;
                if(!int.TryParse(value.Substring(0, pos), out int min))
                {
                    int pos2 = value.IndexOf(":");
                    if ( (pos2 == -1) || (pos == pos2) ) continue;
                    if (!int.TryParse(value.Substring(0, pos2), out hour)) continue;
                    if (!int.TryParse(value.Substring(pos2+1, pos-pos2-1), out min)) continue;
                }
                if(!int.TryParse(value.Substring(pos+1), out int sec)) continue;
                time = (hour * 3600) + (min * 60) + sec;
            }
            sortedTimes.Add(time);
        }
        sortedTimes.Sort();
        sortedTimes.Reverse();
        if(sortedTimes.Count == 0) yield break;

        yield return "release";

        TimerComponent timerComponent = BombCommander.Bomb.GetTimer();

        int timeTarget = sortedTimes[0];
        sortedTimes.RemoveAt(0);

        int waitTime = (int)(timerComponent.TimeRemaining + 0.25f);
        waitTime -= timeTarget;
        if (waitTime >= 30)
            yield return "elevator music";

        float timeRemaining = float.PositiveInfinity;
        while (timeRemaining > 0.0f)
        {
            if (Canceller.ShouldCancel)
            {
                Canceller.ResetCancel();
				yield return string.Format("sendtochat The button was not {0} due to a request to cancel.", _held ? "released" : "tapped");
				yield break;
            }

            timeRemaining = (int)(timerComponent.TimeRemaining + 0.25f);

            if (timeRemaining < timeTarget)
            {
                if (sortedTimes.Count == 0)
                {
					yield return string.Format("sendtochaterror The button was not {0} because all of your specfied times are greater than the time remaining.", _held ? "released" : "tapped");
                    yield break;
                }
                timeTarget = sortedTimes[0];
                sortedTimes.RemoveAt(0);

                waitTime = (int)timeRemaining;
                waitTime -= timeTarget;
                if (waitTime >= 30)
                    yield return "elevator music";

                continue;
            }
            if (timeRemaining == timeTarget)
            {
                if (!_held)
                {
                    DoInteractionStart(_button);
                    yield return new WaitForSeconds(0.1f);
                }
                DoInteractionEnd(_button);
                _held = false;
                yield break;
            }

            yield return null;
        }
    }

    static SquareButtonComponentSolver()
    {
        _componentType = ReflectionHelper.FindType("AdvancedButton");
        _buttonField = _componentType.GetField("Button", BindingFlags.Public | BindingFlags.Instance);
    }

    private static Type _componentType = null;
    private static FieldInfo _buttonField = null;

    private MonoBehaviour _button = null;
    private bool _held = false;
}
