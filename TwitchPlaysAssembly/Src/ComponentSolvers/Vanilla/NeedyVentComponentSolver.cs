﻿using System.Collections;

public class NeedyVentComponentSolver : ComponentSolver
{
    public NeedyVentComponentSolver(BombCommander bombCommander, NeedyVentComponent bombComponent, IRCConnection ircConnection, CoroutineCanceller canceller) :
        base(bombCommander, bombComponent, ircConnection, canceller)
    {
		_yesButton = bombComponent.YesButton;
		_noButton = bombComponent.NoButton;
        modInfo = ComponentSolverFactory.GetModuleInfo("NeedyVentComponentSolver");
    }

    protected override IEnumerator RespondToCommandInternal(string inputCommand)
    {
        inputCommand = inputCommand.ToLowerInvariant();
        if (inputCommand.EqualsAny("y", "yes", "press y", "press yes"))
        {
            yield return "yes";
            yield return DoInteractionClick(_yesButton);
        }
        else if (inputCommand.EqualsAny("n", "no", "press n", "press no"))
        {
            yield return "no";
            yield return DoInteractionClick(_noButton);
        }
    }

    private KeypadButton _yesButton = null;
    private KeypadButton _noButton = null;
}
