using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ResourceColor
{
    public static Color TypeColor(ResourceType type)
    {
        float constant = 180 / 255f;
        switch (type)
        {

            case ResourceType.Cyan:
                return new Color(constant, 1, 1, 1);
            case ResourceType.Magenta:
                return new Color(1, constant, 1, 1);
            case ResourceType.Yellow:
                return new Color(1, 1, constant, 1);
            case ResourceType.Red:
                return new Color(1, constant, constant, 1);
            case ResourceType.Green:
                return new Color(constant, 1, constant, 1);
            case ResourceType.Blue:
                return new Color(constant, constant, 1, 1);
            case ResourceType.Black:
                return new Color(constant, constant, constant, 1);

        }
        return new Color(0,0,0,0);
    }
}
