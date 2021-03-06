﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class PlumbingComponentSolver : ComponentSolver
{
    public PlumbingComponentSolver(BombCommander bombCommander, BombComponent bombComponent, IRCConnection ircConnection, CoroutineCanceller canceller) :
        base(bombCommander, bombComponent, ircConnection, canceller)
    {
        _check = (MonoBehaviour)_checkField.GetValue(bombComponent.GetComponent(_componentType));

        _pipes = new MonoBehaviour[6][];
        for (var i = 0; i < 6; i++)
        {
            _pipes[i] = new MonoBehaviour[6];
            for (var j = 0; j < 6; j++)
            {
                _pipes[i][j] = (MonoBehaviour) _pipesField[i][j].GetValue(bombComponent.GetComponent(_componentType));
            }
        }
        modInfo = ComponentSolverFactory.GetModuleInfo(GetModuleType());
    }

    protected override IEnumerator RespondToCommandInternal(string inputCommand)
    {
		inputCommand = inputCommand.ToLowerInvariant();

        if (inputCommand.EqualsAny("submit", "check"))
        {
            yield return "Checking for leaks Kappa";
            yield return DoInteractionClick(_check);
            yield break;
        }

        if (!inputCommand.StartsWith("rotate "))
        {
            yield break;
        }
        inputCommand = inputCommand.Substring(6);

        string[] sequence = inputCommand.ToLowerInvariant().Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
        List<MonoBehaviour> pipes = new List<MonoBehaviour>();
        bool elevator = false;

        foreach (string buttonString in sequence)
        {
            var letters = "abcdef";
            var numbers = "123456";
            if (buttonString.Length != 2 || letters.IndexOf(buttonString[0]) < 0 ||
                numbers.IndexOf(buttonString[1]) < 0) continue;

            var row = numbers.IndexOf(buttonString[1]);
            var col = letters.IndexOf(buttonString[0]);

            MonoBehaviour button = _pipes[row][col];
            pipes.Add(button);
            elevator |= pipes.FindAll(x => x == button).Count >= 4;
        }

        if (pipes.Count > 0)
        {
            yield return inputCommand;
            if (elevator)
                yield return "elevator music";
        }
        foreach (MonoBehaviour button in pipes)
        {
            if (Canceller.ShouldCancel)
            {
                Canceller.ResetCancel();
                yield break;
            }
            yield return DoInteractionClick(button);
        }
    }

    static PlumbingComponentSolver()
    {
        _componentType = ReflectionHelper.FindType("AdvancedMaze");
        _checkField = _componentType.GetField("ButtonCheck", BindingFlags.Public | BindingFlags.Instance);
        _pipesField = new FieldInfo[6][];
        var letters = "ABCDEF";
        var numbers = "123456";
        for (var i = 0; i < 6; i++)
        {
            _pipesField[i] = new FieldInfo[6];
            var letter = letters.Substring(i, 1);
            for (var j = 0; j < 6; j++)
            {
                var number = numbers.Substring(j, 1);
                _pipesField[i][j] = _componentType.GetField("Button" + letter + number, BindingFlags.Public | BindingFlags.Instance);
            }
        }
    }

    private static Type _componentType = null;
    private static FieldInfo _checkField = null;
    private static FieldInfo[][] _pipesField = null;

    private MonoBehaviour _check = null;
    private MonoBehaviour[][] _pipes = null;
}